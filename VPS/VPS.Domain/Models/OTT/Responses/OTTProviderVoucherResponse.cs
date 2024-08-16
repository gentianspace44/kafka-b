
namespace VPS.Domain.Models.OTT.Responses
{
    public class OttProviderVoucherResponse
    {
        public long VoucherID { get; set; }
        public decimal VoucherAmount { get; set; }
        public bool Success { get; set; }
        public int ErrorCode { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
