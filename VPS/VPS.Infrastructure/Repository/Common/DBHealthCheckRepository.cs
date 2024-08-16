using Microsoft.Extensions.Options;
using VPS.Domain.Models.Configurations;
using VPS.Helpers;
using VPS.Helpers.Logging;

namespace VPS.Infrastructure.Repository.Common
{
    public class DBHealthCheckRepository : IDBHealthCheckRepository
    {

        private readonly DbConnectionStrings _dbConnectionStrings;
        private readonly ILoggerAdapter<DBHealthCheckRepository> _log;
        private readonly MetricsHelper _metricsHelper;


        public DBHealthCheckRepository(IOptions<DbConnectionStrings> dbConnectionStrings, ILoggerAdapter<DBHealthCheckRepository> log, MetricsHelper metricsHelper)
        {
            this._log = log ?? throw new ArgumentNullException(nameof(log));
            this._dbConnectionStrings = dbConnectionStrings.Value;
            this._metricsHelper = metricsHelper;
        }

        public async Task<Tuple<bool,string>> CheckDBStatusAsync()
        {
            try
            {
                var db = new VpsDataContext(_dbConnectionStrings.HollyTopUpConnection, _metricsHelper);

                var result = await db.InLineQueryAsync<int>("SELECT 1");

                if (result == 1)
                {
                    return Tuple.Create(true, "Healthy");
                }

                return Tuple.Create(false, "Select unsuccessful");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, null, "Health Check to DB returns exception");

                return Tuple.Create(false, ex.Message);
            }
        }
    }
}
