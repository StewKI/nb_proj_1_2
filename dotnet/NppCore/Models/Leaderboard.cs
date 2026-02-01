namespace NppCore.Models;

// =====================================================
// ENTITY MODELS (mapiranje na Cassandra tabele)
// =====================================================

/// <summary>
/// Entity za global_leaderboard tabelu
/// </summary>
public class GlobalLeaderboardEntry
{
    public string PeriodType { get; set; } = string.Empty; // 'MONTHLY', 'YEARLY', 'ALL_TIME'
    public string PeriodId { get; set; } = string.Empty; // npr. '2024-01'
    public int RankScore { get; set; }
    public Guid PlayerId { get; set; }
    public string Username { get; set; } = string.Empty;
}

/// <summary>
/// Entity za leaderboard_by_wins tabelu
/// </summary>
public class WinsLeaderboardEntry
{
    public string Category { get; set; } = string.Empty; // npr. 'most_wins'
    public int GamesWon { get; set; }
    public Guid PlayerId { get; set; }
    public string Username { get; set; } = string.Empty;
}

/// <summary>
/// Entity za player_current_streak tabelu
/// </summary>
public class PlayerStreak
{
    public Guid PlayerId { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public string LastResult { get; set; } = string.Empty; // 'WIN' ili 'LOSS'
}

/// <summary>
/// Entity za leaderboard_by_longest_streak tabelu
/// </summary>
public class StreakLeaderboardEntry
{
    public string Category { get; set; } = string.Empty; // npr. 'global_all_time'
    public int LongestStreak { get; set; }
    public Guid PlayerId { get; set; }
    public string Username { get; set; } = string.Empty;
}

// =====================================================
// DTOs (Data Transfer Objects za API)
// =====================================================

/// <summary>
/// DTO za prikaz leaderboard entry-ja (generički)
/// </summary>
public record LeaderboardEntryDto(
    int Rank,
    Guid PlayerId,
    string Username,
    int Score
);

/// <summary>
/// DTO za prikaz streak informacija
/// </summary>
public record PlayerStreakDto(
    Guid PlayerId,
    int CurrentStreak,
    int LongestStreak,
    string LastResult
);

/// <summary>
/// Response za global leaderboard
/// </summary>
public record GlobalLeaderboardResponse(
    string PeriodType,
    string PeriodId,
    List<LeaderboardEntryDto> Entries
);

/// <summary>
/// Response za wins leaderboard
/// </summary>
public record WinsLeaderboardResponse(
    string Category,
    List<LeaderboardEntryDto> Entries
);

/// <summary>
/// Response za streak leaderboard
/// </summary>
public record StreakLeaderboardResponse(
    string Category,
    List<LeaderboardEntryDto> Entries
);
