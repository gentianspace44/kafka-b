namespace VPS.Domain.Models.Common.Response
{
    public class ApiClientBalanceUpdateResponse
    {
        public ApiClientBalance? ResponseObject { get; set; }

        public int ResponseType { get; set; }

        public string ResponseMessage { get; set; } = string.Empty;

    }
}
