using VPS.Domain.Models.Enums;

namespace VPS.Domain.Models.VRW.VPSCrediting
{
    public class VpsCreditingServiceResponse
    {
        public string Message { get; set; } = string.Empty;

        public decimal? Amount { get; set; }

        public decimal? Bonus { get; set; }

        public SyxCreditOutcome? CreditOutcome { get; set; }
    }
}
