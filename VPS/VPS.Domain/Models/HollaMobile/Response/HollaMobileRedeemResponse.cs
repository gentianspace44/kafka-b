namespace VPS.Domain.Models.HollaMobile.Response
{
    public class HollaMobileRedeemResponse
    {
        public int VoucherID { get; set; }
        public decimal VoucherAmount { get; set; }
        public DateTime CreateDatetime { get; set; }
        public int StatusID { get; set; }
    }
}
