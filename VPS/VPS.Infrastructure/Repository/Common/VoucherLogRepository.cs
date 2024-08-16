using Dapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Data;
using System.Reflection;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Helpers;
using VPS.Helpers.Logging;

namespace VPS.Infrastructure.Repository.Common
{
    public class VoucherLogRepository : IVoucherLogRepository
    {
        private readonly DbConnectionStrings _connectionString;

        private readonly ILoggerAdapter<VoucherLogRepository> _logger;
        private readonly MetricsHelper _metricsHelper;

        public VoucherLogRepository(IOptions<DbConnectionStrings> connectionString, ILoggerAdapter<VoucherLogRepository> log, MetricsHelper metricsHelper)
        {
            _connectionString = connectionString.Value;
            _logger = log ?? throw new ArgumentNullException(nameof(log));
            _metricsHelper = metricsHelper;
        }

        public async Task InsertVoucherLog(VoucherDBLogRecordModel voucher, string endpoint, string logStoreProcedureName, string uniqueReference = "", string apiResponse = "", bool creditedOnSyx = false)
        {
            try
            {
                if (voucher.VoucherTypeId > 0)
                {
                    _logger.LogInformation(voucher.ClientId.ToString(), "{voucherType}_RedemptionAttempt- {voucher}",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                        EnumHelper.GetEnumDescription(voucher.VoucherTypeId), JsonConvert.SerializeObject(voucher));
                }

                if(Guid.TryParse(uniqueReference, out Guid uniqueRef))
                {
                    var db = new VpsDataContext(_connectionString.HollyTopUpConnection, _metricsHelper);
                    var parameters = new DynamicParameters();
                    parameters.Add("VoucherPin", voucher.VoucherPin);
                    parameters.Add("ClientID", voucher.ClientId, DbType.Int64);
                    parameters.Add("Amount", voucher.Amount, DbType.Decimal);
                    parameters.Add("VoucherReferenceID", voucher.VoucherReferenceId, DbType.Int64);
                    parameters.Add("VoucherTypeID", (int)voucher.VoucherTypeId, DbType.Int16);
                    parameters.Add("VoucherStatusID", (int)VoucherStatus.Pending, DbType.Int16);
                    parameters.Add("SyXPlatform", voucher.SyXPlatform, DbType.String);
                    parameters.Add("CreditedOnSyx", creditedOnSyx, DbType.Boolean);
                    parameters.Add("UniqueReference", uniqueRef, DbType.Guid);
                    parameters.Add("ApiResponse", apiResponse, DbType.String);
                    parameters.Add("OriginEndpoint", endpoint, DbType.String);
                    await db.QueryAsync<VoucherDBLogRecordModel>(logStoreProcedureName, parameters);
                }
                else
                {
                    _logger.LogError(voucher.VoucherPin, "{VoucherTypeId)}_{VoucherPin}_Unable to insert VoucherLog. Failed to parse uniqueReference.",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                        voucher.VoucherTypeId, voucher.VoucherPin);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, voucher.VoucherPin, "{VoucherTypeId)}_{VoucherPin}_Unable to insert VoucherLog",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    voucher.VoucherTypeId, voucher.VoucherPin);
            }

        }

     
        public async Task UpdateVoucherLogAPIResponse(string voucherPin, string uniqueReference, VoucherType voucherType, VoucherStatus voucherStatus, long voucherID, decimal amount, string apiResponse)
        {
            try
            {
                if(Guid.TryParse(uniqueReference, out Guid uniqueRef))
                {
                    var db = new VpsDataContext(_connectionString.HollyTopUpConnection, _metricsHelper);
                    var parameters = new DynamicParameters();
                    parameters.Add("VoucherPin", voucherPin);
                    parameters.Add("VoucherStatusID", (int)voucherStatus, DbType.Int16);
                    parameters.Add("VoucherReferenceID", voucherID, DbType.Int64);
                    parameters.Add("VoucherTypeID", (int)voucherType, DbType.Int16);
                    parameters.Add("UniqueReference", uniqueRef, DbType.Guid);
                    parameters.Add("Amount", amount, DbType.Decimal);
                    parameters.Add("ApiResponse", apiResponse, DbType.String);
                    await db.ExecuteAsync("UpdateVoucherLogAPIResponse", parameters);
                }
                else
                {
                    _logger.LogError(voucherPin, "Unable to update VoucherLogAPIResponse for {uniqueRef}. Failed to parse uniqueReference.",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                        uniqueReference);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, voucherPin, "Unable to update VoucherLogAPIResponse for {uniqueRef}", 
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty, 
                    uniqueReference);
            }
        }

        public async Task UpdateVoucherLogSyxResponse(string voucherPin, string uniqueReference, VoucherType voucherType, VoucherStatus voucherStatus, string syxResponse, bool isSyxSuccessful)
        {
            try
            {
                if(Guid.TryParse(uniqueReference, out Guid uniqueRef))
                {
                    var db = new VpsDataContext(_connectionString.HollyTopUpConnection, _metricsHelper);
                    var parameters = new DynamicParameters();
                    parameters.Add("VoucherPin", voucherPin);
                    parameters.Add("VoucherStatusID", (int)voucherStatus, DbType.Int16);
                    parameters.Add("VoucherTypeID", (int)voucherType, DbType.Int16);
                    parameters.Add("UniqueReference", uniqueRef, DbType.Guid);
                    parameters.Add("SyxResponse", syxResponse, DbType.String);
                    parameters.Add("IsSyxSuccessful", isSyxSuccessful, DbType.Boolean);
                    await db.ExecuteAsync("UpdateVoucherLogSyxResponse", parameters);
                }
                else
                {
                    _logger.LogError(voucherPin, "Unable to update VoucherLogSyxResponse for {uniqueRef}. Failed to parse uniqueReference.",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                        uniqueReference);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, voucherPin, "Unable to update VoucherLogSyxResponse for {uniqueRef}", 
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty, 
                    uniqueReference);
            }
        }

    }
}
