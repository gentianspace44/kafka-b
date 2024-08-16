using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using VPS.ControlCenter.Logic.Models;

namespace VPS.ControlCenter.Logic.RedisServices
{
    public class RedisRepository : IRedisRepository
    {
        private readonly RedisStore _redisService;
        private readonly RedisSettings _redisSettings;
        private readonly ILogger<RedisRepository> _log;

        public RedisRepository(IOptions<RedisSettings> redisSettings, RedisStore redisService, ILogger<RedisRepository> log)
        {
            this._redisSettings = redisSettings.Value;
            this._redisService = redisService;
            this._log = log;
        }

        public RedisValue Add(string key, dynamic entity)
        {
            try
            {
                var redis = RedisStore.RedisCache;
                var json = JsonConvert.SerializeObject(entity);

                return redis.StringSet(key, json);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to add key to Redis. Key {key}", args: new object[] { key });
                return RedisValue.Null;
            }
        }

        public async Task<RedisValue> Get(string key)
        {
            try
            {
                var redis = RedisStore.RedisCache;
                return await redis.StringGetAsync(key);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to get key from Redis. Key {key}", args: new object[] { key });
                return RedisValue.Null;
            }
        }

        public async Task<RedisValue> Remove(string key, dynamic entity)
        {
            try
            {
                var redis = RedisStore.RedisCache;
                var json = JsonConvert.SerializeObject(entity);
                return await redis.SetRemoveAsync(key, json);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to remove key to Redis. Key {key}", args: new object[] { key });
                return RedisValue.Null;
            }
        }

        public async Task<RedisValue> Delete(string key)
        {
            try
            {
                var redis = RedisStore.RedisCache;
                return await redis.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to delete key from Redis. Key {key}", args: new object[] { key });
                return RedisValue.Null;
            }
        }
    }
}
