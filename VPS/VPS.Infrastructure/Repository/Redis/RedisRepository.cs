using Newtonsoft.Json;
using Prometheus;
using StackExchange.Redis;
using System.Reflection;
using VPS.Domain.Models.Enums;
using VPS.Helpers;
using VPS.Helpers.Logging;

namespace VPS.Infrastructure.Repository.Redis
{
    public class RedisRepository : IRedisRepository
    {

        private readonly ILoggerAdapter<RedisRepository> log;
        private readonly MetricsHelper _metricsHelper;

        public RedisRepository(ILoggerAdapter<RedisRepository> log, MetricsHelper metricsHelper)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this._metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
        }

        public async Task<RedisValue> AddWithTTL(string key, dynamic entity, RedisStoreType storeType, int timeToLiveInSec)
        {
            try
            {
                using (_metricsHelper.redisResponse.NewTimer())
                {
                    var redis = RedisStore.RedisCache(storeType);
                    var json = JsonConvert.SerializeObject(entity);
                                        
                    return await redis.StringSetAsync(StringHelper.ComposeRedisKey(key,storeType), json, TimeSpan.FromSeconds(timeToLiveInSec));
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, null, "Failed to add key to Redis. Key {key}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    StringHelper.ComposeRedisKey(key, storeType));
                return RedisValue.Null;
            }
        }

        public async Task<RedisValue> Add(string key, dynamic entity, RedisStoreType storeType)
        {
            try
            {
                using (_metricsHelper.redisResponse.NewTimer())
                {
                    var redis = RedisStore.RedisCache(storeType);
                    var json = JsonConvert.SerializeObject(entity);

                    return await redis.StringSetAsync(StringHelper.ComposeRedisKey(key, storeType), json);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, null, "Failed to add key to Redis. Key {key}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    StringHelper.ComposeRedisKey(key, storeType));
                return RedisValue.Null;
            }
        }

        public async Task<RedisValue> Get(string key, RedisStoreType storeType)
        {
            try
            {
                using (_metricsHelper.redisResponse.NewTimer())
                {
                    var redis = RedisStore.RedisCache(storeType);
                    return await redis.StringGetAsync(StringHelper.ComposeRedisKey(key, storeType));
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, null, "Failed to get key from Redis. Key {key}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    StringHelper.ComposeRedisKey(key, storeType));
                return RedisValue.Null;
            }
        }

        public async Task<RedisValue> Remove(string key, dynamic entity, RedisStoreType storeType)
        {
            try
            {
                using (_metricsHelper.redisResponse.NewTimer())
                {
                    var redis = RedisStore.RedisCache(storeType);
                    var json = JsonConvert.SerializeObject(entity);
                    return await redis.SetRemoveAsync(StringHelper.ComposeRedisKey(key, storeType), json);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, null, "Failed to remove key to Redis. Key {key}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    StringHelper.ComposeRedisKey(key, storeType));
                return RedisValue.Null;
            }
        }

        public async Task<RedisValue> Delete(string key, RedisStoreType storeType)
        {
            try
            {
                using (_metricsHelper.redisResponse.NewTimer())
                {
                    var redis = RedisStore.RedisCache(storeType);
                    return await redis.KeyDeleteAsync(StringHelper.ComposeRedisKey(key, storeType));
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, null, "Failed to delete key from Redis. Key {key}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    StringHelper.ComposeRedisKey(key, storeType));
                return RedisValue.Null;
            }
        }

        public async Task<Tuple<bool,string>> CheckRedisHealth(RedisStoreType storeType)
        {
            try
            {
                using (_metricsHelper.redisResponse.NewTimer())
                {
                    var redis = RedisStore.RedisCache(storeType);
                    var result  = await redis.PingAsync();

                    if(result.Ticks > 0)
                    {
                       return Tuple.Create(true, "Healthy");
                    }

                     return Tuple.Create(false, "Pin Unsuccessful");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, null, "Failed to ping Redis DB", MethodBase.GetCurrentMethod()?.Name ?? string.Empty);
                return Tuple.Create(false, ex.Message);
            }
        }

      
    }
}
