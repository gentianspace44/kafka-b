using VPS.Domain.Models.Common;
using VPS.Domain.Models.Enums;

namespace VPS.Infrastructure.Repository.Common
{
    public interface IVoucherLogRepository
    {
        Task InsertVoucherLog(VoucherDBLogRecordModel voucher, string endpoint, string logStoreProcedureName, string uniqueReference = "", string apiResponse = "", bool creditedOnSyx = false);
      
        Task UpdateVoucherLogAPIResponse(string voucherPin, string uniqueReference, VoucherType voucherType, VoucherStatus voucherStatus, long voucherID, decimal amount, string apiResponse);

        Task UpdateVoucherLogSyxResponse(string voucherPin, string uniqueReference, VoucherType voucherType, VoucherStatus voucherStatus, string syxResponse, bool isSyxSuccessful);
    }
}
