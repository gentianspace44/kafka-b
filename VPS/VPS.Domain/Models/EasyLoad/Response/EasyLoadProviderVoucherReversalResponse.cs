using VPS.Domain.Models.EasyLoad.Enums;

namespace VPS.Domain.Models.EasyLoad.Response
{
    public class EasyLoadProviderVoucherReversalResponse
    {
        public string VoucherNumber { get; set; } = string.Empty;
        public int ResponseCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public ReverseVoucherResponse Response { get { return (ReverseVoucherResponse)ResponseCode; } }

    }
}
