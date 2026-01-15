namespace NppCore.Models;

public enum GameState
{
    WaitingForPlayer,
    Playing,
    Finished
}

public class Player
{
    public required string ConnectionId { get; set; }
    public Guid PlayerId { get; set; } // Potrebno za ažuriranje statistike
    public required string Name { get; set; }
    public int Score { get; set; }
}

public class Ball
{
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
}

public class Paddle
{
    public double Y { get; set; }
}

public class Game
{
    public required string Id { get; set; }
    public Player? Player1 { get; set; }
    public Player? Player2 { get; set; }
    public Ball Ball { get; set; } = new();
    public Paddle Paddle1 { get; set; } = new();
    public Paddle Paddle2 { get; set; } = new();
    public GameState State { get; set; } = GameState.WaitingForPlayer;

    public const int CanvasWidth = 800;
    public const int CanvasHeight = 600;
    public const int PaddleHeight = 100;
    public const int PaddleWidth = 10;
    public const int PaddleOffset = 20;
    public const int BallSize = 10;
    public const int WinScore = 5;
}

public class GameStateDto
{
    public required string GameId { get; set; }
    public required BallDto Ball { get; set; }
    public required PaddleDto Paddle1 { get; set; }
    public required PaddleDto Paddle2 { get; set; }
    public required PlayerDto? Player1 { get; set; }
    public required PlayerDto? Player2 { get; set; }
    public required string State { get; set; }
}

public class BallDto
{
    public double X { get; set; }
    public double Y { get; set; }
}

public class PaddleDto
{
    public double Y { get; set; }
}

public class PlayerDto
{
    public required string Name { get; set; }
    public int Score { get; set; }
}

public class LobbyGameDto
{
    public required string GameId { get; set; }
    public required string HostName { get; set; }
}
