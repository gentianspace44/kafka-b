using VPS.Domain.Models.Common;

namespace VPS.Services.Common
{
    public interface IClientBonusService
    {
        Task<decimal> GetBonusAmount(EligibleVoucherBonus? activeBonus, decimal redeemAmount, int clientId);

    }
}
