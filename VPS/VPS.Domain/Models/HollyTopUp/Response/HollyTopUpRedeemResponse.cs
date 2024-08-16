namespace VPS.Domain.Models.HollyTopUp.Response
{
    public class HollyTopUpRedeemResponse
    {
        public int VoucherID { get; set; }
        public decimal VoucherAmount { get; set; }
        public DateTime CreateDatetime { get; set; }
        public int StatusID { get; set; }

    }
}
