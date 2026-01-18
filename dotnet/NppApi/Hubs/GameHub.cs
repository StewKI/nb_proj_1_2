using Microsoft.AspNetCore.SignalR;
using NppApi.Services;
using NppCore.Models;

namespace NppApi.Hubs;

public class GameHub : Hub
{
    private readonly GameManager _gameManager;

    public GameHub(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public async Task CreateGame(string playerName)
    {
        var (game, token) = await _gameManager.CreateGameAsync(Context.ConnectionId, playerName);
        await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);

        // Send reconnect token to the player
        if (!string.IsNullOrEmpty(token))
        {
            await Clients.Caller.SendAsync("ReconnectToken", new ReconnectTokenDto
            {
                Token = token,
                GameId = game.Id,
                PlayerNumber = 1
            });
        }

        await Clients.All.SendAsync("LobbyUpdated", _gameManager.GetOpenGames());
    }

    public async Task JoinGame(string gameId, string playerName)
    {
        var (game, token) = await _gameManager.JoinGameAsync(gameId, Context.ConnectionId, playerName);
        if (game == null)
        {
            await Clients.Caller.SendAsync("JoinFailed", "Game not found or already started");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);

        // Send reconnect token to the joining player
        if (!string.IsNullOrEmpty(token))
        {
            await Clients.Caller.SendAsync("ReconnectToken", new ReconnectTokenDto
            {
                Token = token,
                GameId = game.Id,
                PlayerNumber = 2
            });
        }

        await Clients.All.SendAsync("LobbyUpdated", _gameManager.GetOpenGames());

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

        await Clients.Group(gameId).SendAsync("GameStarted", stateDto);
    }

    public async Task CheckPendingGame(string token)
    {
        var session = await _gameManager.CheckPendingGameAsync(token);
        if (session != null)
        {
            await Clients.Caller.SendAsync("PendingGameFound", new
            {
                gameId = session.GameId,
                playerNumber = session.PlayerNumber,
                playerName = session.PlayerName
            });
        }
        else
        {
            await Clients.Caller.SendAsync("NoPendingGame");
        }
    }

    public async Task Reconnect(string token)
    {
        var result = await _gameManager.ReconnectAsync(token, Context.ConnectionId);

        if (result.Success && result.GameId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, result.GameId);

            var game = _gameManager.GetGame(result.GameId);
            if (game != null)
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

                await Clients.Caller.SendAsync("Reconnected", new
                {
                    success = true,
                    gameId = result.GameId,
                    playerNumber = result.PlayerNumber,
                    playerName = result.PlayerName,
                    gameState = stateDto
                });

                // If game resumed (both players connected), notify both
                if (game.State == GameState.Playing && _gameManager.BothPlayersConnected(result.GameId))
                {
                    await Clients.Group(result.GameId).SendAsync("GameResumed", stateDto);
                }
            }
        }
        else
        {
            await Clients.Caller.SendAsync("ReconnectFailed", result.ErrorMessage ?? "Reconnection failed");
        }
    }

    public async Task GetLobby()
    {
        await Clients.Caller.SendAsync("LobbyUpdated", _gameManager.GetOpenGames());
    }

    public Task MovePaddle(double y)
    {
        _gameManager.UpdatePaddle(Context.ConnectionId, y);
        return Task.CompletedTask;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _gameManager.HandlePlayerDisconnectAsync(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
