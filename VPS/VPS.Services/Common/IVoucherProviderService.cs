using VPS.Domain.Models.VRW.Voucher;

namespace VPS.Services.Common
{
    public interface IVoucherProviderService
    {
        Task<List<VoucherServiceEnabler>?> GetProviders();
        Task<List<VoucherServiceEnabler>?> SetProviders();
    }
}
