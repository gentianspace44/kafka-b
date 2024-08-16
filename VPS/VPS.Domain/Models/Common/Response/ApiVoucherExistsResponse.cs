namespace VPS.Domain.Models.Common.Response
{
    public class ApiVoucherExistsResponse
    {
        public bool VoucherExists { get; set; }
        public int ResponseType { get; set; }
        public string ResponseMessage { get; set; } = string.Empty;

    }
}
