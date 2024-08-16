using Dapper;
using Microsoft.Extensions.Options;
using System.Data;
using System.Reflection;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.HollyTopUp.Response;
using VPS.Helpers;
using VPS.Helpers.Logging;

namespace VPS.Infrastructure.Repository.HollyTopUp;

public class HollyTopUpRepository : IHollyTopUpRepository
{
    private readonly DbConnectionStrings _dbConnectionStrings;
    private readonly ILoggerAdapter<HollyTopUpRepository> _log;
    private readonly MetricsHelper _metricsHelper;

    public HollyTopUpRepository(IOptions<DbConnectionStrings> dbConnectionStrings, ILoggerAdapter<HollyTopUpRepository> log, MetricsHelper metricsHelper)
    {
        this._log = log ?? throw new ArgumentNullException(nameof(log));
        this._dbConnectionStrings = dbConnectionStrings?.Value ?? throw new ArgumentNullException(nameof(dbConnectionStrings));
        this._metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
    }

    public async Task<HollyTopUpRedeemResponse> RedeemHollyTopUpVoucher(string voucherPin, int clientId)
    {
        try
        {
            var db = new VpsDataContext(_dbConnectionStrings.HollyTopUpConnection, _metricsHelper);
            var parameters = new DynamicParameters();
            parameters.Add("@VoucherNumber", voucherPin, DbType.String);
            parameters.Add("@AccountNumber", clientId, DbType.Int32);
            var result = await db.QueryAsync<HollyTopUpRedeemResponse>("RedeemHollyTopUpVoucher", parameters);
            return result.FirstOrDefault() ?? new HollyTopUpRedeemResponse();

        }
        catch (Exception ex)
        {
            _log.LogError(ex, null, "HollyTopUp Voucher Error Unable to call the SP 'RedeemHollyTopUpVoucher' ({voucherPin} clientId ({clientId}))",
                MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                voucherPin, clientId);
            throw;
        }
    }
}
