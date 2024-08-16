using VPS.Domain.Models.BluVoucher.Responses;

namespace VPS.Services.BluVoucher
{
    public interface IRemitBluVoucherService
    {
        Task<BluVoucherProviderResponse> RemitBluVoucher(string reference, string voucherPin);
    }
}
