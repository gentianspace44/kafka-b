using Microsoft.Extensions.Diagnostics.HealthChecks;
using VPS.API.Syx;

namespace VPS.Services.Common.HealthCheck
{
    public sealed class SyxHealthCheck : IHealthCheck
    {

        private readonly ISyxApiService _syxAPIService;

        public SyxHealthCheck(ISyxApiService syxService)
        {
            this._syxAPIService = syxService ?? throw new ArgumentNullException(nameof(syxService));

        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {

            bool syxHealthCheck =  _syxAPIService.HealthCheck().Result;           

            if (!syxHealthCheck)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("SyxAPI is Unavailable"));
            }

            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
