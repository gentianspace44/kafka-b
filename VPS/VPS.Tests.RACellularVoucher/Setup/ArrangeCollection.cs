using VPS.Domain.Models.Common;
using VPS.Domain.Models.RACellularVoucher.Requests;
using VPS.Domain.Models.RACellularVoucher.Response;

namespace VPS.Tests.RACellularVoucher.Setup;
public static class RACellularArrangeCollection
{
    public static RaCellularVoucherRedeemResponse CreateSuccessVoucherResponse()
    {
        var random = new Random();
        var voucherId = random.Next(999, 999999).ToString();

        return new RaCellularVoucherRedeemResponse
        {
            Amount = 1.00M,
            PinNumber = voucherId,
            Reference = voucherId
        };
    }

    public static RedeemOutcome SuccessRedeemOutCome(decimal voucherAmount, string voucherId)
    {
        return new RedeemOutcome
        {
            OutComeTypeId = 1,
            OutcomeMessage = "Voucher was redeemed successfully",
            VoucherAmount = voucherAmount,
            VoucherID = long.Parse(voucherId)
        };
    }

    public static RaCellularVoucherRedeemRequest CreateVoucherRedeemRequest()
    {
        return new RaCellularVoucherRedeemRequest()
        {
            ClientId = 10009107,
            DevicePlatform = "newweb",
            VoucherNumber = "36790374555124",
            VoucherReference = ""
        };
    }

    public static RaCellularVoucherRedeemResponse CreateErrorVoucherResponse() => new()
    {
        Amount = 0,
        HasFault = true,
        FaultMsg = "Invalid voucher",
        PinNumber = "0"
    };

    public static RedeemOutcome FailVoucherRedeemOutCome(decimal voucherAmount, string pinNumber)
    {
        return new RedeemOutcome
        {
            OutcomeMessage = "R&A redeem failed, Please try again.",
            OutComeTypeId = -1,
            VoucherAmount = voucherAmount,
            VoucherID = long.Parse(pinNumber)
        };
    }

    public static RedeemOutcome AlreadyRedeemedOutCome()
      => new()
      {
          OutcomeMessage = "Voucher Already Redeemed",
          OutComeTypeId = -1,
      };

    public static RaCellularVoucherRedeemResponse CreateAlreadyVoucherResponse()
    {
        var random = new Random();

        return new RaCellularVoucherRedeemResponse
        {
            HasFault = true,
            PinNumber = random.Next(999, 999999).ToString(),
            FaultNumber = "-1",
            FaultMsg = "Voucher Already Redeemed"
        };
    }
}
