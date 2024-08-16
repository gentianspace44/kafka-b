using Microsoft.AspNetCore.Mvc;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.HollyTopUp.Requests;
using VPS.Helpers;
using VPS.Services.HollyTopUp;

namespace VPS.Provider.HollyTopUp
{
    public static class HollyTopUpVoucherExtention
    {
        public static void UseVoucherProcessor(this IEndpointRouteBuilder endpoint)
        {
            endpoint.MapGet("/healthcheck", () =>
            {
                return "HollyTopUp Voucher is up and running!";
            });

            endpoint.MapPost("/Redeem", async ([FromBody] HollyTopUpRedeemRequest request, HollyTopUpRedeemService _hollyTopUpRedeemService) =>
            {
                request.VoucherReference = Guid.NewGuid().ToString();
                request.Provider = EnumHelper.GetEnumDescription(VoucherType.HollyTopUp);
                return await _hollyTopUpRedeemService.PerformRedeem(request);
            });
        }
    }
}
