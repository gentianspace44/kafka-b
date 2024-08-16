using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Reflection;
using VPS.API.OTT;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Common.Request;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.OTT.Requests;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Services.Common;
using VPS.Services.Common.Controllers;

namespace VPS.Services.OTT;

public class OttVoucherRedeemService : VoucherRedeemService<Domain.Models.OTT.OttVouchers>
{
    private readonly ILoggerAdapter<OttVoucherRedeemService> _log;
    private readonly IVoucherLogRepository _voucherLogRepository;
    private readonly IOttApiService _oTTAPIService;
    private readonly IOttKafkaProducer _oTTKafkaProducer;
    private readonly MetricsHelper _metricsHelper;

    public OttVoucherRedeemService(ILoggerAdapter<OttVoucherRedeemService> log,
                  IVoucherValidationService voucherValidationService,
                  IOttApiService oTTAPIService,
                  IVoucherLogRepository voucherLogRepository,
                  IHttpContextAccessor httpContextAccessor,
                  IOttKafkaProducer oTTKafkaProducer,
                  IRedisService redisService,
                  IOptions<RedisSettings> redisSettings,
                  ILoggerAdapter<VoucherRedeemService<Domain.Models.OTT.OttVouchers>> vlog,
                  MetricsHelper metricsHelper,
                  IOptions<DBSettings> dbSettings,
                  IVoucherProviderService voucherProviderService)
        : base(vlog, voucherValidationService, voucherLogRepository, httpContextAccessor, redisService, redisSettings, metricsHelper, dbSettings, voucherProviderService)
    {

        this._log = log ?? throw new System.ArgumentNullException(nameof(log));
        this._voucherLogRepository = voucherLogRepository ?? throw new ArgumentNullException(nameof(voucherLogRepository));
        this._oTTAPIService = oTTAPIService ?? throw new ArgumentNullException(nameof(oTTAPIService));
        this._oTTKafkaProducer = oTTKafkaProducer ?? throw new ArgumentNullException(nameof(oTTKafkaProducer));
        this._metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
    }
    /// <summary>
    /// PerformRedeem is the base method called from the endpoint of OTTVoucher
    /// This method calls the base method PerformRedeem on the common logic VoucherRedeemService
    /// </summary>
    /// <param name="oTTVoucherRedeemRequest">Request object.</param>
    /// <returns>Return ServiceResponse.</returns>
    public async Task<ServiceResponse> PerformRedeem(OttVoucherRedeemRequest oTTVoucherRedeemRequest)
    {
        _metricsHelper.IncOTTRequestCounter(_log);
        return await ProcessRedeemRequest(oTTVoucherRedeemRequest, VoucherType.OTT);
    }

    public override async Task<RedeemOutcome> GetRedeemOutcome(VoucherRedeemRequestBase voucherRedeemRequest, VoucherType voucherType)
    {
        try
        {

            var redeemResult = await _oTTAPIService.RemitOTTVoucher(voucherRedeemRequest.VoucherReference, voucherRedeemRequest);

            decimal voucherAmount = 0;

            if (redeemResult is not null &&
                !string.IsNullOrEmpty(redeemResult.VoucherAmount.ToString()) &&
                voucherType == VoucherType.OTT)
            {
                Decimal.TryParse(redeemResult.VoucherAmount.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out voucherAmount);
            }


            if (redeemResult?.ErrorCode == 1)
            {
                var checkRemitVoucherResponse = await _oTTAPIService.CheckRemitResponse(voucherRedeemRequest.VoucherReference);
                if (checkRemitVoucherResponse?.ErrorCode == 0 && checkRemitVoucherResponse.Success)
                {
                    redeemResult.ErrorCode = 0;
                    redeemResult.Success = true;
                }
            }

            await _voucherLogRepository.UpdateVoucherLogAPIResponse(
             voucherRedeemRequest.VoucherNumber,
             voucherRedeemRequest.VoucherReference,
             VoucherType.OTT,
             VoucherStatus.Pending,
             redeemResult == null ? 0 : redeemResult.VoucherID,
             voucherAmount,
             redeemResult?.Message ?? "");

            if (redeemResult?.Success == true)
            {

                _log.LogInformation(voucherRedeemRequest.VoucherNumber, "reference: {voucherId} - OTT voucher redeem success for pin: {voucherNumber}, Client ID {ClientId}, GUID: {uniqueReference}",
                   MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    redeemResult.VoucherID, voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId, voucherRedeemRequest.VoucherReference);

                var response = BuildResult(true, "Success", voucherAmount, redeemResult.VoucherID);

                //Produce on Kafka
                await _oTTKafkaProducer.Produce((OttVoucherRedeemRequest)voucherRedeemRequest, voucherType, response);

                return response;
            }
            else //error while redeeming
            {
                _log.LogInformation(null, "OTT voucher redeem error for pin {voucherNumber}, Client ID {clientId}. Reason: {message}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId, redeemResult?.Message);

                return BuildResult(false, redeemResult == null ? "OTT redeem failed,Please try again." : redeemResult.Message, 0, 0);
            }

        }
        catch (Exception ex)
        {
            _log.LogError(ex, null, "OTTVoucher Redeem Failed: {message} - {voucherNumber} - {clientId}",
                MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                ex.Message, voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId);

            return new RedeemOutcome
            {
                OutcomeMessage = $"OTT redeem failed,Please try again.",
                OutComeTypeId = -1
            };
        }
    }
    private static RedeemOutcome BuildResult(bool isSuccessful, string errorMessage, decimal voucherAmount, long voucherId)
    {
        return isSuccessful ? new RedeemOutcome { OutComeTypeId = 1, OutcomeMessage = "Voucher redeem successfully", VoucherAmount = voucherAmount, VoucherID = voucherId } : new RedeemOutcome { OutComeTypeId = -1, OutcomeMessage = errorMessage };
    }
}
