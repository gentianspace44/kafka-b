namespace VPS.Domain.Models.Configurations.EasyLoad;

public class EasyLoadConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public string RedeemVoucherUrl { get; set; } = string.Empty;
    public string ReverseVoucherUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;  
    public string MaxPolyRetry { get; set; } = string.Empty;

}
