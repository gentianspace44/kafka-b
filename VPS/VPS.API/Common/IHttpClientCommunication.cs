using VPS.Domain.Models.Enums;

namespace VPS.API.Common
{
    public interface IHttpClientCommunication
    {
        Task<HttpResponseMessage> SendRequestAsync(string url, Domain.Models.Enums.HttpMethod httpMethod, string? content = null, string contentType = "application/json", IEnumerable<KeyValuePair<string, string>>? headers = null, CharsetEncoding charsetEncoding = CharsetEncoding.UTF8, TimeSpan? timeoutTimespan = null);
    }
}
