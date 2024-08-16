using Dapper;
using Microsoft.Data.SqlClient;
using Prometheus;
using System.Data;
using VPS.Helpers;

namespace VPS.Infrastructure.Repository
{
    public class VpsDataContext : IVpsDataContext
    {
        private readonly string _dbConnectionString;
        private readonly MetricsHelper _metricsHelper;


        public VpsDataContext(string connectionString, MetricsHelper metricsHelper)
        {
            _dbConnectionString = connectionString;
            _metricsHelper = metricsHelper;
        }

        public async Task ExecuteAsync(string storedProcedure, DynamicParameters? parameters = null)
        {
            using (_metricsHelper.databaseResponse.NewTimer())
            {

                using var db = new SqlConnection(_dbConnectionString);
                if (parameters != null)
                {
                    await db.ExecuteAsync(storedProcedure, param: parameters, commandType: CommandType.StoredProcedure);
                }
                else
                {
                    await db.ExecuteAsync(storedProcedure, commandType: CommandType.StoredProcedure);
                }
            }
        }

        public async Task<T> InLineQueryAsync<T>(string query, DynamicParameters? parameters = null)
        {
            using (_metricsHelper.databaseResponse.NewTimer())
            {
                using var db = new SqlConnection(_dbConnectionString);
                if (parameters != null)
                {
                    return await db.QueryFirstAsync<T>(query, param: parameters, commandType: CommandType.Text);
                }
                else
                {
                    return await db.QueryFirstAsync<T>(query, commandType: CommandType.Text);
                }
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string storedProcedure, DynamicParameters? parameters = null) where T : class
        {
            using (_metricsHelper.databaseResponse.NewTimer())
            {
                using var db = new SqlConnection(_dbConnectionString);
                if (parameters != null)
                {
                    return await db.QueryAsync<T>(storedProcedure, param: parameters, commandType: CommandType.StoredProcedure);
                }
                else
                {
                    return await db.QueryAsync<T>(storedProcedure, commandType: CommandType.StoredProcedure);
                }
            }
        }
    }
}
