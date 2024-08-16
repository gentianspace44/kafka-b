using Microsoft.Extensions.Diagnostics.HealthChecks;
using VPS.Infrastructure.Repository.Common;

namespace VPS.Services.Common.HealthCheck
{
    public sealed class SqlHealthCheck : IHealthCheck
    {
        private readonly IDBHealthCheckRepository _dbHealthCheckRepository;

        public SqlHealthCheck(IDBHealthCheckRepository dbHealthCheckRepository)
        {
            this._dbHealthCheckRepository = dbHealthCheckRepository ?? throw new ArgumentNullException(nameof(dbHealthCheckRepository));
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var dbStatus = _dbHealthCheckRepository.CheckDBStatusAsync().Result;


            if (!dbStatus.Item1)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(dbStatus.Item2));
            }

            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
