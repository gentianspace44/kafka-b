using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Prometheus;
using System.Net;
using System.Reflection;
using VPS.API.Common;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Common.Request;
using VPS.Domain.Models.Common.Response;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Helpers;
using VPS.Helpers.Logging;


namespace VPS.API.Syx;
public class SyxApiService : ISyxApiService
{

    private readonly SyxSettings _syxSettings;
    private readonly ILoggerAdapter<SyxApiService> _log;
    private readonly IHttpClientCommunication _httpClientCommunication;
    private readonly MetricsHelper _metricsHelper;
    private readonly VpsControlCenterEndpoints _vpsControlCenterEndpoints;

    public SyxApiService(IOptions<SyxSettings> syxSettings,
        ILoggerAdapter<SyxApiService> log,
        IHttpClientCommunication httpClientCommunication,
        MetricsHelper metricsHelper,
        IOptions<VpsControlCenterEndpoints> vpsControlCenterEndpoints)
    {
        this._syxSettings = syxSettings.Value;
        this._log = log;
        this._httpClientCommunication = httpClientCommunication ?? throw new ArgumentNullException(nameof(httpClientCommunication));
        this._metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
        this._vpsControlCenterEndpoints = vpsControlCenterEndpoints.Value;
    }

    public async Task<ApiLoginResponse?> Login(string username, string password)
    {
        try
        {
            var loginProfile = new ApiLoginRequest() { username = username, password = password };
            var requestContent = JsonConvert.SerializeObject(loginProfile);

            string postUrl = UrlHelper.CombineUrls(_syxSettings.SyxEndPoint, "Account/LoginUser");

            int noOfAttempts = 0;

            AsyncRetryPolicy _retryPolicy = Policy
                .Handle<Exception>()
            .WaitAndRetryAsync(
                    _syxSettings.MaxPolyRetry,
                    retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(10, noOfAttempts)),
                    onRetry: (exception, _) =>
                    {
                        noOfAttempts++;
                        _log.LogInformation(username, "Calling SendRequestAsync with payload: {loginProfile}, Retry Attempt: {noOfAttempts}",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                                loginProfile,
                                noOfAttempts
                            );

                        if (!string.IsNullOrWhiteSpace(exception.Message))
                        {
                            _log.LogError(username, "Calling SendRequestAsync with payload: {loginProfile}, Retry Attempt: {noOfAttempts}. Error: {exception}",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty, 
                                loginProfile,
                                noOfAttempts,
                                exception
                            );
                        }
                    }
                );

            var response = await _retryPolicy.ExecuteAsync(() => _httpClientCommunication.SendRequestAsync(postUrl, Domain.Models.Enums.HttpMethod.POST, requestContent, "application/json"));

            // Read the response content as a string
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.Accepted)
            {
                throw new HttpRequestException(response.StatusCode.ToString(), new Exception(responseBody));
            }

