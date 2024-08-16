
using VPS.Domain.Models.Common.Request;
using VPS.Domain.Models.OTT.Responses;

namespace VPS.API.OTT
{
    public interface IOttApiService
    {
        Task<OttProviderVoucherResponse> RemitOTTVoucher(string uniqueReference, VoucherRedeemRequestBase voucherRedeemRequest);
        Task<OttProviderVoucherResponse?> CheckRemitResponse(string uniqueReference);
    }
}
