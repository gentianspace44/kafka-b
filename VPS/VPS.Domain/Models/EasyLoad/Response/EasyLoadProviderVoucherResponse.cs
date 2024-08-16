using Newtonsoft.Json;

namespace VPS.Domain.Models.EasyLoad.Response
{
    public class EasyLoadProviderVoucherResponse
    {
        [JsonProperty("vouchernumber")]
        public string VoucherNumber { get; set; } = string.Empty;

        [JsonProperty("voucherid")]
        public long VoucherId { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("responsecode")]
        public int ResponseCode { get; set; }

        [JsonProperty("usermessage")]
        public string ResponseMessage { get; set; } = string.Empty;
    }
}
