using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Prometheus;
using System.Net;
using System.Reflection;
using System.Web.Http;
using VPS.API.Common;
using VPS.API.Flash.Processes;
using VPS.Domain.Models.Configurations.Flash;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.Flash.Requests;
using VPS.Domain.Models.Flash.Responses;
using VPS.Helpers;
using VPS.Helpers.Logging;

namespace VPS.API.Flash;

public class FlashApiService : IFlashApiService
{
    private readonly ILoggerAdapter<FlashApiService> _log;
    private readonly FlashConfiguration _flashSettings;
    private readonly IHttpClientCommunication _httpClientCommunication;
    private readonly MetricsHelper _metricsHelper;
    private readonly IFlashApiAuthenticationService _flashAPIAuthenticationService;
    private readonly IRedisServiceBridge _redisServiceBridge;

    public FlashApiService(ILoggerAdapter<FlashApiService> log,
                       IOptions<FlashConfiguration> flashSettings,
                       IHttpClientCommunication httpClientCommunication,
                       MetricsHelper metricsHelper,
                       IFlashApiAuthenticationService flashAPIAuthenticationService,
                       IRedisServiceBridge redisServiceBridge)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _flashSettings = flashSettings?.Value ?? throw new ArgumentNullException(nameof(flashSettings));
        _httpClientCommunication = httpClientCommunication ?? throw new ArgumentNullException(nameof(httpClientCommunication));
        _metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
        _flashAPIAuthenticationService = flashAPIAuthenticationService ?? throw new ArgumentNullException(nameof(flashAPIAuthenticationService));
        _redisServiceBridge = redisServiceBridge ?? throw new ArgumentNullException(nameof(redisServiceBridge));
    }

    public async Task<FlashRedeemResponse> RedeemVoucher(FlashRequest flashRequest)
    {
        var flashAccessTokenResponse = await _flashAPIAuthenticationService.GetFlashApiToken(flashRequest.pin);
        if (!_flashAPIAuthenticationService.IsFlashAccessTokenValid(flashAccessTokenResponse?.Access_Token?? "", flashRequest.pin))
        {
            _log.LogInformation(null, "Flash API Token Response: {request}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, flashAccessTokenResponse);          
            await _redisServiceBridge.DeleteIdempotency(flashRequest.pin + "_" + flashRequest.metadata.clientId.ToString());
            return new FlashRedeemResponse { ResponseCode = -1, ResponseMessage = $"Failed to get flash token for voucher {flashRequest.pin}" };
        }

        var response = await RedeemFlashVoucher(flashAccessTokenResponse?.Access_Token?? "", flashRequest);
        return await ValidateAndReturnResponse(response, flashRequest);
    }
    private async Task<HttpResponseMessage> RedeemFlashVoucher(string flashAccessToken, FlashRequest voucherRequest)
    {
        try
        {
            var headers = FlashRedeemProcess.CreateFlashRedeemHeaders(flashAccessToken);
            string postUrl = UrlHelper.CombineUrls(_flashSettings.FlashEndpoint, "aggregation/4.0/1voucher/redeem");
            var requestContent = FlashRedeemProcess.CreateFlashRedeemRequest(voucherRequest);
            int noOfAttempts = 0;

            AsyncRetryPolicy _retryPolicy = Policy
                .Handle<Exception>()
            .WaitAndRetryAsync(
                    int.Parse(_flashSettings.MaxPolyRetry),
                    retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(10, noOfAttempts)),
                    onRetry: (exception, _) =>
                    {
                        noOfAttempts++;
                        _log.LogInformation(voucherRequest.pin, "Calling SendRequestAsync with payload: {voucherRequest}, for voucher type {voucherType} Retry Attempt: {noOfAttempts}",
                         MethodBase.GetCurrentMethod()?.Name ?? string.Empty,                            
                            voucherRequest,
                            "FlashVoucher",
                            noOfAttempts
                            );

                        if (!string.IsNullOrWhiteSpace(exception.Message))
                        {
                            _log.LogError(voucherRequest.pin, "Calling SendRequestAsync with payload: {voucherRequest}, for voucher type {voucherType} Retry Attempt: {noOfAttempts}. Error: {exception}",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,                          
                            voucherRequest,
                            "FlashVoucher",
                            noOfAttempts,
                            exception
                            );
                        }
                    }
                );

            using (_metricsHelper.providerFlashApiResponse.NewTimer())
            {
                var timeout = FlashAuthenticationProcess.CreateSecondsTimespan(_flashSettings.FlashAPITimeoutSeconds);
                var response = await _retryPolicy.ExecuteAsync(() => _httpClientCommunication.SendRequestAsync(postUrl, Domain.Models.Enums.HttpMethod.POST, requestContent, "application/json", headers, CharsetEncoding.UTF8, timeout));

                _log.LogInformation(voucherRequest.pin, "Flash Redeem Request: {request}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  requestContent );

                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a string
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _log.LogInformation(voucherRequest.pin, "Flash Redeem Response: {response}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  responseBody );

                    return response;
                }
                else
                {
                    _log.LogInformation(voucherRequest.pin, "HttpResponse is not Successful", args: Array.Empty<object>());
                    throw new HttpRequestException("HttpResponse is not Success");
                }
            }
        }
        catch (HttpRequestException ex)
        {
            var response = new FlashRedeemResponse
            {
                ResponseCode = -1,
                ResponseMessage = $"Flash Redeem Request Failed",
            };

            _log.LogInformation(null, $"Flash Redeem Response [Error] : {response}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(response));
            var res = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(ex.Message)
            };
            throw new HttpResponseException(res);
        }
        catch (Exception ex)
        {
            var response = new FlashRedeemResponse
            {
                ResponseCode = -1,
                ResponseMessage = $"Flash Redeem Request Failed",
            };

            _log.LogInformation(null, $"Flash Redeem Response [Error] : {response}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  JsonConvert.SerializeObject(response));
            var res = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(ex.Message)
            };
            throw new HttpResponseException(res);
        }
    }
    private async Task<FlashRedeemResponse> ValidateAndReturnResponse(HttpResponseMessage response, FlashRequest flashRedeemRequest)
    {
        var responseBody = await response.Content.ReadAsStringAsync();
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                await _redisServiceBridge.DeleteIdempotency(flashRedeemRequest.pin + "_" + flashRedeemRequest.metadata.clientId.ToString());
                return JsonConvert.DeserializeObject<FlashRedeemResponse>(responseBody)!;

            case HttpStatusCode.Unauthorized:
                _log.LogError(null, $"Flash API Invalid token Voucher Pin {flashRedeemRequest.pin} request id -  {flashRedeemRequest.reference}. Expired access token.");
                return new FlashRedeemResponse { ResponseCode = -1, ResponseMessage = "Unauthorized" };

            case HttpStatusCode.RequestTimeout:
            case HttpStatusCode.GatewayTimeout:
                _log.LogError(null, $"Flash API Timeout for Voucher pin {flashRedeemRequest.pin} request id -  {flashRedeemRequest.reference}. Flash API taking too long to send a response back.");
                return new FlashRedeemResponse { ResponseCode = -1, ResponseMessage = "Timeout" };

            default:
                _log.LogError(null, $"Flash API Error for Voucher pin {flashRedeemRequest.pin} request id -  {flashRedeemRequest.reference}, {response.Content}");
                return new FlashRedeemResponse { ResponseCode = -1, ResponseMessage = "Flash API Error Occurred" };
        }
    }
}
