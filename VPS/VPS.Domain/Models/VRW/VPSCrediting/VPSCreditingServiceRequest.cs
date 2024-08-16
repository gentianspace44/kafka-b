namespace VPS.Domain.Models.VRW.VPSCrediting
{
    public class VpsCreditingServiceRequest
    {
        public string ClientId { get; set; } = string.Empty;
        public string DevicePlatform { get; set; } = string.Empty;
        public string VoucherNumber { get; set; } = string.Empty;
        
        public string SyXSessionToken { get; set; } = "SessionToken";
       
        public long SyXUserId { get; set; } = 1;
    }

}
