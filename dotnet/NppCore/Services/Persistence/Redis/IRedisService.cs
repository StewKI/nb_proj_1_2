namespace NppCore.Services.Persistence.Redis;

public interface IRedisService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
    Task<bool> DeleteAsync(string key);
    Task<bool> KeyExistsAsync(string key);

    Task<double> LeaderboardAddOrUpdateAsync(string leaderboardKey, string member, double score);
    Task<List<(string Member, double Score)>> LeaderboardGetTopAsync(string leaderboardKey, int count = 100);
    Task<long?> LeaderboardGetRankAsync(string leaderboardKey, string member);
    Task<double?> LeaderboardGetScoreAsync(string leaderboardKey, string member);
    Task<bool> LeaderboardRemoveAsync(string leaderboardKey, string member);
}
