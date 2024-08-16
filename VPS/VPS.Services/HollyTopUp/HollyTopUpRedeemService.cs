using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Reflection;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Common.Request;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.HollyTopUp;
using VPS.Domain.Models.HollyTopUp.Requests;
using VPS.Domain.Models.HollyTopUp.Response;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Infrastructure.Repository.HollyTopUp;
using VPS.Services.Common;
using VPS.Services.Common.Controllers;

namespace VPS.Services.HollyTopUp;

public class HollyTopUpRedeemService : VoucherRedeemService<HollyTopUpVoucher>
{
    private readonly IHollyTopUpRepository _hollyTopUpRepository;
    private readonly IVoucherLogRepository _voucherLogRepository;
    private readonly ILoggerAdapter<HollyTopUpRedeemService> _log;
    private readonly IHollyTopUpKafkaProducer _hollyTopUpKafkaProducer;
    private readonly MetricsHelper _metricsHelper;

    public HollyTopUpRedeemService(ILoggerAdapter<HollyTopUpRedeemService> log,
                  IVoucherValidationService voucherValidationService,
                  IVoucherLogRepository voucherLogRepository,
                  IHttpContextAccessor httpContextAccessor,
                  IHollyTopUpRepository hollyTopUpRepository,
                  ILoggerAdapter<VoucherRedeemService<HollyTopUpVoucher>> vlog,
                  IRedisService redisService,
                  IOptions<RedisSettings> redisSettings,
                  MetricsHelper metricsHelper,
                  IHollyTopUpKafkaProducer hollyTopUpKafkaProducer,
                  IOptions<DBSettings> dbSettings,
                  IVoucherProviderService voucherProviderService)
        : base(vlog, voucherValidationService, voucherLogRepository, httpContextAccessor, redisService, redisSettings, metricsHelper, dbSettings, voucherProviderService)
    {
        this._log = log ?? throw new ArgumentNullException(nameof(log));
        this._hollyTopUpRepository = hollyTopUpRepository;
        this._voucherLogRepository = voucherLogRepository ?? throw new ArgumentNullException(nameof(voucherLogRepository));
        this._hollyTopUpKafkaProducer = hollyTopUpKafkaProducer ?? throw new ArgumentNullException(nameof(hollyTopUpKafkaProducer));
        this._metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
    }

    /// <summary>
    /// PerformRedeem is the base method called from the endpoint of HollyTopUpVoucher
    /// This method calls the base method PerformRedeem on the common logic VoucherRedeemService
    /// </summary>
    /// <param name="hollyTopUpRedeemRequest">Request object.</param>
    /// <returns>Return ServiceResponse.</returns>
    public async Task<ServiceResponse> PerformRedeem(HollyTopUpRedeemRequest hollyTopUpRedeemRequest)
    {
        _metricsHelper.IncHollyTopUpRequestCounter(_log);
        return await ProcessRedeemRequest(hollyTopUpRedeemRequest, VoucherType.HollyTopUp);
    }

