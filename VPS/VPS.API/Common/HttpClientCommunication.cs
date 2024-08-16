using System.Reflection;
using System.Text;
using VPS.Domain.Models.Enums;
using VPS.Helpers.Logging;

namespace VPS.API.Common
{
    public class HttpClientCommunication : IHttpClientCommunication
    {
        private readonly ILoggerAdapter<HttpClientCommunication> _log;
        private readonly IHttpClientWrapper _httpClientWrapper;

        public HttpClientCommunication(IHttpClientWrapper httpClientWrapper, ILoggerAdapter<HttpClientCommunication> log)
        {
            _httpClientWrapper = httpClientWrapper ?? throw new ArgumentNullException(nameof(httpClientWrapper));
            _log = log;
        }

        public async Task<HttpResponseMessage> SendRequestAsync(string url, Domain.Models.Enums.HttpMethod httpMethod, string? content = null, string contentType = "application/json", IEnumerable<KeyValuePair<string, string>>? headers = null, CharsetEncoding charsetEncoding = CharsetEncoding.UTF8, TimeSpan? timeoutTimespan = null)
        {
            Encoding encoding = GetEncoding(charsetEncoding);

            var requestContent = new StringContent(content ?? string.Empty, encoding, contentType);

            _httpClientWrapper.CreateClient(headers);

            try
            {
                HttpResponseMessage? response = null;

                switch (httpMethod)
                {
                    case Domain.Models.Enums.HttpMethod.GET:
                        response = await _httpClientWrapper.GetAsync(url);
                        break;
                    case Domain.Models.Enums.HttpMethod.HEAD:
                        response = await _httpClientWrapper.SendAsync(new HttpRequestMessage(System.Net.Http.HttpMethod.Head, url));
                        break;
                    case Domain.Models.Enums.HttpMethod.POST:
                        response = await _httpClientWrapper.PostAsync(url, requestContent, timeoutTimespan);
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported HttpMethod: {httpMethod}");
                        //Other HTTP methods as needed.
                }

                return response;
            }
            catch (HttpRequestException ex)
            {
                _log.LogError(ex, null, "Failed to perform the HttpClientCommunication to {url}, with message: {message}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  url, ex.Message );
                throw;
            }
        }

        #region Private Utils

        private static Encoding GetEncoding(CharsetEncoding charsetEncoding)
        {
            if (charsetEncoding == CharsetEncoding.Default)
            {
                return Encoding.GetEncoding(28591);
            }
            else if (CharsetEncoding.UTF8 == charsetEncoding)
            {
                return Encoding.UTF8;
            }
            else
            {
                if (CharsetEncoding.ASCII != charsetEncoding)
                {
                    throw new ArgumentException("Unknown charset encoding.");
                }
                return Encoding.ASCII;
            }
        }

        #endregion
    }
}
