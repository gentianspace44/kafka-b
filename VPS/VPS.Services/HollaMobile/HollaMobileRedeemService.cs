using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Reflection;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Common.Request;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.HollaMobile;
using VPS.Domain.Models.HollaMobile.Requests;
using VPS.Domain.Models.HollaMobile.Response;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Infrastructure.Repository.HollaMobile;
using VPS.Services.Common;
using VPS.Services.Common.Controllers;

namespace VPS.Services.HollaMobile
{
    public class HollaMobileRedeemService : VoucherRedeemService<HollaMobileVoucher>
    {
        private readonly IHollaMobileRepository _hollaMobileTopUpRepository;
        private readonly IVoucherLogRepository _voucherLogRepository;
        private readonly ILoggerAdapter<HollaMobileRedeemService> _log;
        private readonly MetricsHelper _metricsHelper;

        public HollaMobileRedeemService(ILoggerAdapter<HollaMobileRedeemService> log,
                      IVoucherValidationService voucherValidationService,
                      IVoucherLogRepository voucherLogRepository,
                      IHttpContextAccessor httpContextAccessor,
                      IHollaMobileRepository hollaMobileTopUpRepository,
                      ILoggerAdapter<VoucherRedeemService<HollaMobileVoucher>> vlog,
                      IRedisService redisService,
                      IOptions<RedisSettings> redisSettings,
                      MetricsHelper metricsHelper,
                      IOptions<DBSettings> dbSettings,
                      IVoucherProviderService voucherProviderService)
            : base(vlog, voucherValidationService, voucherLogRepository, httpContextAccessor, redisService, redisSettings, metricsHelper, dbSettings, voucherProviderService)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _hollaMobileTopUpRepository = hollaMobileTopUpRepository;
            _voucherLogRepository = voucherLogRepository ?? throw new ArgumentNullException(nameof(voucherLogRepository));
            _metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
        }

        public async Task<ServiceResponse> PerformRedeem(HollaMobileRedeemRequest hollaMobileRedeemRequest)
        {
            _metricsHelper.IncHollaMobileRequestCounter(_log);
            return await ProcessRedeemRequest(hollaMobileRedeemRequest, VoucherType.HollaMobile);
        }

        public override async Task<RedeemOutcome> GetRedeemOutcome(VoucherRedeemRequestBase voucherRedeemRequest, VoucherType voucherType)
        {
           try
            {
                var redeemResult = await _hollaMobileTopUpRepository.RedeemHollaMobileVoucher(voucherRedeemRequest.VoucherNumber,voucherRedeemRequest.ClientId);
                _log.LogInformation(voucherRedeemRequest.VoucherNumber, "reference: {voucherID} - HollaMobile voucher {voucherNumber}. SP Returned values: | {statusID} | {voucherID} | {createDatetime} | {voucherAmount}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                 redeemResult.VoucherID, voucherRedeemRequest.VoucherNumber, redeemResult.StatusID, redeemResult.VoucherID, redeemResult.CreateDatetime, redeemResult.VoucherAmount);

                await _voucherLogRepository.UpdateVoucherLogAPIResponse(
                              voucherRedeemRequest.VoucherNumber,
                              voucherRedeemRequest.VoucherReference,
                              VoucherType.HollaMobile,
                              VoucherStatus.Pending,
                              redeemResult.VoucherID,
                              redeemResult.VoucherAmount,
                              EnumHelper.GetHollyTopUpRedeemStatusString(redeemResult.StatusID));

                var response = BuildHollaMobileUpOutcome(redeemResult);

                if (redeemResult.StatusID != (int)HollaMobileRedeemStatus.RedeemSuccessful)
                {
                    return response;
                }

                if (redeemResult.StatusID == (int)HollaMobileRedeemStatus.RedeemSuccessful)
                {
                    _log.LogInformation(voucherRedeemRequest.VoucherNumber, "reference: {voucherId} - HollyTopUp voucher redeem success for pin: {voucherNumber}, Client ID: {clientId}",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                         redeemResult.VoucherID, voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId );
                    _metricsHelper.IncHollaMobileUpSuccessCounter(_log);
                }
                return response;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, null, "HollaMobile Redeem Failed: {voucherNumber} - Client Id: {clientId}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                     ex.Message, voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId);

                return new RedeemOutcome
                {
                    OutcomeMessage = "HollaMobile redeem failed, Please try again",
                    OutComeTypeId = -1,
                };
            }
        }

        private static RedeemOutcome BuildHollaMobileUpOutcome(HollaMobileRedeemResponse hollaMobileTopUpVoucherResult)
        {
            switch (hollaMobileTopUpVoucherResult.StatusID)
            {
                case (int)HollaMobileRedeemStatus.RedeemSuccessful:
                    return new RedeemOutcome
                    {
                        OutComeTypeId = 1,
                        OutcomeMessage = "Airtime redeem successfully",
                        VoucherID = hollaMobileTopUpVoucherResult.VoucherID,
                        VoucherAmount = hollaMobileTopUpVoucherResult.VoucherAmount
                    };
                case (int)HollaMobileRedeemStatus.AlreadyRedeemed:
                    return new RedeemOutcome
                    {
                        OutComeTypeId = -1,
                        OutcomeMessage = "Airtime already redeemed",
                        VoucherID = hollaMobileTopUpVoucherResult.VoucherID,
                        VoucherAmount = hollaMobileTopUpVoucherResult.VoucherAmount
                    };
                case (int)HollaMobileRedeemStatus.InvalidAirtime:
                    return new RedeemOutcome
                    {
                        OutComeTypeId = -1,
                        OutcomeMessage = "Invalid Airtime",
                        VoucherID = hollaMobileTopUpVoucherResult.VoucherID,
                        VoucherAmount = hollaMobileTopUpVoucherResult.VoucherAmount
                    };
                case (int)HollaMobileRedeemStatus.RedeemInProgress:
                    return new RedeemOutcome
                    {
                        OutComeTypeId = -1,
                        OutcomeMessage = "The Airtime is in the process of being redeemed",
                        VoucherID = hollaMobileTopUpVoucherResult.VoucherID,
                        VoucherAmount = hollaMobileTopUpVoucherResult.VoucherAmount
                    };
                case (int)HollaMobileRedeemStatus.Expired:
                    return new RedeemOutcome
                    {
                        OutComeTypeId = -1,
                        OutcomeMessage = "Airtime expired",
                        VoucherID = hollaMobileTopUpVoucherResult.VoucherID,
                        VoucherAmount = hollaMobileTopUpVoucherResult.VoucherAmount
                    };      
                case (int)HollaMobileRedeemStatus.Suspended:
                    return new RedeemOutcome
                    {
                        OutComeTypeId = -1,
                        OutcomeMessage = "Airtime suspended",
                        VoucherID = hollaMobileTopUpVoucherResult.VoucherID,
                        VoucherAmount = hollaMobileTopUpVoucherResult.VoucherAmount
                    };
                default:
                    return new RedeemOutcome
                    {
                        OutComeTypeId = -1,
                        OutcomeMessage = "Unknown Airtime status",
                        VoucherID = hollaMobileTopUpVoucherResult.VoucherID,
                        VoucherAmount = hollaMobileTopUpVoucherResult.VoucherAmount
                    };
            }
        }
    }
}
