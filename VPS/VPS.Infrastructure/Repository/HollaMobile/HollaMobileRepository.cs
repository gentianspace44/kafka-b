using Dapper;
using Microsoft.Extensions.Options;
using System.Data;
using System.Reflection;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.HollaMobile.Response;
using VPS.Helpers;
using VPS.Helpers.Logging;

namespace VPS.Infrastructure.Repository.HollaMobile
{
    public class HollaMobileRepository : IHollaMobileRepository
    {
        private readonly DbConnectionStrings _dbConnectionStrings;
        private readonly ILoggerAdapter<HollaMobileRepository> _log;
        private readonly MetricsHelper _metricsHelper;


        public HollaMobileRepository(IOptions<DbConnectionStrings> dbConnectionStrings, ILoggerAdapter<HollaMobileRepository> log, MetricsHelper metricsHelper)
        {

            this._log = log ?? throw new ArgumentNullException(nameof(log));
            this._dbConnectionStrings = dbConnectionStrings?.Value ?? throw new ArgumentNullException(nameof(dbConnectionStrings));
            this._metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));

        }


        public async Task<HollaMobileRedeemResponse> RedeemHollaMobileVoucher(string voucherPin, int clientId)
        {
            try
            {
                var db = new VpsDataContext(_dbConnectionStrings.HollyTopUpConnection, _metricsHelper);
                var parameters = new DynamicParameters();
                parameters.Add("@VoucherPin", voucherPin, DbType.String);
                parameters.Add("@AccountNo", clientId, DbType.Int32);
                var result = await db.QueryAsync<HollaMobileRedeemResponse>("RedeemHollaMobile", parameters);
                return result.FirstOrDefault() ?? new HollaMobileRedeemResponse();

            }
            catch (Exception ex)
            {
                _log.LogError(ex, null, "RedeemHollaMobile Voucher Error Unable to call the SP 'RedeemHollaMobile' ({voucherNumber} clientId ({clientId}))",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    voucherPin, clientId);
                throw;
            }
        }
    }
}
