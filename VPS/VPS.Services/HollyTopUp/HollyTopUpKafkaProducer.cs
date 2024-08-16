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
using VPS.Domain.Models.HollyTopUp.Requests;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Infrastructure.Repository.Redis;
using VPS.Services.Kafka;

namespace VPS.Services.HollyTopUp
{
    public class HollyTopUpKafkaProducer : IHollyTopUpKafkaProducer
    {

        private readonly IVpsKafkaProducer _kafkaProducer;
        private readonly KafkaQueueConfiguration _queueConfiguration;
        private readonly ILoggerAdapter<HollyTopUpKafkaProducer> _log;
        private readonly IVoucherBatchProcessingRepository _voucherBatchProcessingRepository;
        private readonly IHttpClientCommunication _httpClientCommunication;
        private readonly VpsControlCenterEndpoints _vpsControlCenterEndpoints;
        private readonly VoucherRedeemClientNotifications _voucherRedeemClientNotifications;
        private readonly IRedisRepository _redisRepository;
        private readonly RedisSettings _redisSettings;

        public HollyTopUpKafkaProducer(IVpsKafkaProducer kafkaProducer,
            IOptions<KafkaQueueConfiguration> queueConfiguration,
            ILoggerAdapter<HollyTopUpKafkaProducer> log,
            IVoucherBatchProcessingRepository voucherBatchProcessingRepository,
            IHttpClientCommunication httpClientCommunication,
            IRedisRepository redisRepository,
            IOptions<VpsControlCenterEndpoints> vpsControlCenterEndpoints,
            IOptions<VoucherRedeemClientNotifications> voucherRedeemClientNotifications,
            IOptions<RedisSettings> redisSettings
           )
        {
            this._kafkaProducer = kafkaProducer ?? throw new ArgumentNullException(nameof(kafkaProducer));
            this._queueConfiguration = queueConfiguration.Value ?? throw new ArgumentNullException(nameof(queueConfiguration));
            this._log = log ?? throw new ArgumentNullException(nameof(log));
            this._voucherBatchProcessingRepository = voucherBatchProcessingRepository ?? throw new ArgumentNullException(nameof(voucherBatchProcessingRepository));
            this._httpClientCommunication = httpClientCommunication ?? throw new ArgumentNullException(nameof(httpClientCommunication));
            this._vpsControlCenterEndpoints = vpsControlCenterEndpoints.Value;
            this._voucherRedeemClientNotifications = voucherRedeemClientNotifications.Value;
            this._redisRepository = redisRepository ?? throw new ArgumentNullException(nameof(redisRepository));
            this._redisSettings = redisSettings.Value;
        }

