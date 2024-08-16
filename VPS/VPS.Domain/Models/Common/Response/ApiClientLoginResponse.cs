namespace VPS.Domain.Models.Common.Response
{
    public class ApiClientLoginResponse
    {
        public ApiClient? ResponseObject { set; get; }

        public int ResponseType { set; get; }

        public string? ResponseMessage { set; get; }

        public string? SessionToken { set; get; }

    }
}
