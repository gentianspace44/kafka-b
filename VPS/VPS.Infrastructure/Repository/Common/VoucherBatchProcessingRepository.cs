using Dapper;
using Microsoft.Extensions.Options;
using System.Data;
using System.Reflection;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Helpers;
using VPS.Helpers.Logging;

namespace VPS.Infrastructure.Repository.Common
{
    public class VoucherBatchProcessingRepository : IVoucherBatchProcessingRepository
    {
        private readonly DbConnectionStrings _connectionString;
        private readonly ILoggerAdapter<VoucherBatchProcessingRepository> _logger;
        private readonly MetricsHelper _metricsHelper;

        public VoucherBatchProcessingRepository(IOptions<DbConnectionStrings> connectionString, ILoggerAdapter<VoucherBatchProcessingRepository> log, MetricsHelper metricsHelper)
        {
            _connectionString = connectionString.Value;
            _logger = log ?? throw new ArgumentNullException(nameof(log));
            _metricsHelper = metricsHelper;
        }

        public async Task InsertPendingBatchVoucher(PendingBatchVoucher voucher)
        {
            try
            {
                var db = new VpsDataContext(_connectionString.HollyTopUpConnection, _metricsHelper);
                var parameters = new DynamicParameters();
                parameters.Add("VoucherType", (int)voucher.VoucherType, DbType.Int16);
                parameters.Add("VoucherPin", voucher.VoucherPin, DbType.String);
                parameters.Add("UniqueReference", voucher.UniqueReference, DbType.Guid);
                parameters.Add("ClientId", voucher.ClientId, DbType.Int32);
                parameters.Add("DevicePlatform", voucher.DevicePlatform, DbType.String);
                parameters.Add("VoucherID", voucher.VoucherID, DbType.Int64);
                parameters.Add("VoucherAmount", voucher.VoucherAmount, DbType.Decimal);
                parameters.Add("VoucherPrefix", voucher.VoucherPrefix, DbType.String);
                parameters.Add("Source", (int)voucher.Source, DbType.Int16);
                parameters.Add("Message", voucher.Message, DbType.String);
                await db.ExecuteAsync("spInsertPendingBatchVoucher", parameters);
                _metricsHelper.IncVouchersScheduledForManualProcessing(_logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, voucher.VoucherPin, "{VoucherTypeId)}_{VoucherPin} Unable to insert PendingBatchVoucher",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    voucher.VoucherType.ToString(), voucher.VoucherPin);
            }
        }

        public async Task UpdatePendingBatchVoucherAttempts(VoucherType voucherType, string voucherPin, int processingAttempts)
        {
            try
            {
                var db = new VpsDataContext(_connectionString.HollyTopUpConnection, _metricsHelper);
                var parameters = new DynamicParameters();
                parameters.Add("VoucherType", (int)voucherType, DbType.Int16);
                parameters.Add("VoucherPin", voucherPin, DbType.String);
                parameters.Add("ProcessingAttempts", processingAttempts, DbType.Int16);
                await db.ExecuteAsync("spUpdatePendingBatchVoucherAttempts", parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, voucherPin, "{VoucherTypeId)}_{VoucherPin} Unable to update PendingBatchVoucher on UpdateNumberOfAttempts",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    voucherType.ToString(), voucherPin);
            }
        }

        public async Task FinalizePendingBatchVoucherSuccess(VoucherType voucherType, string voucherPin)
        {
            try
            {
                var db = new VpsDataContext(_connectionString.HollyTopUpConnection, _metricsHelper);
                var parameters = new DynamicParameters();
                parameters.Add("VoucherType", (int)voucherType, DbType.Int16);
                parameters.Add("VoucherPin", voucherPin, DbType.String);
                await db.ExecuteAsync("spFinalizePendingBatchVoucherSuccess", parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, voucherPin, "{VoucherTypeId)}_{VoucherPin} Unable to update PendingBatchVoucher on FinaliseCreditingWithSuccess", 
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty, 
                    voucherType.ToString(), voucherPin);
            }
        }

        public async Task FinalizePendingBatchVoucherFail(VoucherType voucherType, string voucherPin)
        {
            try
            {
                var db = new VpsDataContext(_connectionString.HollyTopUpConnection, _metricsHelper);
                var parameters = new DynamicParameters();
                parameters.Add("VoucherType", (int)voucherType, DbType.Int16);
                parameters.Add("VoucherPin", voucherPin, DbType.String);
                await db.ExecuteAsync("spFinalizePendingBatchVoucherFail", parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, voucherPin, "{VoucherTypeId)}_{VoucherPin} Unable to update PendingBatchVoucher on FinaliseCreditingWithFail", 
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty, 
                    voucherType.ToString(), voucherPin);
            }
        }

        public async Task<PendingBatchVoucher?> SelectBatchProcessingVoucher(VoucherType voucherType, string voucherPin)
        {
            try
            {
                var db = new VpsDataContext(_connectionString.HollyTopUpConnection, _metricsHelper);
                var parameters = new DynamicParameters();
                parameters.Add("VoucherType", (int)voucherType, DbType.Int16);
                parameters.Add("VoucherPin", voucherPin, DbType.String);
                var result = await db.QueryAsync<PendingBatchVoucher>("spSelectPendingBatchVoucher", parameters);

                if (result != null && result.Any())
                {
                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, voucherPin, "{VoucherTypeId)}_{VoucherPin} Unable to find PendingBatchVoucher",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                   voucherType.ToString(), voucherPin);
            }

            return null;
        }

        public async Task<IEnumerable<PendingBatchVoucher>?> SelectPendingBatchVouchersToBeProcessedAutomatically(int batchSize)
        {
            try
            {
                var db = new VpsDataContext(_connectionString.HollyTopUpConnection, _metricsHelper);
                var parameters = new DynamicParameters();
                parameters.Add("BatchSize", batchSize, DbType.Int16);
                var result = await db.QueryAsync<PendingBatchVoucher>("spSelectPendingBatchVouchersToBeProcessedAutomatically", parameters);

                if (result != null && result.Any())
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null, "Unable to SelectPendingBatchVouchersToBeProcessedAutomatically with error: {message}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    ex.Message);
            }

            return null;
        }
    }
}
