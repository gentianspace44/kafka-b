using VPS.Domain.Models.Flash.Requests;
using VPS.Domain.Models.Flash.Responses;

namespace VPS.API.Flash
{
    public interface IFlashApiService
    {
        Task<FlashRedeemResponse> RedeemVoucher(FlashRequest flashRequest);
    }
}
