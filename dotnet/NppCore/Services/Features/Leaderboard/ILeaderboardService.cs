using NppCore.Models;

namespace NppCore.Services.Features.Leaderboard;

public interface ILeaderboardService
{
    // =====================================================
    // GLOBAL LEADERBOARD
    // =====================================================
    
    /// <summary>
    /// Vraća global leaderboard za određeni period
    /// </summary>
    /// <param name="periodType">Tip perioda: MONTHLY, YEARLY, ALL_TIME</param>
    /// <param name="periodId">ID perioda: npr. '2024-01', '2024', 'all'</param>
    /// <param name="limit">Broj rezultata (default 10)</param>
    Task<GlobalLeaderboardResponse> GetGlobalLeaderboardAsync(string periodType, string periodId, int limit = 10);

    /// <summary>
    /// Dodaje ili ažurira entry u global leaderboard
    /// </summary>
    Task AddOrUpdateGlobalLeaderboardAsync(string periodType, string periodId, Guid playerId, string username, int rankScore);

    // =====================================================
    // WINS LEADERBOARD
    // =====================================================
    
    /// <summary>
    /// Vraća leaderboard po broju pobeda
    /// </summary>
    /// <param name="category">Kategorija (default 'most_wins')</param>
    /// <param name="limit">Broj rezultata (default 10)</param>
    Task<WinsLeaderboardResponse> GetWinsLeaderboardAsync(string category = "most_wins", int limit = 10);

    /// <summary>
    /// Dodaje ili ažurira entry u wins leaderboard
    /// </summary>
    Task AddOrUpdateWinsLeaderboardAsync(string category, Guid playerId, string username, int gamesWon);

    // =====================================================
    // PLAYER STREAK
    // =====================================================
    
    /// <summary>
    /// Vraća streak informacije za igrača
    /// </summary>
    Task<PlayerStreakDto?> GetPlayerStreakAsync(Guid playerId);

    /// <summary>
    /// Ažurira streak informacije nakon meča
    /// </summary>
    /// <param name="playerId">ID igrača</param>
    /// <param name="username">Username igrača</param>
    /// <param name="won">Da li je igrač pobedio</param>
    Task UpdatePlayerStreakAsync(Guid playerId, string username, bool won);

    // =====================================================
    // LONGEST STREAK LEADERBOARD
    // =====================================================
    
    /// <summary>
    /// Vraća leaderboard po najdužem streaku
    /// </summary>
    /// <param name="category">Kategorija (default 'global_all_time')</param>
    /// <param name="limit">Broj rezultata (default 10)</param>
    Task<StreakLeaderboardResponse> GetStreakLeaderboardAsync(string category = "global_all_time", int limit = 10);

    /// <summary>
    /// Dodaje ili ažurira entry u streak leaderboard
    /// </summary>
    Task AddOrUpdateStreakLeaderboardAsync(string category, Guid playerId, string username, int longestStreak);
}
