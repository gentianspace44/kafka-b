using Prometheus;
using StackExchange.Redis;
using System.Reflection;
using VPS.Domain.Models.Enums;
using VPS.Helpers;
using VPS.Helpers.Logging;

namespace VPS.Infrastructure.Repository.Redis
{
    public class BatchServiceRedisRepository : IBatchServiceRedisRepository
    {
        private readonly ILoggerAdapter<BatchServiceRedisRepository> log;
        private readonly MetricsHelper _metricsHelper;

        public BatchServiceRedisRepository(ILoggerAdapter<BatchServiceRedisRepository> log, MetricsHelper metricsHelper)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this._metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
        }

        public async Task<RedisValue> Delete(string key, VoucherType voucherType)
        {
            try
            {
                using (_metricsHelper.redisResponse.NewTimer())
                {
                    var redis = BatchServiceRedisStore.RedisCache(voucherType);
                    return await redis.KeyDeleteAsync(StringHelper.ComposeRedisKey(key, RedisStoreType.InProgressStore));
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, null, "Failed to delete key from Redis. Key {key}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    StringHelper.ComposeRedisKey(key, RedisStoreType.InProgressStore));
                return RedisValue.Null;
            }
        }

    }
}
