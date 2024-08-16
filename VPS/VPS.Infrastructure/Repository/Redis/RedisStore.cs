using StackExchange.Redis;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;

namespace VPS.Infrastructure.Repository.Redis
{
    public class RedisStore
    {
        static RedisStore()
        {
                
        }

        private static readonly Lazy<ConnectionMultiplexer> LazyConcurrencyConnection =
            new(() => ConnectionMultiplexer.Connect(CreateConfiguration("ConcurrencyRedisServer", "ConcurrencyRedisDb", "ConcurrencyConnectionClientName")));

        private static readonly Lazy<ConnectionMultiplexer> LazyDelayConnection =
            new(() => ConnectionMultiplexer.Connect(CreateConfiguration("DelayRedisServer", "DelayRedisDb", "DelayConnectionClientName")));

        private static readonly Lazy<ConnectionMultiplexer> LazyInProgressConnection =
            new(() => ConnectionMultiplexer.Connect(CreateConfiguration("InProgressRedisServer", "InProgressRedisDb", "InProgressConnectionClientName")));

        private static ConfigurationOptions CreateConfiguration(string serverKey, string dbKey, string clientNameKey)
        {
            return new ConfigurationOptions
            {
                EndPoints = { ConfigurationHelper.StaticConfig.GetSection($"RedisSettings:{serverKey}").Value! },
                DefaultDatabase = Convert.ToInt32(ConfigurationHelper.StaticConfig.GetSection($"RedisSettings:{dbKey}").Value),
                AbortOnConnectFail = false,
                Ssl = Convert.ToBoolean(ConfigurationHelper.StaticConfig.GetSection("RedisSettings:UseSSL").Value),
                Proxy = Proxy.None,
                ClientName = ConfigurationHelper.StaticConfig.GetSection($"RedisSettings:{clientNameKey}").Value
            };
        }

        public static ConnectionMultiplexer DelayConnection => LazyDelayConnection.Value;
        public static ConnectionMultiplexer ConcurrentConnection => LazyConcurrencyConnection.Value;
        public static ConnectionMultiplexer InProgressConnection => LazyInProgressConnection.Value;

        public static IDatabase RedisCache(RedisStoreType storeType)
        {
            switch (storeType)
            {
                case RedisStoreType.DelayStore:
                    return DelayConnection.GetDatabase();
                case RedisStoreType.ConcurencyStore:
                case RedisStoreType.TokenRefreshStore:
                case RedisStoreType.IdempotencyStore:
                    return ConcurrentConnection.GetDatabase();
                case RedisStoreType.InProgressStore:
                    return InProgressConnection.GetDatabase();
                default:
                    throw new ArgumentOutOfRangeException(nameof(storeType), storeType, "Invalid Redis store type");
            }
        }
    }
}
