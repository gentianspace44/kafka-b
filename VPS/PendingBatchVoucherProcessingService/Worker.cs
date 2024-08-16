using Azure.Messaging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System.Reflection;
using VPS.API.Common;
using VPS.API.Syx;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.Flash.Responses;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Infrastructure.Repository.Redis;
using VPS.Services.Common;

namespace PendingBatchVoucherProcessingService
{
    public class Worker : BackgroundService
    {
        private readonly ILoggerAdapter<Worker> _logger;
        private readonly IVoucherBatchProcessingRepository _voucherBatchProcessingRepository;
        private readonly IClientBalanceService _clientBalanceService;
        private readonly ISyxApiService _syxService;
        private readonly PendingBatchVoucherSettings _pendingBatchVoucherSettings;
        private readonly VpsControlCenterEndpoints _vpsControlCenterEndpoints;
        private readonly IReferenceGeneratorService _referenceGeneratorService;
        private readonly IHttpClientCommunication _httpClientCommunication;
        private readonly VoucherRedeemClientNotifications _voucherRedeemClientNotifications;
        private readonly BatchProcessingDBSettings _batchProcessingDBSettings;
        private readonly IBatchServiceRedisRepository _redisRepository;
        private readonly BatchServiceRedisSettings _redisSettings;
        private readonly CountrySettings _countrySettings;
        private readonly MetricsHelper _metricsHelper;

        public Worker(ILoggerAdapter<Worker> logger,
            IVoucherBatchProcessingRepository voucherBatchProcessingRepository,
            IClientBalanceService clientBalanceService,
            ISyxApiService syxService,
            IOptions<PendingBatchVoucherSettings> pendingBatchVoucherSettings,
            IOptions<VpsControlCenterEndpoints> vpsControlCenterEndpoints,
            IReferenceGeneratorService referenceGeneratorService,
            IHttpClientCommunication httpClientCommunication,
            IBatchServiceRedisRepository redisRepository,
            IOptions<VoucherRedeemClientNotifications> voucherRedeemClientNotifications,
            IOptions<BatchProcessingDBSettings> batchProcessingDBSettings,
            IOptions<BatchServiceRedisSettings> redisSettings,
            IOptions<CountrySettings> countrySettings,
            MetricsHelper metricsHelper)
        {
            this._logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            this._voucherBatchProcessingRepository = voucherBatchProcessingRepository ?? throw new System.ArgumentNullException(nameof(voucherBatchProcessingRepository));
            this._clientBalanceService = clientBalanceService ?? throw new System.ArgumentNullException(nameof(clientBalanceService));
            this._syxService = syxService ?? throw new System.ArgumentNullException(nameof(syxService));
            this._referenceGeneratorService = referenceGeneratorService ?? throw new System.ArgumentNullException(nameof(referenceGeneratorService));
            this._pendingBatchVoucherSettings = pendingBatchVoucherSettings.Value;
            this._vpsControlCenterEndpoints = vpsControlCenterEndpoints.Value;
            this._httpClientCommunication = httpClientCommunication ?? throw new ArgumentNullException(nameof(httpClientCommunication));
            this._voucherRedeemClientNotifications = voucherRedeemClientNotifications.Value;
            this._batchProcessingDBSettings = batchProcessingDBSettings.Value;
            this._redisRepository = redisRepository ?? throw new ArgumentNullException(nameof(redisRepository));
            this._redisSettings = redisSettings.Value;
            this._countrySettings = countrySettings.Value;
            this._metricsHelper = metricsHelper;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                int delayInSeconds = _pendingBatchVoucherSettings.JobIntervalInSeconds;
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("PendingBatchVoucherProcessingService", "Worker running at: {time}", DateTimeOffset.Now.ToString() );

                    //Perform task
                    try
                    {
                        int batchSize = _pendingBatchVoucherSettings.BatchSize;
                        int retryLimit = _pendingBatchVoucherSettings.RetryLimit;

                        //Get pending vouchers to be processed
                        _logger.LogInformation("PendingVoucherBatchProcessingJob", "Getting pending vouchers to be processed");

                        var vouchersToBeProcessed = await _voucherBatchProcessingRepository.SelectPendingBatchVouchersToBeProcessedAutomatically(batchSize);
                        if (vouchersToBeProcessed != null && vouchersToBeProcessed.Any())
                        {
                            _logger.LogInformation("PendingVoucherBatchProcessingJob", "{numberOfVouchers} vouchers to be processed found!",  vouchersToBeProcessed.Count().ToString() );

                            //Check if SyxApi is up and running
                            if (!await _syxService.HealthCheck())
                            {
                                _logger.LogInformation("PendingBatchVoucherProcessingService", "SyX healthcheck failed, for PendingBatchVoucherProcessingService");
                                return;
                            }

                            await ProcessEachPendingVoucher(retryLimit, vouchersToBeProcessed);
                        }
                        else
                        {
                            _logger.LogInformation("PendingVoucherBatchProcessingJob", "No vouchers to be processed found!");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Null", "Error in PendingVoucherBatchProcessingJob: {ExMessage}",  ex.Message.ToString());
                    }

                    await Task.Delay(TimeSpan.FromSeconds(delayInSeconds), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PendingBatchVoucherProcessingService", "Worker Failed running at: {time} with message: {message}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, DateTimeOffset.Now.ToString(), ex.Message );
            }
        }

