using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Prometheus;
using System.Reflection;
using VPS.Domain.Models.BluVoucher;
using VPS.Domain.Models.BluVoucher.Enums;
using VPS.Domain.Models.BluVoucher.Requests;
using VPS.Domain.Models.BluVoucher.Responses;
using VPS.Domain.Models.Configurations.BluVoucher;
using VPS.Helpers;
using VPS.Helpers.Logging;

namespace VPS.Services.BluVoucher
{
    public class RemitBluVoucherService : IRemitBluVoucherService
    {
        private readonly ILoggerAdapter<RemitBluVoucherService> _log;
        private readonly BluVoucherConfiguration _bluVoucherSettings;
        private readonly ITcpClient _tcpClient;
        private readonly IAirtimeAuthentication _airtimeAuthentication;
        private readonly IGetStreamResults _getStreamResults;
        private readonly MetricsHelper _metricsHelper;

        public RemitBluVoucherService(ILoggerAdapter<RemitBluVoucherService> log,
                            IOptions<BluVoucherConfiguration> bluVoucherSettings,
                            ITcpClient tcpClient,
                            IAirtimeAuthentication airtimeAuthentication,
                            IGetStreamResults getStreamResults,
                            MetricsHelper metricsHelper)
        {
            this._log = log ?? throw new ArgumentNullException(nameof(log));
            this._bluVoucherSettings = bluVoucherSettings?.Value ?? throw new ArgumentNullException(nameof(bluVoucherSettings));
            this._tcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
            this._airtimeAuthentication = airtimeAuthentication ?? throw new ArgumentNullException(nameof(airtimeAuthentication));
            this._getStreamResults = getStreamResults ?? throw new ArgumentNullException(nameof(getStreamResults));
            this._metricsHelper = metricsHelper ?? throw new ArgumentNullException(nameof(metricsHelper));
        }
        public async Task<BluVoucherProviderResponse> RemitBluVoucher(string reference, string voucherPin)
        {

            try
            {

                #region defensive steps

                var userTerminal = new Terminal
                {
                    DeviceId = _bluVoucherSettings.DeviceId,
                    DeviceSerial = _bluVoucherSettings.DeviceSerial
                };

                if (userTerminal.DeviceId.ToUpper() == "DISABLED" || userTerminal.DeviceSerial.ToUpper() == "DISABLED")
                {
                    _log.LogInformation(null, "RemitVoucher request:{reference}, User terminal disabled. Device Id :{deviceId}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, reference, userTerminal.DeviceId);

                    return BuildVoucherResponse(0, 0, false, -1, "User Terminal Disabled");
                }

                if (string.IsNullOrWhiteSpace(_bluVoucherSettings.RemoteServer) ||
                string.IsNullOrWhiteSpace(_bluVoucherSettings.RemoteUsername) ||
                  string.IsNullOrWhiteSpace(_bluVoucherSettings.RemotePassword))
                {
                    _log.LogInformation(null, "RemitVoucher request: Outlet details are incorrect or missing - remote server {remoteServer} , " +
                        "remote username {remoteUser}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, _bluVoucherSettings.RemoteServer, _bluVoucherSettings.RemoteUsername);

                    return BuildVoucherResponse(0, 0, false, -1, "No valid outlet details found");

                }

                #endregion

                using (_metricsHelper.providerBlueVoucherApiResponse.NewTimer())
                {
                    await _tcpClient.ConnectAsync(_bluVoucherSettings.RemoteServer, Convert.ToInt32(_bluVoucherSettings.RemoteUsername));

                    using var stream = _tcpClient.GetStream();
                    var authenticationRequestModel = new BluVoucherProviderAuthenticationRequest
                    {
                        EventType = "Authentication",
                        TransType = "BluVoucher",
                        NetworkStream = stream,
                        UserTerminal = userTerminal,
                        Reference = reference,
                        Password = _bluVoucherSettings.RemotePassword
                    };

                    //Object used just for logging without network stream
                    var authRequestLogModel = new BluVoucherAuthenticationRequestBase
                    {
                        EventType = authenticationRequestModel.EventType,
                        TransType = authenticationRequestModel.TransType,
                        UserTerminal = authenticationRequestModel.UserTerminal,
                        Reference = authenticationRequestModel.Reference,
                        Password = authenticationRequestModel.Password
                    };

                    _log.LogInformation(null, "RemitBluVoucher authenticateRequest: {0}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(authRequestLogModel));

                    if (authenticationRequestModel == null) throw new FormatException("Empty authenticationRequestModel");

                    var authenticateResponse = await _airtimeAuthentication.Authenticate(authenticationRequestModel);

                    _log.LogInformation(null, "RemitBluVoucher authenticateResponse: {0}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(authenticateResponse));

                    if (string.IsNullOrEmpty(authenticateResponse?.SessionId))
                    {
                        _log.LogInformation(null, "RemitBluVoucher request:{0}, SessionId is null", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, reference);
                        return BuildVoucherResponse(0, 0, false, -1, "Invalid request");
                    }

                    var airtimeVoucherStatusRequest = new BluLabelProviderRequest()
                    {
                        EventType = "getVoucherStatus",
                        SessionId = authenticateResponse.SessionId,
                        Event = new AirtimeRequestEvent()
                        {
                            Reference = reference,
                            VoucherPin = voucherPin
                        }
                    };

                    _log.LogInformation(null, "RemitBluVoucher request:{0},{1}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, reference, JsonConvert.SerializeObject(airtimeVoucherStatusRequest));

                    var getVoucherStatusRequest = await _getStreamResults.GetResults(airtimeVoucherStatusRequest, stream);

                    _log.LogInformation(null, "RemitBluVoucher getVoucherStatusRequest: {0}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(authenticateResponse));

                    var voucherStatusResult = XmlConvert.Deserialize<BluVoucherProviderXmlResponse>(getVoucherStatusRequest);

                    _log.LogInformation(null, "RemitBluVoucher getVoucherStatusResponse: {0}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(voucherStatusResult));

                    if (voucherStatusResult.Data?.Status?.Code != ((int)BluVoucherStatus.Active).ToString())
                    {
                        return BuildVoucherResponse(0, 0, false, -1, $"{voucherStatusResult.Data?.Status?.Description}");
                    }
                    // only proceed on event code 0 and active status of voucher
                    if (voucherStatusResult.Data.Status.Code == ((int)BluVoucherStatus.Active).ToString() && voucherStatusResult.Event?.EventCode == ((int)BluVoucherEventCode.Success).ToString())
                    {
                        var airtimeRequest = new BluLabelProviderRequest()
                        {
                            EventType = "redeemVoucher",
                            SessionId = authenticateResponse.SessionId,
                            Event = new AirtimeRequestEvent()
                            {
                                Reference = reference,
                                Amount = voucherStatusResult.Data.Status.Amount.ToString(),
                                Pin = voucherPin
                            }
                        };

                        _log.LogInformation(null, "RemitVoucher redeemVoucherRequest : {airtimeRequest}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(airtimeRequest));

                        var redeemVoucherRequestResult = await _getStreamResults.GetResults(airtimeRequest, stream);

                        _log.LogInformation(null, "RemitVoucher redeemVoucherRequestResult : {redeemVoucherRequestResult}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, redeemVoucherRequestResult);

                        var redeemBluVoucherResponse = XmlConvert.Deserialize<BluVoucherProviderXmlResponse>(redeemVoucherRequestResult);

                        if (redeemBluVoucherResponse.Data?.Status?.Code == ((int)BluVoucherRedeemStatus.Successful).ToString() &&
                           redeemBluVoucherResponse.Event?.EventCode == ((int)BluVoucherEventCode.Success).ToString())
                        {
                            return BuildVoucherResponse(Convert.ToInt64(redeemBluVoucherResponse.Data.Status.RedemtionTransRef), redeemBluVoucherResponse.Data.Status.Amount, true, 1, $"{voucherStatusResult.Data.Status.Description}");
                        }

                        // means some error occurred here check the logs

                    }

                    return BuildVoucherResponse(0, 0, false, -1, "invalid request");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, null, "RemitBluVoucher {reference} {voucherPin} an error has occurred {errorMessage}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, reference, voucherPin, ex.Message);
                return BuildVoucherResponse(0, 0, false, -1, "invalid request");

            }
        }

        private static BluVoucherProviderResponse BuildVoucherResponse(long voucherId, decimal voucherAmount, bool success, int errorCode, string message)
        {
            return new BluVoucherProviderResponse
            {
                VoucherID = voucherId,
                Success = success,
                VoucherAmount = voucherAmount,
                ErrorCode = errorCode,
                Message = message
            };

        }
    }
}
