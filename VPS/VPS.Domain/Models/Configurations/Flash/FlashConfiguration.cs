namespace VPS.Domain.Models.Configurations.Flash
{
    public  class FlashConfiguration
    {
        public string FlashEndpoint { get; set; } = string.Empty;
        public string FlashConsumerKey { get; set; } = string.Empty;
        public string FlashConsumerSecret { get; set; } = string.Empty;
        public string FlashAccountNumber { get; set; } = string.Empty;
        public int FlashAPITimeoutSeconds { get; set; }
        public string MaxPolyRetry { get; set; } = string.Empty;
        public string FlashAccessTokenCacheKey { get; set; } = string.Empty;
        public int AccessTokenCacheLifespanSeconds { get; set; }
        public int IdempotencyLifespanSeconds { get; set; }
    }
}
