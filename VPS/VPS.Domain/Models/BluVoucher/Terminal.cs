namespace VPS.Domain.Models.BluVoucher
{
    public class Terminal
    {
        public long TerminalId { get; set; }
        public string? DeviceId { get; set; }
        public string? DeviceSerial { get; set; }
        public string? DeviceIp { get; set; }
        public int DeviceBranchId { get; set; }
    }
}
