using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Reflection;
using VPS.Domain.Models.Configurations;
using VPS.Helpers;
using VPS.Helpers.Logging;

namespace VPS.Services.Kafka
{
    public class VpsKafkaConsumer : BackgroundService
    {
        private readonly ConsumerConfig _config;
        private readonly KafkaQueueConfiguration _queueConfiguration;
        private readonly ILoggerAdapter<VpsKafkaConsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly MetricsHelper _metricsHelper;

        public VpsKafkaConsumer(IOptions<KafkaQueueConfiguration> options, ILoggerAdapter<VpsKafkaConsumer> logger, IServiceScopeFactory scopeFactory, MetricsHelper metricsHelper)
        {
            this._queueConfiguration = options.Value ?? throw new ArgumentNullException(nameof(options));
            this._config = new ConsumerConfig
            {
                BootstrapServers = _queueConfiguration.Broker,
                EnableAutoCommit = false,
                GroupId = _queueConfiguration.Group,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            this._metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        private async Task StartConsumerLoop(CancellationToken cancellationToken)
        {
            using var consumer = new ConsumerBuilder<Ignore, string>(_config)
                .SetLogHandler((_, message) =>
                    _logger.LogInformation("Consumer", "VPS Kafka Consumer issue: '{message}' '{_config}'",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    JsonConvert.SerializeObject(message), JsonConvert.SerializeObject(_config)))
                .SetStatisticsHandler((_, json) =>
                    _logger.LogInformation("Consumer", "VPS Kafka Statistics '{json}'",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    json))
                .SetErrorHandler((_, err) =>
                    _logger.LogError("Consumer", "VPS Kafka Consumer Error: '{err}' '{_config}'",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                   JsonConvert.SerializeObject(err), JsonConvert.SerializeObject(_config)))
                .Build();

            consumer.Subscribe(_queueConfiguration?.MessageTopic);

            try
            {
                using var scope = _scopeFactory.CreateScope();

                var kafkaConsumerResultProcessor = scope.ServiceProvider.GetRequiredService<IVpsKafkaConsumer>();

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = consumer.Consume(cancellationToken);

                        if (result != null)
                        {
                            bool processed = await kafkaConsumerResultProcessor.ConsumeAndProcessTransaction(result.Message.Value);
                            _logger.LogInformation("Consumer", "Consumed message '{Message}' from topic '{Topic}'",
                                MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                                result.Message.Value, result.TopicPartition.Topic);

                            consumer.Commit();

                            if (processed)
                            {
                                _logger.LogInformation("Consumer", "Message processed '{Message}' from topic '{Topic}'",
                                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                                    result.Message.Value, result.TopicPartition.Topic);
                                _metricsHelper.IncVouchersRedeemConsumedOnKafka(_logger);
                            }
                            else
                            {
                                _logger.LogInformation("Consumer", "Consumed message '{Message}' Failed to process' from topic '{Topic}'",
                                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                                    result.Message.Value, result.TopicPartition.Topic);
                                continue;
                            }
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Consumer", "Provider consumer error: {Message}",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            ex.ToString());
                        if (ex.Error.IsFatal)
                        {
                            _logger.LogError(ex, "Consumer", "Fatal Error occurred in Provider consumer stopping the consumer: {Message}",
                                MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                                ex.Message);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Consumer", "Provider consumer error: {Message}",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            ex.Message);
                    }
                }

                consumer.Close();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consumer", "Provider consumer error on startup: {Message}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    ex.Message);
            }
        }
    }
}
