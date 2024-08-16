using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;
using VPS.API.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.VRW.Voucher;
using VPS.Domain.Models.VRW.VPSCrediting;
using VPS.Domain.Models.VRWEnumHelpers;
using VPS.Helpers.Logging;

namespace VPS.API.CreditingService
{
    public class CreditingService
    {

        private readonly IHttpClientCommunication _httpClientCommunication;
        private readonly ILoggerAdapter<CreditingService> _log;
        private readonly CountrySettings _countrySettings;

        public CreditingService(IHttpClientCommunication httpClientCommunication, ILoggerAdapter<CreditingService> log, IOptions<CountrySettings> countrySettings)
        {

            this._httpClientCommunication = httpClientCommunication ?? throw new ArgumentNullException(nameof(httpClientCommunication));
            this._log = log ?? throw new ArgumentNullException(nameof(log));
            this._countrySettings = countrySettings.Value;

        }



        public async Task<VrwViewModel> ProcessCredit(string endPoint, VpsCreditingServiceRequest payload, VrwViewModel model, bool useSyxCreditEndpoint = false)
        {

            var requestContent = JsonConvert.SerializeObject(payload);

            var response = await _httpClientCommunication.SendRequestAsync(endPoint, Domain.Models.Enums.HttpMethod.POST, requestContent);
            var responseBody = await response.Content.ReadAsStringAsync();
            var detail = JsonConvert.DeserializeObject<VpsCreditingServiceResponse>(responseBody);

            _log.LogInformation(model.VoucherNumber, "VRW Redemption response: {response}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, args: responseBody.ToString());

            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (!useSyxCreditEndpoint && detail?.CreditOutcome != SyxCreditOutcome.Success)
                {
                    model.ErrorMessage = detail == null ? StringConstants.GENERIC_ERROR_MESSAGE : detail.Message;
                }
                                   
                if(useSyxCreditEndpoint)
                 model.Message = $"Voucher redeemed successfully for value of: {_countrySettings.CurrencyCode}{detail?.Amount?.ToString("0,0.00")}";

                model.IsSuccessful = true;
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                model.ErrorMessage = detail?.Message ?? StringConstants.GENERIC_ERROR_MESSAGE;
            }
            else if (response.StatusCode == HttpStatusCode.RequestTimeout)
            {
                model.ErrorMessage = StringConstants.TIMEOUT_ERROR_MESSAGE;
            }
            else
            {
                model.ErrorMessage = StringConstants.TRYAGAIN_ERROR_MESSAGE;
            }

            return model;

        }

    }
}
