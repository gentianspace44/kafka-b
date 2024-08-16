using VPS.Domain.Models.Common;

namespace VPS.Infrastructure.Repository.Common
{
    public interface IEligibleBonusRepository
    {
        Task<ActiveBonus?> GetActiveBonus();
        Task<ActiveBonus?> GetBonusForVoucher(DateTime redeemDateTime);
        Task<EligibleVoucherBonus?> GetEligibleVoucherBonus(DateTime redeemDateTime, int clientId);
        Task<PunterBonusTransaction?> GetPunterBonusTotals(int bonusId, int punterId);
        Task InsertPunterBonusTransaction(int punterId, int bonusId, decimal redeemTotal, decimal payedOut);
    }
}
