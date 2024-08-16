using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Prometheus;
using System.Reflection;
using VPS.API.Common;
using VPS.Domain.Models.Configurations.EasyLoad;
using VPS.Domain.Models.EasyLoad.Request;
using VPS.Domain.Models.EasyLoad.Response;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.Flash.Responses;
using VPS.Helpers;
using VPS.Helpers.Logging;

namespace VPS.API.EasyLoad;

public class EasyLoadApiService : IEasyLoadApiService
{
    private readonly ILoggerAdapter<EasyLoadApiService> _log;
    private readonly EasyLoadConfiguration _easyLoadSettings;
    private readonly IHttpClientCommunication _httpClientCommunication;
    private readonly MetricsHelper _metricsHelper;
    public EasyLoadApiService(ILoggerAdapter<EasyLoadApiService> log,
                           IOptions<EasyLoadConfiguration> easyLoadSettings, IHttpClientCommunication httpClientCommunication, MetricsHelper metricsHelper)
    {
        this._log = log ?? throw new System.ArgumentNullException(nameof(log));
        this._easyLoadSettings = easyLoadSettings.Value ?? throw new ArgumentNullException(nameof(easyLoadSettings));
        this._httpClientCommunication = httpClientCommunication ?? throw new ArgumentNullException(nameof(httpClientCommunication));
        this._metricsHelper = metricsHelper  ?? throw new ArgumentNullException(nameof(metricsHelper));
    }

    public async Task<EasyLoadProviderVoucherResponse?> RedeemVoucher(string voucherPin, int clientId)
    {
        try
        {
            var voucherRequest = new EasyLoadProviderVoucherRequest()
            {
                CustomerAccount = clientId,
                VoucherNumber = voucherPin
            };

            var requestContent = JsonConvert.SerializeObject(voucherRequest);

            var headers = new List<KeyValuePair<string, string>>
            {
                new("ApiKey", _easyLoadSettings.ApiKey)
            };

            string postUrl = UrlHelper.CombineUrls(_easyLoadSettings.BaseUrl, _easyLoadSettings.RedeemVoucherUrl);

            int noOfAttempts = 0;

            AsyncRetryPolicy _retryPolicy = Policy
                .Handle<Exception>()
            .WaitAndRetryAsync(
                    int.Parse(_easyLoadSettings.MaxPolyRetry),
                    retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(10, noOfAttempts)),
                    onRetry: (exception, _) =>
                    {
                        noOfAttempts++;
                        _log.LogInformation(voucherRequest.VoucherNumber, 
                            "Calling SendRequestAsync with payload: {voucherRequest}, for voucher type {voucherType} Retry Attempt: {noOfAttempts}",
                           MethodBase.GetCurrentMethod()?.Name ?? string.Empty,                            
                                voucherRequest,
                                "EasyLoadVoucher",
                                noOfAttempts
                            );

                        if (!string.IsNullOrWhiteSpace(exception.Message))
                        {
                            _log.LogError(voucherRequest.VoucherNumber, 
                                "Calling SendRequestAsync with payload: {voucherRequest}, for voucher type {voucherType} Retry Attempt: {noOfAttempts}. Error: {exception}",
                                MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            
                                voucherRequest,
                                "EasyLoadVoucher",
                                noOfAttempts,
                                exception
                            );
                        }
                    }
                );

            using (_metricsHelper.providerEasyLoadApiResponse.NewTimer())
            {
                var response = await _retryPolicy.ExecuteAsync(() => _httpClientCommunication.SendRequestAsync(postUrl, Domain.Models.Enums.HttpMethod.POST, requestContent, "application/json", headers));

                _log.LogInformation(voucherPin, "EasyLoad Redeem Request: {request}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, requestContent );

                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a string
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _log.LogInformation(voucherPin, "EasyLoad Redeem Response: {response}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, responseBody );

                   
                        return JsonConvert.DeserializeObject<EasyLoadProviderVoucherResponse>(responseBody);
                                   
                }
                else
                {
                    _log.LogInformation(voucherPin, "HttpResponse is not Successful");
                   
                    var failedResponse = new EasyLoadProviderVoucherResponse()
                    {
                        ResponseMessage = $"EasyLoad Redeem Request Failed.",
                    };
                  
                    var reversalResponse = await ReverseVoucher(new EasyLoadProviderVoucherRequest()
                    {
                        CustomerAccount = clientId,
                        VoucherNumber = voucherPin
                    });

                    _log.LogInformation(voucherPin, "EasyLoad Reverse Response [Error]: {reversalResponse}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(reversalResponse));

                    return failedResponse;
                }
            }

        }
        catch (Exception ex)
        {

            var response = new EasyLoadProviderVoucherResponse()
            {
                ResponseMessage = $"EasyLoad Redeem Request Failed.",
            };

            _log.LogError(ex, null, "EasyLoad Redeem Response [Error]: {response}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  JsonConvert.SerializeObject(response));

            var reversalResponse = await ReverseVoucher(new EasyLoadProviderVoucherRequest()
            {
                CustomerAccount = clientId,
                VoucherNumber = voucherPin
            });

            _log.LogInformation(voucherPin, "EasyLoad Reverse Response [Error]: {reversalResponse}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(reversalResponse));

            return response;
        }
    }

    private async Task<EasyLoadProviderVoucherReversalResponse?> ReverseVoucher(EasyLoadProviderVoucherRequest voucherRequest)
    {
        try
        {
            var requestContent = JsonConvert.SerializeObject(voucherRequest);

            var headers = new List<KeyValuePair<string, string>>
            {
                new("ApiKey", _easyLoadSettings.ApiKey)
            };

            string postUrl = UrlHelper.CombineUrls(_easyLoadSettings.BaseUrl, _easyLoadSettings.ReverseVoucherUrl);

            var response = await _httpClientCommunication.SendRequestAsync(postUrl, Domain.Models.Enums.HttpMethod.POST, requestContent, "application/json", headers);
            _log.LogInformation(null, "EasyLoad Reverse Request: {request}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  requestContent);

            if (response.IsSuccessStatusCode)
            {
                // Read the response content as a string
                var responseBody = await response.Content.ReadAsStringAsync();
                _log.LogInformation(null, "EasyLoad Reverse Response: {response}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  responseBody );

                return JsonConvert.DeserializeObject<EasyLoadProviderVoucherReversalResponse>(responseBody);
            }
            else
            {
                _log.LogError(null, null, "HttpResponse is not Success", args: Array.Empty<object>());
                return new EasyLoadProviderVoucherReversalResponse()
                {
                    Message = $"EasyLoad Reverse Request Failed.",
                };
            }
        }
        catch (Exception ex)
        {

            var response = new EasyLoadProviderVoucherReversalResponse()
            {
                Message = $"EasyLoad Reverse Request Failed.",
            };

            _log.LogError(ex, null, "EasyLoad Reverse Response [Error]: {response}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  JsonConvert.SerializeObject(response));

            return response;
        }
    }
}
