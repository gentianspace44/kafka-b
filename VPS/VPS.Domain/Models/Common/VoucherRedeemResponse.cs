namespace VPS.Domain.Models.Common
{
    public class VoucherRedeemResponse
    {
        public string Message { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public decimal Bonus { get; set; }

        public decimal BalanceAvailable { get; set; }

    }
}
