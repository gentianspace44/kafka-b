using Microsoft.AspNetCore.Mvc;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.OTT.Requests;
using VPS.Helpers;
using VPS.Services.OTT;

namespace VPS.Provider.OTT
{
    public static class OTTExtension

    {
        public static void UseVoucherProcessor(this IEndpointRouteBuilder endpoint)
        {
            endpoint.MapGet("/healthcheck", () =>
            {
                return "OTTVoucher is up and running";
            });

            endpoint.MapPost("/Redeem", async ([FromBody] OttVoucherRedeemRequest request, OttVoucherRedeemService _ottVoucherRedeemService) =>
            {
                request.VoucherReference = Guid.NewGuid().ToString();
                request.Provider = EnumHelper.GetEnumDescription(VoucherType.OTT);
                return await _ottVoucherRedeemService.PerformRedeem(request);
            });
        }
    }
}
