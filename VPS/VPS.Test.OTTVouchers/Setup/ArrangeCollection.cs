using Newtonsoft.Json;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.OTT;
using VPS.Domain.Models.OTT.Requests;
using VPS.Domain.Models.OTT.Responses;

namespace VPS.Test.OTTVoucher.Setup;

public static class ArrangeCollection
{
    public static OttProviderVoucherResponse CreateSuccessOTTVoucherResponse()
    {
        var random = new Random();

        return new OttProviderVoucherResponse
        {
            Success = true,
            ErrorCode = 0,
            Message = "Success",
            VoucherID = random.Next(10000000, 99999999),
            VoucherAmount = decimal.Parse(random.Next(1000, 9999).ToString()),
        };
    }

    public static HttpResponseMessage CreateSucessOTTVoucherRestResponse(OttProviderVoucherResponse expectedResponse)
    {
        return new HttpResponseMessage()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(expectedResponse))
        };
    }

    public static OttProviderVoucherResponse CreateErrorOTTVoucherResponse()
    {
        var random = new Random();
        return new OttProviderVoucherResponse
        {
            Success = false,
            ErrorCode = 401,
            Message = "Invalid Voucher",
            VoucherID = random.Next(10000000, 99999999),
            VoucherAmount = random.Next(1000, 9999),
        };
    }

    public static OttVouchers CreateNotNullOTTVoucherBucketResponse(OttVoucherRedeemRequest oTTVoucherRedeemRequest, long voucherId, decimal voucherAmount)
    {
        return new OttVouchers
        {
            VoucherReferenceId = voucherId,
            Amount = voucherAmount,
            ClientId = oTTVoucherRedeemRequest.ClientId,
            RedeemDateTime = DateTime.Now,
            VoucherPin = oTTVoucherRedeemRequest.VoucherNumber,
            VoucherStatusId = 0,//pending processing
            VoucherTransactionTypeId = 1,
            VoucherTypeId = VoucherType.OTT,
            SyXPlatform = oTTVoucherRedeemRequest.DevicePlatform
        };

    }

    public static OttVouchers CreateOTTVoucherUpsertResponse(OttVoucherRedeemRequest oTTVoucherRedeemRequest, long voucherId, decimal voucherAmount)
    {
        return new OttVouchers
        {
            VoucherReferenceId = voucherId,
            Amount = voucherAmount,
            ClientId = oTTVoucherRedeemRequest.ClientId,
            RedeemDateTime = DateTime.Now,
            VoucherPin = oTTVoucherRedeemRequest.VoucherNumber,
            VoucherStatusId = 0,//pending processing
            VoucherTransactionTypeId = 1,
            VoucherTypeId = VoucherType.OTT,
            SyXPlatform = oTTVoucherRedeemRequest.DevicePlatform
        };

    }

    public static RedeemOutcome SucessRedeemOutCome(decimal voucherAmount, long voucherId)
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
            OutcomeMessage = "Invalid Voucher",
            OutComeTypeId = -1,
        };
    }

    public static RedeemOutcome AlreadyRedeemedOutCome()
    {
        return new RedeemOutcome
        {
            OutcomeMessage = "Voucher Already Redeemed",
            OutComeTypeId = -1,
        };
    }

    public static OttProviderVoucherResponse CreateAlreadyRedeemedOTTVoucherResponse()
         => new()
         {
             Success = false,
             ErrorCode = -1,
             Message = "Voucher Already Redeemed"
         };
}
