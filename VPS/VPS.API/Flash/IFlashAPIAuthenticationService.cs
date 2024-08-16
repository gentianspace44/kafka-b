using VPS.Domain.Models.Flash;

namespace VPS.API.Flash
{
    public interface IFlashApiAuthenticationService
    {
        Task<FlashAccessToken?> GetFlashApiToken(string voucherPin);
        bool IsFlashAccessTokenValid(string flashAccessToken, string voucherPin);
    }
}
