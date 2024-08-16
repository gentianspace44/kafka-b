using VPS.Domain.Models.Common;
using VPS.Domain.Models.Common.Response;
using VPS.Domain.Models.Enums;

namespace VPS.Services.Common
{
    public interface IClientBalanceService
    {
        Task<SyXCreditOutcome> CreditOnSyX(int clientId, string platform, long voucherId, string voucherPin, decimal amount,
            string voucherPrefix, VoucherType voucherType, string uniqueReference, string logStoreProcedureName);
        Task<SyXCreditOutcome> CreditBonusOnSyX(int clientId, string platform, long voucherId, decimal voucherAmount, EligibleVoucherBonus? eligibleVoucherBonus, string voucherPrefix);
        Task<ApiVoucherExistsResponse?> CheckVoucherExistsOnSyx(long clientId, string reference);

    }
}
