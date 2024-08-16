namespace VPS.Domain.Models.Configurations
{
    public class VpsControlCenterEndpoints
    {
        public string NotifyClientEndpoint { get; set; } = string.Empty;
        public string NotifyAllEndpoint { get; set; } = string.Empty;
        public string GetSyxToken { get; set; } = string.Empty;
        public string ForceSyxTokenUpdate { get; set; } = string.Empty;
        public string ForceRedisVoucherUpdate { get; set; } = string.Empty;
        public string BaseEndpoint { get; set; } = string.Empty;
    }
}