        public async Task Produce(HollyTopUpRedeemRequest hollyTopUpRedeemRequest, VoucherType voucherType, RedeemOutcome outcome)
        {
            //Produce in Kafka and return custom message
            var quePayload = new KafkaQueuePayload<HollyTopUpRedeemRequest>
            {
                VoucherRedeemRequest = hollyTopUpRedeemRequest,
                VoucherType = voucherType,
                RedeemOutcome = outcome
            };

            string kafkaMessage = JsonConvert.SerializeObject(quePayload);

            try
            {
                //Produce event using PollyRetry
                int noOfAttempts = 0;
                
                AsyncRetryPolicy<DeliveryResult<Null, string>?> _retryPolicy = Policy
                    .HandleResult<DeliveryResult<Null, string>?>(response => response == null || response.Status != PersistenceStatus.Persisted)
                    .WaitAndRetryAsync(
                        _queueConfiguration.ProducerMaximumRetryCount,
                        retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(10, noOfAttempts)),
                        onRetry: (response, timeElapsed) =>
                        {
                            noOfAttempts++;
                            _log.LogInformation(hollyTopUpRedeemRequest.VoucherNumber, "Producing message on Kafka: {message}. Retry Attempt: {noOfAttempts}",
                                MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                                 kafkaMessage, noOfAttempts );
                        }
                    );

                var kafkaResponse = await _retryPolicy.ExecuteAsync(() => _kafkaProducer.Produce(_queueConfiguration.MessageTopic?? "", kafkaMessage));

                _log.LogInformation(hollyTopUpRedeemRequest.VoucherNumber, "Produce on Kafka response: {response}.",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                      JsonConvert.SerializeObject(kafkaResponse));

                if (kafkaResponse== null || kafkaResponse.Status != PersistenceStatus.Persisted)
                {
                    _log.LogInformation(hollyTopUpRedeemRequest.VoucherNumber, "Failed to Persist message on Kafka: {message}.",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                                 kafkaMessage);
                    //Log to sql table for batch processing, should be flagged that its from Producer
                    _log.LogInformation(hollyTopUpRedeemRequest.VoucherNumber, "Inserting voucher {voucherNumber} to VoucherBatchProcessing",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty, hollyTopUpRedeemRequest.VoucherNumber);

                    await InsertToBatchTable(hollyTopUpRedeemRequest, quePayload, kafkaMessage);

                    //Send instant response pending when pending service process request
                    var messageContent = JsonConvert.SerializeObject(new SignalRNotifyModel
                    {
                        ClientId = hollyTopUpRedeemRequest.ClientId.ToString(),
                        Message = _voucherRedeemClientNotifications.VoucherRedeemInProgressMessage,
                        Balance = 0
                    });

                    //Send success result with signalR here
                    await _httpClientCommunication.SendRequestAsync(_vpsControlCenterEndpoints.NotifyClientEndpoint, Domain.Models.Enums.HttpMethod.POST, messageContent, "application/json");

                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, hollyTopUpRedeemRequest.VoucherNumber, "Failed to produce message on Kafka with error message: {message}.",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    ex.Message);

                //Log to sql table for batch processing, should be flagged that its from Producer
                _log.LogInformation(hollyTopUpRedeemRequest.VoucherNumber, "Inserting voucher {voucherNumber} to VoucherBatchProcessing",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                   hollyTopUpRedeemRequest.VoucherNumber);
                await InsertToBatchTable(hollyTopUpRedeemRequest, quePayload, kafkaMessage);

                //Send instant response pending when pending service process request
                var messageContent = JsonConvert.SerializeObject(new SignalRNotifyModel
                {
                    ClientId = hollyTopUpRedeemRequest.ClientId.ToString(),
                    Message = _voucherRedeemClientNotifications.VoucherRedeemInProgressMessage,
                    Balance = 0
                });

                //Send success result with signalR here
                await _httpClientCommunication.SendRequestAsync(_vpsControlCenterEndpoints.NotifyClientEndpoint, Domain.Models.Enums.HttpMethod.POST, messageContent, "application/json");

            }
        }

        private async Task InsertToBatchTable(HollyTopUpRedeemRequest hollyTopUpRedeemRequest, KafkaQueuePayload<HollyTopUpRedeemRequest> quePayload, string kafkaMessage)
        {
            string messageContent;
            try
            {
                var voucherPrefix = EnumHelper.GetEnumDescription(quePayload.VoucherType);
                if(Guid.TryParse(quePayload.VoucherRedeemRequest?.VoucherReference, out Guid uniqueRef))
                {
                    if (quePayload.RedeemOutcome == null)
                    {
                        _log.LogError(quePayload.VoucherRedeemRequest?.VoucherNumber, "Empty quePayload RedeemOutcome: {VoucherReference}.",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            quePayload.VoucherRedeemRequest?.VoucherReference);
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
                    _log.LogError(quePayload.VoucherRedeemRequest?.VoucherNumber, "Failed to Parse VoucherReference on Producer: {VoucherReference}.",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                        quePayload.VoucherRedeemRequest?.VoucherReference);
                    throw new FormatException("Invalid VoucherReference");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, hollyTopUpRedeemRequest.VoucherNumber, MethodBase.GetCurrentMethod()?.Name ?? string.Empty, "Failed to insert record message on VoucherBatchProcessing with error message: {message}.",  ex.Message );
                _log.LogCritical(ex, hollyTopUpRedeemRequest.VoucherNumber, MethodBase.GetCurrentMethod()?.Name ?? string.Empty, "Failed to insert record message on VoucherBatchProcessing with error message: {message} and payload: {payload}.",  ex.Message, kafkaMessage);

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
}