            return JsonConvert.DeserializeObject<ApiLoginResponse>(responseBody);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, null, "Failed to login user {username}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, username );
            throw;
        }
    }

    public async Task<bool> HealthCheck()
    {
        string postUrl = UrlHelper.CombineUrls(_syxSettings.SyxEndPoint, "Health/Check");

        var response = await _httpClientCommunication.SendRequestAsync(postUrl, Domain.Models.Enums.HttpMethod.GET);

       
        if (!response.IsSuccessStatusCode)
        {
            _log.LogError(null, "Syx API HealthCheck failed with IsSuccessStatusCode: {IsSuccessStatusCode}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, response.IsSuccessStatusCode);
            return false;
        }
        return true;
    }

    public async Task<ApiClientLoginResponse?> LoginClient(string accountNumber, string password)
    {
        try
        {
            var syxTokenResponse = await GetSyXSessionToken();

            var loginProfile = new ApiClientLoginRequest() { SessionToken = syxTokenResponse == null? string.Empty: syxTokenResponse.SyxSessionToken , ClientAccountNumber = accountNumber, ClientPin = password };

            var requestContent = JsonConvert.SerializeObject(loginProfile);

            string postUrl = UrlHelper.CombineUrls(_syxSettings.SyxEndPoint, "Account/LoginUser");

            var headers = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("SessionToken", loginProfile.SessionToken)
            };

            var response = await _httpClientCommunication.SendRequestAsync(postUrl, Domain.Models.Enums.HttpMethod.POST, requestContent, "application/json", headers);

            // Read the response content as a string
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.Accepted)
            {

                throw new HttpRequestException(response.StatusCode.ToString(), new Exception(responseBody));

            }

            return JsonConvert.DeserializeObject<ApiClientLoginResponse>(responseBody);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, null, "Failed to login user {accountNumber}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, accountNumber);
            throw;
        }
    }

    public async Task<ApiClientBalanceUpdateResponse?> UpdateClientBalance(long clientId, int transactionTypeId, decimal transactionAmount, int branchId, string referenceComments)
    {
        try
        {
            var syxTokenResponse = await GetSyXSessionToken();

            if(syxTokenResponse == null)
            {
                throw new FormatException("Null session token returned");
            }

            var payload = new ApiUpdateClientBalanceRequest()
            {
                SessionToken = syxTokenResponse.SyxSessionToken,
                ClientID = clientId,
                ClientTransactionTypeID = transactionTypeId,
                TransactionAmount = -transactionAmount,
                ReferenceComment = referenceComments,
                BranchID = branchId,
                UserID = syxTokenResponse.SyxUserId,
                BetID = null,
                PendingBalanceChange = null,
                TaxRate = null,
                UpdateClientBalances = true
            };

            var requestContent = JsonConvert.SerializeObject(payload);

            var headers = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("SessionToken", payload.SessionToken)
            };

            string postUrl = UrlHelper.CombineUrls(_syxSettings.SyxEndPoint, "Client/UpdateClientBalance");

            using (_metricsHelper.syxAPIResponse.NewTimer())
            {
                var response = await _httpClientCommunication.SendRequestAsync(postUrl, Domain.Models.Enums.HttpMethod.POST, requestContent, "application/json", headers);

                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a string
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<ApiClientBalanceUpdateResponse>(responseBody);
                    _log.LogInformation(null, "status code: {StatusCode}, response message: {ResponseMessage}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, response.StatusCode, responseObject == null ? string.Empty : responseObject.ResponseMessage);
                    return responseObject;
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _log.LogError(null, "Response is not success with message: {message}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, responseBody);
                    return new ApiClientBalanceUpdateResponse
                    {
                        ResponseType = -1,
                        ResponseMessage = $"Error 6: An Error occurred, please try again. {response.Content}",
                        ResponseObject = new ApiClientBalance
                        {
                            BalanceAvailable = 0,
                            BalancePending = 0,
                        }
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, null, "Failed to login user {clientId}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, clientId);
            throw;
        }
    }


    public async Task<ApiVoucherExistsResponse?> CheckVoucherExists(long clientId, string reference)
    {
        var syxTokenResponse = await GetSyXSessionToken();

        if(syxTokenResponse == null)
        {
            throw new FormatException("Null Token returned");
        }

        var requestContent = JsonConvert.SerializeObject(new ApiVoucherExistsRequest { ClientID = clientId, Reference = reference });

        var headers = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("SessionToken", syxTokenResponse.SyxSessionToken)
        };

        string postUrl = UrlHelper.CombineUrls(_syxSettings.SyxEndPoint, "Client/CheckClientVoucherExists");

        var response = await _httpClientCommunication.SendRequestAsync(postUrl, Domain.Models.Enums.HttpMethod.POST, requestContent, "application/json", headers);

        if (response.IsSuccessStatusCode)
        {
            // Read the response content as a string
            var responseBody = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<ApiVoucherExistsResponse>(responseBody);
            _log.LogInformation(null, "status code: {StatusCode}, response message: {ResponseMessage}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, response.StatusCode, responseObject?.ResponseMessage );
            return responseObject;
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _log.LogError(null, "Response is not success with message: {message}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, responseBody);
            return new ApiVoucherExistsResponse
            {
                ResponseType = -1,
                ResponseMessage = $"Error 6: An Error occurred, please try again. {response.Content}",
            };
        }
    }

    #region Private Util methods
    private async Task<SyxSessionModel?> GetSyXSessionToken()
    {
        try
        {
            //Get SessionToken from VPS Control Center
            var vpsControlCenterResponse = await _httpClientCommunication.SendRequestAsync(_vpsControlCenterEndpoints.GetSyxToken, Domain.Models.Enums.HttpMethod.GET);

            var vpsResponseBody = await vpsControlCenterResponse.Content.ReadAsStringAsync();

            if (!vpsControlCenterResponse.IsSuccessStatusCode || String.IsNullOrWhiteSpace(vpsResponseBody))
            {
                _log.LogCritical(null, "SyxAPIService", "Failed to get SyX Session Token from VPS Control Center");
                throw new InvalidOperationException(vpsControlCenterResponse.StatusCode.ToString(), new Exception(vpsResponseBody));

            }

            return JsonConvert.DeserializeObject<SyxSessionModel>(vpsResponseBody);
        }
        catch (Exception ex)
        {
            _log.LogCritical(ex, null, "SyxAPIService", "Failed to get SyX Session Token from VPS Control Center");
            throw;
        }
    }
    #endregion Private Util methods
}

