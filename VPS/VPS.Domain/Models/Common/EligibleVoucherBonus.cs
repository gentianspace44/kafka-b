namespace VPS.Domain.Models.Common
{
    public class EligibleVoucherBonus
    {
        public int Status { get; set; }
        public string? Message { get; set; }
        public int BonusID { get; set; }
        public string? Name { get; set; }
        public DateTime startDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Percentage { get; set; }
        public int MaxRedeemAmount { get; set; }
        public bool Active { get; set; }
        public bool ForTUV { get; set; }

    }
}
