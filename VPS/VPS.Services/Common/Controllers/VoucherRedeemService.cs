using Intercom.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Reflection;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Common.Request;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;

namespace VPS.Services.Common.Controllers
{
    public abstract class VoucherRedeemService<T>
    {

        private readonly ILoggerAdapter<VoucherRedeemService<T>> _log;
        private readonly IVoucherValidationService _voucherValidationService;
        private readonly IVoucherLogRepository _voucherLogRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRedisService _redisService;
        private readonly RedisSettings _redisSettings;
        private readonly MetricsHelper _metricsHelper;
        private readonly DBSettings _dbSettings;
        private readonly IVoucherProviderService _voucherProviderService;

        protected VoucherRedeemService(ILoggerAdapter<VoucherRedeemService<T>> log,
                              IVoucherValidationService voucherValidationService,
                              IVoucherLogRepository voucherLogRepository,
                              IHttpContextAccessor httpContextAccessor,
                              IRedisService redisService,
                              IOptions<RedisSettings> redisSettings,
                              MetricsHelper metricsHelper,
                              IOptions<DBSettings> dbSettings,
                              IVoucherProviderService voucherProviderService)
        {
            this._log = log ?? throw new ArgumentNullException(nameof(log));
            this._voucherValidationService = voucherValidationService ?? throw new ArgumentNullException(nameof(voucherValidationService));
            this._voucherLogRepository = voucherLogRepository ?? throw new ArgumentNullException(nameof(voucherLogRepository));
            this._httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this._redisService = redisService ?? throw new ArgumentNullException(nameof(redisService));
            this._metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
            this._redisSettings = redisSettings.Value;
            this._dbSettings = dbSettings.Value;
            this._voucherProviderService = voucherProviderService ?? throw new ArgumentNullException(nameof(voucherProviderService));
        }

        /// <summary> 
        /// ProcessRedeemRequest is the base method for Redeem process for all providers
        /// Inside this method is the GetRedeemOutcome(voucherRedeemRequest, voucherType) which is different for every provider 
        /// and should be overridden in the specific provider based on it's logic.
        /// </summary> 
        /// <param name="voucherRedeemRequest">Request object.</param> 
        /// <param name="voucherType">Voucher type.</param> 
        /// <returns>Return ServiceResponse.</returns>
        public async Task<ServiceResponse> ProcessRedeemRequest(VoucherRedeemRequestBase voucherRedeemRequest, VoucherType voucherType, bool validateVoucherNumberLength = true, int idempotencyTTL = 2)
        {

            bool clearConcurrencyCheck = false;
            voucherRedeemRequest.ServiceVersion = ServiceVersion;
            try
            {
                //update provider settings latest from control center
                await _voucherProviderService.SetProviders();

                if (!VoucherProviderHelper.IsProviderEnabled(voucherType))
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = $"{voucherType} is disabled."
                    };
                }

                _log.LogInformation(voucherRedeemRequest.VoucherNumber, "Incoming Payload: {voucherRedeemRequest}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    JsonConvert.SerializeObject(voucherRedeemRequest) );

                _metricsHelper.IncVouchersRedeemInitiated(_log);

                var validationResult = await ProcessValidation(voucherRedeemRequest, voucherType, validateVoucherNumberLength, idempotencyTTL);

                if(validationResult != null)
                {
                    return validationResult;
                }
                
                clearConcurrencyCheck = true;

                if (await this.IsDelayStillAlive(_redisSettings, voucherRedeemRequest, _redisService, _log))
                {
                    _metricsHelper.IncVouchersAbortedByDelayPolicy(_log);
                    return new ServiceResponse { IsSuccess = false, Message = "Failed to pass delay policy" };
                }

                await LogToSQL(voucherRedeemRequest, voucherType, _dbSettings.LogStoreProcedureName);

                // Redeem
                var redeemOutcome = await GetRedeemOutcome(voucherRedeemRequest, voucherType);


                if(redeemOutcome.OutComeTypeId == 1 && voucherType== VoucherType.HollaMobile) {
                    return new ServiceResponse { IsSuccess = true, CreditOutcome = redeemOutcome.OutComeTypeId, Amount=redeemOutcome.VoucherAmount, Message = "Redeemed Successfully" };

                }

                else if (redeemOutcome.OutComeTypeId == 1)
                {
                    return new ServiceResponse { IsSuccess = true, CreditOutcome = redeemOutcome.OutComeTypeId, Message = "Transaction is being processed" };
                }
                else
                {
                    _metricsHelper.IncVouchersRedeemFailed(_log);
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = redeemOutcome.OutcomeMessage,
                        CreditOutcome = redeemOutcome.OutComeTypeId,
                        Amount = redeemOutcome.VoucherAmount
                    };
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, null, "{voucherNumber} - {message}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, voucherRedeemRequest.VoucherNumber, ex.Message);
                return new ServiceResponse { IsSuccess = false, Message = " InternalServerError" };
            }
            finally
            {
                if (_redisSettings.EnableConcurrencyCheck && clearConcurrencyCheck)
                {
                    _ = Task.Run(async () =>
                     {
                         await Task.Delay(TimeSpan.FromSeconds(_redisSettings.ConcurrencyDelayInSeconds));
                         await _redisService.DeleteConcurrencyEntry(voucherRedeemRequest.VoucherNumber);
                         _log.LogInformation(voucherRedeemRequest.VoucherNumber, "Deleted voucher with pin Entry from Redis completed {voucherNumber}",
                           MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            voucherRedeemRequest.VoucherNumber );
                     });
                }
            }
        }


