
using VPS.Domain.Models.HollaMobile.Response;

namespace VPS.Infrastructure.Repository.HollaMobile
{
    public  interface IHollaMobileRepository
    {
        Task<HollaMobileRedeemResponse> RedeemHollaMobileVoucher(string voucherPin, int clientId);

    }
}
