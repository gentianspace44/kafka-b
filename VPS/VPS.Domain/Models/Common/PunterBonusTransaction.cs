
namespace VPS.Domain.Models.Common
{
    public class PunterBonusTransaction
    {
        public int BonusTransactionID { get; set; }
        public int BonusID { get; set; }
        public int PunterID { get; set; }
        public decimal RedeemTotal { get; set; }
        public decimal payedOut { get; set; }

    }
}
