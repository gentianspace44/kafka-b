namespace VPS.Domain.Models.Common.Request
{
    public class ApiVoucherExistsRequest
    {
        public long ClientID { get; set; }
        public string? Reference { get; set; }
    }
}