    public override async Task<RedeemOutcome> GetRedeemOutcome(VoucherRedeemRequestBase voucherRedeemRequest, VoucherType voucherType)
    {
        try
        {
            var redeemResult = await _hollyTopUpRepository.RedeemHollyTopUpVoucher(voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId);

            _log.LogInformation(voucherRedeemRequest.VoucherNumber, "reference: {voucherID} - HollyTopup voucher {voucherNumber}. SP Returned values: | {statusID} | {voucherID} | {createDatetime} | {voucherAmount}",
                MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                 redeemResult.VoucherID, voucherRedeemRequest.VoucherNumber, redeemResult.StatusID, redeemResult.VoucherID, redeemResult.CreateDatetime, redeemResult.VoucherAmount);

            await _voucherLogRepository.UpdateVoucherLogAPIResponse(
              voucherRedeemRequest.VoucherNumber,
              voucherRedeemRequest.VoucherReference,
              VoucherType.HollyTopUp,
              VoucherStatus.Pending,
              redeemResult.VoucherID,
              redeemResult.VoucherAmount,
              EnumHelper.GetHollyTopUpRedeemStatusString(redeemResult.StatusID));

            var response = BuildHollyTopUpOutcome(redeemResult);

            if (redeemResult.StatusID != (int)HollyTopUpRedeemStatus.RedeemSuccessful)
            {
                return response;
            }

            if (redeemResult.StatusID == (int)HollyTopUpRedeemStatus.RedeemSuccessful)
            {
                _log.LogInformation(voucherRedeemRequest.VoucherNumber, "reference: {voucherId} - HollyTopUp voucher redeem success for pin: {voucherNumber}, Client ID: {clientId}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    redeemResult.VoucherID, voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId);
                //Produce on Kafka
                await _hollyTopUpKafkaProducer.Produce((HollyTopUpRedeemRequest)voucherRedeemRequest, voucherType, response);
            }

            return response;

        }
        catch (Exception ex)
        {
            _log.LogError(ex, null, "HollyTopUp Redeem Failed: {voucherNumber} - Client Id: {clientId}",
                MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                 ex.Message, voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId );

            return new RedeemOutcome
            {
                OutcomeMessage = "HollyTopUp redeem failed, Please try again",
                OutComeTypeId = -1,
            };
        }
    }

    private static RedeemOutcome BuildHollyTopUpOutcome(HollyTopUpRedeemResponse hollyTopUpVoucherResult)
    {
        switch (hollyTopUpVoucherResult.StatusID)
        {
            case (int)HollyTopUpRedeemStatus.RedeemSuccessful:
                return new RedeemOutcome
                {
                    OutComeTypeId = 1,
                    OutcomeMessage = "Voucher redeem successfully",
                    VoucherID = hollyTopUpVoucherResult.VoucherID,
                    VoucherAmount = hollyTopUpVoucherResult.VoucherAmount
                };
            case (int)HollyTopUpRedeemStatus.AlreadyRedeemed:
                return new RedeemOutcome
                {
                    OutComeTypeId = -1,
                    OutcomeMessage = "Voucher already redeemed",
                    VoucherID = hollyTopUpVoucherResult.VoucherID,
                    VoucherAmount = hollyTopUpVoucherResult.VoucherAmount
                };
            case (int)HollyTopUpRedeemStatus.InvalidVoucher:
                return new RedeemOutcome
                {
                    OutComeTypeId = -1,
                    OutcomeMessage = "Invalid voucher",
                    VoucherID = hollyTopUpVoucherResult.VoucherID,
                    VoucherAmount = hollyTopUpVoucherResult.VoucherAmount
                };
            case (int)HollyTopUpRedeemStatus.RedeemInProgress:
                return new RedeemOutcome
                {
                    OutComeTypeId = -1,
                    OutcomeMessage = "The Voucher is in the process of being redeemed",
                    VoucherID = hollyTopUpVoucherResult.VoucherID,
                    VoucherAmount = hollyTopUpVoucherResult.VoucherAmount
                };
            case (int)HollyTopUpRedeemStatus.Expired:
                return new RedeemOutcome
                {
                    OutComeTypeId = -1,
                    OutcomeMessage = "Voucher expired",
                    VoucherID = hollyTopUpVoucherResult.VoucherID,
                    VoucherAmount = hollyTopUpVoucherResult.VoucherAmount
                };
            case (int)HollyTopUpRedeemStatus.Suspended:
                return new RedeemOutcome
                {
                    OutComeTypeId = -1,
                    OutcomeMessage = "Voucher suspended",
                    VoucherID = hollyTopUpVoucherResult.VoucherID,
                    VoucherAmount = hollyTopUpVoucherResult.VoucherAmount
                };
            default:
                return new RedeemOutcome
                {
                    OutComeTypeId = -1,
                    OutcomeMessage = "Unknown voucher status",
                    VoucherID = hollyTopUpVoucherResult.VoucherID,
                    VoucherAmount = hollyTopUpVoucherResult.VoucherAmount
                };
        }
    }

}
