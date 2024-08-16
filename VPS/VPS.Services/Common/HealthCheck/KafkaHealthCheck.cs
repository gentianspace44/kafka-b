using Confluent.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using VPS.Domain.Models.Configurations;

namespace VPS.Services.Common.HealthCheck
{
    public sealed class KafkaHealthCheck : IHealthCheck
    {

        private readonly KafkaQueueConfiguration _kafkaQueueConfiguration;

        public KafkaHealthCheck(IOptions<KafkaQueueConfiguration> options)
        {
            _kafkaQueueConfiguration = options.Value;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _kafkaQueueConfiguration.Broker
            };

            try
            {
                using var producer = new ProducerBuilder<Null, string>(producerConfig).Build();
                return Task.FromResult(HealthCheckResult.Healthy());
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Degraded(ex.Message));
            }

        }
    }
}
