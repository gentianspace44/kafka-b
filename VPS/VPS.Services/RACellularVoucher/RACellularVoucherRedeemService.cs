using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Reflection;
using VPS.API.RACellularVoucher;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Common.Request;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.RACellularVoucher.Requests;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Services.Common;
using VPS.Services.Common.Controllers;

namespace VPS.Services.RACellularVoucher;

public class RaCellularVoucherRedeemService : VoucherRedeemService<Domain.Models.RACellularVoucher.RaCellularVoucher>
{
    private readonly IVoucherLogRepository _voucherLogRepository;
    private readonly IraCellularVoucherApiService _raCellularVoucherService;
    private readonly ILoggerAdapter<RaCellularVoucherRedeemService> _log;
    private readonly IRaCellularVoucherKafkaProducer _raCellularVoucherKafkaProducer;
    private readonly MetricsHelper _metricsHelper;

    public RaCellularVoucherRedeemService(
                      ILoggerAdapter<RaCellularVoucherRedeemService> log,
                      IVoucherValidationService voucherValidationService,
                      IraCellularVoucherApiService raCellularVoucherService,
                      IVoucherLogRepository voucherLogRepository,
                      IHttpContextAccessor httpContextAccessor,
                      ILoggerAdapter<VoucherRedeemService<Domain.Models.RACellularVoucher.RaCellularVoucher>> vlog,
                      IRedisService redisService,
                      IOptions<RedisSettings> redisSettings,
                      IRaCellularVoucherKafkaProducer raCellularVoucherKafkaProducer,
                      MetricsHelper metricsHelper,
                      IOptions<DBSettings> dbSettings,
                      IVoucherProviderService voucherProviderService) :
        base(vlog, voucherValidationService, voucherLogRepository, httpContextAccessor, redisService, redisSettings, metricsHelper, dbSettings, voucherProviderService)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _voucherLogRepository = voucherLogRepository ?? throw new ArgumentNullException(nameof(voucherLogRepository));
        _raCellularVoucherService = raCellularVoucherService ?? throw new ArgumentNullException(nameof(raCellularVoucherService));
        _raCellularVoucherKafkaProducer = raCellularVoucherKafkaProducer ?? throw new ArgumentNullException(nameof(raCellularVoucherKafkaProducer));
        _metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
    }

    public async Task<ServiceResponse> PerformRedeemAsync(RaCellularVoucherRedeemRequest voucherRedeemRequest)
    {
        _metricsHelper.IncRAVoucherRequestCounter(_log);
        return await ProcessRedeemRequest(voucherRedeemRequest, VoucherType.RACellular);
    }

    public override async Task<RedeemOutcome> GetRedeemOutcome(VoucherRedeemRequestBase voucherRedeemRequest, VoucherType voucherType)
    {
        try
        {
            var redeemResult = await _raCellularVoucherService.RedeemVoucherAsync(voucherRedeemRequest.ClientId.ToString(), voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.DevicePlatform);

            if (!long.TryParse(redeemResult.Reference, out long voucherId))
            {
                _log.LogError(voucherRedeemRequest.VoucherNumber, "Failed to Parse voucherId on RedeemVoucherAsync: {VoucherReference}.",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    redeemResult.Reference);
                throw new FormatException("Invalid voucherId");
            }


            await _voucherLogRepository.UpdateVoucherLogAPIResponse(
            voucherRedeemRequest.VoucherNumber,
            voucherRedeemRequest.VoucherReference,
            VoucherType.RACellular,
            VoucherStatus.Pending,
            voucherId,
            redeemResult.Amount,
            redeemResult.MsgID);

            if (!redeemResult.HasFault)
            {

                _log.LogInformation(voucherRedeemRequest.VoucherNumber, "reference: {voucherId} - R&A voucher redeem success for pin: {voucherNumber}, Client ID {clientId}, GUID: {uniqueReference}",
                  MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                   
                        voucherId,
                        voucherRedeemRequest.VoucherNumber,
                        voucherRedeemRequest.ClientId,
                        voucherRedeemRequest.VoucherReference
                    );

                var response = BuildResult(true, "Success", redeemResult.Amount, voucherId);

                await _raCellularVoucherKafkaProducer.ProduceAsync((RaCellularVoucherRedeemRequest)voucherRedeemRequest, voucherType, response);

                return response;
            }
            else
            {
                _log.LogInformation(voucherRedeemRequest.VoucherNumber, "R&A voucher redeem error for pin  {voucherNumber}, Client ID {clientId}. Reason: {message}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                   
                        voucherRedeemRequest.VoucherNumber,
                        voucherRedeemRequest.ClientId,
                        redeemResult.FaultMsg
                    );

                return BuildResult(false, redeemResult.FaultMsg, 0, 0);
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, null, "R&A Redeem Failed: {message} - {voucherNumber} - {clientId}",
                MethodBase.GetCurrentMethod()?.Name ?? string.Empty,                    
                        ex.Message,
                        voucherRedeemRequest.VoucherNumber,
                        voucherRedeemRequest.ClientId
                   );

            return new RedeemOutcome
            {
                OutcomeMessage = $"R&A redeem failed, Please try again.",
                OutComeTypeId = -1
            };
        }
    }

    private static RedeemOutcome BuildResult(bool isSuccessful, string errorMessage, decimal voucherAmount, long voucherId)
    {
        return isSuccessful ? new RedeemOutcome
        {
            OutComeTypeId = 1,
            OutcomeMessage = "Voucher was redeemed successfully",
            VoucherAmount = voucherAmount,
            VoucherID = voucherId
        }
        : new RedeemOutcome
        {
            OutComeTypeId = -1,
            OutcomeMessage = errorMessage,
            VoucherAmount = voucherAmount,
            VoucherID = voucherId
        };
    }
}
