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
using VPS.Services.Common;
using VPS.Services.Kafka;

namespace VPS.Services.RACellularVoucher;

public class RaCellularVoucherKafkaConsumer : IVpsKafkaConsumer
{
    private readonly KafkaQueueConfiguration _queueConfiguration;
    private readonly IClientBalanceService _clientBalanceService;
    private readonly ILoggerAdapter<RaCellularVoucherKafkaConsumer> _logger;
    private readonly IVoucherBatchProcessingRepository _voucherBatchProcessingRepository;
    private readonly IHttpClientCommunication _httpClientCommunication;
    private readonly VpsControlCenterEndpoints _vpsControlCenterEndpoints;
    private readonly VoucherRedeemClientNotifications _voucherRedeemClientNotifications;
    private readonly DBSettings _dbSettings;
    private readonly IRedisRepository _redisRepository;
    private readonly RedisSettings _redisSettings;
    private readonly CountrySettings _countrySettings;
    private readonly MetricsHelper _metricsHelper;

    public RaCellularVoucherKafkaConsumer(
        IClientBalanceService clientBalanceService,
        IOptions<KafkaQueueConfiguration> queueConfiguration,
        ILoggerAdapter<RaCellularVoucherKafkaConsumer> logger,
        IVoucherBatchProcessingRepository voucherBatchProcessingRepository,
        IHttpClientCommunication httpClientCommunication,
        IRedisRepository redisRepository,
        IOptions<VpsControlCenterEndpoints> vpsControlCenterEndpoints,
        IOptions<VoucherRedeemClientNotifications> voucherRedeemClientNotifications,
        IOptions<DBSettings> dbSettings,
        IOptions<RedisSettings> redisSettings,
        IOptions<CountrySettings> countrySettings,
        MetricsHelper metricsHelper)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this._clientBalanceService = clientBalanceService ?? throw new ArgumentNullException(nameof(clientBalanceService));
        this._queueConfiguration = queueConfiguration?.Value ?? throw new ArgumentNullException(nameof(queueConfiguration));
        this._voucherBatchProcessingRepository = voucherBatchProcessingRepository ?? throw new ArgumentNullException(nameof(voucherBatchProcessingRepository));
        this._httpClientCommunication = httpClientCommunication ?? throw new ArgumentNullException(nameof(httpClientCommunication));
        this._vpsControlCenterEndpoints = vpsControlCenterEndpoints?.Value ?? throw new ArgumentNullException(nameof(vpsControlCenterEndpoints));
        this._voucherRedeemClientNotifications = voucherRedeemClientNotifications?.Value ?? throw new ArgumentNullException(nameof(voucherRedeemClientNotifications));
        this._dbSettings = dbSettings?.Value ?? throw new ArgumentNullException(nameof(dbSettings));
        this._redisRepository = redisRepository ?? throw new ArgumentNullException(nameof(redisRepository));
        this._redisSettings = redisSettings?.Value ?? throw new ArgumentNullException(nameof(redisSettings));
        this._countrySettings = countrySettings?.Value ?? throw new ArgumentNullException(nameof(countrySettings));
        this._metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
    }

    public async Task<bool> ConsumeAndProcessTransaction(string message)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogError("Null", "KafkaQuePayload Object is null: {KafkaMessage}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, message);
                return false;
            }
            var nullablePayload = JsonConvert.DeserializeObject<KafkaQueuePayload<RaCellularVoucherRedeemRequest>>(message);

            if (!IsPayloadValid(nullablePayload))
            {
                return false;
            }

            KafkaQueuePayload<RaCellularVoucherRedeemRequest> requestObject = nullablePayload!;

            int noOfAttempts = 0;

            AsyncRetryPolicy<CreditOnSyxResult> _retryPolicy = Policy
                .HandleResult<CreditOnSyxResult>(response => !response.IsSuccess)
                .WaitAndRetryAsync(
                    _queueConfiguration.ConsumerMaximumRetryCount,
                    retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(10, noOfAttempts)),
                    onRetry: (response, timeElapsed) =>
                    {
                        noOfAttempts++;
                        _logger.LogInformation(requestObject.VoucherRedeemRequest?.VoucherNumber, "Calling CreditOnSyx on SyxApi with payload: {request}. Retry Attempt: {noOfAttempts}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                                message,
                                noOfAttempts
                           );
                    }
                );

            var creditOnSyxResult = await _retryPolicy.ExecuteAsync(() => CreditOnSyx(requestObject));
            string messageContent;

            if (!creditOnSyxResult.IsSuccess)
            {
                _logger.LogError(requestObject.VoucherRedeemRequest?.VoucherNumber, "Failed to CreditOnSyx message on Consumer: {message}.", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, message);

                //Log to sql table for batch processing, should be flagged that its from Consumer
                _logger.LogInformation(requestObject.VoucherRedeemRequest?.VoucherNumber, "Inserting voucher {voucherNumber} to VoucherBatchProcessing", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, requestObject.VoucherRedeemRequest?.VoucherNumber ?? "");

                try
                {
                    var voucherPrefix = EnumHelper.GetEnumDescription(requestObject.VoucherType);
                    if (requestObject.RedeemOutcome == null)
                    {
                        _logger.LogError(requestObject.VoucherRedeemRequest?.VoucherNumber, "Missing VoucherID on RedeemOutcome: {VoucherNumber}.", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, requestObject.VoucherRedeemRequest?.VoucherNumber ?? "");
                        throw new FormatException("Missing VoucherID");
                    }

                    if (Guid.TryParse(requestObject.VoucherRedeemRequest?.VoucherReference, out Guid uniqueRef))
                    {
                        var voucherBatchRecord = new PendingBatchVoucher
                        {
                            VoucherType = requestObject.VoucherType,
                            VoucherPin = requestObject.VoucherRedeemRequest.VoucherNumber,
                            UniqueReference = uniqueRef,
                            ClientId = requestObject.VoucherRedeemRequest.ClientId,
                            DevicePlatform = requestObject.VoucherRedeemRequest.DevicePlatform,
                            VoucherID = requestObject.RedeemOutcome.VoucherID,
                            VoucherAmount = requestObject.RedeemOutcome.VoucherAmount,
                            VoucherPrefix = voucherPrefix,
                            Source = BatchProcessingSource.Consumer,
                            Message = message
                        };

                        await _voucherBatchProcessingRepository.InsertPendingBatchVoucher(voucherBatchRecord);

                        if (_redisSettings.EnableInProgressCheck)
                        {
                            int timeToLive = (int)(DateTime.Now.AddHours(_redisSettings.InProgressPolicyTimeToLiveInHours) - DateTime.Now).TotalSeconds;
                            await _redisRepository.AddWithTTL(requestObject.VoucherRedeemRequest.VoucherNumber, message, RedisStoreType.InProgressStore, timeToLive);
                        }
                    }
                    else
                    {
                        _logger.LogError(requestObject.VoucherRedeemRequest?.VoucherNumber, "Failed to Parse VoucherReference on Consumer: {VoucherReference}.",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            requestObject.VoucherRedeemRequest?.VoucherReference);
                        throw new FormatException("Invalid VoucherReference");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, requestObject.VoucherRedeemRequest?.VoucherNumber, "Failed to insert record message on VoucherBatchProcessing with error message: {message}.", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, ex.Message);
                    _logger.LogCritical(ex, requestObject.VoucherRedeemRequest?.VoucherNumber, "Failed to insert record message on VoucherBatchProcessing with error message: {message} with payload: {payload}.", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, ex.Message, message);

                    //Here message failed to be persisted and failed to be inserted for BatchProcessing we return fail result to user and ask to retry again.
                    messageContent = JsonConvert.SerializeObject(new SignalRNotifyModel
                    {
                        ClientId = requestObject.VoucherRedeemRequest?.ClientId.ToString(),
                        Message = _voucherRedeemClientNotifications.VoucherRedeemCriticalFailOnConsumer,
                        Balance = 0
                    });

                    //Send result with signalR here
                    await _httpClientCommunication.SendRequestAsync(_vpsControlCenterEndpoints.NotifyClientEndpoint, Domain.Models.Enums.HttpMethod.POST, messageContent, "application/json");
                }
            }
            else
            {
                messageContent = JsonConvert.SerializeObject(new SignalRNotifyModel
                {
                    ClientId = requestObject.VoucherRedeemRequest?.ClientId.ToString(),
                    Message = $"{_voucherRedeemClientNotifications.VoucherRedeemSuccess}{_countrySettings.CurrencyCode}{requestObject.RedeemOutcome?.VoucherAmount.ToString("0,0.00")}",
                    Balance = creditOnSyxResult.Balance
                });
                //Send success result with signalR here
                await _httpClientCommunication.SendRequestAsync(_vpsControlCenterEndpoints.NotifyClientEndpoint, Domain.Models.Enums.HttpMethod.POST, messageContent, "application/json");
                _metricsHelper.IncRAVoucherSuccessCounter(_logger);
            }

            return creditOnSyxResult.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ConsumeAndProcessTransaction", "Error in ClientBalanceSyxTransactionConsumerService: {ExMessage}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, ex.Message.ToString());
            _logger.LogCritical(ex, "ConsumeAndProcessTransaction", "Error in ClientBalanceSyxTransactionConsumerService: {ExMessage} with payload: {payload}.", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, ex.Message, message);
            throw;
        }
    }

    private async Task<CreditOnSyxResult> CreditOnSyx(KafkaQueuePayload<RaCellularVoucherRedeemRequest> requestObject)
    {
        //CreditBonusOnSyX is not. From the last session, seems Payment Portal team does not use CreditBonusOnSyX any longer, we can confirm that
        if (requestObject.VoucherRedeemRequest == null || requestObject.RedeemOutcome == null) throw new FormatException("VoucherRedeemRequest or RedeemOutcome is null");

        var voucherPrefix = EnumHelper.GetEnumDescription(requestObject.VoucherType);

        var syxCreditOutcome = await _clientBalanceService.CreditOnSyX(requestObject.VoucherRedeemRequest.ClientId, requestObject.VoucherRedeemRequest.DevicePlatform, requestObject.RedeemOutcome.VoucherID, requestObject.VoucherRedeemRequest.VoucherNumber, requestObject.RedeemOutcome.VoucherAmount, voucherPrefix, requestObject.VoucherType, requestObject.VoucherRedeemRequest.VoucherReference, _dbSettings.LogStoreProcedureName);

        if (syxCreditOutcome.OutComeTypeId == (int)SyxCreditOutcome.Success)
        {
            _logger.LogInformation(requestObject.VoucherRedeemRequest.VoucherNumber, "{voucherPrefix} voucher with pin {voucherNumber} - Message: {outcomeMessage}, Amount: {voucherAmount}, BalanceAvailable: {balanceAvailable}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    voucherPrefix,
                    requestObject.VoucherRedeemRequest.VoucherNumber,
                    syxCreditOutcome.OutcomeMessage,
                    requestObject.RedeemOutcome.VoucherAmount,
                    syxCreditOutcome.BalanceAvailable
                );

            return new CreditOnSyxResult
            {
                IsSuccess = true,
                Balance = syxCreditOutcome.BalanceAvailable,
            };
        }

        //eligible voucher detected. complete voucher credit.
        if (syxCreditOutcome.OutComeTypeId == (int)SyxCreditOutcome.EligibleForBonus)
        {
            var bonusResult = await _clientBalanceService.CreditBonusOnSyX(requestObject.VoucherRedeemRequest.ClientId,
                        requestObject.VoucherRedeemRequest.DevicePlatform, requestObject.RedeemOutcome.VoucherID, requestObject.RedeemOutcome.VoucherAmount, syxCreditOutcome.VoucherBonus, voucherPrefix);

            _logger.LogInformation(requestObject.VoucherRedeemRequest.VoucherNumber, "{voucherPrefix} voucher with pin {voucherNumber} - Message: {outcomeMessage}, Amount: {voucherAmount}, Bonus: {redeemedBonusAmount}, BalanceAvailable: {balanceAvailable}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    voucherPrefix,
                    requestObject.VoucherRedeemRequest.VoucherNumber,
                    syxCreditOutcome.OutcomeMessage,
                    requestObject.RedeemOutcome.VoucherAmount,
                    bonusResult.RedeemedBonusAmount,
                    bonusResult.BalanceAvailable
                );

            return new CreditOnSyxResult
            {
                IsSuccess = true,
                Balance = syxCreditOutcome.BalanceAvailable,
            };
        }

        _logger.LogError(requestObject.VoucherRedeemRequest.VoucherNumber, "{voucherPrefix} voucher with pin {voucherNumber} - Message: {outcomeMessage}, Amount: {voucherAmount}, BalanceAvailable: {balanceAvailable}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                voucherPrefix,
                requestObject.VoucherRedeemRequest.VoucherNumber,
                syxCreditOutcome.OutcomeMessage,
                requestObject.RedeemOutcome.VoucherAmount,
                syxCreditOutcome.BalanceAvailable
            );

        //We will not send signal R here, since it will go to BatchProcessing and from there we send the final result if its FAIL
        return new CreditOnSyxResult
        {
            IsSuccess = false,
            Balance = 0,
        };
    }

    private bool IsPayloadValid(KafkaQueuePayload<RaCellularVoucherRedeemRequest>? queuePayload)
    {
        if (queuePayload == null)
        {
            _logger.LogError("Null", "KafkaQuePayload Object is null: {KafkaMessage}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(queuePayload));
            return false;
        }

        if (queuePayload.VoucherRedeemRequest == null)
        {
            _logger.LogError("Null", "KafkaQuePayload VoucherRedeemRequest Object is null: {KafkaMessage}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(queuePayload));
            return false;
        }

        if (queuePayload.RedeemOutcome == null)
        {
            _logger.LogError("Null", "KafkaQuePayload RedeemOutcome Object is null: {KafkaMessage}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(queuePayload));
            return false;
        }

        return true;
    }
}
