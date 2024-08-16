using System.Net;
using VPS.Domain.Models.Configurations;

namespace VPS.API.Common
{
    public class HttpClientWrapper : IHttpClientWrapper
    {
        private readonly HttpClient _httpClient;


        public HttpClientWrapper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return await _httpClient.GetAsync(requestUri);
        }


        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return await _httpClient.SendAsync(request);
        }


        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, TimeSpan? timeoutTimespan)
        {
            if (timeoutTimespan.HasValue)
            {
                _httpClient.Timeout = timeoutTimespan.Value;
            }
            return await _httpClient.PostAsync(requestUri, content);
        }

        public void CreateClient(IEnumerable<KeyValuePair<string, string>>? headers)
        {

            // Apply headers if provided
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if(_httpClient.DefaultRequestHeaders.Contains(header.Key))
                    {
                        //Remove existing header
                        _httpClient.DefaultRequestHeaders.Remove(header.Key);
                    }

                    _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            // Set SecurityProtocol
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

    }
}
