namespace VPS.Domain.Models.Common
{
    public class SyxSessionModel
    {
        public long SyxUserId { get; set; }
        public string SyxSessionToken { get; set; } = string.Empty;
    }
}
