using Cassandra;

namespace NppCore.Services.Persistence.Cassandra;

public interface ICassandraService
{
    ISession Session { get; }

    Task<IEnumerable<T>> QueryAsync<T>(string cql, params object[] parameters) where T : new();
    Task<T?> QueryFirstOrDefaultAsync<T>(string cql, params object[] parameters) where T : class, new();
    Task ExecuteAsync(string cql, params object[] parameters);

    Task<T?> GetByIdAsync<T>(string table, params object[] primaryKeyValues) where T : class, new();
    Task InsertAsync<T>(string table, T entity) where T : class;
    Task DeleteAsync(string table, params object[] primaryKeyValues);
}
