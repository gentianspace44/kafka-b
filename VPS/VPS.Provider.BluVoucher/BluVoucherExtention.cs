using Microsoft.AspNetCore.Mvc;
using VPS.Domain.Models.BluVoucher.Requests;
using VPS.Domain.Models.Enums;
using VPS.Helpers;
using VPS.Services.BluVoucher;

namespace VPS.Provider.BluVoucher
{
    public static class BluVoucherExtention
    {
        public static void UseVoucherProcessor(this IEndpointRouteBuilder endpoint)
        {
            endpoint.MapGet("/healthcheck", () =>
            {
                return "BluVoucher is up and running!";
            });

            endpoint.MapPost("/Redeem", async ([FromBody] BluVoucherRedeemRequest request, BluVoucherRedeemService _bluVoucherRedeemService) =>
            {
                request.VoucherReference = Guid.NewGuid().ToString();
                request.Provider = EnumHelper.GetEnumDescription(VoucherType.BluVoucher);
                return await _bluVoucherRedeemService.PerformRedeem(request);
            });
        }
    }
}
