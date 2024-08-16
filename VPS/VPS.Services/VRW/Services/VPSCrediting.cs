using VPS.API.CreditingService;
using VPS.Domain.Models.VRW.Voucher;
using VPS.Domain.Models.VRW.VPSCrediting;
using VPS.Domain.Models.VRWEnumHelpers;
using VPS.Helpers.Logging;
using VPS.Services.VRW.Interface;

namespace VPS.Services.VRW.Services
{
    public class VpsCrediting : ICreditingService
    {


        private readonly CreditingService _syxCreditService;
        private readonly ILoggerAdapter<VpsCrediting> _log;

        public VpsCrediting(CreditingService syxCreditingService, ILoggerAdapter<VpsCrediting> log)
        {
            _syxCreditService = syxCreditingService;
            _log = log;
        }

        public async Task<VrwViewModel> SubmitVoucher(VrwViewModel model, bool useSyxCreditingService = false)
        {
            try
            {
                var endpoint = model.SyxVoucherCreditingEndPoint;

                var body = new VpsCreditingServiceRequest
                {
                    ClientId = model.ClientId,
                    DevicePlatform = model.DevicePlatform,
                    VoucherNumber = model.VoucherNumber,
                   
                };

                model = await _syxCreditService.ProcessCredit(endpoint, body, model, useSyxCreditingService);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = StringConstants.TRYAGAIN_ERROR_MESSAGE;
                _log.LogError(ex, model.VoucherNumber, ex.Message);
            }

            return model;
        }
    }

}
