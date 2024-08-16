using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System.Reflection;
using VPS.API.Common;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.RACellularVoucher.Requests;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Infrastructure.Repository.Redis;
using VPS.Services.Kafka;

namespace VPS.Services.RACellularVoucher;

public class RaCellularVoucherKafkaProducer : IRaCellularVoucherKafkaProducer
{
    private readonly IVpsKafkaProducer _kafkaProducer;
    private readonly KafkaQueueConfiguration _queueConfiguration;
    private readonly ILoggerAdapter<RaCellularVoucherKafkaProducer> _log;
    private readonly IVoucherBatchProcessingRepository _voucherBatchProcessingRepository;
    private readonly IHttpClientCommunication _httpClientCommunication;
    private readonly VpsControlCenterEndpoints _vpsControlCenterEndpoints;
    private readonly VoucherRedeemClientNotifications _voucherRedeemClientNotifications;
    private readonly IRedisRepository _redisRepository;
    private readonly RedisSettings _redisSettings;

    public RaCellularVoucherKafkaProducer(
        IVpsKafkaProducer kafkaProducer,
        IOptions<KafkaQueueConfiguration> queueConfiguration,
        ILoggerAdapter<RaCellularVoucherKafkaProducer> log,
        IVoucherBatchProcessingRepository voucherBatchProcessingRepository,
        IHttpClientCommunication httpClientCommunication,
        IRedisRepository redisRepository,
        IOptions<VpsControlCenterEndpoints> vpsControlCenterEndpoints,
        IOptions<VoucherRedeemClientNotifications> voucherRedeemClientNotifications,
        IOptions<RedisSettings> redisSettings)
    {
        this._log = log ?? throw new ArgumentNullException(nameof(log));
        this._kafkaProducer = kafkaProducer ?? throw new ArgumentNullException(nameof(kafkaProducer));
        this._queueConfiguration = queueConfiguration?.Value ?? throw new ArgumentNullException(nameof(queueConfiguration));
        this._voucherBatchProcessingRepository = voucherBatchProcessingRepository ?? throw new ArgumentNullException(nameof(voucherBatchProcessingRepository));
        this._httpClientCommunication = httpClientCommunication ?? throw new ArgumentNullException(nameof(httpClientCommunication));
        this._vpsControlCenterEndpoints = vpsControlCenterEndpoints?.Value ?? throw new ArgumentNullException(nameof(vpsControlCenterEndpoints));
        this._voucherRedeemClientNotifications = voucherRedeemClientNotifications?.Value ?? throw new ArgumentNullException(nameof(voucherRedeemClientNotifications));
        this._redisRepository = redisRepository ?? throw new ArgumentNullException(nameof(redisRepository));
        this._redisSettings = redisSettings?.Value ?? throw new ArgumentNullException(nameof(redisSettings));
    }

