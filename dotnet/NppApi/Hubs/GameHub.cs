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
        var game = _gameManager.CreateGame(Context.ConnectionId, playerName);
        await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
        await Clients.All.SendAsync("LobbyUpdated", _gameManager.GetOpenGames());
    }

    public async Task JoinGame(string gameId, string playerName)
    {
        var game = _gameManager.JoinGame(gameId, Context.ConnectionId, playerName);
        if (game == null)
        {
            await Clients.Caller.SendAsync("JoinFailed", "Game not found or already started");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
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
        _gameManager.RemovePlayer(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
