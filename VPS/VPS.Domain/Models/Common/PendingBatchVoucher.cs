using VPS.Domain.Models.Enums;

namespace VPS.Domain.Models.Common
{
    public class PendingBatchVoucher
    {
        public VoucherType VoucherType { get; set; }
        public  string VoucherPin { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public Guid UniqueReference { get; set; }
        public string DevicePlatform { get; set; } = string.Empty;
        public long VoucherID { get; set; }
        public decimal VoucherAmount { get; set; }
        public string VoucherPrefix { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public BatchProcessingSource Source { get; set; }
        public bool NeedsManualProcessing { get; set; }
        public bool CreditedOnSyX { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ProcessingAttempts { get; set; }
        public DateTime? BatchProcessingTime { get; set; }
        public bool ProcessingClosed { get; set; }
    }
}
