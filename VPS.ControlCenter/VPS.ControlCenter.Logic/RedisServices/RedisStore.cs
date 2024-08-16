using StackExchange.Redis;
using VPS.ControlCenter.Logic.Helpers;

namespace VPS.ControlCenter.Logic.RedisServices
{
    public class RedisStore
    {
        //https://taswar.zeytinsoft.com/redis-for-net-developer-connecting-with-c/
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection;

        static RedisStore()
        {

            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = { ConfigurationHelper.StaticConfig.GetSection("RedisSettings:RedisServer").Value },
                DefaultDatabase = Convert.ToInt32(ConfigurationHelper.StaticConfig.GetSection("RedisSettings:RedisDb").Value),
                AbortOnConnectFail = false,
            };

            LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configurationOptions));
        }

        public static ConnectionMultiplexer Connection => LazyConnection.Value;

        public static IDatabase RedisCache => Connection.GetDatabase();
    }
}
