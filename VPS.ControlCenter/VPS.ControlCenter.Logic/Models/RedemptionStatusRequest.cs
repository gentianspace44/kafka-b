namespace VPS.ControlCenter.Logic.Models
{
    public class RedemptionStatusRequest
    {
        public string ClientId { get; set; }
        public string VoucherName { get; set; }
        public string VoucherPin { get; set; } = string.Empty;
        public bool DefaultVoucherProvider { get; set; }
        public string VoucherNumberLength { get; set; }

    }
}
