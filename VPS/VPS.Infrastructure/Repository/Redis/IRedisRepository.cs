using StackExchange.Redis;
using VPS.Domain.Models.Enums;

namespace VPS.Infrastructure.Repository.Redis
{
    public interface IRedisRepository
    {
        Task<RedisValue> Get(string key, RedisStoreType storeType);
        Task<RedisValue> Add(string key, dynamic entity, RedisStoreType storeType);
        Task<RedisValue> AddWithTTL(string key, dynamic entity, RedisStoreType storeType, int timeToLiveInSec);
        Task<RedisValue> Remove(string key, dynamic entity, RedisStoreType storeType);
        Task<RedisValue> Delete(string key, RedisStoreType storeType);
        Task<Tuple<bool,string>> CheckRedisHealth(RedisStoreType storeType);
    

    }
}
