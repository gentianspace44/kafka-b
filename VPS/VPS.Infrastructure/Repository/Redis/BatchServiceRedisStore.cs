using StackExchange.Redis;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;

namespace VPS.Infrastructure.Repository.Redis
{
    public class BatchServiceRedisStore
    {
        static BatchServiceRedisStore()
        {

        }

        private static readonly Lazy<ConnectionMultiplexer> LazyInProgressBluVoucherConnection = new(() => ConnectionMultiplexer.Connect(CreateConfiguration("BluVoucher")));
        private static readonly Lazy<ConnectionMultiplexer> LazyInProgressHTUConnection = new(() => ConnectionMultiplexer.Connect(CreateConfiguration("HTU")));
        private static readonly Lazy<ConnectionMultiplexer> LazyInProgressEasyLoadConnection = new(() => ConnectionMultiplexer.Connect(CreateConfiguration("EasyLoad")));
        private static readonly Lazy<ConnectionMultiplexer> LazyInProgressFlashConnection = new(() => ConnectionMultiplexer.Connect(CreateConfiguration("Flash")));
        private static readonly Lazy<ConnectionMultiplexer> LazyInProgressOTTConnection = new(() => ConnectionMultiplexer.Connect(CreateConfiguration("OTT")));
        private static readonly Lazy<ConnectionMultiplexer> LazyInProgressRAVoucherConnection = new(() => ConnectionMultiplexer.Connect(CreateConfiguration("RAVoucher")));

        private static ConfigurationOptions CreateConfiguration(string serviceKey)
        {
            return new ConfigurationOptions
            {
                EndPoints = { ConfigurationHelper.StaticConfig.GetSection($"BatchServiceRedisSettings:{serviceKey}InProgressRedisServer").Value! },
                DefaultDatabase = Convert.ToInt32(ConfigurationHelper.StaticConfig.GetSection($"BatchServiceRedisSettings:{serviceKey}RedisDb").Value),
                AbortOnConnectFail = false,
                Ssl = Convert.ToBoolean(ConfigurationHelper.StaticConfig.GetSection("BatchServiceRedisSettings:UseSSL").Value),
                Proxy = Proxy.None,
                ClientName = ConfigurationHelper.StaticConfig.GetSection($"BatchServiceRedisSettings:{serviceKey}ConnectionClientName").Value
            };
        }

        public static ConnectionMultiplexer BluVoucherConnection => LazyInProgressBluVoucherConnection.Value;
        public static ConnectionMultiplexer HtuConnection => LazyInProgressHTUConnection.Value;
        public static ConnectionMultiplexer EasyLoadConnection => LazyInProgressEasyLoadConnection.Value;
        public static ConnectionMultiplexer FlashConnection => LazyInProgressFlashConnection.Value;
        public static ConnectionMultiplexer OttConnection => LazyInProgressOTTConnection.Value;
        public static ConnectionMultiplexer RaConnection => LazyInProgressRAVoucherConnection.Value;


        public static IDatabase RedisCache(VoucherType voucherType)
        {
            return voucherType switch
            {
                VoucherType.BluVoucher => BluVoucherConnection.GetDatabase(),
                VoucherType.HollyTopUp => HtuConnection.GetDatabase(),
                VoucherType.EasyLoad => EasyLoadConnection.GetDatabase(),
                VoucherType.Flash => FlashConnection.GetDatabase(),
                VoucherType.OTT => OttConnection.GetDatabase(),
                VoucherType.RACellular => RaConnection.GetDatabase(),
                _ => throw new ArgumentOutOfRangeException(nameof(voucherType), voucherType, null)
            };
        }

    }
}
