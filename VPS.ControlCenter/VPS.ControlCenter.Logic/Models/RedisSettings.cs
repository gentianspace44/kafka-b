namespace VPS.ControlCenter.Logic.Models
{
    public class RedisSettings
    {
       public string RedisServer { get; set; }
        public int RedisDb { get; set; }
        public bool EnableRedis { get; set; }

        public int ConnectRetry { get; set; }

        public string ClientName { get; set; }
        public string ChannelPrefix { get; set; }
        public bool AbortOnConnectFail { get; set; }
    }
}
