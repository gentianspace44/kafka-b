using Newtonsoft.Json;
using System.Net;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Common.Response;
using VPS.Domain.Models.Enums;
using VPS.Test.Common.Models;

namespace VPS.Test.Common.Setup;

public static class ArrangeCollection
{
    public static EligibleVoucherBonus NoEligibleVoucherBonusFound()
    {
        return new EligibleVoucherBonus()
        {
            Status = 2,
            Message = "No Bonus active at the moment."
        };
    }

    public static ApiClientBalanceUpdateResponse SuccessApiClientBalanceUpdateResponse()
    {
        var random = new Random();

        var balance = new ApiClientBalance
        {
            BalanceAvailable = random.Next(1000, 9999),
            BalancePending = random.Next(1000, 9999)
        };

        return new ApiClientBalanceUpdateResponse
        {
            ResponseObject = balance,
            ResponseType = 1,
            ResponseMessage = "Voucher credited on SyX successfully",
        };
    }

    public static ApiVoucherExistsResponse SuccessApiVoucherExistsResponse()
    {
        return new ApiVoucherExistsResponse
        {
            VoucherExists = true,
            ResponseType = 1,
            ResponseMessage = "Voucher Exists",
        };
    }

    public static ApiVoucherExistsResponse SuccessApiVoucherExistsResponseNewVoucher()
    {
        return new ApiVoucherExistsResponse
        {
            VoucherExists = false,
            ResponseType = 1,
            ResponseMessage = "Voucher not exist",
        };
    }

    public static ApiVoucherExistsResponse FailApiVoucherExistsResponse()
    {
        return new ApiVoucherExistsResponse
        {
            VoucherExists = false,
            ResponseType = -1,
            ResponseMessage = "Voucher not found",
        };
    }

    public static ApiClientBalanceUpdateResponse FailApiClientBalanceUpdateResponse()
    {
        return new ApiClientBalanceUpdateResponse
        {
            ResponseType = -1,
            ResponseMessage = "An Error occurred, please try again.",
        };
    }

