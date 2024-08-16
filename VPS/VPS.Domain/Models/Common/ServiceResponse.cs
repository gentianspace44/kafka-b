namespace VPS.Domain.Models.Common
{
    public class ServiceResponse
    {
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public object? Response { get; set; }
        public decimal Amount { get; set; }
        public int CreditOutcome { get; set; }
    }
}
