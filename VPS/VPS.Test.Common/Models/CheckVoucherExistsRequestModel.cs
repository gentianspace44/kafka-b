namespace VPS.Test.Common.Models
{
    public class CheckVoucherExistsRequestModel
    {
        public string SessionToken { get; set; } = string.Empty;
        public long ClientId { get; set; }
        public string Reference { get; set; } = string.Empty;
    }
}
