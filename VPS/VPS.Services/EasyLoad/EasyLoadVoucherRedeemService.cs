using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Reflection;
using VPS.API.EasyLoad;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Common.Request;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.EasyLoad.Enums;
using VPS.Domain.Models.EasyLoad.Request;
using VPS.Domain.Models.EasyLoad.Response;
using VPS.Domain.Models.Enums;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Services.Common;
using VPS.Services.Common.Controllers;

namespace VPS.Services.EasyLoad;

public class EasyLoadVoucherRedeemService : VoucherRedeemService<Domain.Models.EasyLoad.EasyLoadVoucher>
{
    private readonly ILoggerAdapter<EasyLoadVoucherRedeemService> _log;
    private readonly IVoucherLogRepository _voucherLogRepository;
    private readonly IEasyLoadApiService _easyLoadService;
    private readonly IEasyLoadKafkaProducer _easyLoadKafkaProducer;
    private readonly MetricsHelper _metricsHelper;

    public EasyLoadVoucherRedeemService(ILoggerAdapter<EasyLoadVoucherRedeemService> log,
        IVoucherValidationService voucherValidationService,
        IVoucherLogRepository voucherLogRepository,
        IHttpContextAccessor httpContextAccessor,
        ILoggerAdapter<VoucherRedeemService<Domain.Models.EasyLoad.EasyLoadVoucher>> vlog,
        IEasyLoadApiService easyLoadService,
        IRedisService redisService,
        IOptions<RedisSettings> redisSettings,
        MetricsHelper metricsHelper,
        IOptions<DBSettings> dbSettings,
        IEasyLoadKafkaProducer easyLoadKafkaProducer,
        IVoucherProviderService voucherProviderService)
        : base(vlog, voucherValidationService, voucherLogRepository, httpContextAccessor, redisService, redisSettings, metricsHelper, dbSettings, voucherProviderService)
    {
        this._log = log ?? throw new System.ArgumentNullException(nameof(log));
        this._voucherLogRepository = voucherLogRepository ?? throw new ArgumentNullException(nameof(voucherLogRepository));
        this._easyLoadService = easyLoadService ?? throw new ArgumentNullException(nameof(easyLoadService));
        this._easyLoadKafkaProducer = easyLoadKafkaProducer ?? throw new ArgumentNullException(nameof(easyLoadKafkaProducer));
        this._metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
    }

    /// <summary>
    /// PerformRedeem is the base method called from the endpoint of EasyLoadVoucher
    /// This method calls the base method PerformRedeem on the common logic VoucherRedeemService
    /// </summary>
    /// <param name="voucherRedeemRequest">Request object.</param>
    /// <returns>Return ServiceResponse.</returns>
    public async Task<ServiceResponse> PerformRedeem(EasyLoadVoucherRedeemRequest voucherRedeemRequest)
    {
        _metricsHelper.IncEasyLoadRequestCounter(_log);
        return await ProcessRedeemRequest(voucherRedeemRequest, VoucherType.EasyLoad);
    }

    /// <summary>
    /// GetRedeemOutcome is the overridden method from base here based on the logic of EasyLoadVoucher
    /// </summary>
    /// <param name="voucherRedeemRequest">Request object.</param>
    /// <param name="voucherType">Voucher type.</param>
    /// <returns>Return RedeemOutcome.</returns>
    public override async Task<RedeemOutcome> GetRedeemOutcome(VoucherRedeemRequestBase voucherRedeemRequest, VoucherType voucherType)
    {
        try
        {
            var redeemResult = await _easyLoadService.RedeemVoucher(voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId) ?? throw new FormatException("RedeemResult is null");
            await _voucherLogRepository.UpdateVoucherLogAPIResponse(
            voucherRedeemRequest.VoucherNumber,
            voucherRedeemRequest.VoucherReference,
            VoucherType.EasyLoad,
            VoucherStatus.Pending,
            redeemResult.VoucherId,
            redeemResult.Amount > 0 ? redeemResult.Amount / 100 : 0,
            redeemResult.ResponseMessage);

            _log.LogInformation(null, "EasyLoad Voucher redeem VoucherPin: {voucherNumber}, Client ID {clientId}, Api Response: {redeemResult}.", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId, JsonConvert.SerializeObject(redeemResult) );
            var redeemOutcome = BuildResult(redeemResult);
            if (redeemResult.ResponseCode == (int)EasyLoadRedeemResponseCodes.VoucherRedeemSuccessful)
            {
                _log.LogInformation(null, "Voucher redeem success for pin: {voucherNumber}, Client ID {clientId}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId);
                //Produce in Kafka
                await _easyLoadKafkaProducer.Produce((EasyLoadVoucherRedeemRequest)voucherRedeemRequest, voucherType, redeemOutcome);
            }
            //error while redeeming
            else
            {
                _log.LogError(null, "EasyLoad Voucher redeem error for pin  {voucherNumber}, Client ID {clientId}. Reason: {reason}",  voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId, redeemResult.ResponseMessage );
            }
            return redeemOutcome;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, null, "EasyLoad Redeem Failed: {errMessage} - {voucherNumber} - {clientId}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, ex.Message, voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId );

            return new RedeemOutcome
            {
                OutcomeMessage = $"EasyLoad redeem failed,Please try again.",
                OutComeTypeId = -1
            };
        }
    }

    #region Private Utils

    private static RedeemOutcome BuildResult(EasyLoadProviderVoucherResponse voucherResponse)
    {
        switch (voucherResponse.ResponseCode)
        {
            case (int)EasyLoadRedeemResponseCodes.VoucherRedeemSuccessful:
                return new RedeemOutcome
                {
                    OutComeTypeId = 1,
                    OutcomeMessage = "Voucher redeem successfully",
                    VoucherAmount = voucherResponse.Amount / 100,
                    VoucherID = voucherResponse.VoucherId
                };

            case (int)EasyLoadRedeemResponseCodes.InvalidVoucher:
                return new RedeemOutcome
                {
                    OutcomeMessage = "Invalid Voucher.",
                    OutComeTypeId = -1,
                };
            case (int)EasyLoadRedeemResponseCodes.VoucherAlreadyRedeemed:
                return new RedeemOutcome
                {
                    OutcomeMessage = "Voucher Already Redeemed.",
                    OutComeTypeId = -1,
                };
            case (int)EasyLoadRedeemResponseCodes.VoucherExpired:
                return new RedeemOutcome
                {
                    OutcomeMessage = "Voucher Expired.",
                    OutComeTypeId = -1,
                };
            case (int)EasyLoadRedeemResponseCodes.VoucherNotActive:
                return new RedeemOutcome
                {
                    OutcomeMessage = "Voucher not Active.",
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
