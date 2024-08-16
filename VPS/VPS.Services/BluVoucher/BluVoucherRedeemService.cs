using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Reflection;
using VPS.Domain.Models.BluVoucher.Requests;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Common.Request;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Services.Common;
using VPS.Services.Common.Controllers;

namespace VPS.Services.BluVoucher;

public class BluVoucherRedeemService : VoucherRedeemService<Domain.Models.BluVoucher.BluVoucher>
{
    private readonly ILoggerAdapter<BluVoucherRedeemService> _log;
    private readonly IVoucherLogRepository _voucherLogRepository;
    private readonly IRemitBluVoucherService _remitBluVoucherService;
    private readonly IBluVoucherKafkaProducer _bluVoucherKafkaProducer;
    private readonly MetricsHelper _metricsHelper;

    public BluVoucherRedeemService(ILoggerAdapter<BluVoucherRedeemService> log,
                          IVoucherValidationService voucherValidationService,
                          IVoucherLogRepository voucherLogRepository,
                          IHttpContextAccessor httpContextAccessor,
                          IRemitBluVoucherService remitBluVoucherService,
                          IBluVoucherKafkaProducer bluVoucherKafkaProducer,
                          IRedisService redisService,
                          IOptions<RedisSettings> redisSettings,
                          ILoggerAdapter<VoucherRedeemService<Domain.Models.BluVoucher.BluVoucher>> vlog,
                          MetricsHelper metricsHelper,
                          IOptions<DBSettings> dbSettings,
                          IVoucherProviderService voucherProviderService)
        : base(vlog, voucherValidationService, voucherLogRepository, httpContextAccessor, redisService, redisSettings, metricsHelper, dbSettings, voucherProviderService)
    {
        this._log = log ?? throw new System.ArgumentNullException(nameof(log));
        this._voucherLogRepository = voucherLogRepository ?? throw new ArgumentNullException(nameof(voucherLogRepository));
        this._remitBluVoucherService = remitBluVoucherService ?? throw new ArgumentNullException(nameof(remitBluVoucherService));
        this._bluVoucherKafkaProducer = bluVoucherKafkaProducer ?? throw new ArgumentNullException(nameof(bluVoucherKafkaProducer));
        this._metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
    }

    /// <summary>
    /// PerformRedeem is the base method called from the endpoint of BlueVoucher
    /// This method calls the base method PerformRedeem on the common logic VoucherRedeemService
    /// </summary>
    /// <param name="voucherRedeemRequest">Request object.</param>
    /// <returns>Return ServiceResponse.</returns>
    public async Task<ServiceResponse> PerformRedeem(BluVoucherRedeemRequest voucherRedeemRequest)
    {
        _metricsHelper.IncBluVoucherRequestCounter(_log);
        return await ProcessRedeemRequest(voucherRedeemRequest, VoucherType.BluVoucher);
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

            var redeemResult = await _remitBluVoucherService.RemitBluVoucher(voucherRedeemRequest.VoucherReference, voucherRedeemRequest.VoucherNumber);

            await _voucherLogRepository.UpdateVoucherLogAPIResponse(
             voucherRedeemRequest.VoucherNumber,
             voucherRedeemRequest.VoucherReference,
             VoucherType.BluVoucher,
             VoucherStatus.Pending,
             redeemResult.VoucherID,
             redeemResult.VoucherAmount,
             redeemResult.Message?? "");

            if (redeemResult.Success)
            {

                _log.LogInformation(voucherRedeemRequest.VoucherNumber, "reference: {voucherId} - Blu voucher redeem success for pin: {voucherNumber}, Client ID {clientId}, GUID: {uniqueReference}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    redeemResult.VoucherID, voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId, voucherRedeemRequest.VoucherReference );

             
                var response = BuildResult(true, "Success", redeemResult.VoucherAmount, redeemResult.VoucherID);

                //Produce on Kafka
                await _bluVoucherKafkaProducer.Produce((BluVoucherRedeemRequest)voucherRedeemRequest, voucherType, response);

                return response;
            }
            else //error while redeeming
            {

                _log.LogInformation(voucherRedeemRequest.VoucherNumber, "Blu voucher redeem error for pin  {voucherNumber}, Client ID {clientId}. Reason: {message}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                     voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId, redeemResult.Message );

                return BuildResult(false, redeemResult.Message ?? "", 0, 0);
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, null, "BluVoucher Redeem Failed: {message} - {voucherNumber} - {clientId}",
                MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                 ex.Message, voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId );

            return new RedeemOutcome
            {
                OutcomeMessage = $"BluVoucher redeem failed, Please try again.",
                OutComeTypeId = -1
            };
        }
    }

    #region Private Utils
    private static RedeemOutcome BuildResult(bool isSuccessful, string errorMessage, decimal voucherAmount, long voucherId)
    {
        return isSuccessful ? new RedeemOutcome { OutComeTypeId = 1, OutcomeMessage = "Voucher redeem successfully", VoucherAmount = voucherAmount, VoucherID = voucherId }
        : new RedeemOutcome { OutComeTypeId = -1, OutcomeMessage = errorMessage };
    }

    #endregion
}
