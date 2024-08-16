using Newtonsoft.Json;
using VPS.Domain.Models.Common.Request;
using VPS.Domain.Models.Flash.Requests;

namespace VPS.API.Flash.Processes
{
    public static class FlashRedeemProcess
    {
        public static List<KeyValuePair<string, string>> CreateFlashRedeemHeaders(string flashAccessToken)
        {
            var headers = new List<KeyValuePair<string, string>>
            {
                    new("Authorization", $"Bearer {flashAccessToken}"),
                    new("Accept", $"application/json")

                };
            return headers;
        }

        public static string CreateFlashRedeemRequest(FlashRequest redeemRequest)
        {
            return JsonConvert.SerializeObject(redeemRequest);   
        }

        public static FlashRequest CreateFlashRedeemRequest(VoucherRedeemRequestBase voucherRedeemRequest, string flashAccountNumber)
        {
            return new FlashRequest()
            {
                accountNumber = flashAccountNumber,
                amount = 0,
                pin = voucherRedeemRequest.VoucherNumber,
                reference = voucherRedeemRequest.VoucherReference,
                metadata = new Metadata { clientId = voucherRedeemRequest.ClientId.ToString() }
            };
        }
    }
}
