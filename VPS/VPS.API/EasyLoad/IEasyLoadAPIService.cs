using VPS.Domain.Models.EasyLoad.Response;

namespace VPS.API.EasyLoad
{
    public interface IEasyLoadApiService
    {
        Task<EasyLoadProviderVoucherResponse?> RedeemVoucher(string voucherPin, int clientId);
    }
}
