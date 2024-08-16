using VPS.Domain.Models.BluVoucher.Requests;
using VPS.Domain.Models.BluVoucher.Responses;

namespace VPS.Services.BluVoucher
{
    public interface IAirtimeAuthentication
    {
        Task<AirtimeAuthenticationResponse?> Authenticate(BluVoucherProviderAuthenticationRequest authenticationRequestModel);
    }
}
