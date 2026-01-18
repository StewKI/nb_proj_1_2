namespace NppCore.Constants;

public static class GameConstants
{
    // Game Physics
    public const int CanvasWidth = 800;
    public const int CanvasHeight = 600;
    public const int PaddleWidth = 10;
    public const int PaddleHeight = 100;
    public const int BallSize = 10;
    public const int InitialBallSpeed = 5;
    public const int MaxBallSpeed = 15;
    public const double BallSpeedMultiplier = 1.05; // Povećanje brzine nakon svake kolizije sa palicom
    public const int MaxBallVerticalSpeed = 8; // Max vertikalna brzina lopte
    public const int BallSpeedIncrement = 1;
    public const int PaddleSpeed = 5;
    
    // Game Rules
    public const int WinningScore = 5;
    public const int PointsPerScore = 10; // Poeni koje igrač dobija za svaki osvojeni poen
    
    // Timing (milliseconds)
    public const int GameTickInterval = 16; // ~60 FPS
    public const int CountdownSeconds = 3;
    
    // Leaderboards
    public const string LeaderboardCategoryMostWins = "most_wins";
    public const string LeaderboardCategoryGlobalAllTime = "global_all_time";
    public const string LeaderboardPeriodAllTime = "all";
    public const string LeaderboardPeriodTypeMonthly = "MONTHLY";
    public const string LeaderboardPeriodTypeYearly = "YEARLY";
    public const string LeaderboardPeriodTypeAllTime = "ALL_TIME";
    
    // Player Stats
    public const int MaxUsernameLength = 50;
    public const int MinUsernameLength = 3;
    public const int MinPasswordLength = 6;
    public const int MaxPasswordLength = 100;
    
    // Cache/Performance
    public const int MaxCachedPreparedStatements = 100;
    
    // Admin Limits
    public const int MaxPointsPerUpdate = 10000;
    public const int MaxWinsLossesPerUpdate = 100;
}