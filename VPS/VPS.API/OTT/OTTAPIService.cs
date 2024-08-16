using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Prometheus;
using System.Reflection;
using System.Text;
using VPS.API.Common;
using VPS.Domain.Models.Common.Request;
using VPS.Domain.Models.Configurations.OTT;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.OTT.Requests;
using VPS.Domain.Models.OTT.Responses;
using VPS.Helpers;
using VPS.Helpers.Logging;

namespace VPS.API.OTT;

public class OttApiService : IOttApiService
{

    private readonly ILoggerAdapter<OttApiService> _log;
    private readonly IHttpClientCommunication _httpClientCommunication;
    private readonly OttVoucherConfiguration _ottVoucherSettings;
    private readonly MetricsHelper _metricsHelper;

    public OttApiService(ILoggerAdapter<OttApiService> log, IOptions<OttVoucherConfiguration> ottVoucherSettings, IHttpClientCommunication httpClientCommunication, MetricsHelper metricsHelper)
    {
        _log = log ?? throw new NotImplementedException(nameof(log));
        _ottVoucherSettings = ottVoucherSettings.Value;
        _httpClientCommunication = httpClientCommunication ?? throw new ArgumentNullException(nameof(httpClientCommunication));
        this._metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
    }

    /// <summary>
    /// /redeem a OTT Voucher
    /// </summary>
    /// <param name="uniqueReference"></param>
    /// <param name="voucherRedeemRequest"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<OttProviderVoucherResponse> RemitOTTVoucher(string uniqueReference, VoucherRedeemRequestBase voucherRedeemRequest)
    {

        try
        {
            var hashValue = EncryptionHelper.Sha256Hash($"{_ottVoucherSettings.OTTApiKey}0{voucherRedeemRequest.ClientId}{voucherRedeemRequest.VoucherNumber}{uniqueReference}{_ottVoucherSettings.OTTVendorId}");

            var remitVoucherRequest = new OttProviderVoucherRequest()
            {
                Amount = "0",
                ClientId = voucherRedeemRequest.ClientId,
                Pin = voucherRedeemRequest.VoucherNumber,
                UniqueReference = uniqueReference,
                VendorID = _ottVoucherSettings.OTTVendorId,
                Hash = hashValue
            };

            var username = _ottVoucherSettings.OTTUsername;
            var password = _ottVoucherSettings.OTTPassword;
            var encodedAuth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

            var headers = new List<KeyValuePair<string, string>>
               {
                  new KeyValuePair<string, string>("Authorization", $"Basic {encodedAuth}")
                };

            string postUrl = UrlHelper.CombineUrls(_ottVoucherSettings.OTTBaseUrl, _ottVoucherSettings.OTTRemitVoucherUrl);

            var parameter = $"amount={remitVoucherRequest.Amount}&clientId={remitVoucherRequest.ClientId}" +
                $"&pin={remitVoucherRequest.Pin}&uniqueReference={remitVoucherRequest.UniqueReference}" +
                $"&vendorID={remitVoucherRequest.VendorID}&hash={remitVoucherRequest.Hash}";

            int noOfAttempts = 0;

            AsyncRetryPolicy _retryPolicy = Policy
                .Handle<Exception>()
            .WaitAndRetryAsync(
                    _ottVoucherSettings.MaxPolyRetry,
                    retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(10, noOfAttempts)),
                    onRetry: (exception, _) =>
                    {
                        noOfAttempts++;
                        _log.LogInformation(voucherRedeemRequest.VoucherNumber, "Calling SendRequestAsync with payload: {voucherRedeemRequest}, for voucher type {voucherType} Retry Attempt: {noOfAttempts}",

                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                                voucherRedeemRequest,
                                "OTTVoucher",
                                noOfAttempts
                            );

                        if (!string.IsNullOrWhiteSpace(exception.Message))
                        {
                            _log.LogError(voucherRedeemRequest.VoucherNumber, "Calling SendRequestAsync with payload: {voucherRedeemRequest}, for voucher type {voucherType} Retry Attempt: {noOfAttempts}. Error: {exception}",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,                            
                                voucherRedeemRequest,
                                "OTTVoucher",
                                noOfAttempts,
                                exception
                            );
                        }
                    }
                );

            using (_metricsHelper.providerOTTApiResponse.NewTimer())
            {
                var response = await _retryPolicy.ExecuteAsync(() => _httpClientCommunication.SendRequestAsync(postUrl, Domain.Models.Enums.HttpMethod.POST, parameter, "application/x-www-form-urlencoded", headers));

                _log.LogInformation(null, "OTTVoucher Redeem Request: {request}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  parameter );
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<OttProviderVoucherResponse>(responseContent) 
                        ?? new OttProviderVoucherResponse
                    {
                        ErrorCode = 1,
                        Message = "Unable to desterilize content",
                        Success = false
                    }; 
                }
                else
                {
                    _log.LogError(null, null, "HttpResponse is not Success", args: Array.Empty<object>());
                    return new OttProviderVoucherResponse
                    {
                        ErrorCode = 1,
                        Message = $"HTTP error: {response.StatusCode}",
                        Success = false
                    };
                }
            }
        }
        catch (Exception)
        {

            var resp = new OttProviderVoucherResponse
            {
                ErrorCode = 1,
                Message = $"OTT Redeem Request Failed",
                Success = false
            };

            _log.LogInformation(null, $"OTT Redeem Response [Error] : {resp}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  JsonConvert.SerializeObject(resp));

            return resp;
        }
    }

    public async Task<OttProviderVoucherResponse?> CheckRemitResponse(string uniqueReference)
    {
        try
        {
            var hashValue = EncryptionHelper.Sha256Hash($"{_ottVoucherSettings.OTTApiKey}{uniqueReference}");

            var username = _ottVoucherSettings.OTTUsername;
            var password = _ottVoucherSettings.OTTPassword;
            var encodedAuth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

            var headers = new List<KeyValuePair<string, string>>
               {
                  new KeyValuePair<string, string>("Authorization", $"Basic {encodedAuth}")
                };

            string postUrl = UrlHelper.CombineUrls(_ottVoucherSettings.OTTBaseUrl, _ottVoucherSettings.OTTCheckRemitVoucherUrl);
            var parameter = $"uniqueReference={uniqueReference}&hash={hashValue}";
            var response = await _httpClientCommunication.SendRequestAsync(postUrl, Domain.Models.Enums.HttpMethod.POST, parameter, "application/x-www-form-urlencoded", headers);
            _log.LogInformation(null, $"CheckRemitVoucher Response :{JsonConvert.SerializeObject(response)}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<OttProviderVoucherResponse>(responseContent);

            }
            else
            {
                _log.LogInformation(null, $"CheckRemitVoucher Response [Error]:", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(response));
                return new OttProviderVoucherResponse
                {
                    ErrorCode = 1,
                    Message = $"HTTP error: {response.StatusCode}",
                    Success = false
                };
            }
        }
        catch (Exception ex)
        {

            var response = new OttProviderVoucherResponse()
            {
                ErrorCode = 1,
                Message = $"{ex.Message} {ex.StackTrace}",
                Success = false
            };
            _log.LogInformation(null, $"OTT Redeem Response [Error] : {response}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(response));

            return response;
        }
    }
}
