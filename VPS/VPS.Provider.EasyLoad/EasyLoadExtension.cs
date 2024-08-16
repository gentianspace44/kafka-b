using Microsoft.AspNetCore.Mvc;
using VPS.API.Common;
using VPS.Domain.Models.EasyLoad.Request;
using VPS.Domain.Models.Enums;
using VPS.Helpers;
using VPS.Services.EasyLoad;

namespace VPS.Provider.EasyLoad
{
    public static class EasyLoadExtension
    {
        public static void UseVoucherProcessor(this IEndpointRouteBuilder endpoint)
        {
            endpoint.MapGet("/healthcheck", () =>
            {
                return "Easyload is up and running!";
            });

            endpoint.MapPost("/Redeem", async ([FromBody] EasyLoadVoucherRedeemRequest request, EasyLoadVoucherRedeemService _easyLoadVoucherRedeemService) =>
            {
                request.VoucherReference = Guid.NewGuid().ToString();
                request.Provider = EnumHelper.GetEnumDescription(VoucherType.EasyLoad);
                return await _easyLoadVoucherRedeemService.PerformRedeem(request);
            });
        }
    }
}
