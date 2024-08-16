using System.Reflection;
using VPS.API.Syx;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Common.Response;
using VPS.Domain.Models.Enums;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;

namespace VPS.Services.Common
{
    public class ClientBalanceService<T> : IClientBalanceService
    {
        private readonly ILoggerAdapter<ClientBalanceService<T>> _log;
        private readonly ISyxApiService _syxService;
        private readonly IClientBonusService _clientBonusService;
        private readonly IEligibleBonusRepository _eligibleBonusRepository;
        private readonly IReferenceGeneratorService _referenceGenerator;
        private readonly IVoucherLogRepository _voucherLogRepository;
        public ClientBalanceService(ILoggerAdapter<ClientBalanceService<T>> log, ISyxApiService syxService, IClientBonusService clientBonusService,
            IEligibleBonusRepository eligibleBonusRepository, IReferenceGeneratorService referenceGenerator, IVoucherLogRepository voucherLogRepository)
        {
            this._log = log ?? throw new ArgumentNullException(nameof(log));
            this._syxService = syxService ?? throw new ArgumentNullException(nameof(syxService));
            this._clientBonusService = clientBonusService ?? throw new ArgumentNullException(nameof(clientBonusService));
            this._eligibleBonusRepository = eligibleBonusRepository ?? throw new ArgumentNullException(nameof(eligibleBonusRepository));
            this._referenceGenerator = referenceGenerator ?? throw new ArgumentNullException(nameof(referenceGenerator));
            this._voucherLogRepository = voucherLogRepository ?? throw new ArgumentNullException(nameof(voucherLogRepository));
        }

