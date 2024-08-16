namespace VPS.API.Common
{
    public interface IHttpClientWrapper
    {
        Task<HttpResponseMessage> GetAsync(string requestUri);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, TimeSpan? timeoutTimespan);
        void CreateClient(IEnumerable<KeyValuePair<string, string>>? headers);
    }
}
