namespace VPS.Domain.Models.Configurations.RACellularVoucher;

public class RACellularVoucherConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public string AdviceUrl { get; set; } = string.Empty;
    public string RandAPassword { get; set; } = string.Empty;
    public string RandAUsername { get; set; } = string.Empty;
    public string VoucherLookupUrl { get; set; } = string.Empty;
    public string RedeemVoucherUrl { get; set; } = string.Empty;
    public string RandAAPITimeoutMS { get; set; } = string.Empty;
    public string TerminalOperator { get; set; } = string.Empty;
    public string MaxPolyRetry { get; set; } = string.Empty;
}
