namespace VPS.Domain.Models.OTT.Requests
{
    public class OttProviderVoucherRequest
    {
        public string Account { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
        public string UniqueReference { get; set; } = string.Empty;
        public int VendorID { get; set; }
        public string Hash { get; set; } = string.Empty;

    }
}
