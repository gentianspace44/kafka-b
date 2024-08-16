using VPS.Domain.Models.Enums;

namespace VPS.Domain.Models.Common
{
    public class VoucherDBLogRecordModel
    {
        public long VoucherLogId { get; set; }
        public long ClientId { get; set; }
        public DateTime RedeemDateTime { get; set; }
        public VoucherType VoucherTypeId { get; set; }
        public string VoucherPin { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public long VoucherReferenceId { get; set; }
        public int VoucherStatusId { get; set; }
        public Guid UniqueReference { get; set; }
        public string SyXPlatform { get; set; } = string.Empty;
        public bool CreditedOnSyx { get; set; }
        public string ApiResponse { get; set; } = string.Empty;
        public DateTime DateAdded { get; set; }

    }
}
