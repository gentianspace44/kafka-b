using Microsoft.AspNetCore.Mvc;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.RACellularVoucher.Requests;
using VPS.Helpers;
using VPS.Services.RACellularVoucher;

namespace VPS.Provider.RACellularVoucher;

public static class RACellularVoucherExtension
{
    public static void UseVoucherProcessor(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("/healthcheck", () =>
        {
            return "R & A Cellular Voucher is up and running";
        });

        endpoint.MapPost("/Redeem",
            async ([FromBody] RaCellularVoucherRedeemRequest request,
            RaCellularVoucherRedeemService _raCellularVoucherRedeemService) =>
            {
                request.VoucherReference = Guid.NewGuid().ToString();
                request.Provider = EnumHelper.GetEnumDescription(VoucherType.RACellular);
                return await _raCellularVoucherRedeemService.PerformRedeemAsync(request);
        });
    }
}