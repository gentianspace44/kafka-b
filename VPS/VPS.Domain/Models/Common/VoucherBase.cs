using VPS.Domain.Models.Enums;

namespace VPS.Domain.Models.Common
{
    public abstract class VoucherBase
    {
        public long ClientId { get; set; }
        public DateTime RedeemDateTime { get; set; }
        public VoucherType VoucherTypeId { get; set; }
        public string VoucherPin { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public long VoucherReferenceId { get; set; }
        public int VoucherStatusId { get; set; }
        public int VoucherTransactionTypeId { get; set; }
        public string SyXPlatform { get; set; } = string.Empty;
        public bool BonusApplied { get; set; }
        public decimal BonusAmount { get; set; }
        public string VoucherReference { get; set; } = string.Empty;

    }
}