        private async Task ProcessEachPendingVoucher(int retryLimit, IEnumerable<PendingBatchVoucher> vouchersToBeProcessed)
        {
            foreach (var voucher in vouchersToBeProcessed)
            {
                try
                {
                    _logger.LogInformation(voucher.VoucherPin, "PendingVoucherBatchProcessingJob - Starting to process voucher with pin {voucherPin} and message: {message}!", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, voucher.VoucherPin, voucher.Message );

                    voucher.ProcessingAttempts += 1;
                    await _voucherBatchProcessingRepository.UpdatePendingBatchVoucherAttempts(voucher.VoucherType, voucher.VoucherPin, voucher.ProcessingAttempts);
                                        
                    //If current number of attempts are more than one then check if it was already credited on syx but failed to finalize here
                    if (voucher.ProcessingAttempts > 1)
                    {
                        var reference = _referenceGeneratorService.Generate(voucher.VoucherPrefix, voucher.VoucherID.ToString(), voucher.VoucherPin, voucher.DevicePlatform);
                        var checkExistsResponse = await _clientBalanceService.CheckVoucherExistsOnSyx(voucher.ClientId, reference);

                        if (checkExistsResponse == null)
                        {
                            _logger.LogError("PendingVoucherBatchProcessingJob", "Failed to CheckVoucherExistsOnSyx from PendingVoucherBatchProcessingJob (response is null) for reference: {reference}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, reference);
                        }
                        else if (checkExistsResponse.VoucherExists && checkExistsResponse.ResponseType == 1)
                        {
                            await ProcessBatchFinalize(voucher, reference);
                            continue;
                        }
                        //If voucher exists on syx but is not credited we mark for manual check.
                        else if (checkExistsResponse.VoucherExists && checkExistsResponse.ResponseType != 1)
                        {
                            await ProcessFailedBatch(voucher);
                            continue;
                        }
                    }

                   await  ProcessSyxCredit(voucher, retryLimit);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, voucher.VoucherPin, "Error in PendingVoucherBatchProcessingJob for voucher: {voucherPin} with message: {ExMessage}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  voucher.VoucherPin, ex.Message.ToString());                   
                }
            }
        }

        private async Task<CreditOnSyxResult> CreditOnSyx(PendingBatchVoucher voucher)
        {
            //CreditBonusOnSyX is not. From the last session, seems Payment Portal team does not use CreditBonusOnSyX any longer, we can confirm that
            //For SignalR we need to ensure that the parameters to use to communicate with the client is added as kafkaQueue payload
            var syxCreditOutcome = await _clientBalanceService.CreditOnSyX(voucher.ClientId, voucher.DevicePlatform, voucher.VoucherID, voucher.VoucherPin, voucher.VoucherAmount, voucher.VoucherPrefix, voucher.VoucherType, voucher.UniqueReference.ToString(), GetLogStoredProcedureNameBasedOnProvider(voucher.VoucherType));

            if (syxCreditOutcome.OutComeTypeId == (int)SyxCreditOutcome.Success)
            {
                _logger.LogInformation(voucher.VoucherPin, "{voucherPrefix} voucher with pin {voucherNumber} - Message: {outcomeMessage}, Amount: {voucherAmount}, BalanceAvailable: {balanceAvailable}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                     voucher.VoucherPrefix, voucher.VoucherPin, syxCreditOutcome.OutcomeMessage, voucher.VoucherAmount, syxCreditOutcome.BalanceAvailable );

                return new CreditOnSyxResult
                {
                    IsSuccess = true,
                    Balance = syxCreditOutcome.BalanceAvailable,
                };
            }

            //eligible voucher detected. complete voucher credit.
            if (syxCreditOutcome.OutComeTypeId == (int)SyxCreditOutcome.EligibleForBonus)
            {
                var bonusResult = await _clientBalanceService.CreditBonusOnSyX(voucher.ClientId,
                            voucher.DevicePlatform, voucher.VoucherID, voucher.VoucherAmount, syxCreditOutcome.VoucherBonus, voucher.VoucherPrefix);

                _logger.LogInformation(voucher.VoucherPin, "{voucherPrefix} voucher with pin {voucherNumber} - Message: {outcomeMessage}, Amount: {voucherAmount}, Bonus: {redeemedBonusAmount}, BalanceAvailable: {balanceAvailable}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                     voucher.VoucherPrefix, voucher.VoucherPin, syxCreditOutcome.OutcomeMessage, voucher.VoucherAmount, bonusResult.RedeemedBonusAmount, bonusResult.BalanceAvailable );

                return new CreditOnSyxResult
                {
                    IsSuccess = true,
                    Balance = syxCreditOutcome.BalanceAvailable,
                };
            }

            _logger.LogError(voucher.VoucherPin, null, "{voucherPrefix} voucher with pin {voucherNumber} - Message: {outcomeMessage}, Amount: {voucherAmount}, BalanceAvailable: {balanceAvailable}",
                MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                 voucher.VoucherPrefix, voucher.VoucherPin, syxCreditOutcome.OutcomeMessage, voucher.VoucherAmount, syxCreditOutcome.BalanceAvailable);

            //We will send the fail error message only when voucher is marked for Manual Processing (Finalize fail voucher)
            return new CreditOnSyxResult
            {
                IsSuccess = false,
                Balance = 0,
            };
        }

        private async Task ProcessSyxCredit(PendingBatchVoucher? voucher, int retryLimit)
        {

            if(voucher == null) throw new ArgumentNullException(nameof(voucher), "PendingBatchVoucher is null");

            int pollyRetryAttempts = 0;

            AsyncRetryPolicy<CreditOnSyxResult> _retryPolicy = Policy
                .HandleResult<CreditOnSyxResult>(response => !response.IsSuccess)
                .WaitAndRetryAsync(
                    retryLimit,
                    retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(10, pollyRetryAttempts)),
                    onRetry: (response, timeElapsed) => 
                    {
                        pollyRetryAttempts++;
                        _logger.LogInformation(voucher.VoucherPin, "Calling CreditOnSyx on SyxApi from PendingVoucherBatchProcessingJob with payload: {request}. Retry Attempt: {noOfAttempts}",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            voucher.Message??string.Empty, pollyRetryAttempts );
                    }
            );

            var creditOnSyxResult = await _retryPolicy.ExecuteAsync(() => CreditOnSyx(voucher));

            //If final result is false, insert into sql table for Batch processing, flagged that it came from consume.
            if (!creditOnSyxResult.IsSuccess)
            {
                _logger.LogError(voucher.VoucherPin, "Failed to CreditOnSyx voucher on PendingVoucherBatchProcessingJob: {message}.",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  voucher.Message );
                //If it reached max retry attempts
                if (voucher.ProcessingAttempts == retryLimit)
                {
                    //Marking voucher {voucherNumber} to NeedsManualProcessing true
                    _logger.LogInformation(voucher.VoucherPin, "Marking voucher {voucherNumber} to NeedsManualProcessing true", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  voucher.VoucherPin );

                    try
                    {
                        //Here after consuming all automatic tries to credit the voucher we notify the client that it's marked for manual processing
                       var  messageContent = JsonConvert.SerializeObject(new SignalRNotifyModel
                        {
                            ClientId = voucher.ClientId.ToString(),
                            Message = _voucherRedeemClientNotifications.VocherRedeemFailPendingManualProcessing,
                            Balance = 0
                        });

                        //Send result with singalR here 
                        await _httpClientCommunication.SendRequestAsync(_vpsControlCenterEndpoints.NotifyClientEndpoint, VPS.Domain.Models.Enums.HttpMethod.POST, messageContent, "application/json");

                        await _voucherBatchProcessingRepository.FinalizePendingBatchVoucherFail(voucher.VoucherType, voucher.VoucherPin);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, voucher.VoucherPin, "Failed to FinalizePendingBatchVoucherFail on PendingVoucherBatchProcessingJob with error message: {message}.", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  ex.Message );
                       
                    }
                }
            }
            else
            {
                //Closing voucher processing
                _logger.LogInformation(voucher.VoucherPin, "CreditOnSyx was success for voucher: {voucherNumber},  Marking voucher {voucherNumber} to CreditedOnSyX true", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  voucher.VoucherPin, voucher.VoucherPin );
                try
                {

                   var messageContent = JsonConvert.SerializeObject(new SignalRNotifyModel
                    {
                        ClientId = voucher.ClientId.ToString(),
                        Message = $"{_voucherRedeemClientNotifications.VoucherRedeemSuccess}{_countrySettings.CurrencyCode}{voucher.VoucherAmount.ToString("0,0.00")}",
                        Balance = creditOnSyxResult.Balance
                    });

                    //Send success result with singalR here
                    await _httpClientCommunication.SendRequestAsync(_vpsControlCenterEndpoints.NotifyClientEndpoint, VPS.Domain.Models.Enums.HttpMethod.POST, messageContent, "application/json");

                    await _voucherBatchProcessingRepository.FinalizePendingBatchVoucherSuccess(voucher.VoucherType, voucher.VoucherPin);

                    //Delete Progress Status From Redis
                    if (_redisSettings.EnableInProgressCheck)
                    {
                        _ = await _redisRepository.Delete(voucher.VoucherPin, voucher.VoucherType);
                        _logger.LogInformation(voucher.VoucherPin, "Removed in-progress log from redis.");
                    }

                    switch (voucher.VoucherType)
                    {
                        case VoucherType.RACellular:
                            _metricsHelper.IncRAVoucherSuccessCounter(_logger);
                            break;
                        case VoucherType.HollyTopUp:
                            _metricsHelper.IncHollyTopUpSuccessCounter(_logger);
                            break;
                        case VoucherType.EasyLoad:
                            _metricsHelper.IncEasyLoadSuccessCounter(_logger);
                            break;
                        case VoucherType.BluVoucher:
                            _metricsHelper.IncBluVoucherSuccessCounter(_logger);
                            break;
                        case VoucherType.OTT:
                            _metricsHelper.IncOTTSuccessCounter(_logger);
                            break;
                        case VoucherType.Flash:
                            _metricsHelper.IncFlashSuccessCounter(_logger);
                            break;
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, voucher.VoucherPin, "Failed to FinalizePendingBatchVoucherSuccess on PendingVoucherBatchProcessingJob with error message: {message}.", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, ex.Message);
                    
                }
            }
        }

        private async Task ProcessBatchFinalize(PendingBatchVoucher? voucher, string? reference)    
        {

            if (voucher == null) throw new ArgumentNullException(nameof(voucher), "PendingBatchVoucher is null");

            _logger.LogError("PendingVoucherBatchProcessingJob", "Voucher: {reference} already credited on Syx", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  reference ?? string.Empty );

            try
            {
                //Here after consuming all automatic tries to credit the voucher we notify the client that it's marked for manual processing
               var  messageContent = JsonConvert.SerializeObject(new SignalRNotifyModel
                {
                    ClientId = voucher.ClientId.ToString(),
                    Message = _voucherRedeemClientNotifications.VoucherAlreadyCreditedOnSyx,
                    Balance = 0
                });

                //Send result with singalR here 
                await _httpClientCommunication.SendRequestAsync(_vpsControlCenterEndpoints.NotifyClientEndpoint, VPS.Domain.Models.Enums.HttpMethod.POST, messageContent, "application/json");

                await _voucherBatchProcessingRepository.FinalizePendingBatchVoucherSuccess(voucher.VoucherType, voucher.VoucherPin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, voucher.VoucherPin, "Failed to FinalizePendingBatchVoucherSuccess on PendingVoucherBatchProcessingJob with error message: {message}.", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, ex.Message );               
            }
        }

        private async Task ProcessFailedBatch(PendingBatchVoucher? voucher)
        {
            if (voucher == null) throw new ArgumentNullException( nameof(voucher), "PendingBatchVoucher is null");

            //Marking voucher {voucherNumber} to NeedsManualProcessing true
            _logger.LogInformation(voucher.VoucherPin, "Marking voucher {voucherNumber} to NeedsManualProcessing true", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  voucher.VoucherPin );

            try
            {
               
                    //Here after consuming all automatic tries to credit the voucher we notify the client that it's marked for manual processing
                    var messageContent = JsonConvert.SerializeObject(new SignalRNotifyModel
                    {
                        ClientId = voucher.ClientId.ToString(),
                        Message = _voucherRedeemClientNotifications.VocherRedeemFailPendingManualProcessing,
                        Balance = 0
                    });

                    //Send result with singalR here 
                    await _httpClientCommunication.SendRequestAsync(_vpsControlCenterEndpoints.NotifyClientEndpoint, VPS.Domain.Models.Enums.HttpMethod.POST, messageContent, "application/json");

                    await _voucherBatchProcessingRepository.FinalizePendingBatchVoucherFail(voucher.VoucherType, voucher.VoucherPin);
               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, voucher.VoucherPin, "Failed to FinalizePendingBatchVoucherFail on PendingVoucherBatchProcessingJob with error message: {message}.", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, ex.Message);              
            }
        }

        private string GetLogStoredProcedureNameBasedOnProvider(VoucherType voucherType)
        {
            switch (voucherType)
            {
                case VoucherType.HollyTopUp:
                    return _batchProcessingDBSettings.LogSPNameHTU;
                case VoucherType.OTT:
                    return _batchProcessingDBSettings.LogSPNameOTT;
                case VoucherType.Flash:
                    return _batchProcessingDBSettings.LogSPNameFlash;
                case VoucherType.BluVoucher:
                    return _batchProcessingDBSettings.LogSPNameBluVoucher;
                case VoucherType.EasyLoad:
                    return _batchProcessingDBSettings.LogSPNameEasyLoad;
                case VoucherType.RACellular:
                    return _batchProcessingDBSettings.LogSPNameRA;
                default:
                    throw new NotSupportedException($"Unsupported VoucherType: {voucherType}");
            }
        }
    }
}