        /// <summary> 
        /// GetRedeemOutcome is abstract method in base here. The logic will be overridden method on each provider
        /// </summary> 
        /// <param name="voucherRedeemRequest">Request object.</param> 
        /// <param name="voucherType">Voucher type.</param> 
        /// <returns>Return RedeemOutcome.</returns>
        public abstract Task<RedeemOutcome> GetRedeemOutcome(VoucherRedeemRequestBase voucherRedeemRequest, VoucherType voucherType);

        #region Properties
        public string ServiceVersion
        {
            get
            {
                return $"{ProjectPropertiesHelper.GetProjectName()} {ProjectPropertiesHelper.GetProjectVersion()}";
            }
        }
        #endregion
        #region Class Helpers

        [NonAction]
        public async Task LogToSQL(VoucherRedeemRequestBase voucherRedeemRequest, VoucherType voucherType, string logStoreProcedureName, string? message = null)
        {

            // Construct the absolute URL using the request's scheme, host, and path
            var absoluteUrl = $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}{_httpContextAccessor.HttpContext?.Request.Path}{_httpContextAccessor.HttpContext?.Request.QueryString}";

            await _voucherLogRepository.InsertVoucherLog(new VoucherDBLogRecordModel
            {
                VoucherTypeId = voucherType,
                ClientId = voucherRedeemRequest.ClientId,
                VoucherPin = voucherRedeemRequest.VoucherNumber,
                SyXPlatform = voucherRedeemRequest.DevicePlatform
            }, absoluteUrl, logStoreProcedureName, voucherRedeemRequest.VoucherReference, message?? "");

        }

        [NonAction]
        private async Task<ServiceResponse?> ProcessValidation(VoucherRedeemRequestBase voucherRedeemRequest, VoucherType voucherType, bool validateVoucherNumberLength, int idempotencyTTL = 2)
        {
            var validationOutCome = _voucherValidationService.IsVoucherRequestValid(voucherRedeemRequest);

            if (!string.IsNullOrEmpty(validationOutCome))
            {
                _metricsHelper.IncInvalidVouchersFormat(_log);
                return new ServiceResponse { IsSuccess = false, Message = validationOutCome };
            }

            if (validateVoucherNumberLength)
            {
                var voucherLengthValidationResponse = VoucherProviderHelper.IsVoucherLengthValid(voucherRedeemRequest.VoucherNumber, voucherType);
                if (!string.IsNullOrEmpty(voucherLengthValidationResponse))
                {
                    _metricsHelper.IncInvalidVouchersFormat(_log);
                    return new ServiceResponse { IsSuccess = false, Message = voucherLengthValidationResponse };
                }
            }

            if (_redisSettings.EnableInProgressCheck && await this.IsRedemptionInProgress(_redisSettings, voucherRedeemRequest.VoucherNumber, _redisService, _log))
            {
                return new ServiceResponse { IsSuccess = false, Message = "Transaction is being processed" };
            }
            if (voucherType == VoucherType.Flash)
            {
                if (await this.DoesConcurrencyAndIdempotencyExist(_redisSettings, voucherRedeemRequest, _redisService, _log, idempotencyTTL))
                {
                    _metricsHelper.IncVouchersConcurrentRequest(_log);
                    return new ServiceResponse { IsSuccess = false, Message = "Duplicate Request" };
                }
            }
            else
            {
                if (await this.DoesConcurrencyExist(_redisSettings, voucherRedeemRequest, _redisService, _log))
                {
                    _metricsHelper.IncVouchersConcurrentRequest(_log);
                    return new ServiceResponse { IsSuccess = false, Message = "Duplicate Request" };
                }
            }

            return null;
        }

        #endregion
    }
}
