using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Common.Request;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.EasyLoad.Request;
using VPS.Domain.Models.Enums;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Services.Common;
using VPS.Services.Common.Controllers;
using VPS.Services.Kafka;
using VPS.Test.Common.Models;
using VPS.Test.Common.Setup;

namespace VPS.Test.Common.Mocks
{
    public class VoucherRedeemServiceBaseImplementation : VoucherRedeemService<ProviderVoucher>
    {
        public VoucherRedeemServiceBaseImplementation(
            ILoggerAdapter<VoucherRedeemService<ProviderVoucher>> vlog,
            IVoucherValidationService voucherValidationService,
            IVoucherLogRepository voucherLogRepository,
            IHttpContextAccessor httpContextAccessor,
            IRedisService redisServic,
            IOptions<RedisSettings> redisSettingse,
            MetricsHelper metricsHelper,
            IOptions<DBSettings> dbSettings,
            IVoucherProviderService voucherProviderService)
        : base(vlog, voucherValidationService, voucherLogRepository, httpContextAccessor, redisServic, redisSettingse, metricsHelper, dbSettings, voucherProviderService)
        {

        }
        /// <summary> 
        /// PerformRedeem is the base method called from the endpoint of EasyLoadVoucher
        /// This method calls the base method PerformRedeem on the common logic VoucherRedeemService 
        /// </summary> 
        /// <param name="voucherRedeemRequest">Request object.</param> 
        /// <returns>Return ServiceResponse.</returns>
        public async Task<ServiceResponse> PerformRedeem(EasyLoadVoucherRedeemRequest voucherRedeemRequest)
        {
            return await ProcessRedeemRequest(voucherRedeemRequest, VoucherType.EasyLoad);
        }

        /// <summary> 
        /// GetRedeemOutcome is the overriden method from base here based on the logic of EasyLoadVoucher
        /// </summary> 
        /// <param name="voucherRedeemRequest">Request object.</param> 
        /// <param name="voucherType">Voucher type.</param> 
        /// <returns>Return RedeemOutcome.</returns>
        public override Task<RedeemOutcome> GetRedeemOutcome(VoucherRedeemRequestBase voucherRedeemRequest, VoucherType voucherType)
        {
            //For testing purposes if voucherType is EasyLoad we will return success RedeemOutcome and fail RedeemOutcome for other cases
            if (voucherType == VoucherType.EasyLoad)
            {
                return ArrangeCollection.SuccessRedeemOutCome(100, Convert.ToInt64(voucherRedeemRequest.VoucherNumber));
            }
            else
            {
                return ArrangeCollection.FailVoucherRedeemOutCome();
            }
        }
    }
}
