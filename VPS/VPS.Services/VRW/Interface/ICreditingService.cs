using VPS.Domain.Models.VRW.Voucher;

namespace VPS.Services.VRW.Interface
{
    public interface ICreditingService
    {
        Task<VrwViewModel> SubmitVoucher(VrwViewModel model, bool useSyxCreditingService = false);

    }
}
