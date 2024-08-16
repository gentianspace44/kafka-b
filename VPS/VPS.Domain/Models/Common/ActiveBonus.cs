
namespace VPS.Domain.Models.Common
{
    public class ActiveBonus
    {
        public int BonusID { get; set; }
        public string? Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Percentage { get; set; }
        public int MaxRedeemAmount { get; set; }
        public bool Active { get; set; }
        public bool ForEFT { get; set; }
        public bool ForTUV { get; set; }
        public bool ForPP { get; set; }

    }
}
