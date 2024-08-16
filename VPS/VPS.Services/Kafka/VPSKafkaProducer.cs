using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Prometheus;
using System.Reflection;
using VPS.Domain.Models.Configurations;
using VPS.Helpers;
using VPS.Helpers.Logging;

namespace VPS.Services.Kafka
{
    public class VpsKafkaProducer : IVpsKafkaProducer
    {
        private readonly ProducerConfig _config;
        private readonly ILoggerAdapter<VpsKafkaProducer> _logger;
        private readonly MetricsHelper _metricsHelper;
        public VpsKafkaProducer(IOptions<KafkaQueueConfiguration> queueConfiguration, ILoggerAdapter<VpsKafkaProducer> logger, MetricsHelper metricsHelper)
        {
            if (queueConfiguration.Value == null) throw new System.ArgumentNullException(nameof(queueConfiguration));
            var _queueConfiguration = queueConfiguration.Value;
            this._logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            _config = new ProducerConfig
            {
                BootstrapServers = _queueConfiguration.Broker,
                EnableDeliveryReports = true,
                Acks = Acks.Leader,
                MessageTimeoutMs = _queueConfiguration.SessionTimeoutMs
            };
            _metricsHelper = metricsHelper;
        }

        public async Task<DeliveryResult<Null, string>?> Produce(string topic, string message)
        {
            try
            {
                using var producer = new ProducerBuilder<Null, string>(_config)
         .SetLogHandler((_, message) => _logger.LogError("Producer", "VPS Kafka Producer issue: '{Message}' '{Config}' ",
                MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                message, _config))
         .Build();

                DeliveryResult<Null, string> result;
                using (_metricsHelper.kafkaProducerResponse.NewTimer())
                {
                    result = await producer.ProduceAsync(topic, new Message<Null, string> { Value = message });

                    if (result.Status == PersistenceStatus.Persisted)
                    {
                        _metricsHelper.IncVouchersRedeemProducedOnKafka(_logger);
                    }
                }

                _logger.LogInformation("Producer", "Produced message: '{message}' to topic: '{topic}' returned result: {result}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    message, result.TopicPartition.Topic, JsonConvert.SerializeObject(result));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null, "Failed to Produce on Kafka", MethodBase.GetCurrentMethod()?.Name ?? string.Empty);
                return null;
            }
        }
    }
}
