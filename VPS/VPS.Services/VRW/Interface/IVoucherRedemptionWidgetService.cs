using VPS.Domain.Models.VRW.Voucher;

namespace VPS.Services.VRW.Interface
{
    public interface IVoucherRedemptionWidgetService
    {
        Task<VrwViewModel> InitializeViewModel(string clientId, string devicePlatform);

        Task<VrwViewModel> RedeemVoucher(VrwViewModel model);
    }
}
