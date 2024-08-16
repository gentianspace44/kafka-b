using StackExchange.Redis;

namespace VPS.ControlCenter.Logic.RedisServices
{
    public interface IRedisRepository
    {
        Task<RedisValue> Get(string key);
        RedisValue Add(string key, dynamic entity);
        Task<RedisValue> Remove(string key, dynamic entity);
        Task<RedisValue> Delete(string key);
    }


}
