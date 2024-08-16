using System.Reflection;
using VPS.Domain.Models.Common;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;

namespace VPS.Services.Common
{
    public class ClientBonusService : IClientBonusService
    {
        private readonly ILoggerAdapter<ClientBonusService> _log;
        private readonly IEligibleBonusRepository _eligibleBonusRepository;

        public ClientBonusService(ILoggerAdapter<ClientBonusService> log, IEligibleBonusRepository eligibleBonusRepository)
        {
            this._log = log ?? throw new ArgumentNullException(nameof(log));
            this._eligibleBonusRepository = eligibleBonusRepository ?? throw new ArgumentNullException(nameof(eligibleBonusRepository));
        }

        public async Task<decimal> GetBonusAmount(EligibleVoucherBonus? activeBonus, decimal redeemAmount, int clientId)
        {
            try
            {
                var bonusAmount = 0.0M;

                if(activeBonus == null)
                {
                    throw new FormatException("Null EligibleVoucherBonus found");
                }

                var incomingBonusAmount = Convert.ToDecimal(activeBonus.Percentage) / 100 * redeemAmount;
                var trueMaxBonusAmount = Convert.ToDecimal(activeBonus.Percentage) / 100 * activeBonus.MaxRedeemAmount;

                decimal punterRedeemTotal = 0;
                decimal punterBonusPayedOutSum = 0;

                //Step 1.1 - Get punters current redeem total and paid out bonus total.
                var punterBonusTransaction = await _eligibleBonusRepository.GetPunterBonusTotals(activeBonus.BonusID, clientId);
                if (punterBonusTransaction != null)
                {
                    punterRedeemTotal = punterBonusTransaction.RedeemTotal;
                    punterBonusPayedOutSum = punterBonusTransaction.payedOut;
                }

                //step 3.1 - Calculate the Bonus and check it isn't over the bonus amount. 
                if (punterRedeemTotal == 0 && punterBonusPayedOutSum == 0 && redeemAmount <= activeBonus.MaxRedeemAmount)
                {
                    bonusAmount = incomingBonusAmount;
                    punterRedeemTotal = redeemAmount;
                    punterBonusPayedOutSum = bonusAmount;
                }
                else if (punterBonusPayedOutSum + incomingBonusAmount <= trueMaxBonusAmount)
                {
                    bonusAmount = incomingBonusAmount;
                    punterRedeemTotal += redeemAmount;
                    punterBonusPayedOutSum += bonusAmount;
                }
                else if (punterBonusPayedOutSum + incomingBonusAmount > trueMaxBonusAmount)
                {
                    //This is not a new bonus and the amount is greater than the max.
                    var remainingAmount = trueMaxBonusAmount - punterBonusPayedOutSum;

                    //This is a special case scenario where a punter can redeem more than the max in one go.
                    if (punterRedeemTotal == 0 && punterBonusPayedOutSum == 0)
                    {
                        remainingAmount = trueMaxBonusAmount;
                    } //if

                    bonusAmount = remainingAmount;
                    punterRedeemTotal += redeemAmount;
                    punterBonusPayedOutSum += remainingAmount;
                }

                if (bonusAmount > 0)
                {
                    await _eligibleBonusRepository.InsertPunterBonusTransaction(clientId, activeBonus.BonusID, punterRedeemTotal, punterBonusPayedOutSum);

                    _log.LogInformation(clientId.ToString(), "Bonus amount identified for client ID {clientId} with bonus amount: {bonusAmount}.Redeem Total: {punterRedeemTotal} & Paid Out Sum: {punterBonusPayedOutSum}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  clientId, bonusAmount, punterRedeemTotal, punterBonusPayedOutSum);

                }

                return bonusAmount;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, clientId.ToString(), "Failed to get bonus amount for {clientId}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, clientId );
                return 0;

            }
        }
    }
}
