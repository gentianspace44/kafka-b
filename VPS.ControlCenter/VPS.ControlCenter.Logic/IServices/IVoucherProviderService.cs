using VPS.ControlCenter.Logic.Models;

namespace VPS.ControlCenter.Logic.IServices
{
    public interface IVoucherProviderService
    {
        Task<int> CreateVoucherProvider(VoucherProviderModel model);
        Task<bool> UpdateVoucherProvider(VoucherProviderModel model);
        Task<List<VoucherProviderModel>> GetAll();
        Task<bool> UpdateMultipleVoucherProviders(List<VoucherProviderModel> models);
        Task<List<VoucherProviderModel>> SetOrUpdateRedis();

        Task<RedemptionStatusResponse> VerifyRedemptionStatus(RedemptionStatusRequest redemptionStatusRequest);
    }
}
