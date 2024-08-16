using System.Diagnostics.CodeAnalysis;

namespace VPS.Domain.Models.Configurations
{
    [ExcludeFromCodeCoverage]
    public class KafkaQueueConfiguration
    {
        public string? MessageTopic { get; set; }
        public string? ProducerName { get; set; }
        public string? ConsumerName { get; set; }
        public string? Broker { get; set; }
        public string? Group { get; set; }
        public string? BufferSize { get; set; }
        public int ProducerMaximumRetryCount { get; set; }
        public int SessionTimeoutMs { get; set; }
        public int MaxPollIntervalMs { get; set; }     
        public int ConsumerMaximumRetryCount { get; set; }

    }
}
