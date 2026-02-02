using NppCore.Constants;
using NppCore.Models;
using NppCore.Services.Persistence.Cassandra;

namespace NppCore.Services.Features.Leaderboard;

public class LeaderboardService : ILeaderboardService
{
    private readonly ICassandraService _cassandra;

    public LeaderboardService(ICassandraService cassandra)
    {
        _cassandra = cassandra;
    }

    // =====================================================
    // GLOBAL LEADERBOARD
    // =====================================================

    public async Task<GlobalLeaderboardResponse> GetGlobalLeaderboardAsync(string periodType, string periodId, int limit = 10)
    {
        var cql = @"
            SELECT period_type, period_id, rank_score, player_id, username 
            FROM global_leaderboard 
            WHERE period_type = ? AND period_id = ? 
            LIMIT ?";

        var entries = await _cassandra.QueryAsync<GlobalLeaderboardEntry>(cql, periodType, periodId, limit);
        
        var entriesList = entries.ToList();
        var dtos = entriesList.Select((entry, index) => new LeaderboardEntryDto(
            Rank: index + 1,
            PlayerId: entry.PlayerId,
            Username: entry.Username,
            Score: entry.RankScore
        )).ToList();

        return new GlobalLeaderboardResponse(periodType, periodId, dtos);
    }

    public async Task AddOrUpdateGlobalLeaderboardAsync(string periodType, string periodId, Guid playerId, string username, int rankScore)
    {
        var cql = @"
            INSERT INTO global_leaderboard (period_type, period_id, rank_score, player_id, username) 
            VALUES (?, ?, ?, ?, ?)";

        await _cassandra.ExecuteAsync(cql, periodType, periodId, rankScore, playerId, username);
    }

    // =====================================================
    // WINS LEADERBOARD
    // =====================================================

    public async Task<WinsLeaderboardResponse> GetWinsLeaderboardAsync(string category = "most_wins", int limit = 10)
    {
        var cql = @"
            SELECT category, games_won, player_id, username 
            FROM leaderboard_by_wins 
            WHERE category = ? 
            LIMIT ?";

        var entries = await _cassandra.QueryAsync<WinsLeaderboardEntry>(cql, category, limit);
        
        var entriesList = entries.ToList();
        var dtos = entriesList.Select((entry, index) => new LeaderboardEntryDto(
            Rank: index + 1,
            PlayerId: entry.PlayerId,
            Username: entry.Username,
            Score: entry.GamesWon
        )).ToList();

        return new WinsLeaderboardResponse(category, dtos);
    }

    public async Task AddOrUpdateWinsLeaderboardAsync(string category, Guid playerId, string username, int gamesWon)
    {
        var cql = @"
            INSERT INTO leaderboard_by_wins (category, games_won, player_id, username) 
            VALUES (?, ?, ?, ?)";

        await _cassandra.ExecuteAsync(cql, category, gamesWon, playerId, username);
    }

    // =====================================================
    // PLAYER STREAK
    // =====================================================

    public async Task<PlayerStreakDto?> GetPlayerStreakAsync(Guid playerId)
    {
        var cql = @"
            SELECT player_id, current_streak, longest_streak, last_result 
            FROM player_current_streak 
            WHERE player_id = ?";

        var streak = await _cassandra.QueryFirstOrDefaultAsync<PlayerStreak>(cql, playerId);

        if (streak == null)
            return null;

        return new PlayerStreakDto(
            streak.PlayerId,
            streak.CurrentStreak,
            streak.LongestStreak,
            streak.LastResult
        );
    }

    public async Task UpdatePlayerStreakAsync(Guid playerId, string username, bool won)
    {
        // First try to read current streak
        var currentStreak = await _cassandra.QueryFirstOrDefaultAsync<PlayerStreak>(
            "SELECT player_id, current_streak, longest_streak, last_result FROM player_current_streak WHERE player_id = ?",
            playerId
        );

        int newCurrentStreak;
        int newLongestStreak;
        string newLastResult = won ? GameConstants.ResultWin : GameConstants.ResultLoss;

        if (currentStreak == null)
        {
            // First match for this player
            newCurrentStreak = won ? 1 : 0;
            newLongestStreak = won ? 1 : 0;
        }
        else
        {
            if (won)
            {
                // Win - streak continues
                newCurrentStreak = currentStreak.CurrentStreak + 1;
                newLongestStreak = Math.Max(newCurrentStreak, currentStreak.LongestStreak);
            }
            else
            {
                // Loss - streak resets
                newCurrentStreak = 0;
                newLongestStreak = currentStreak.LongestStreak;
            }
        }

        // INSERT will overwrite existing row since player_id is PRIMARY KEY
        var cql = @"
            INSERT INTO player_current_streak (player_id, current_streak, longest_streak, last_result) 
            VALUES (?, ?, ?, ?)";

        await _cassandra.ExecuteAsync(cql, playerId, newCurrentStreak, newLongestStreak, newLastResult);

        // If longest_streak was updated, update streak leaderboard as well
        if (currentStreak == null || newLongestStreak > currentStreak.LongestStreak)
        {
            await AddOrUpdateStreakLeaderboardAsync(GameConstants.LeaderboardCategoryGlobalAllTime, playerId, username, newLongestStreak);
        }
    }

    // =====================================================
    // LONGEST STREAK LEADERBOARD
    // =====================================================

    public async Task<StreakLeaderboardResponse> GetStreakLeaderboardAsync(string category = "global_all_time", int limit = 10)
    {
        var cql = @"
            SELECT category, longest_streak, player_id, username 
            FROM leaderboard_by_longest_streak 
            WHERE category = ? 
            LIMIT ?";

        var entries = await _cassandra.QueryAsync<StreakLeaderboardEntry>(cql, category, limit);
        
        var entriesList = entries.ToList();
        var dtos = entriesList.Select((entry, index) => new LeaderboardEntryDto(
            Rank: index + 1,
            PlayerId: entry.PlayerId,
            Username: entry.Username,
            Score: entry.LongestStreak
        )).ToList();

        return new StreakLeaderboardResponse(category, dtos);
    }

    public async Task AddOrUpdateStreakLeaderboardAsync(string category, Guid playerId, string username, int longestStreak)
    {
        var cql = @"
            INSERT INTO leaderboard_by_longest_streak (category, longest_streak, player_id, username) 
            VALUES (?, ?, ?, ?)";

        await _cassandra.ExecuteAsync(cql, category, longestStreak, playerId, username);
    }
}