    public static HttpResponseMessage CreateSuccessApiClientBalanceUpdateResponse(ApiClientBalanceUpdateResponse expectedResponse)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(expectedResponse))
        };
    }

    public static HttpResponseMessage CreateSuccessApiVoucherExistsResponse(ApiVoucherExistsResponse expectedResponse)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(expectedResponse))
        };
    }

    public static HttpResponseMessage CreateFailApiVoucherExistsResponse(ApiVoucherExistsResponse expectedResponse)
    {
        return new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(JsonConvert.SerializeObject(expectedResponse))
        };
    }

    public static HttpResponseMessage CreateFailApiClientBalanceUpdateResponse(ApiClientBalanceUpdateResponse expectedResponse)
    {
        return new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent(JsonConvert.SerializeObject(expectedResponse))
        };
    }

    public static UpdateClientBalanceRequestModel CreateUpdateClientBalanceRequest()
    {
        string platform = "newweb";
        string eligibleVoucherName = "test";
        long voucherId = 24062925501036;

        return new UpdateClientBalanceRequestModel
        {
            Platform = platform,
            EligibleVoucherName = eligibleVoucherName,
            VoucherId = voucherId,
            UserId = 791,
            SessionToken = "vbGOdmhkg%utZRQBC2ZXkmX1j%RpG7ugjpMebc4B~ImWvYuuH7Y7OX~JEVwe",
            ClientId = 10009107,
            TransactionTypeId = 21,
            TransactionAmount = 100,
            BranchId = 1,
            ReferenceComments = $"{platform} - {eligibleVoucherName} (Voucher) {voucherId}"
        };
    }

    public static CreditOnSyXRequestModel CreateCreditOnSyxRequest()
    {
        var random = new Random();
        string platform = "newweb";
        string eligibleVoucherName = "test";
        long voucherId = 24062925501036;

        return new CreditOnSyXRequestModel
        {
            Platform = platform,
            VoucherPin = "24062925501036",
            VoucherId = voucherId,
            UserId = 791,
            SessionToken = "vbGOdmhkg%utZRQBC2ZXkmX1j%RpG7ugjpMebc4B~ImWvYuuH7Y7OX~JEVwe",
            ClientId = 10009107,
            Amount = (decimal)(random.Next(1000, 9999) / 100),
            VoucherPrefix = eligibleVoucherName,
            VoucherType = VoucherType.EasyLoad,
            VoucherReference = Guid.NewGuid().ToString(),
        };
    }

    public static CreditBonusOnSyXRequestModel CreateCreditBonusOnSyXRequest()
    {
        var random = new Random();
        string platform = "newweb";
        string eligibleVoucherName = "test";
        long voucherId = 24062925501036;

        return new CreditBonusOnSyXRequestModel
        {
            Platform = platform,
            VoucherId = voucherId,
            UserId = 791,
            SessionToken = "vbGOdmhkg%utZRQBC2ZXkmX1j%RpG7ugjpMebc4B~ImWvYuuH7Y7OX~JEVwe",
            ClientId = 10009107,
            VoucherAmount = (decimal)(random.Next(1000, 9999) / 100),
            VoucherPrefix = eligibleVoucherName,
            EligibleVoucherBonus = new EligibleVoucherBonus { Name = "test" }
        };
    }

    public static string GetReference(string platform, string voucherId)
    {
        return $"{platform} Voucher Redeem - test - {voucherId}";
    }

    public static CheckVoucherExistsRequestModel CreateCheckVoucherExistsRequest()
    {
        return new CheckVoucherExistsRequestModel
        {
            SessionToken = "vbGOdmhkg%utZRQBC2ZXkmX1j%RpG7ugjpMebc4B~ImWvYuuH7Y7OX~JEVwe",
            ClientId = 10009107,
            Reference = "24062925501036"
        };
    }

    public static async Task<RedeemOutcome> SuccessRedeemOutCome(decimal voucherAmount, long voucherId)
    {
       await Task.Delay(1);
        
        return new RedeemOutcome
        {
            OutComeTypeId = 1,
            OutcomeMessage = "Voucher redeem successfully",
            VoucherAmount = voucherAmount / 100,
            VoucherID = voucherId
        };
    }

    public static async Task<RedeemOutcome> FailVoucherRedeemOutCome()
    {
        await Task.Delay(1);

        return new RedeemOutcome
        {
            OutcomeMessage = "Invalid Voucher.",
            OutComeTypeId = -1,
        };
    }

  

    public static SyXCreditOutcome SuccessSyxCreditOutCome()
    {
        var random = new Random();
        return new SyXCreditOutcome
        {
            OutComeTypeId = 1,
            OutcomeMessage = "Voucher redeem successfully",
            BalanceAvailable = random.Next(1000, 9999),
            RedeemedBonusAmount = random.Next(1000, 9999)
        };
    }

    public static SyXCreditOutcome FailSyxCreditOutCome()
    {
        return new SyXCreditOutcome
        {
            OutComeTypeId = -1,
            OutcomeMessage = "Update client balance failed.",
        };
    }

    public static ProviderVoucher CreateNotNullVoucherBucketResponse(VoucherRedeemRequestModel easyLoadVoucherRedeemRequest)
    {
        var random = new Random();
        return new ProviderVoucher
        {
            VoucherReferenceId = random.Next(1000, 9999),
            Amount = (decimal)random.Next(1000, 9999) / 100,
            ClientId = easyLoadVoucherRedeemRequest.ClientId,
            RedeemDateTime = DateTime.Now,
            VoucherPin = easyLoadVoucherRedeemRequest.VoucherNumber,
            VoucherStatusId = 1,//already redeemed
            VoucherTransactionTypeId = 1,
            VoucherTypeId = VoucherType.EasyLoad,
            SyXPlatform = easyLoadVoucherRedeemRequest.DevicePlatform
        };
    }

    public static VoucherRedeemRequestModel? GetNullVoucherRedeemRequestModel()
    {
        return null;
    }

    public static VoucherRedeemRequestModel GetVoucherRedeemRequestModel(string voucherNumber, int clientId)
    {
        return new VoucherRedeemRequestModel
        {
            VoucherNumber = voucherNumber,
            ClientId = clientId,
        };
    }

    public static HttpResponseMessage GetSyxToken()
    {
        var syxSessionModel = new SyxSessionModel
        {
            SyxSessionToken = "vbGOdmhkg%utZRQBC2ZXkmX1j%RpG7ugjpMebc4B~ImWvYuuH7Y7OX~JEVwe",
            SyxUserId = new Random().Next(1000, 9999)
        };
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(syxSessionModel))
        };
    }

}
