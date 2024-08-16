using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Prometheus;
using System.Net;
using System.Reflection;
using VPS.API.Common;
using VPS.API.Flash.Processes;
using VPS.Domain.Models.Configurations.Flash;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.Flash;
using VPS.Helpers;
using VPS.Helpers.Logging;

namespace VPS.API.Flash;

public class FlashApiAuthenticationService : IFlashApiAuthenticationService
{
    private readonly ILoggerAdapter<FlashApiAuthenticationService> _log;
    private readonly IHttpClientCommunication _httpClientCommunication;
    private readonly FlashConfiguration _flashSettings;
    private readonly MetricsHelper _metricsHelper;
    private readonly IRedisServiceBridge _redisServiceBridge;
    public FlashApiAuthenticationService(ILoggerAdapter<FlashApiAuthenticationService> log,
                         IOptions<FlashConfiguration> flashSettings,
                         IHttpClientCommunication httpClientCommunication,
                         MetricsHelper metricsHelper,
                         IRedisServiceBridge redisServiceBridge)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _flashSettings = flashSettings?.Value ?? throw new ArgumentNullException(nameof(flashSettings));
        _httpClientCommunication = httpClientCommunication ?? throw new ArgumentNullException(nameof(httpClientCommunication));
        _metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
        _redisServiceBridge = redisServiceBridge ?? throw new ArgumentNullException(nameof(redisServiceBridge));
    }

    public async Task<FlashAccessToken?> GetFlashApiToken(string voucherPin)
    {

        var token = await _redisServiceBridge.GetCachedFlashToken(_flashSettings.FlashAccessTokenCacheKey);
        if (token.IsTokenValid)
        {
            return new FlashAccessToken() { Access_Token = token.AccessToken };
        }
        var refreshToken = await RefreshFlashToken(voucherPin);
        if (refreshToken != null && refreshToken.Access_Token != null)
        {
            await _redisServiceBridge.SaveFlashTokenCache(_flashSettings.FlashAccessTokenCacheKey, refreshToken, _flashSettings.AccessTokenCacheLifespanSeconds);
        }
        return refreshToken;
    }

    private async Task<FlashAccessToken?> RefreshFlashToken(string voucherPin)
    {
        try
        {
            var headers = FlashAuthenticationProcess.CreateFlashAuthenticationHeaders(_flashSettings);
            string postUrl = UrlHelper.CombineUrls(_flashSettings.FlashEndpoint, "token");
            var requestContent = FlashAuthenticationProcess.CreateFlashAuthenticationParameters();
            var timeout = FlashAuthenticationProcess.CreateSecondsTimespan(_flashSettings.FlashAPITimeoutSeconds);

            int noOfAttempts = 0;

            AsyncRetryPolicy _retryPolicy = Policy
                .Handle<Exception>()
            .WaitAndRetryAsync(
                    int.Parse(_flashSettings.MaxPolyRetry),
                    retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(10, noOfAttempts)),
                    onRetry: (exception, _) =>
                    {
                        noOfAttempts++;
                        _log.LogInformation(voucherPin, "Calling SendRequestAsync to get Token for pin: {voucherPin}, for Flash Retry Attempt: {noOfAttempts}",
                           MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                           
                            "FlashToken",
                            noOfAttempts
                            );

                        if (!string.IsNullOrWhiteSpace(exception.Message))
                        {
                            _log.LogError(voucherPin, "Calling SendRequestAsync to get Token for pin: {voucherPin}, for Flash Retry Attempt: {noOfAttempts}. Error: {exception}",
                           MethodBase.GetCurrentMethod()?.Name ?? string.Empty,                         
                            "FlashToken",
                            noOfAttempts,
                            exception
                           );
                        }
                    }
                );
            using (_metricsHelper.providerFlashApiResponse.NewTimer())
            {
                var response = await _retryPolicy.ExecuteAsync(() => _httpClientCommunication.SendRequestAsync(postUrl, Domain.Models.Enums.HttpMethod.POST, requestContent, "application/x-www-form-urlencoded", headers, CharsetEncoding.UTF8, timeout));

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    if (response.StatusCode == HttpStatusCode.RequestTimeout)
                    {
                        _log.LogError($"Flash API Timeout (Access Token) for pin {voucherPin}. Flash API taking too long to send a response back.", null);
                        return new FlashAccessToken();
                    }

                    _log.LogError($"Flash API Error (Access Token) for pin {voucherPin}, {response.Content}", null);
                    return new FlashAccessToken();
                }
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<FlashAccessToken>(responseJson);
            }
        }
        catch (HttpRequestException ex)
        {
            _log.LogError(ex, null, $"Flash API Token [Error]");
            return new FlashAccessToken();
        }
    }

    public bool IsFlashAccessTokenValid(string flashAccessToken, string voucherPin)
    {
        if (string.IsNullOrEmpty(flashAccessToken))
        {
            _log.LogError($"Flash API Error - Access Token is empty for pin {voucherPin}", null);
            return false;
        }
        return true;
    }
}
