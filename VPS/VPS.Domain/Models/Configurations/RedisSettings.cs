namespace VPS.Domain.Models.Configurations
{
    public class RedisSettings
    {
        public string ConcurrencyRedisServer { get; set; } = string.Empty;
        public string DelayRedisServer { get; set; } = string.Empty;
        public string InProgressRedisServer { get; set; } = string.Empty;

        public int ConcurrencyRedisDb { get; set; }
        public int DelayRedisDb { get; set; }
        public int InProgressRedisDb { get; set; }

        public bool EnableConcurrencyCheck { get; set;  }        
        public bool EnableDelayCheck { get; set; }
        public bool EnableInProgressCheck { get; set; }

        public int CachePolicyTimeToLiveInSeconds { get; set; }
        public int InProgressPolicyTimeToLiveInHours { get; set; }
        public int ConcurrencyDelayInSeconds { get; set; }

        public bool UseSSL { get; set; }

        public int MaxConcurrencyPolyRetry { get; set; }
        public int MaxDelayPolyRetry { get; set; }
        public int MaxInProgressPolyRetry { get; set; }

        public string ConcurrencyConnectionClientName { get; set; } = string.Empty;
        public string DelayConnectionClientName { get; set; } = string.Empty;
        public string InProgressConnectionClientName { get; set; } = string.Empty;
    }
}
