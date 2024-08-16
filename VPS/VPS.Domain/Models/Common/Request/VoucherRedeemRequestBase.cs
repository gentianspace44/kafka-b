
namespace VPS.Domain.Models.Common.Request
{
    public abstract class VoucherRedeemRequestBase
    {
        public string VoucherNumber { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public string DevicePlatform { get; set; } = string.Empty;
        public string VoucherReference { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public DateTime CreatedDate { get; } = DateTime.Now;
        public string ServiceVersion { get; set; } = string.Empty;
    }
}