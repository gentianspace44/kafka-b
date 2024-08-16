namespace VPS.Domain.Models.Common
{
    public class RedeemOutcome : OutComeBase
    {
        public long VoucherID { get; set; }

        public decimal VoucherAmount { get; set; }

    }
}
