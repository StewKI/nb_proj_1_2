using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using NppApi.Hubs;
using NppCore.Constants;
using NppCore.Models;
using NppCore.Services.Features.Player;

namespace NppApi.Services;

public class GameManager : IHostedService, IDisposable
{
    private readonly ConcurrentDictionary<string, Game> _games = new();
    private readonly ConcurrentDictionary<string, string> _playerGameMap = new();
    private readonly IHubContext<GameHub> _hubContext;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private Timer? _gameLoopTimer;
    private readonly Random _random = new();

    public GameManager(IHubContext<GameHub> hubContext, IServiceScopeFactory serviceScopeFactory)
    {
        _hubContext = hubContext;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _gameLoopTimer = new Timer(GameLoop, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000.0 / 60));
        return Task.CompletedTask;
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

    public Game CreateGame(string connectionId, Guid playerId, string playerName)
    {
        var gameId = Guid.NewGuid().ToString()[..8];
        var game = new Game
        {
            Id = gameId,
            Player1 = new Player { ConnectionId = connectionId, PlayerId = playerId, Name = playerName },
            State = GameState.WaitingForPlayer
        };

        InitializeBall(game);
        InitializePaddles(game);

        _games[gameId] = game;
        _playerGameMap[connectionId] = gameId;

        return game;
    }

    public Game? JoinGame(string gameId, string connectionId, Guid playerId, string playerName)
    {
        if (!_games.TryGetValue(gameId, out var game))
            return null;

        if (game.State != GameState.WaitingForPlayer)
            return null;

        game.Player2 = new Player { ConnectionId = connectionId, PlayerId = playerId, Name = playerName };
        game.State = GameState.Playing;
        _playerGameMap[connectionId] = gameId;

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
        var speed = (double)GameConstants.InitialBallSpeed;

        game.Ball.VelocityX = Math.Cos(angle) * speed * direction;
        game.Ball.VelocityY = Math.Sin(angle) * speed;
    }

    private void InitializePaddles(Game game)
    {
        game.Paddle1.Y = (Game.CanvasHeight - Game.PaddleHeight) / 2.0;
        game.Paddle2.Y = (Game.CanvasHeight - Game.PaddleHeight) / 2.0;
    }

    private async void GameLoop(object? state)
    {
        foreach (var game in _games.Values.Where(g => g.State == GameState.Playing))
        {
            await UpdateBallAsync(game);
            await BroadcastGameStateAsync(game);
        }
    }

    private async Task UpdateBallAsync(Game game)
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
            game.Ball.VelocityX = -game.Ball.VelocityX * GameConstants.BallSpeedMultiplier;
            var hitPos = (game.Ball.Y - game.Paddle1.Y) / Game.PaddleHeight;
            game.Ball.VelocityY = (hitPos - 0.5) * GameConstants.MaxBallVerticalSpeed;
            game.Ball.X = Game.PaddleOffset + Game.PaddleWidth;
        }

        // Paddle 2 collision (right)
        if (game.Ball.X >= Game.CanvasWidth - Game.PaddleOffset - Game.PaddleWidth - Game.BallSize &&
            game.Ball.Y + Game.BallSize >= game.Paddle2.Y &&
            game.Ball.Y <= game.Paddle2.Y + Game.PaddleHeight &&
            game.Ball.VelocityX > 0)
        {
            game.Ball.VelocityX = -game.Ball.VelocityX * GameConstants.BallSpeedMultiplier;
            var hitPos = (game.Ball.Y - game.Paddle2.Y) / Game.PaddleHeight;
            game.Ball.VelocityY = (hitPos - 0.5) * GameConstants.MaxBallVerticalSpeed;
            game.Ball.X = Game.CanvasWidth - Game.PaddleOffset - Game.PaddleWidth - Game.BallSize;
        }

        // Score
        if (game.Ball.X < 0)
        {
            game.Player2!.Score++;
            await CheckWinAsync(game);
            InitializeBall(game);
        }
        else if (game.Ball.X > Game.CanvasWidth)
        {
            game.Player1!.Score++;
            await CheckWinAsync(game);
            InitializeBall(game);
        }
    }

    private async Task CheckWinAsync(Game game)
    {
        if (game.Player1!.Score >= Game.WinScore || game.Player2!.Score >= Game.WinScore)
        {
            game.State = GameState.Finished;
            
            var winner = game.Player1.Score >= Game.WinScore ? game.Player1 : game.Player2!;
            var loser = winner == game.Player1 ? game.Player2! : game.Player1;

            // Ažuriraj statistiku nakon meča (PROPERLY async now)
            await UpdatePlayerStatsAsync(winner, loser);

            await _hubContext.Clients.Client(game.Player1.ConnectionId).SendAsync("GameEnded", winner.Name);
            await _hubContext.Clients.Client(game.Player2.ConnectionId).SendAsync("GameEnded", winner.Name);

            _playerGameMap.TryRemove(game.Player1.ConnectionId, out _);
            _playerGameMap.TryRemove(game.Player2.ConnectionId, out _);
            _games.TryRemove(game.Id, out _);
        }
    }

    private async Task UpdatePlayerStatsAsync(Player winner, Player loser)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var playerStatsService = scope.ServiceProvider.GetRequiredService<IPlayerStatsService>();

        try
        {
            await playerStatsService.UpdateStatsAfterMatchAsync(
                winner.PlayerId,
                winner.Name,
                winner.Score,
                loser.PlayerId,
                loser.Name,
                loser.Score
            );
        }
        catch (Exception ex)
        {
            // Log error ali ne blokiraj igru
            Console.WriteLine($"Error updating player stats: {ex.Message}");
        }
    }

    private async Task BroadcastGameStateAsync(Game game)
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
            await _hubContext.Clients.Client(game.Player1.ConnectionId).SendAsync("GameStateUpdated", stateDto);
        if (game.Player2 != null)
            await _hubContext.Clients.Client(game.Player2.ConnectionId).SendAsync("GameStateUpdated", stateDto);
    }
}
