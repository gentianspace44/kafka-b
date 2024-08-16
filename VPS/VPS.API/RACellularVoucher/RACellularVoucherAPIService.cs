using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Prometheus;
using System.Reflection;
using System.Text;
using VPS.API.Common;
using VPS.Domain.Models.Configurations.RACellularVoucher;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.RACellularVoucher.Requests;
using VPS.Domain.Models.RACellularVoucher.Response;
using VPS.Helpers;
using VPS.Helpers.Logging;

namespace VPS.API.RACellularVoucher;

public class RaCellularVoucherApiService : IraCellularVoucherApiService
{
    private readonly ILoggerAdapter<RaCellularVoucherApiService> _log;
    private readonly IHttpClientCommunication _httpClientCommunication;
    private readonly RACellularVoucherConfiguration _raConfiguration;
    private readonly MetricsHelper _metricsHelper;
    private const int DefaultVoucherLength = 11;

    public RaCellularVoucherApiService(
        ILoggerAdapter<RaCellularVoucherApiService> log,
        IHttpClientCommunication httpClientCommunication,
        IOptions<RACellularVoucherConfiguration> raCellularVoucherConfiguration,
        MetricsHelper metricsHelper)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _httpClientCommunication = httpClientCommunication ?? throw new ArgumentNullException(nameof(httpClientCommunication));
        _raConfiguration = raCellularVoucherConfiguration?.Value ?? throw new ArgumentNullException(nameof(raCellularVoucherConfiguration));
        _metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
    }

    public async Task<RaCellularVoucherRedeemResponse> RedeemVoucherAsync(string clientId, string voucherPin, string platform)
    {
        try
        {
            var postUrl = GetRAVoucherPostUrl(_raConfiguration?.RedeemVoucherUrl!);

            var requestContent = GetRequestContent(clientId, voucherPin, RAEndPointType.RedeemVoucher);

            _log.LogInformation(voucherPin, "R&A Redeem Request: {requestContent} ", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, requestContent);

            var response = await SendRequestAsync(requestContent, postUrl);

            _log.LogInformation(voucherPin, "R&A Redeem Response: {response} ", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, response);

            return await ProcessResponseAsync(voucherPin, response);
        }
        catch (Exception ex)
        {
            return HandleRedeemException(ex);
        }
    }

    public async Task<RaCellularVoucherRedeemResponse> AdviceAsync(string clientId, string voucherPin, string platform)
    {
        try
        {
            var postUrl = GetRAVoucherPostUrl(_raConfiguration.AdviceUrl);

            var requestContent = GetRequestContent(clientId, voucherPin, RAEndPointType.Advice);

            var response = await SendRequestAsync(requestContent, postUrl);

            _log.LogInformation(voucherPin, "R&A Advice Request: {requestContent} ", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, requestContent);

            return response.IsSuccessStatusCode
                ? await ProcessResponseAsync(voucherPin, response)
                : await CallAdviceAsync(clientId, voucherPin, platform);
        }
        catch (Exception ex)
        {
            return HandleAdviceException(ex);
        }
    }

    public async Task<RaCellularVoucherRedeemResponse> LookUpVoucherAsync(string clientId, string voucherPin)
    {
        try
        {
            var postUrl = GetRAVoucherPostUrl(_raConfiguration.VoucherLookupUrl);

            var requestContent = GetRequestContent(clientId, voucherPin, RAEndPointType.LookUpVoucher);

            var response = await SendRequestAsync(requestContent, postUrl);

            _log.LogInformation(voucherPin, "R&A Look up voucher Request: {requestContent} ", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                HandleLookUpResponseFailure(voucherPin, response);
            }

            return await ProcessResponseAsync(voucherPin, response);
        }
        catch (Exception ex)
        {
            return HandleLookUpException(ex);
        }
    }

    private string GetRAVoucherPostUrl(string endpoint)
        => UrlHelper.CombineUrls(_raConfiguration.BaseUrl, endpoint);

    private async Task<HttpResponseMessage> SendRequestAsync(string requestContent, string postUrl)
    {
        var headers = GetHeaders();

        int noOfAttempts = 0;

        AsyncRetryPolicy<HttpResponseMessage> _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(x => !x.IsSuccessStatusCode || x.StatusCode != System.Net.HttpStatusCode.OK)
        .WaitAndRetryAsync(
                int.Parse(_raConfiguration.MaxPolyRetry),
                retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(10, noOfAttempts)),
                onRetry: (exception, _) =>
                {
                    noOfAttempts++;
                    _log.LogInformation(null, "Calling SendRequestAsync with payload: {requestContent}, Retry Attempt: {noOfAttempts}",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            requestContent,
                            "R&AVoucher",
                            noOfAttempts
                       );

                    if (!string.IsNullOrWhiteSpace(exception?.Exception?.Message))
                    {
                        _log.LogError(null, "Calling SendRequestAsync with payload: {requestContent}, Retry Attempt: {noOfAttempts}. Error: {exception}",
                       MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            requestContent,
                            "R&AVoucher",
                            noOfAttempts,
                            exception
                        );
                    }
                }
            );

        using (_metricsHelper.providerRAVoucherApiResponse.NewTimer())
        {
            return await _retryPolicy.ExecuteAsync(() => _httpClientCommunication.SendRequestAsync(postUrl, Domain.Models.Enums.HttpMethod.POST, requestContent, "application/json", headers));
        }
    }

    private static RaCellularProviderVoucherRequest GetAdviceRequest(string clientId)
        => new()
        {
            terminalID = clientId,
            msgID = Guid.NewGuid().ToString("N"),
            adviceMsgID = Guid.NewGuid().ToString("N")
        };

    private RaCellularProviderVoucherRequest GetRAVoucherRedeemRequest(string clientId, string voucherPin)
        => new()
        {
            pinNumber = voucherPin,
            terminalID = clientId,
            msgID = Guid.NewGuid().ToString("N"),
            reason = "voucher redemption",
            terminalOperator = _raConfiguration.TerminalOperator
        };

    private static RaCellularProviderVoucherRequest GetLookUpRequest(string clientId, string voucherPin)
        => new()
        {
            terminalID = clientId,
            pinNumber = voucherPin,
            msgID = Guid.NewGuid().ToString("N")
        };

    private List<KeyValuePair<string, string>> GetHeaders()
    {
        var base64String = GetBase64String();
        return new()
        {
           new KeyValuePair<string, string>("Authorization", $"Basic {base64String}"),
        };
    }

    private string GetRequestContent(string clientId, string voucherPin, RAEndPointType type)
    {
        ValidateInput(clientId, voucherPin);

        var voucherRequest = type switch
        {
            RAEndPointType.Advice => GetAdviceRequest(clientId),
            RAEndPointType.LookUpVoucher => GetLookUpRequest(clientId, voucherPin),
            RAEndPointType.RedeemVoucher => GetRAVoucherRedeemRequest(clientId, voucherPin),
            _ => throw new NotSupportedException($"Unsupported type: {type}"),
        };

        return JsonConvert.SerializeObject(voucherRequest);
    }

    private string GetBase64String()
    {
        byte[] bytes = Encoding.UTF8.GetBytes($"{_raConfiguration.RandAUsername}:{_raConfiguration.RandAPassword}");
        return Convert.ToBase64String(bytes);
    }

    private async Task<RaCellularVoucherRedeemResponse> ProcessResponseAsync(string voucherPin, HttpResponseMessage response)
    {
        var responseBody = await response.Content.ReadAsStringAsync();
        _log.LogInformation(voucherPin, "R&A Redeem Response: {response}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, responseBody);

        return JsonConvert.DeserializeObject<RaCellularVoucherRedeemResponse>(responseBody)!;
    }

    private RaCellularVoucherRedeemResponse HandleAdviceException(Exception ex)
    {
        var response = new RaCellularVoucherRedeemResponse
        {
            FaultMsg = "R&A Advice Request Failed.",
        };

        _log.LogError(ex, null, "R&A Advice Response [Error]: {response}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(response));

        return response;
    }

    private RaCellularVoucherRedeemResponse HandleRedeemException(Exception ex)
    {
        var response = new RaCellularVoucherRedeemResponse()
        {
            FaultMsg = "R&A Redeem Request Failed.",
        };

        _log.LogError(ex, null, "R&A Redeem Response [Error]: {response}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(response));

        return response;
    }

    private RaCellularVoucherRedeemResponse HandleLookUpException(Exception ex)
    {
        var response = new RaCellularVoucherRedeemResponse()
        {
            FaultMsg = "R&A Lookup Request Failed.",
        };

        _log.LogError(ex, null, "R&A Lookup voucher Response [Error]: {response}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(response));

        return response;
    }

    private void HandleLookUpResponseFailure(string voucherPin, HttpResponseMessage response)
    {
        _log.LogInformation(voucherPin, "R&A Look up voucher Request was not successful: {response} ", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, response);
        throw new Exception("R&A Look up voucher Request was not successful")!;
    }

    /// <summary>
    // This function will be called only if one of the following conditions are met.
    // No response is received from a Revenue Request (for example due to Timeout)
    // A response is received to a Revenue requests with hasFault=true and mustALR=true
    // A response is received to any request (not just Revenue transactions) where hasFault=true and
    // the fault is of the type “IncompleteTransactionFault” which specifies the MsgID of the locking transaction.
    // No response is received from an Advice Request
    // A response is received to an Advice Request with hasFault=true and mustALR=true
    /// </summary>
    private async Task<RaCellularVoucherRedeemResponse> CallAdviceAsync(string clientId, string voucherPin, string platform)
    {
        var adviceRequest = await AdviceAsync(clientId, voucherPin, platform);

        _log.LogInformation(voucherPin, "R&A HttpResponse was not Successful, Advice was called : {adviceRequest}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, adviceRequest);

        return adviceRequest;
    }

    private void ValidateInput(string clientId, string voucherPin)
    {
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(voucherPin))
        {
            _log.LogError(null, "R&A request input values are invalid {clientId}, {voucherPin}: ", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, clientId, voucherPin);
            throw new Exception("R&A Request was not successful")!;
        }

        if (voucherPin.Length != DefaultVoucherLength)
        {
            _log.LogError(null, "R&A voucher is invalid, {voucherPin}: ", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, voucherPin);
            throw new Exception("R&A Request was not successful")!;
        }
    }
}
