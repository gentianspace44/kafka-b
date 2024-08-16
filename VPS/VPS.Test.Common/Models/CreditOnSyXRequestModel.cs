using VPS.Domain.Models.Enums;

namespace VPS.Test.Common.Models
{
    public class CreditOnSyXRequestModel
    {
        public string SessionToken { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public int UserId { get; set; }
        public string Platform { get; set; } = string.Empty;
        public long VoucherId { get; set; }
        public string VoucherPin { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string VoucherPrefix { get; set; } = string.Empty;
        public VoucherType VoucherType { get; set; }

        public string VoucherReference { get; set; } = string.Empty;
    }
}
