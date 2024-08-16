namespace VPS.Domain.Models.Configurations
{
    public class BatchServiceRedisSettings
    {
        public string BluVoucherInProgressRedisServer { get; set; } = string.Empty;
        public string HTUInProgressRedisServer { get; set; } = string.Empty;
        public string EasyLoadInProgressRedisServer { get; set; }   = string.Empty;                     
        public string FlashInProgressRedisServer { get; set; } = string.Empty;  
        public string OTTInProgressRedisServer { get; set; } = string.Empty;
        public string RAVoucherInProgressRedisServer { get; set; } = string.Empty;

        public string BluVoucherConnectionClientName { get; set; } = string.Empty;
        public string HTUConnectionClientName { get; set; } = string.Empty;
        public string EasyLoadConnectionClientName { get; set; } = string.Empty;
        public string FlashConnectionClientName { get; set; } = string.Empty;
        public string OTTConnectionClientName { get; set; } = string.Empty;
        public string RAVoucherConnectionClientName { get; set; } = string.Empty;

        public int BluVoucherRedisDb { get; set; }
        public int HTURedisDb { get; set; }
        public int EasyLoadRedisDb { get; set; }
        public int FlashRedisDb { get; set; }
        public int OTTRedisDb { get; set; }
        public int RAVoucherRedisDb { get; set; }

        public bool UseSSL { get; set; }
        public bool EnableInProgressCheck { get; set; }

    }
}
