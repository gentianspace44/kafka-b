using Microsoft.Extensions.Diagnostics.HealthChecks;
using VPS.Domain.Models.Enums;

namespace VPS.Services.Common.HealthCheck
{
    public sealed class RedisHealthCheck: IHealthCheck
    {

        private readonly IRedisService _redisService;

        public RedisHealthCheck(IRedisService redisService)
        {
            this._redisService = redisService ?? throw new ArgumentNullException(nameof(redisService));
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {

            var redisConcurrentStatus = _redisService.CheckRedisHealth(RedisStoreType.ConcurencyStore).Result;
            var redisDelayStatus = _redisService.CheckRedisHealth(RedisStoreType.DelayStore).Result;


            if (!redisConcurrentStatus.Item1)
            {
                return Task.FromResult(HealthCheckResult.Degraded(redisConcurrentStatus.Item2));
            }


            if (!redisDelayStatus.Item1)
            {
                return Task.FromResult(HealthCheckResult.Degraded(redisDelayStatus.Item2));
            }

            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