    public async Task ProduceAsync(RaCellularVoucherRedeemRequest raCellularVoucherRedeemRequest, VoucherType voucherType, RedeemOutcome outcome)
    {
        var quePayload = new KafkaQueuePayload<RaCellularVoucherRedeemRequest>
        {
            VoucherRedeemRequest = raCellularVoucherRedeemRequest,
            VoucherType = voucherType,
            RedeemOutcome = outcome
        };

        var kafkaMessage = JsonConvert.SerializeObject(quePayload);

        try
        {

            int noOfAttempts = 0;

            AsyncRetryPolicy<DeliveryResult<Null, string>?> _retryPolicy = Policy
                .HandleResult<DeliveryResult<Null, string>?>(response => response == null || response.Status != PersistenceStatus.Persisted)
                .WaitAndRetryAsync(
                    _queueConfiguration.ProducerMaximumRetryCount,
                    retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(10, noOfAttempts)),
                    onRetry: (response, timeElapsed) =>
                    {
                        noOfAttempts++;
                        _log.LogInformation(raCellularVoucherRedeemRequest.VoucherNumber, "Producing message on Kafka: {message}. Retry Attempt: {noOfAttempts}",
                           MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            kafkaMessage,
                            noOfAttempts
                            );
                    }
                );

            var kafkaResponse = await _retryPolicy.ExecuteAsync(() => _kafkaProducer.Produce(_queueConfiguration.MessageTopic!, kafkaMessage));

            _log.LogInformation(raCellularVoucherRedeemRequest.VoucherNumber, "Produce on Kafka response: {response}.",
                MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                JsonConvert.SerializeObject(kafkaResponse));

            if (kafkaResponse == null || kafkaResponse.Status != PersistenceStatus.Persisted)
            {
                _log.LogInformation(raCellularVoucherRedeemRequest.VoucherNumber, "Failed to Persist message on Kafka: {message}.",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,

                            kafkaMessage
                            );

                //Log to sql table for batch processing, should be flagged that its from Producer
                _log.LogInformation(raCellularVoucherRedeemRequest.VoucherNumber, "Inserting voucher {voucherNumber} to VoucherBatchProcessing",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty, raCellularVoucherRedeemRequest.VoucherNumber);

                await InsertToBatchTable(raCellularVoucherRedeemRequest, quePayload, kafkaMessage);

                //Send instant response pending when pending service process request
                var messageContent = JsonConvert.SerializeObject(new SignalRNotifyModel
                {
                    ClientId = raCellularVoucherRedeemRequest.ClientId.ToString(),
                    Message = _voucherRedeemClientNotifications.VoucherRedeemInProgressMessage,
                    Balance = 0
                });

                //Send success result with signalR here
                await _httpClientCommunication.SendRequestAsync(_vpsControlCenterEndpoints.NotifyClientEndpoint, Domain.Models.Enums.HttpMethod.POST, messageContent, "application/json");

            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, raCellularVoucherRedeemRequest.VoucherNumber, "Failed to produce message on Kafka with error message: {message}.", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, ex.Message);

            //Log to sql table for batch processing, should be flagged that its from Producer
            _log.LogInformation(raCellularVoucherRedeemRequest.VoucherNumber, "Inserting voucher {voucherNumber} to VoucherBatchProcessing", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, raCellularVoucherRedeemRequest.VoucherNumber);
            await InsertToBatchTable(raCellularVoucherRedeemRequest, quePayload, kafkaMessage);

            //Send instant response pending when pending service process request
            var messageContent = JsonConvert.SerializeObject(new SignalRNotifyModel
            {
                ClientId = raCellularVoucherRedeemRequest.ClientId.ToString(),
                Message = _voucherRedeemClientNotifications.VoucherRedeemInProgressMessage,
                Balance = 0
            });

            //Send success result with signalR here
            await _httpClientCommunication.SendRequestAsync(_vpsControlCenterEndpoints.NotifyClientEndpoint, Domain.Models.Enums.HttpMethod.POST, messageContent, "application/json");

        }
    }

    private async Task InsertToBatchTable(RaCellularVoucherRedeemRequest raCellularVoucherRedeemRequest, KafkaQueuePayload<RaCellularVoucherRedeemRequest> quePayload, string kafkaMessage)
    {
        string messageContent;
        try
        {
            var voucherPrefix = EnumHelper.GetEnumDescription(quePayload.VoucherType);
            if (Guid.TryParse(quePayload.VoucherRedeemRequest?.VoucherReference, out Guid uniqueRef))
            {
                if (quePayload.RedeemOutcome == null)
                {
                    _log.LogError(quePayload.VoucherRedeemRequest?.VoucherNumber, "Empty quePayload RedeemOutcome: {VoucherReference}.", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, quePayload.VoucherRedeemRequest?.VoucherReference ?? "");
                    throw new FormatException("Empty RedeemOutcome");
                }

                var voucherBatchRecord = new PendingBatchVoucher
                {
                    VoucherType = quePayload.VoucherType,
                    VoucherPin = quePayload.VoucherRedeemRequest.VoucherNumber,
                    UniqueReference = uniqueRef,
                    ClientId = quePayload.VoucherRedeemRequest.ClientId,
                    DevicePlatform = quePayload.VoucherRedeemRequest.DevicePlatform,
                    VoucherID = quePayload.RedeemOutcome.VoucherID,
                    VoucherAmount = quePayload.RedeemOutcome.VoucherAmount,
                    VoucherPrefix = voucherPrefix,
                    Source = BatchProcessingSource.Producer,
                    Message = kafkaMessage
                };

                await _voucherBatchProcessingRepository.InsertPendingBatchVoucher(voucherBatchRecord);

                //Add to Redis in-progress message response
                if (_redisSettings.EnableInProgressCheck)
                {
                    int timeToLive = (int)(DateTime.Now.AddHours(_redisSettings.InProgressPolicyTimeToLiveInHours) - DateTime.Now).TotalSeconds;
                    await _redisRepository.AddWithTTL(quePayload.VoucherRedeemRequest.VoucherNumber, kafkaMessage, RedisStoreType.InProgressStore, timeToLive);
                }
            }
            else
            {
                _log.LogError(quePayload.VoucherRedeemRequest?.VoucherNumber, "Failed to Parse VoucherReference on Producer: {VoucherReference}.", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, quePayload.VoucherRedeemRequest?.VoucherReference ?? "");
                throw new FormatException("Invalid VoucherReference");
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, raCellularVoucherRedeemRequest.VoucherNumber, "Failed to insert record message on VoucherBatchProcessing with error message: {message}.", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, ex.Message);
            _log.LogCritical(ex, raCellularVoucherRedeemRequest.VoucherNumber, "Failed to insert record message on VoucherBatchProcessing with error message: {message} and payload: {payload}.", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, ex.Message, kafkaMessage);

            //Here message failed to be persisted and failed to be inserted for BatchProcessing we return fail result to user and ask to retry again.
            messageContent = JsonConvert.SerializeObject(new SignalRNotifyModel
            {
                ClientId = quePayload.VoucherRedeemRequest?.ClientId.ToString(),
                Message = _voucherRedeemClientNotifications.VoucherRedeemCriticalFailOnProducer,
                Balance = 0
            });

            //Send result with signalR here
            await _httpClientCommunication.SendRequestAsync(_vpsControlCenterEndpoints.NotifyClientEndpoint, Domain.Models.Enums.HttpMethod.POST, messageContent, "application/json");

        }
    }
}
