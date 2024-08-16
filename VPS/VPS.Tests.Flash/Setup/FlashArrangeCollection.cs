using Bogus;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Configurations.Flash;
using VPS.Domain.Models.Flash.Requests;
using VPS.Domain.Models.Flash.Responses;

namespace VPS.Tests.Flash.Setup;
public class FlashArrangeCollection
{
    public FlashConfiguration FlashConfiguration { get; set; }
    public DBSettings DBSettings { get; set; }

    public FlashArrangeCollection()
    {
        DBSettings = CreateDBSettings();
        FlashConfiguration = SetFlashSettings();
    }

    public static FlashConfiguration SetFlashSettings()
    {
        return new FlashConfiguration
        {
            FlashEndpoint = "https://qa-voucher-service.shoptoshop.co.za/",
            FlashAccountNumber = "1230",
            FlashAPITimeoutSeconds = 30,
            FlashConsumerKey = "consumer-key",
            FlashConsumerSecret = "consumer-secreat",
            MaxPolyRetry = "3"
        };
    }

    public static FlashRequest CreateVoucherRedeemRequest()
    {
        var random = new Random();
        return new FlashRequest()
        {
            accountNumber = random.Next(15, 15).ToString(),
            mobileNumber = random.Next(10, 10).ToString(),
            pin = random.Next(15, 15).ToString(),
            reference = random.Next(14, 14).ToString(),
            storeId = random.Next(15, 15).ToString(),
            terminalId = random.Next(15, 15).ToString()
        };
    }

    public static DBSettings CreateDBSettings() => new Faker<DBSettings>()
        .RuleFor(r => r.LogStoreProcedureName, x => x.System.Random.AlphaNumeric(16));

    public static FlashRedeemResponse CreateSuccessFlashVoucherResponse(string voucherNumber)
    {
        var random = new Random();
        return new FlashRedeemResponse
        {
            AccountNumber = voucherNumber,
            Amount = random.Next(500, 900),
            Reference = random.Next(10, 10).ToString(),
            ResponseCode = 0,
            ResponseMessage = "OK",
            Voucher = new Voucher
            {
                Amount = random.Next(500, 900),
                Pin = random.Next(15, 15).ToString(),
            }
        };
    }

    public static FlashRedeemRequest CreateFlashRedeemRequest()
    {
        var random = new Random();
        return new FlashRedeemRequest()
        {
            ClientId = random.Next(1, 9),
            DevicePlatform = "mob",
            Provider = "flash",
            VoucherNumber = random.Next(15, 15).ToString(),
            VoucherReference = random.Next(15, 15).ToString()
        };
    }

    public static FlashRedeemResponse CreateFailFlashVoucherResponse(string voucherNumber)
    {
        var random = new Random();
        return new FlashRedeemResponse
        {
            AccountNumber = voucherNumber,
            Amount = random.Next(500, 900),
            Reference = random.Next(10, 10).ToString(),
            ResponseCode = -1,
            ResponseMessage = "OK",
            Voucher = new Voucher
            {
                Amount = random.Next(500, 900),
                Pin = random.Next(15, 15).ToString(),
            }
        };
    }

    public static RedeemOutcome AlreadyRedeemedOutCome()
       => new()
       {
           OutcomeMessage = "Voucher Already Redeemed.",
           OutComeTypeId = -1,
       };

    public static FlashRedeemResponse CreateAlreadyRedeemedVoucherResponse()
    => new()
    {
        ResponseCode = 2401,
        ResponseMessage = "Voucher Already Redeemed."
    };
}
