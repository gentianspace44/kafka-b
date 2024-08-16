namespace VPS.Domain.Models.EasyLoad.Request
{
    public class EasyLoadProviderVoucherRequest
    {
        public string VoucherNumber { get; set; } = string.Empty;
        public int CustomerAccount { get; set; }

    }
}
