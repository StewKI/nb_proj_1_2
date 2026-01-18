using System.Text.Json;
using Microsoft.Extensions.Options;
using NppCore.Configuration;
using StackExchange.Redis;

namespace NppCore.Services.Persistence.Redis;

public class RedisService : IRedisService, IDisposable
{
    private readonly ConnectionMultiplexer _connection;
    private readonly IDatabase _db;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisService(IOptions<RedisConfiguration> options)
    {
        var config = options.Value;
        _connection = ConnectionMultiplexer.Connect(config.ConnectionString);
        _db = _connection.GetDatabase();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<T>((string)value!, _jsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        if (expiry.HasValue)
            await _db.StringSetAsync(key, json, new Expiration(expiry.Value));
        else
            await _db.StringSetAsync(key, json);
    }

    public async Task<bool> DeleteAsync(string key)
    {
        return await _db.KeyDeleteAsync(key);
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        return await _db.KeyExistsAsync(key);
    }

    public async Task<double> LeaderboardAddOrUpdateAsync(string leaderboardKey, string member, double score)
    {
        await _db.SortedSetAddAsync(leaderboardKey, member, score);
        return score;
    }

    public async Task<List<(string Member, double Score)>> LeaderboardGetTopAsync(string leaderboardKey, int count = 100)
    {
        var entries = await _db.SortedSetRangeByRankWithScoresAsync(
            leaderboardKey,
            start: 0,
            stop: count - 1,
            order: Order.Descending
        );

        return entries
            .Select(e => (Member: e.Element.ToString(), Score: e.Score))
            .ToList();
    }

    public async Task<long?> LeaderboardGetRankAsync(string leaderboardKey, string member)
    {
        var rank = await _db.SortedSetRankAsync(leaderboardKey, member, Order.Descending);
        return rank;
    }

    public async Task<double?> LeaderboardGetScoreAsync(string leaderboardKey, string member)
    {
        return await _db.SortedSetScoreAsync(leaderboardKey, member);
    }

    public async Task<bool> LeaderboardRemoveAsync(string leaderboardKey, string member)
    {
        return await _db.SortedSetRemoveAsync(leaderboardKey, member);
    }

    // Hash operations
    public async Task HashSetAsync(string key, string field, string value)
    {
        await _db.HashSetAsync(key, field, value);
    }

    public async Task HashSetAsync(string key, Dictionary<string, string> fields)
    {
        var hashEntries = fields.Select(f => new HashEntry(f.Key, f.Value)).ToArray();
        await _db.HashSetAsync(key, hashEntries);
    }

    public async Task<string?> HashGetAsync(string key, string field)
    {
        var value = await _db.HashGetAsync(key, field);
        return value.IsNullOrEmpty ? null : value.ToString();
    }

    public async Task<Dictionary<string, string>> HashGetAllAsync(string key)
    {
        var entries = await _db.HashGetAllAsync(key);
        return entries.ToDictionary(
            e => e.Name.ToString(),
            e => e.Value.ToString()
        );
    }

    public async Task<bool> HashDeleteAsync(string key, string field)
    {
        return await _db.HashDeleteAsync(key, field);
    }

    // Set operations
    public async Task<bool> SetAddAsync(string key, string value)
    {
        return await _db.SetAddAsync(key, value);
    }

    public async Task<bool> SetRemoveAsync(string key, string value)
    {
        return await _db.SetRemoveAsync(key, value);
    }

    public async Task<List<string>> SetMembersAsync(string key)
    {
        var members = await _db.SetMembersAsync(key);
        return members.Select(m => m.ToString()).ToList();
    }

    // Key operations
    public async Task<bool> KeyDeleteAsync(string key)
    {
        return await _db.KeyDeleteAsync(key);
    }

    public async Task<List<string>> KeysAsync(string pattern)
    {
        var server = _connection.GetServer(_connection.GetEndPoints().First());
        var keys = server.Keys(pattern: pattern);
        return keys.Select(k => k.ToString()).ToList();
    }

    // String operations
    public async Task StringSetAsync(string key, string value, TimeSpan? expiry = null)
    {
        if (expiry.HasValue)
            await _db.StringSetAsync(key, value, expiry.Value);
        else
            await _db.StringSetAsync(key, value);
    }

    public async Task<string?> StringGetAsync(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.IsNullOrEmpty ? null : value.ToString();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
