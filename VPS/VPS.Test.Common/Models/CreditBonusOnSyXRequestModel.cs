using VPS.Domain.Models.Common;

namespace VPS.Test.Common.Models
{
    public class CreditBonusOnSyXRequestModel
    {
        public string SessionToken { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public int UserId { get; set; }
        public string Platform { get; set; } = string.Empty;
        public long VoucherId { get; set; }
        public decimal VoucherAmount { get; set; }
        public EligibleVoucherBonus? EligibleVoucherBonus { get; set; }
        public string VoucherPrefix { get; set; } = string.Empty;
    }
}
