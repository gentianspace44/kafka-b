using VPS.Domain.Models.Common;
using VPS.Domain.Models.Enums;

namespace VPS.Infrastructure.Repository.Common
{
    public interface IVoucherBatchProcessingRepository
    {
        Task InsertPendingBatchVoucher(PendingBatchVoucher voucher);
        Task UpdatePendingBatchVoucherAttempts(VoucherType voucherType, string voucherPin, int processingAttempts);
        Task FinalizePendingBatchVoucherSuccess(VoucherType voucherType, string voucherPin);
        Task FinalizePendingBatchVoucherFail(VoucherType voucherType, string voucherPin);
        Task<PendingBatchVoucher?> SelectBatchProcessingVoucher(VoucherType voucherType, string voucherPin);
        Task<IEnumerable<PendingBatchVoucher>?> SelectPendingBatchVouchersToBeProcessedAutomatically(int batchSize);
    }
}
