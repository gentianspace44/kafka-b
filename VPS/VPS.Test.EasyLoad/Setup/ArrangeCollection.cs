using Newtonsoft.Json;
using System.Net;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.EasyLoad;
using VPS.Domain.Models.EasyLoad.Request;
using VPS.Domain.Models.EasyLoad.Response;
using VPS.Domain.Models.Enums;

namespace VPS.Test.EasyLoad.Setup;

public static class ArrangeCollection
{
    public static EasyLoadProviderVoucherResponse CreateSuccessEasyLoadVoucherResponse(string voucherNumber)
    {
        var random = new Random();

        return new EasyLoadProviderVoucherResponse
        {
            VoucherNumber = voucherNumber,
            VoucherId = random.Next(10000000, 99999999),
            Amount = random.Next(1000, 9999),
            ResponseCode = 0,
            ResponseMessage = "Voucher Succesfully Redeemed"
        };
    }

    public static HttpResponseMessage CreateSuccessEasyLoadRestResponse(EasyLoadProviderVoucherResponse expectedResponse)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(expectedResponse))
        };
    }

    public static EasyLoadProviderVoucherResponse CreateErrorEasyLoadVoucherResponse(string voucherNumber)
    {
        var random = new Random();

        return new EasyLoadProviderVoucherResponse
        {
            VoucherNumber = voucherNumber,
            VoucherId = random.Next(10000000, 99999999),
            Amount = random.Next(1000, 9999),
            ResponseCode = 20,
            ResponseMessage = "Invalid Voucher"
        };
    }

    public static EasyLoadVoucher CreateNotNullEasyLoadVoucherBucketResponse(EasyLoadVoucherRedeemRequest easyLoadVoucherRedeemRequest, decimal voucherAmount, long voucherId)
    {
        return new EasyLoadVoucher
        {
            VoucherReferenceId = voucherId,
            Amount = voucherAmount / 100,
            ClientId = easyLoadVoucherRedeemRequest.ClientId,
            RedeemDateTime = DateTime.Now,
            VoucherPin = easyLoadVoucherRedeemRequest.VoucherNumber,
            VoucherStatusId = 0,//pending processing
            VoucherTransactionTypeId = 1,
            VoucherTypeId = VoucherType.EasyLoad,
            SyXPlatform = easyLoadVoucherRedeemRequest.DevicePlatform
        };
    }

    public static EasyLoadVoucher CreateEasyLoadVucherUpsertResponse(EasyLoadVoucherRedeemRequest easyLoadVoucherRedeemRequest, decimal voucherAmount, long voucherId)
    {
        return new EasyLoadVoucher
        {
            VoucherReferenceId = voucherId,
            Amount = voucherAmount / 100,
            ClientId = easyLoadVoucherRedeemRequest.ClientId,
            RedeemDateTime = DateTime.Now,
            VoucherPin = easyLoadVoucherRedeemRequest.VoucherNumber,
            VoucherStatusId = 0,//pending processing
            VoucherTransactionTypeId = 1,
            VoucherTypeId = VoucherType.EasyLoad,
            SyXPlatform = easyLoadVoucherRedeemRequest.DevicePlatform
        };
    }

    public static RedeemOutcome SuccessRedeemOutCome(decimal voucherAmount, long voucherId)
    {
        return new RedeemOutcome
        {
            OutComeTypeId = 1,
            OutcomeMessage = "Voucher redeem successfully",
            VoucherAmount = voucherAmount / 100,
            VoucherID = voucherId
        };
    }

    public static RedeemOutcome FailVoucherRedeemOutCome()
    {
        return new RedeemOutcome
        {
            OutcomeMessage = "Invalid Voucher.",
            OutComeTypeId = -1,
        };
    }

    public static RedeemOutcome AlreadyRedeemedOutCome()
       => new()
       {
           OutcomeMessage = "Redeem failed,Voucher Already Redeemed.",
           OutComeTypeId = -1,
       };

    public static EasyLoadProviderVoucherResponse CreateAlreadyRedeemedEasyLoadVoucherResponse()
        => new()
        {
            ResponseCode = -1,
            ResponseMessage = "Redeem failed,Voucher Already Redeemed."
        };

    public static SyXCreditOutcome FailSyxCreditOutCome()
    {
        return new SyXCreditOutcome { OutcomeMessage = "Failed", OutComeTypeId = -1 };
    }

    public static SyXCreditOutcome SuccessSyxCreditOutCome()
    {
        return new SyXCreditOutcome { OutcomeMessage = "Voucher redeem successful.", OutComeTypeId = 1, BalanceAvailable = 1 };
    }

}
