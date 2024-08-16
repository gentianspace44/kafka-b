namespace VPS.Domain.Models.Common
{
    public class SyXCreditOutcome : OutComeBase
    {
        public EligibleVoucherBonus? VoucherBonus { get; set; }

        public decimal RedeemedBonusAmount { get; set; }

        public decimal BalanceAvailable { get; set; }

    }
}
