using VPS.Domain.Models.HollyTopUp.Response;

namespace VPS.Infrastructure.Repository.HollyTopUp
{
    public interface IHollyTopUpRepository
    {
        Task<HollyTopUpRedeemResponse> RedeemHollyTopUpVoucher(string voucherPin, int clientId);
    }
}
