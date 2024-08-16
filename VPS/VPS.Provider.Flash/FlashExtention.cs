using Microsoft.AspNetCore.Mvc;
using VPS.Domain.Models.BluVoucher.Requests;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.Flash.Requests;
using VPS.Helpers;
using VPS.Services.BluVoucher;
using VPS.Services.Flash;

namespace VPS.Provider.Flash
{
    public static class FlashExtention
    {
        public static void UseVoucherProcessor(this IEndpointRouteBuilder endpoint)
        {
            endpoint.MapGet("/healthcheck", () =>
            {
                return "Flash is up and running!";
            });

            endpoint.MapPost("/Redeem", async ([FromBody] FlashRedeemRequest request, FlashRedeemService _flashRedeemService) =>
            {
                request.VoucherReference = Guid.NewGuid().ToString();
                request.Provider = EnumHelper.GetEnumDescription(VoucherType.Flash);
                return await _flashRedeemService.PerformRedeem(request);
            });
        }
    }
}
