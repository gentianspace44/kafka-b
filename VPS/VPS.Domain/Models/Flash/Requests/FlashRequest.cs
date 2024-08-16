namespace VPS.Domain.Models.Flash.Requests
{
    public class FlashRequest
    {
        public string reference { get; set; } = string.Empty;
        public string accountNumber { get; set; } = string.Empty;
        public string pin { get; set; } = string.Empty;
        public int amount { get; set; }
        public string mobileNumber { get; set; } = string.Empty;
        public string storeId { get; set; } = string.Empty;
        public string terminalId { get; set; } = string.Empty;
        public Metadata metadata { get; set; } = new Metadata();
    }
    public class Metadata
    {
        public string clientId { get; set; } = string.Empty;
    }
}
