using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Reflection;
using VPS.API.Flash;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Common.Request;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Configurations.Flash;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.Flash;
using VPS.Domain.Models.Flash.Enums;
using VPS.Domain.Models.Flash.Requests;
using VPS.Domain.Models.Flash.Responses;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Services.Common;
using VPS.Services.Common.Controllers;

namespace VPS.Services.Flash;

public class FlashRedeemService : VoucherRedeemService<FlashVoucher>
{
    private readonly ILoggerAdapter<FlashRedeemService> _log;
    private readonly IVoucherLogRepository _voucherLogRepository;
    private readonly IFlashApiService _flashAPIService;
    private readonly IFlashKafkaProducer _flashKafkaProducer;
    private readonly MetricsHelper _metricsHelper;
    private readonly FlashConfiguration _flashSettings;

    public FlashRedeemService(ILoggerAdapter<FlashRedeemService> log,
        IVoucherValidationService voucherValidationService,
        IVoucherLogRepository voucherLogRepository,
        IHttpContextAccessor httpContextAccessor,
        ILoggerAdapter<VoucherRedeemService<FlashVoucher>> vlog,
        IFlashApiService flashAPIService,
        IRedisService redisService,
        IOptions<RedisSettings> redisSettings,
        MetricsHelper metricsHelper,
        IOptions<FlashConfiguration> flashSettings,
        IFlashKafkaProducer flashKafkaProducer,
        IOptions<DBSettings> dbSettings,
        IVoucherProviderService voucherProviderService)
        : base(vlog, voucherValidationService, voucherLogRepository, httpContextAccessor, redisService, redisSettings, metricsHelper, dbSettings, voucherProviderService)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _voucherLogRepository = voucherLogRepository ?? throw new ArgumentNullException(nameof(voucherLogRepository));
        _flashAPIService = flashAPIService ?? throw new ArgumentNullException(nameof(flashAPIService));
        _flashKafkaProducer = flashKafkaProducer ?? throw new ArgumentNullException(nameof(flashKafkaProducer));
        _metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
        _flashSettings = flashSettings.Value ?? throw new ArgumentNullException(nameof(flashSettings));
    }

    /// <summary>
    /// PerformRedeem is the base method called from the endpoint of BlueVoucher
    /// This method calls the base method PerformRedeem on the common logic VoucherRedeemService
    /// </summary>
    /// <param name="voucherRedeemRequest">Request object.</param>
    /// <returns>Return ServiceResponse.</returns>
    public async Task<ServiceResponse> PerformRedeem(FlashRedeemRequest voucherRedeemRequest)
    {

        _metricsHelper.IncFlashRequestCounter(_log);
        return await ProcessRedeemRequest(voucherRedeemRequest, VoucherType.Flash,true, _flashSettings.IdempotencyLifespanSeconds);
    }

    /// <summary>
    /// GetRedeemOutcome is the overridden method from base here based on the logic of BlueVoucher
    /// </summary>
    /// <param name="voucherRedeemRequest">Request object.</param>
    /// <param name="voucherType">Voucher type.</param>
    /// <returns>Return RedeemOutcome.</returns>
    public override async Task<RedeemOutcome> GetRedeemOutcome(VoucherRedeemRequestBase voucherRedeemRequest, VoucherType voucherType)
    {
        try
        {
            var flashRequest = API.Flash.Processes.FlashRedeemProcess.CreateFlashRedeemRequest(voucherRedeemRequest, _flashSettings.FlashAccountNumber);
            var redeemResult = await _flashAPIService.RedeemVoucher(flashRequest);
            var voucherId = StringHelper.ConvertStringToLong(redeemResult.TransactionId);
            
            await _voucherLogRepository.UpdateVoucherLogAPIResponse(
              voucherRedeemRequest.VoucherNumber,
              voucherRedeemRequest.VoucherReference,
              VoucherType.Flash,
              VoucherStatus.Pending,
              voucherId,
              (redeemResult.Amount / 100),
              redeemResult.ResponseMessage);

            _log.LogInformation(null, "Flash Voucher redeem VoucherPin: {VoucherNumber}, {ResponseMessage}, Api Response: redeemResult.",
                MethodBase.GetCurrentMethod()?.Name ?? string.Empty, voucherRedeemRequest.VoucherNumber, redeemResult.ResponseMessage, JsonConvert.SerializeObject(redeemResult));

            var redeemOutcome = BuildResult(redeemResult);
            if (redeemResult.ResponseCode == (int)FlashRedeemResponseCodes.VoucherRedeemSuccessfull)
            {
                _log.LogInformation(null, "Voucher redeem success for pin: {voucherNumber}, Client ID {clientId}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId );
                //Produce in Kafka
                await _flashKafkaProducer.Produce((FlashRedeemRequest)voucherRedeemRequest, voucherType, redeemOutcome);
            }
            //error while redeeming
            else
            {
                _log.LogError(null, "Flash Voucher redeem error for pin  {voucherNumber}, Client ID {clientId}. Reason: {reason}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId, redeemResult.ResponseMessage);
            }
            return redeemOutcome;
        }
        catch (Exception ex)
        {

            _log.LogError(ex, null, "Flash Redeem Failed: {message} - {voucherNumber} - {clientId}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, ex.Message, voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId);

            return new RedeemOutcome
            {
                OutcomeMessage = $"Flash redeem failed,Please try again.",
                OutComeTypeId = -1
            };
        }
    }

    #region Private Utils
    private static RedeemOutcome BuildResult(FlashRedeemResponse voucherResponse)
    {
        switch (voucherResponse.ResponseCode)
        {
            case (int)FlashRedeemResponseCodes.VoucherRedeemSuccessfull:
                return new RedeemOutcome
                {
                    OutComeTypeId = 1,
                    OutcomeMessage = "Voucher redeem successfully",
                    VoucherAmount = (voucherResponse.Amount/ 100),
                    VoucherID = StringHelper.ConvertStringToLong(voucherResponse.TransactionId)
                };

            case (int)FlashRedeemResponseCodes.VoucherInvalid:
                return new RedeemOutcome
                {
                    OutcomeMessage = "Invalid Voucher.",
                    OutComeTypeId = -1,
                };
            case (int)FlashRedeemResponseCodes.VoucherAlreadyRedeemed:
                return new RedeemOutcome
                {
                    OutcomeMessage = "Voucher Already Redeemed.",
                    OutComeTypeId = -1,
                };
            case (int)FlashRedeemResponseCodes.VoucherExpired:
                return new RedeemOutcome
                {
                    OutcomeMessage = "Voucher Expired.",
                    OutComeTypeId = -1,
                };
            case (int)FlashRedeemResponseCodes.VoucherNotFound:
                return new RedeemOutcome
                {
                    OutcomeMessage = "Voucher Not Found.",
                    OutComeTypeId = -1,
                };
            case (int)FlashRedeemResponseCodes.VoucherCancelled:
                return new RedeemOutcome
                {
                    OutcomeMessage = "Voucher Cancelled.",
                    OutComeTypeId = -1,
                };
            case (int)FlashRedeemResponseCodes.VoucherDuplicateTransaction:
                return new RedeemOutcome
                {
                    OutcomeMessage = "Voucher Transaction Duplicate.",
                    OutComeTypeId = -1,
                };
            default:
                return new RedeemOutcome
                {
                    OutcomeMessage = $"{voucherResponse.ResponseMessage}",
                    OutComeTypeId = -1
                };
        }
    }
    #endregion
}
