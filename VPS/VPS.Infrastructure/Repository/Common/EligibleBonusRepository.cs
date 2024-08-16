using Dapper;
using Microsoft.Extensions.Options;
using System.Data;
using System.Reflection;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Helpers;
using VPS.Helpers.Logging;

namespace VPS.Infrastructure.Repository.Common
{
    public class EligibleBonusRepository : IEligibleBonusRepository
    {

        private readonly DbConnectionStrings _connectionString;
        private readonly ILoggerAdapter<EligibleBonusRepository> _log;
        private readonly MetricsHelper _metricsHelper;

        public EligibleBonusRepository(IOptions<DbConnectionStrings> connectionString, ILoggerAdapter<EligibleBonusRepository> log, MetricsHelper metricsHelper)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _connectionString = connectionString.Value;
            this._metricsHelper = metricsHelper;
        }

        public async Task<ActiveBonus?> GetActiveBonus()
        {
            try
            {
                var db = new VpsDataContext(_connectionString.HollyTopUpConnection, _metricsHelper);

                var result = await db.QueryAsync<ActiveBonus>("spGetActiveBonus");

                return result?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, null, "EligibleBonusService=> GetActiveBonus. Error: {message}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    ex.Message);
                return new ActiveBonus();
            }
        }

        public async Task<ActiveBonus?> GetBonusForVoucher(DateTime redeemDateTime)
        {
            try
            {
                var db = new VpsDataContext(_connectionString.HollyTopUpConnection, _metricsHelper);

                var result = await db.QueryAsync<ActiveBonus>("spCheckVoucherHasBonus");

                return result?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, null, "EligibleBonusService=> GetBonusForVoucher. Error: {message}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    ex.Message);
                return new ActiveBonus();
            }
        }

        public async Task<EligibleVoucherBonus?> GetEligibleVoucherBonus(DateTime redeemDateTime, int clientId)
        {
            try
            {
                var db = new VpsDataContext(_connectionString.HollyTopUpConnection, _metricsHelper);
                var parameters = new DynamicParameters();
                parameters.Add("@VoucherRedeemDate", redeemDateTime, DbType.DateTime);
                parameters.Add("@PunterID", clientId, DbType.Int32);

                var result = await db.QueryAsync<EligibleVoucherBonus>("GetEligibleVoucherBonus", parameters);
                return result?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, null, "EligibleBonusService=> Eligible VoucherBonus. Client Id: {clientId} error: {message}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    clientId, ex.Message);
                return new EligibleVoucherBonus();
            }
        }

        public async Task<PunterBonusTransaction?> GetPunterBonusTotals(int bonusId, int punterId)
        {
            try
            {
                var db = new VpsDataContext(_connectionString.HollyTopUpConnection, _metricsHelper);

                var parameters = new DynamicParameters();
                parameters.Add("@PunterID", punterId, DbType.Int32);
                parameters.Add("@BonusID", bonusId, DbType.Int32);

                var result = await db.QueryAsync<PunterBonusTransaction>("spGetPunterBonusTransaction", parameters);

                return result?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, null, "EligibleBonusService=> GetPunterBonusTotals. Punter Id: {punterId} error: {message}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    punterId, ex.Message);
                return new PunterBonusTransaction();
            }
        }

        public async Task InsertPunterBonusTransaction(int punterId, int bonusId, decimal redeemTotal, decimal payedOut)
        {
            try
            {
                var db = new VpsDataContext(_connectionString.HollyTopUpConnection, _metricsHelper);

                var parameters = new DynamicParameters();
                parameters.Add("@PunterID", punterId, DbType.Int32);
                parameters.Add("@BonusID", bonusId, DbType.String);
                parameters.Add("@RedeemTotal", redeemTotal, DbType.Decimal);
                parameters.Add("@PayedOut", payedOut, DbType.Decimal);

                await db.ExecuteAsync("spInsertBonusTransaction", parameters);

            }
            catch (Exception ex)
            {
                _log.LogError(ex, null, "EligibleBonusService=> InsertPunterBonusTransaction. Punter Id: {punterId} error: {message}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    punterId, ex.Message);
            }
        }
    }
}
