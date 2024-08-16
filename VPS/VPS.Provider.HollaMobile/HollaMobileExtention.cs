using VPS.Domain.Models.HollaMobile.Requests;
using VPS.Domain.Models.Enums;
using VPS.Helpers;
using VPS.Services.HollaMobile;
using System.Web.Http;


namespace VPS.Provider.HollaMobile
{
    public static class HollaMobileExtention
    {
        public static void UseAirtimeProcessor(this IEndpointRouteBuilder endpoint)
        {
            endpoint.MapGet("/healthCheck", () =>
            {
                return "HollaMobile Airtime is up and running!";
            });


            endpoint.MapPost("/Redeem", async ([FromBody] HollaMobileRedeemRequest request, HollaMobileRedeemService _hollaMobileRedeemService) =>
            {
                request.Provider = EnumHelper.GetEnumDescription(VoucherType.HollaMobile);
                return await _hollaMobileRedeemService.PerformRedeem(request);
            });

        }
    }
}
