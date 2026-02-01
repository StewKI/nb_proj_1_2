using System.Collections.Concurrent;
using System.Reflection;
using Cassandra;
using Cassandra.Mapping;
using Microsoft.Extensions.Options;
using NppCore.Configuration;
using NppCore.Constants;

namespace NppCore.Services.Persistence.Cassandra;

public class CassandraService : ICassandraService, IDisposable
{
    private readonly Cluster _cluster;
    private readonly ConcurrentDictionary<string, PreparedStatement> _preparedStatements = new();

    public ISession Session { get; }

    public CassandraService(IOptions<CassandraConfiguration> options)
    {
        var config = options.Value;

        var builder = Cluster.Builder()
            .AddContactPoints(config.ContactPoints)
            .WithPort(config.Port);

        if (!string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
        {
            builder.WithCredentials(config.Username, config.Password);
        }

        _cluster = builder.Build();
        Session = _cluster.Connect(config.Keyspace);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string cql, params object[] parameters) where T : new()
    {
        var prepared = await GetOrCreatePreparedStatementAsync(cql);
        var bound = prepared.Bind(parameters);
        var resultSet = await Session.ExecuteAsync(bound);

        return MapResults<T>(resultSet);
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string cql, params object[] parameters) where T : class, new()
    {
        var results = await QueryAsync<T>(cql, parameters);
        return results.FirstOrDefault();
    }

    public async Task ExecuteAsync(string cql, params object[] parameters)
    {
        var prepared = await GetOrCreatePreparedStatementAsync(cql);
        var bound = prepared.Bind(parameters);
        await Session.ExecuteAsync(bound);
    }

    public async Task<T?> GetByIdAsync<T>(string table, params object[] primaryKeyValues) where T : class, new()
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        if (properties.Length == 0)
            return null;

        var columns = string.Join(", ", properties.Select(p => ToSnakeCase(p.Name)));
        var whereClauses = properties
            .Take(primaryKeyValues.Length)
            .Select(p => $"{ToSnakeCase(p.Name)} = ?");

        var cql = $"SELECT {columns} FROM {table} WHERE {string.Join(" AND ", whereClauses)}";
        return await QueryFirstOrDefaultAsync<T>(cql, primaryKeyValues);
    }

    public async Task InsertAsync<T>(string table, T entity) where T : class
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var columns = string.Join(", ", properties.Select(p => ToSnakeCase(p.Name)));
        var placeholders = string.Join(", ", properties.Select(_ => "?"));
        var values = properties.Select(p => p.GetValue(entity)).ToArray();

        var cql = $"INSERT INTO {table} ({columns}) VALUES ({placeholders})";
        await ExecuteAsync(cql, values!);
    }

    public async Task DeleteAsync(string table, params object[] primaryKeyValues)
    {
        var placeholders = string.Join(" AND ", Enumerable.Range(0, primaryKeyValues.Length).Select(i => $"id{i} = ?"));
        var cql = $"DELETE FROM {table} WHERE {placeholders}";
        await ExecuteAsync(cql, primaryKeyValues);
    }

    private async Task<PreparedStatement> GetOrCreatePreparedStatementAsync(string cql)
    {
        if (_preparedStatements.TryGetValue(cql, out var existing))
            return existing;

        var prepared = await Session.PrepareAsync(cql);

        // Use GetOrAdd to handle race conditions - if another thread added it, use theirs
        // Note: Removed flawed eviction logic. The cache will grow but prepared statements
        // are lightweight. If this becomes an issue, implement proper LRU cache.
        return _preparedStatements.GetOrAdd(cql, prepared);
    }

    private static IEnumerable<T> MapResults<T>(RowSet resultSet) where T : new()
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => ToSnakeCase(p.Name), p => p, StringComparer.OrdinalIgnoreCase);

        foreach (var row in resultSet)
        {
            var instance = new T();
            foreach (var column in resultSet.Columns)
            {
                if (properties.TryGetValue(column.Name, out var prop))
                {
                    var value = row.GetValue(prop.PropertyType, column.Name);
                    prop.SetValue(instance, value);
                }
            }
            yield return instance;
        }
    }

    private static string ToSnakeCase(string name)
    {
        return string.Concat(name.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? "_" + char.ToLowerInvariant(c) : char.ToLowerInvariant(c).ToString()));
    }

    public void Dispose()
    {
        Session.Dispose();
        _cluster.Dispose();
    }
}
