using FluentValidation;
using Newtonsoft.Json;
using System.Reflection;
using VPS.Domain.Models.VRW.Voucher;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Services.Common;
using VPS.Services.VRW.Interface;

namespace VPS.Services.VRW.Services
{

    public class VoucherRedemptionWidgetService : IVoucherRedemptionWidgetService
    {
        private readonly ICreditingService _creditingService;
        private readonly IValidator<VrwViewModel> _validator;
        private readonly IVoucherProviderService _voucherProviderService;
        private readonly ILoggerAdapter<VoucherRedemptionWidgetService> _log;

        public VoucherRedemptionWidgetService(ICreditingService creditingService, IValidator<VrwViewModel> validator, IVoucherProviderService voucherProviderService, ILoggerAdapter<VoucherRedemptionWidgetService> log)
        {
            _creditingService = creditingService;
            _validator = validator;
            _voucherProviderService = voucherProviderService;
            _log = log;
        }

        public async Task<VrwViewModel> RedeemVoucher(VrwViewModel model)
        {
            //Get latest data from VPS-ControlCenter
            var modifiedEnablers = await _voucherProviderService.GetProviders();

            //Set latest data on VoucherProviderHelper
            VoucherProviderHelper.SetProviders(modifiedEnablers);

            model.VouchersEnablers = VoucherProviderHelper.GetProviders()?? new List<VoucherServiceEnabler>();
            var results = _validator.Validate(model);

            if (!results.IsValid)
            {
                model.ErrorMessage = string.Join("\n", results.Errors);
                _log.LogInformation(model.VoucherNumber, "Model was invalid for request: {model}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                     JsonConvert.SerializeObject(model));
                return model;
            }

            if (model.DefaultVoucherProvider)
            {                
                return await ProcessDefaultOption(model);
            }
            else
            {
                // model.VoucherName
                var selectedProvider = VoucherProviderHelper.GetProviders()?.Find(x => x.Name.ToLower() == model.VoucherName.ToLower());
                //we return the response here

                bool useSyxCreditEndpoint = false;

                if (selectedProvider == null || !selectedProvider.IsEnabled)
                {
                    model.ErrorMessage = $"{selectedProvider?.Name} is under maintenance. Please try again later.";
                    model.IsSuccessful = false;
                    _log.LogInformation(model.VoucherNumber, "Redemption failed. Provider is under maintenance. {model}",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                         JsonConvert.SerializeObject(model));
                    return model;
                }
                else
                {

                    if (selectedProvider.UseSxyCreditEndPoint != null)
                    {
                        useSyxCreditEndpoint = (bool)selectedProvider.UseSxyCreditEndPoint;
                    }

                    model.SyxVoucherCreditingEndPoint = useSyxCreditEndpoint ? selectedProvider.SyxCreditServiceUrl?? "" : selectedProvider.MicroServiceUrl;
                    _log.LogInformation(model.VoucherNumber, "Redemption endpoint called: {endpoint}", 
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  model.SyxVoucherCreditingEndPoint);
                }

                model.UseSyxCredit = useSyxCreditEndpoint;
                var response = await _creditingService.SubmitVoucher(model, useSyxCreditEndpoint);
                _log.LogInformation(model.VoucherNumber, "Voucher redeem response: {response}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(response));
                return response;
            }

        }

        private async Task<VrwViewModel> ProcessDefaultOption(VrwViewModel model)
        {
            //find the providers that are enabled compatible with provided voucher pin length
            var providers = VoucherProviderHelper.GetProvidersByLength(model.VoucherNumber.Length);

            //if any found try to redeem sequently
            if (providers != null && providers.Any())
            {
                foreach (var provider in providers)
                {
                    bool useSyxCreditEndpoint = false;

                    if (provider.UseSxyCreditEndPoint != null)
                    {
                        useSyxCreditEndpoint = (bool)provider.UseSxyCreditEndPoint;
                    }

                    model.UseSyxCredit = useSyxCreditEndpoint;

                    model.SyxVoucherCreditingEndPoint = useSyxCreditEndpoint ? provider.SyxCreditServiceUrl ?? "" : provider.MicroServiceUrl;
                    model.ErrorMessage = string.Empty;
                    var response = await _creditingService.SubmitVoucher(model, useSyxCreditEndpoint);

                    _log.LogInformation(model.VoucherNumber, "Redemption endpoint called: {endpoint}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  model.SyxVoucherCreditingEndPoint);

                    if (response != null && response.IsSuccessful && String.IsNullOrEmpty(response.ErrorMessage))
                    {
                        _log.LogInformation(model.VoucherNumber, "Voucher redeem successfully: {response}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(response));
                        //we return the response here
                        return response;
                    }
                }
                //none of providers above returned successful redeem
                model.ErrorMessage = "Voucher redeem failed.";
                model.IsSuccessful = false;
                _log.LogInformation(model.VoucherNumber, "Failed to redeem {model} from all providers {providers}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, JsonConvert.SerializeObject(model), JsonConvert.SerializeObject(providers));
            }
            else
            {
                //No providers found with this voucher length
                model.ErrorMessage = "Invalid VoucherPin";
                model.IsSuccessful = false;
                _log.LogInformation(model.VoucherNumber, "Failed to find a matching provider for voucher length {length} for the model {model}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  model.VoucherNumber.Length, JsonConvert.SerializeObject(model));
            }
        return model;
        }

        //This method get  the List of Providers that will be displayed in the Index Page
        public async Task<VrwViewModel> InitializeViewModel(string clientId, string devicePlatform)
        {
            var Validator = new VrwViewModel();

            try
            {
                var modifiedEnablers = await _voucherProviderService.GetProviders();

                if (modifiedEnablers == null ||!modifiedEnablers.Any())
                {
                    await _voucherProviderService.SetProviders();
                }
                else
                {
                    //update static property on helper, because it's where we get provider data on POST
                    VoucherProviderHelper.SetProviders(modifiedEnablers);
                }

                Validator.ClientId = clientId;
                Validator.DevicePlatform = devicePlatform;
                Validator.VouchersEnablers = modifiedEnablers!;

            }
            catch (Exception ex)
            {
                _log.LogError(ex, null, "Failed to Initialize VRW View Model");
            }

            return Validator;
        }



    }
}