        public async Task<SyXCreditOutcome> CreditOnSyX(int clientId, string platform, long voucherId, string voucherPin, decimal amount,
            string voucherPrefix, VoucherType voucherType, string uniqueReference, string logStoreProcedureName)
        {
            bool isSyXUpdated = false;

            var apiClientBalanceUpdateResponse = new ApiClientBalanceUpdateResponse { ResponseType = 0, ResponseObject = null, ResponseMessage = "" };
            try
            {
                //Check if SyxApi is up and running
                if (!await _syxService.HealthCheck())
                {
                    _log.LogInformation(clientId.ToString(), "{voucherPrefix} - SyX health check failed, for processing voucher with details: {voucherId}, {voucherPin}, {clientId}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  voucherPrefix, voucherId, voucherPin, clientId );

                    return new SyXCreditOutcome { OutcomeMessage = "Server busy. Please try again.", OutComeTypeId = -1 };
                }                    

                string reference = _referenceGenerator.Generate(voucherType, voucherId.ToString(), voucherPin, platform);
                var checkExistsResponse = await CheckVoucherExistsOnSyx(clientId, reference) ?? throw new FormatException("Null checkExistsResponse");
                if (!checkExistsResponse.VoucherExists && checkExistsResponse.ResponseType != -1)
                {
                    var updateClientBalance = await _syxService.UpdateClientBalance(clientId, 48, amount, 1, reference);

                    if (updateClientBalance == null || updateClientBalance.ResponseObject == null) throw new FormatException("Null updateClientBalance or  ResponseObject");

                    if (updateClientBalance.ResponseType == 1)
                    {
                        isSyXUpdated = true;
                        apiClientBalanceUpdateResponse = updateClientBalance;

                        _log.LogInformation(clientId.ToString(), "{voucherPrefix} Voucher credited on SyX successfully with voucher ID: {voucherId},VoucherPin: {voucherPin} platform: {platform}, ClientID: {clientId}, Amount: {amount}",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                             voucherPrefix, voucherId, voucherPin, platform, clientId, amount );


                        await _voucherLogRepository.UpdateVoucherLogSyxResponse(
                            voucherPin,
                            uniqueReference,
                            voucherType,
                            VoucherStatus.Completed,
                            updateClientBalance.ResponseMessage,
                            true);

                        //Check for active bonus.
                        var eligibleBonus = await _eligibleBonusRepository.GetEligibleVoucherBonus(DateTime.Now, clientId);

                        if (eligibleBonus?.Status != 1)
                            return new SyXCreditOutcome { OutcomeMessage = "Voucher redeem successful.", OutComeTypeId = 1, BalanceAvailable = updateClientBalance.ResponseObject.BalanceAvailable };

                        _log.LogInformation(clientId.ToString(), "Eligible bonus found with Status: {Status}, for {voucherPrefix} voucher details:  ID: {voucherId}, Client ID: {clientId}.", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,   eligibleBonus.Status, voucherPrefix, voucherId, clientId );

                        //return a type 2 meaning a bonus credit is eligible.
                        return new SyXCreditOutcome { OutcomeMessage = "Voucher redeem successful.", OutComeTypeId = 2, VoucherBonus = eligibleBonus, BalanceAvailable = updateClientBalance.ResponseObject.BalanceAvailable };
                    }


                    _log.LogInformation(clientId.ToString(), "UpdateClientBalance failed with status: {updateClientBalanceResponseType} , {updateClientBalanceResponseMessage} - for client {clientId} - voucher id {voucherId}",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                       updateClientBalance.ResponseType, updateClientBalance.ResponseMessage, clientId, voucherId);

                    
                    await _voucherLogRepository.UpdateVoucherLogSyxResponse(
                            voucherPin,
                            uniqueReference,
                            voucherType,
                            VoucherStatus.Unsuccessful,
                            updateClientBalance.ResponseMessage,
                            false);

                    return new SyXCreditOutcome { OutcomeMessage = "Update client balance failed.", OutComeTypeId = -1 };
                }
                else if (checkExistsResponse.VoucherExists && checkExistsResponse.ResponseType == 1)
                {
                    _log.LogInformation(clientId.ToString(), "Duplicate DB Entry: Voucher already redeemed {voucherPrefix} voucher with pin {voucherPin} -  voucher type : {voucherPrefix}, redeemed voucher id: {reference}, redeemed amount: {amount}",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                        voucherPrefix, voucherPin, voucherPrefix, reference, amount );

                    await _voucherLogRepository.UpdateVoucherLogSyxResponse(
                          voucherPin,
                          uniqueReference,
                          voucherType,
                          VoucherStatus.Unsuccessful,
                          "Voucher already redeemed",
                          false);

                    return new SyXCreditOutcome { OutcomeMessage = "Redeem failed, voucher already redeemed..", OutComeTypeId = -1 };
                }
                else
                {
                    _log.LogInformation(clientId.ToString(), "CheckVoucherExists Failed: {voucherPin} {clientId} {apiVoucherExistsResponseResponseMessage}",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                        voucherPin, clientId, checkExistsResponse.ResponseMessage );

                    await _voucherLogRepository.UpdateVoucherLogSyxResponse(
                        voucherPin,
                        uniqueReference,
                        voucherType,
                        VoucherStatus.Unsuccessful,
                        checkExistsResponse.ResponseMessage,
                        false);

                    return new SyXCreditOutcome { OutcomeMessage = "Failure to complete redemption process. Please try again later." };
                }

            }
            catch (Exception ex)
            {
                _log.LogError(ex, null, "{voucherPrefix}_{voucherId}  - Error raised for voucher pin: {voucherPin}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                     voucherPrefix, voucherId, voucherPin );

                if (isSyXUpdated)
                {
                    return new SyXCreditOutcome { OutcomeMessage = "Voucher redeem successful.", OutComeTypeId = 1, BalanceAvailable = apiClientBalanceUpdateResponse.ResponseObject?.BalanceAvailable?? 0 };
                }
                else
                {

                    return new SyXCreditOutcome { OutcomeMessage = ex.Message, OutComeTypeId = -1 };
                }
            }
                    }

        public async Task<SyXCreditOutcome> CreditBonusOnSyX(int clientId, string platform, long voucherId, decimal voucherAmount, EligibleVoucherBonus? eligibleVoucherBonus, string voucherPrefix)
        {
            try
            {
                var bonusAmount = await _clientBonusService.GetBonusAmount(eligibleVoucherBonus, voucherAmount, clientId);

                if (bonusAmount > 0)
                {
                  
                    var updateBonus = await _syxService.UpdateClientBalance(clientId, 21, bonusAmount, 1, $"{platform} - {eligibleVoucherBonus?.Name} (Voucher) {voucherId}") ?? throw new FormatException("Null updateBonus");
                    if (updateBonus.ResponseType == 1)
                    {
                        _log.LogInformation(clientId.ToString(), "Bonus applied successfully for {voucherPrefix} voucher ID: {voucherId}, Client ID: {clientId}, Platform: {platform}, Redeem Amount: {voucherAmount}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  voucherPrefix, voucherId, clientId, platform, voucherAmount);

                        if (updateBonus.ResponseObject == null) throw new FormatException("Null UpdateBonus ResponseObject");

                        return new SyXCreditOutcome { OutcomeMessage = "SyX bonus credit success.", OutComeTypeId = 1, RedeemedBonusAmount = bonusAmount, BalanceAvailable = updateBonus.ResponseObject.BalanceAvailable };
                    }

                    _log.LogInformation(clientId.ToString(), "Bonus credit failed for {voucherPrefix} voucher ID: {voucherId}, Client ID: {clientId}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  voucherPrefix, voucherId, clientId );

                    //some reason returned -1
                    return new SyXCreditOutcome { OutcomeMessage = updateBonus.ResponseMessage!, OutComeTypeId = -1 };
                }

                //some reason returned -1
                return new SyXCreditOutcome { OutcomeMessage = "Bonus amount less then or equal to 0", OutComeTypeId = -1 };

            }
            catch (Exception ex)
            {
                _log.LogError(ex, null, "{voucherPrefix}_{voucherId}  - voucher pin", MethodBase.GetCurrentMethod()?.Name ?? string.Empty,  voucherPrefix, voucherId );
                return new SyXCreditOutcome { OutcomeMessage = ex.Message, OutComeTypeId = -1 };
            }
        }

        public async Task<ApiVoucherExistsResponse?> CheckVoucherExistsOnSyx(long clientId, string reference)
        {
            var checkExistsResponse = await this._syxService.CheckVoucherExists(clientId, reference);

            if (checkExistsResponse == null)
            {
                _log.LogError(reference, "Failed to CheckVoucherExistsOnSyx from ClientBalanceService (response is null) for reference: {reference}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, reference );
            }

            return checkExistsResponse;
        }
    }
}
