using Dapper;

namespace VPS.Infrastructure.Repository
{
    public interface IVpsDataContext
    {
        Task<IEnumerable<T>> QueryAsync<T>(string storedProcedure, DynamicParameters? parameters = null) where T : class;
        Task ExecuteAsync(string storedProcedure, DynamicParameters? parameters = null);

        Task<T> InLineQueryAsync<T>(string query, DynamicParameters? parameters = null);

    }
}
