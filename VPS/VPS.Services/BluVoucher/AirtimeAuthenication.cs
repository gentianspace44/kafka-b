using Newtonsoft.Json;
using System.Reflection;
using VPS.Domain.Models.BluVoucher.Requests;
using VPS.Domain.Models.BluVoucher.Responses;
using VPS.Helpers;
using VPS.Helpers.Logging;

namespace VPS.Services.BluVoucher
{
    public class AirtimeAuthentication : IAirtimeAuthentication
    {
        private readonly ILoggerAdapter<AirtimeAuthentication> _log;
        private readonly IGetStreamResults _getStreamResults;
        public AirtimeAuthentication(ILoggerAdapter<AirtimeAuthentication> log, IGetStreamResults getStreamResults)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _getStreamResults = getStreamResults ?? throw new ArgumentNullException(nameof(getStreamResults));
        }

        public async Task<AirtimeAuthenticationResponse?> Authenticate(BluVoucherProviderAuthenticationRequest authenticationRequestModel)
        {
            try
            {
                var authenticationRequest = new AuthenticationRequest()
                {
                    EventType = authenticationRequestModel.EventType,
                    RequestEvent = new RequestEvent()
                    {
                        DeviceId = authenticationRequestModel.UserTerminal.DeviceId,
                        DeviceSer = authenticationRequestModel.UserTerminal.DeviceSerial,
                        Reference = authenticationRequestModel.Reference,
                        TransType = "BluVoucher",
                        UserPin = authenticationRequestModel.Password
                    }
                };

                _log.LogInformation(null, "Reference: {reference} - Authenticate request: {requestpayload}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, authenticationRequestModel.Reference, JsonConvert.SerializeObject(authenticationRequest));

                var result = await _getStreamResults.GetResults(authenticationRequest, authenticationRequestModel.NetworkStream!);
                if (string.IsNullOrWhiteSpace(result))
                {
                    _log.LogInformation(null, "Reference: {reference} - Authenticate: Result is empty", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, authenticationRequestModel.Reference);
                    return null;
                }

                return XmlConvert.Deserialize<AirtimeAuthenticationResponse>(result);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, null, "Reference :{reference}, {message}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, authenticationRequestModel.Reference, ex.Message);
                return null;
            }
        }
    }
}
