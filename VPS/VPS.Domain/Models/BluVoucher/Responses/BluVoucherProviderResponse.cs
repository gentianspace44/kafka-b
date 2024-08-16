namespace VPS.Domain.Models.BluVoucher.Responses
{
    public class BluVoucherProviderResponse
    {
        public bool Success { get; set; }
        public int ErrorCode { get; set; }
        public string? Message { get; set; }
        public long VoucherID { get; set; }
        public decimal VoucherAmount { get; set; }

    }
}
