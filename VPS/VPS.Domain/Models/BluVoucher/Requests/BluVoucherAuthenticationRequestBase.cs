namespace VPS.Domain.Models.BluVoucher.Requests
{
    public class BluVoucherAuthenticationRequestBase
    {
        public string Reference { get; set; } = string.Empty;
        public Terminal UserTerminal { get; set; } = new Terminal();
        public string EventType { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string TransType { get; set; } = string.Empty;

    }
}
