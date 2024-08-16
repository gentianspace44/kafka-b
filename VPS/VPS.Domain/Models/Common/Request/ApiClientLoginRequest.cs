namespace VPS.Domain.Models.Common.Request
{
    public class ApiClientLoginRequest
    {
        public string? ClientAccountNumber { set; get; }

        public string? ClientPin { set; get; }

        public string? SessionToken { set; get; }
    }
}
