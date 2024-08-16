namespace VPS.Domain.Models.Configurations.OTT;

public class OttVoucherConfiguration
{
    public string OTTApiKey { get; set; } = string.Empty;
    public string OTTBaseUrl { get; set; } = string.Empty;
    public string OTTPassword { get; set; } = string.Empty;
    public string OTTRemitVoucherUrl { get; set; } = string.Empty;
    public string OTTUsername { get; set; } = string.Empty;
    public string OTTCheckRemitVoucherUrl { get; set; } = string.Empty;
    public int OTTVendorId { get; set; }
    public int MaxPolyRetry { get; set; }
}
