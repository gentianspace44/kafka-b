using Bogus;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Configurations.BluVoucher;

namespace VPS.Test.BluVoucher.Setup;

public class Fixtures
{
    public BluVoucherConfiguration BluVoucherConfiguration { get; set; }

    public DBSettings DBSettings { get; set; }
    public RedisSettings RedisSettings { get; set; }

    public Fixtures()
    {
        BluVoucherConfiguration = SetupBluVoucherConfiguration();
        DBSettings = CreateDBSettings();
        RedisSettings = SetRedisSettings();
    }

    public static BluVoucherConfiguration SetupBluVoucherConfiguration()
    {
        return new BluVoucherConfiguration()
        {
            DeviceId = "868458",
            DeviceSerial = "H09YW6D8TS",
            RemotePassword = "102.134.128.70",
            RemoteServer = "7800",
            RemoteUsername = "011234"
        };
    }

    public static Parameters CreateParameters()
    {
        var random = new Random();

        var result = new Faker<Parameters>()
            .RuleFor(r => r.ClientId, random.Next(10000, 99999))
            .RuleFor(r => r.VoucherNumber, random.Next(111111111, 999999999).ToString())
            .RuleFor(r => r.SyXSessionToken, x => x.System.Random.AlphaNumeric(32))
            .RuleFor(r => r.SyXUserId, random.Next(1000, 9999))
            .RuleFor(r => r.Amount, random.Next(100, 999).ToString())
            .RuleFor(r => r.SessionId, Guid.NewGuid().ToString("N"))
            .RuleFor(r => r.DevicePlatform, random.Next(1, 2) == 1 ? "mob" : "web")
            .RuleFor(r => r.VoucherReference, x => x.System.Random.AlphaNumeric(16));

        return result;
    }

    public static DBSettings CreateDBSettings() => new Faker<DBSettings>()
            .RuleFor(r => r.LogStoreProcedureName, x => x.System.Random.AlphaNumeric(16));

    private static RedisSettings SetRedisSettings()
    {
        return new RedisSettings
        {
            ConcurrencyDelayInSeconds = 1000,
            UseSSL = true,
        };
    }
}
