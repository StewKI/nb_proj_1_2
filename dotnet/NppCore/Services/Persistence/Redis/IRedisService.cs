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

    // Hash operations
    Task HashSetAsync(string key, string field, string value);
    Task HashSetAsync(string key, Dictionary<string, string> fields);
    Task<string?> HashGetAsync(string key, string field);
    Task<Dictionary<string, string>> HashGetAllAsync(string key);
    Task<bool> HashDeleteAsync(string key, string field);

    // Set operations
    Task<bool> SetAddAsync(string key, string value);
    Task<bool> SetRemoveAsync(string key, string value);
    Task<List<string>> SetMembersAsync(string key);

    // Key operations
    Task<bool> KeyDeleteAsync(string key);
    Task<List<string>> KeysAsync(string pattern);

    // String operations (simple key-value)
    Task StringSetAsync(string key, string value, TimeSpan? expiry = null);
    Task<string?> StringGetAsync(string key);
}
