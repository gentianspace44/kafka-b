using VPS.Domain.Models.Common;
using VPS.Domain.Models.HollyTopUp.Requests;
using VPS.Domain.Models.HollyTopUp.Response;

namespace VPS.Tests.HollyTopUp.Setup;

public static class HollyTopUpArrangeCollection
{
    public static HollyTopUpRedeemResponse CreateSuccessHollyTopUpVoucherResponse()
    {
        var random = new Random();

        return new HollyTopUpRedeemResponse
        {
            VoucherAmount = 1.00M,
            CreateDatetime = DateTime.Now,
            StatusID = 1,
            VoucherID = random.Next(1, 7)
        };
    }

    public static RedeemOutcome SuccessRedeemOutCome(decimal voucherAmount, long voucherId)
    {
        return new RedeemOutcome
        {
            OutComeTypeId = 1,
            OutcomeMessage = "Voucher redeem successfully",
            VoucherAmount = voucherAmount,
            VoucherID = voucherId
        };

    }

    public static HollyTopUpRedeemRequest CreateVoucherRedeemRequest()
    {
        return new HollyTopUpRedeemRequest()
        {
            ClientId = 10009107,
            DevicePlatform = "newweb",
            VoucherNumber = "36790374555124",
            VoucherReference = ""
        };
    }

    public static HollyTopUpRedeemResponse CreateErrorVoucherResponse()
    {
        var random = new Random();

        return new HollyTopUpRedeemResponse
        {
            VoucherID = random.Next(1, 7),
            VoucherAmount = 1.00M,
            CreateDatetime = DateTime.Now,
            StatusID = 3
        };
    }

    public static RedeemOutcome FailVoucherRedeemOutCome(decimal voucherAmount, int voucherID)
    {
        return new RedeemOutcome
        {
            OutcomeMessage = "Invalid voucher",
            OutComeTypeId = -1,
            VoucherAmount = voucherAmount,
            VoucherID = voucherID
        };
    }

    public static RedeemOutcome AlreadyRedeemedOutCome()
      => new()
      {
          OutcomeMessage = "Voucher already redeemed",
          OutComeTypeId = -1,
      };

    public static HollyTopUpRedeemResponse CreateAlreadyRedeemedHollyTopUpVoucherResponse()
        => new()
        {
            StatusID = 2
        };
}
