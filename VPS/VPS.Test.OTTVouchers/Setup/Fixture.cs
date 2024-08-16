using Bogus;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Configurations.OTT;
using VPS.Domain.Models.OTT.Requests;

namespace VPS.Test.OTTVoucher.Setup;

public class Fixture
{

    public OttVoucherConfiguration OttVoucherSettings { get; set; }
    public DBSettings DBSettings { get; set; }

    public Fixture()
    {
        DBSettings = CreateDBSettings();
        OttVoucherSettings = SetOTTVoucherSettings();
    }
    public static OttVoucherConfiguration SetOTTVoucherSettings()
    {
        return new OttVoucherConfiguration
        {
            OTTApiKey = "sfsfbsfbsf",
            OTTBaseUrl = "http://localhost",
            OTTPassword = "pass@123",
            OTTRemitVoucherUrl = "/OTTRemitVoucherUrl",
            OTTUsername = "username@2023",
            OTTCheckRemitVoucherUrl = "/OTTCheckRemitVoucherUrl",
            OTTVendorId = 0

        };
    }
    public static OttVoucherRedeemRequest CreateVoucherRedeemRequest()
    {
        return new OttVoucherRedeemRequest()
        {
            ClientId = 10009107,
            DevicePlatform = "newweb",
            VoucherNumber = "36790374555124",
            VoucherReference = ""
        };
    }

    public static DBSettings CreateDBSettings() => new Faker<DBSettings>()
        .RuleFor(r => r.LogStoreProcedureName, x => x.System.Random.AlphaNumeric(16));
}
