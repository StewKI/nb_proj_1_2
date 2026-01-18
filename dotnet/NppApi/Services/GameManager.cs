using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NppApi.Hubs;
using NppCore.Models;
using NppCore.Services.Persistence.Redis;

namespace NppApi.Services;

public class GameManager : IHostedService, IDisposable
{
    private readonly ConcurrentDictionary<string, Game> _games = new();
    private readonly ConcurrentDictionary<string, string> _playerGameMap = new();
    private readonly IHubContext<GameHub> _hubContext;
    private readonly IGameStateRepository _gameStateRepository;
    private readonly ILogger<GameManager> _logger;
    private Timer? _gameLoopTimer;
    private readonly Random _random = new();
    private int _frameCount;
    private const int SyncInterval = 5; // Sync to Redis every 5 frames (~83ms)

    public GameManager(
        IHubContext<GameHub> hubContext,
        IGameStateRepository gameStateRepository,
        ILogger<GameManager> logger)
    {
        _hubContext = hubContext;
        _gameStateRepository = gameStateRepository;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await RecoverStateFromRedisAsync();
        _gameLoopTimer = new Timer(GameLoop, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000.0 / 60));
    }

    private async Task RecoverStateFromRedisAsync()
    {
        try
        {
            var games = await _gameStateRepository.LoadAllGamesAsync();
            var playerMappings = await _gameStateRepository.LoadAllPlayerMappingsAsync();

            foreach (var game in games)
            {
                // Only recover games that are still active (not finished)
                if (game.State != GameState.Finished)
                {
                    _games[game.Id] = game;
                }
            }

            foreach (var (connectionId, gameId) in playerMappings)
            {
                // Only restore mapping if the game exists
                if (_games.ContainsKey(gameId))
                {
                    _playerGameMap[connectionId] = gameId;
                }
            }

            _logger.LogInformation("Recovered {GameCount} games and {MappingCount} player mappings from Redis",
                _games.Count, _playerGameMap.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to recover state from Redis, starting fresh");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _gameLoopTimer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _gameLoopTimer?.Dispose();
    }

    public Game CreateGame(string connectionId, string playerName)
    {
        var gameId = Guid.NewGuid().ToString()[..8];
        var game = new Game
        {
            Id = gameId,
            Player1 = new Player { ConnectionId = connectionId, Name = playerName },
            State = GameState.WaitingForPlayer
        };

        InitializeBall(game);
        InitializePaddles(game);

        _games[gameId] = game;
        _playerGameMap[connectionId] = gameId;

        // Fire-and-forget Redis write
        _ = Task.Run(async () =>
        {
            try
            {
                await _gameStateRepository.CreateGameAsync(game);
                await _gameStateRepository.SetPlayerGameMappingAsync(connectionId, gameId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist game creation to Redis for game {GameId}", gameId);
            }
        });

        return game;
    }

    public Game? JoinGame(string gameId, string connectionId, string playerName)
    {
        if (!_games.TryGetValue(gameId, out var game))
            return null;

        if (game.State != GameState.WaitingForPlayer)
            return null;

        var player2 = new Player { ConnectionId = connectionId, Name = playerName };
        game.Player2 = player2;
        game.State = GameState.Playing;
        _playerGameMap[connectionId] = gameId;

        // Fire-and-forget Redis write
        _ = Task.Run(async () =>
        {
            try
            {
                await _gameStateRepository.JoinGameAsync(gameId, player2);
                await _gameStateRepository.SetPlayerGameMappingAsync(connectionId, gameId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist game join to Redis for game {GameId}", gameId);
            }
        });

        return game;
    }

    public List<LobbyGameDto> GetOpenGames()
    {
        return _games.Values
            .Where(g => g.State == GameState.WaitingForPlayer && g.Player1 != null)
            .Select(g => new LobbyGameDto
            {
                GameId = g.Id,
                HostName = g.Player1!.Name
            })
            .ToList();
    }

    public void UpdatePaddle(string connectionId, double y)
    {
        if (!_playerGameMap.TryGetValue(connectionId, out var gameId))
            return;

        if (!_games.TryGetValue(gameId, out var game))
            return;

        y = Math.Clamp(y, 0, Game.CanvasHeight - Game.PaddleHeight);

        if (game.Player1?.ConnectionId == connectionId)
            game.Paddle1.Y = y;
        else if (game.Player2?.ConnectionId == connectionId)
            game.Paddle2.Y = y;
    }

    public void RemovePlayer(string connectionId)
    {
        if (!_playerGameMap.TryRemove(connectionId, out var gameId))
            return;

        if (_games.TryRemove(gameId, out var game))
        {
            var otherPlayerId = game.Player1?.ConnectionId == connectionId
                ? game.Player2?.ConnectionId
                : game.Player1?.ConnectionId;

            if (otherPlayerId != null)
            {
                _playerGameMap.TryRemove(otherPlayerId, out _);
                _hubContext.Clients.Client(otherPlayerId).SendAsync("GameEnded", "Opponent disconnected");
            }

            // Fire-and-forget Redis cleanup
            _ = Task.Run(async () =>
            {
                try
                {
                    await _gameStateRepository.RemoveGameAsync(gameId);
                    await _gameStateRepository.RemovePlayerMappingAsync(connectionId);
                    if (otherPlayerId != null)
                    {
                        await _gameStateRepository.RemovePlayerMappingAsync(otherPlayerId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to clean up Redis for game {GameId}", gameId);
                }
            });

            _hubContext.Clients.All.SendAsync("LobbyUpdated", GetOpenGames());
        }
    }

    public string? GetGameId(string connectionId)
    {
        _playerGameMap.TryGetValue(connectionId, out var gameId);
        return gameId;
    }

    public Game? GetGame(string gameId)
    {
        _games.TryGetValue(gameId, out var game);
        return game;
    }

    private void InitializeBall(Game game)
    {
        game.Ball.X = Game.CanvasWidth / 2.0;
        game.Ball.Y = Game.CanvasHeight / 2.0;

        var angle = (_random.NextDouble() - 0.5) * Math.PI / 2;
        var direction = _random.Next(2) == 0 ? 1 : -1;
        var speed = 5.0;

        game.Ball.VelocityX = Math.Cos(angle) * speed * direction;
        game.Ball.VelocityY = Math.Sin(angle) * speed;
    }

    private void InitializePaddles(Game game)
    {
        game.Paddle1.Y = (Game.CanvasHeight - Game.PaddleHeight) / 2.0;
        game.Paddle2.Y = (Game.CanvasHeight - Game.PaddleHeight) / 2.0;
    }

    private void GameLoop(object? state)
    {
        _frameCount++;
        var shouldSync = _frameCount % SyncInterval == 0;

        foreach (var game in _games.Values.Where(g => g.State == GameState.Playing))
        {
            UpdateBall(game);
            BroadcastGameState(game);

            // Periodic sync to Redis every N frames
            if (shouldSync)
            {
                var gameId = game.Id;
                var gameCopy = game; // Capture for closure
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _gameStateRepository.SyncGameStateAsync(gameId, gameCopy);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to sync game state to Redis for game {GameId}", gameId);
                    }
                });
            }
        }
    }

    private void UpdateBall(Game game)
    {
        game.Ball.X += game.Ball.VelocityX;
        game.Ball.Y += game.Ball.VelocityY;

        // Top/bottom wall bounce
        if (game.Ball.Y <= 0 || game.Ball.Y >= Game.CanvasHeight - Game.BallSize)
        {
            game.Ball.VelocityY = -game.Ball.VelocityY;
            game.Ball.Y = Math.Clamp(game.Ball.Y, 0, Game.CanvasHeight - Game.BallSize);
        }

        // Paddle 1 collision (left)
        if (game.Ball.X <= Game.PaddleOffset + Game.PaddleWidth &&
            game.Ball.Y + Game.BallSize >= game.Paddle1.Y &&
            game.Ball.Y <= game.Paddle1.Y + Game.PaddleHeight &&
            game.Ball.VelocityX < 0)
        {
            game.Ball.VelocityX = -game.Ball.VelocityX * 1.05;
            var hitPos = (game.Ball.Y - game.Paddle1.Y) / Game.PaddleHeight;
            game.Ball.VelocityY = (hitPos - 0.5) * 8;
            game.Ball.X = Game.PaddleOffset + Game.PaddleWidth;
        }

        // Paddle 2 collision (right)
        if (game.Ball.X >= Game.CanvasWidth - Game.PaddleOffset - Game.PaddleWidth - Game.BallSize &&
            game.Ball.Y + Game.BallSize >= game.Paddle2.Y &&
            game.Ball.Y <= game.Paddle2.Y + Game.PaddleHeight &&
            game.Ball.VelocityX > 0)
        {
            game.Ball.VelocityX = -game.Ball.VelocityX * 1.05;
            var hitPos = (game.Ball.Y - game.Paddle2.Y) / Game.PaddleHeight;
            game.Ball.VelocityY = (hitPos - 0.5) * 8;
            game.Ball.X = Game.CanvasWidth - Game.PaddleOffset - Game.PaddleWidth - Game.BallSize;
        }

        // Score
        if (game.Ball.X < 0)
        {
            game.Player2!.Score++;
            CheckWin(game);
            InitializeBall(game);
        }
        else if (game.Ball.X > Game.CanvasWidth)
        {
            game.Player1!.Score++;
            CheckWin(game);
            InitializeBall(game);
        }
    }

    private void CheckWin(Game game)
    {
        if (game.Player1!.Score >= Game.WinScore || game.Player2!.Score >= Game.WinScore)
        {
            game.State = GameState.Finished;
            var winner = game.Player1.Score >= Game.WinScore ? game.Player1.Name : game.Player2.Name;

            _hubContext.Clients.Client(game.Player1.ConnectionId).SendAsync("GameEnded", winner);
            _hubContext.Clients.Client(game.Player2.ConnectionId).SendAsync("GameEnded", winner);

            var player1Id = game.Player1.ConnectionId;
            var player2Id = game.Player2.ConnectionId;
            var gameId = game.Id;

            _playerGameMap.TryRemove(player1Id, out _);
            _playerGameMap.TryRemove(player2Id, out _);
            _games.TryRemove(gameId, out _);

            // Fire-and-forget Redis cleanup
            _ = Task.Run(async () =>
            {
                try
                {
                    await _gameStateRepository.RemoveGameAsync(gameId);
                    await _gameStateRepository.RemovePlayerMappingAsync(player1Id);
                    await _gameStateRepository.RemovePlayerMappingAsync(player2Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to clean up Redis after game {GameId} finished", gameId);
                }
            });
        }
    }

    private void BroadcastGameState(Game game)
    {
        var stateDto = new GameStateDto
        {
            GameId = game.Id,
            Ball = new BallDto { X = game.Ball.X, Y = game.Ball.Y },
            Paddle1 = new PaddleDto { Y = game.Paddle1.Y },
            Paddle2 = new PaddleDto { Y = game.Paddle2.Y },
            Player1 = game.Player1 != null ? new PlayerDto { Name = game.Player1.Name, Score = game.Player1.Score } : null,
            Player2 = game.Player2 != null ? new PlayerDto { Name = game.Player2.Name, Score = game.Player2.Score } : null,
            State = game.State.ToString()
        };

        if (game.Player1 != null)
            _hubContext.Clients.Client(game.Player1.ConnectionId).SendAsync("GameStateUpdated", stateDto);
        if (game.Player2 != null)
            _hubContext.Clients.Client(game.Player2.ConnectionId).SendAsync("GameStateUpdated", stateDto);
    }
}
