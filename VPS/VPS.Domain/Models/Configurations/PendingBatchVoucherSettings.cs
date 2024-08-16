namespace VPS.Domain.Models.Configurations
{
    public class PendingBatchVoucherSettings
    {
        public int JobIntervalInSeconds { get; set; }
        public int BatchSize { get; set; }
        public int RetryLimit { get; set; }
    }
}
