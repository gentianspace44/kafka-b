using Bogus;
using System.Diagnostics.Contracts;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Configurations.Flash;
using VPS.Test.Common.Models;

namespace VPS.Test.Common.Setup;

public class Fixtures
{
    public SyxSettings SyxSettings { get; set; }
    public RedisSettings RedisSettings { get; set; }
    public VpsControlCenterEndpoints VPSControlCenterEndpoints { get; set; }
    public DBSettings DBSettings { get; set; }
    public FlashConfiguration FlashSettings { get; set; }
    public KafkaQueueConfiguration KafkaQueueConfiguration { get; set; }
    public VoucherRedeemClientNotifications VoucherRedeemClientNotifications { get; set; }
    public CountrySettings CountrySettings { get; set; }

    public Fixtures()
    {
        SyxSettings = SetSyxSettings();
        RedisSettings = SetRedisSettings();
        VPSControlCenterEndpoints = SetVPSControlCenterEndpoints();
        DBSettings = CreateDBSettings();
        FlashSettings = CreateFlashSettings();
        KafkaQueueConfiguration = CreateKafkaQueueConfigurationSettings();
        VoucherRedeemClientNotifications = CreateVoucherRedeemClientNotificationsSettings();
        CountrySettings = CreateCountrySettingsSettings();
    }

    private static CountrySettings CreateCountrySettingsSettings() => new Faker<CountrySettings>();

    private static VoucherRedeemClientNotifications CreateVoucherRedeemClientNotificationsSettings() => new Faker<VoucherRedeemClientNotifications>();

    private static KafkaQueueConfiguration CreateKafkaQueueConfigurationSettings() => new Faker<KafkaQueueConfiguration>()
      .RuleFor(r => r.BufferSize, x => x.System.Random.AlphaNumeric(2))
      .RuleFor(r => r.MessageTopic, x => x.System.Random.Words(2));

    private static RedisSettings SetRedisSettings()
    {
        return new RedisSettings
        {
            ConcurrencyDelayInSeconds = 1000,
            UseSSL = true,
        };
    }

    private static CountrySettings SetCountrySettings()
    {
        return new CountrySettings { 
         CurrencyCode = "R"
         
        };
    }

  private static VoucherRedeemClientNotifications SetVoucherRedeemClientNotifications()
    {
        return new VoucherRedeemClientNotifications()
        {
            VocherRedeemFailPendingManualProcessing = "",
            VoucherAlreadyCreditedOnSyx = "",
            VoucherRedeemCriticalFailOnConsumer = "",
            VoucherRedeemCriticalFailOnProducer = "",
            VoucherRedeemInProgressMessage = "",
            VoucherRedeemSuccess = ""
        };
    }

    private static KafkaQueueConfiguration SetKafkaQueueConfiguration()
    {
        return new KafkaQueueConfiguration()
        {
            Broker = "",
             BufferSize = "",
              ConsumerMaximumRetryCount = 3,
               ConsumerName = "test",
                Group="",
                 MaxPollIntervalMs = 1000,
                  MessageTopic = "test",
                   ProducerMaximumRetryCount = 3,
                    ProducerName = "test",
                     SessionTimeoutMs = 1000,
         
        };
    }

    public static SyxSettings SetSyxSettings()
    {
        return new SyxSettings
        {
            SyxEndPoint = "https://uat3_syxapi.betsolutions.net/",
            MaxPolyRetry = 3
        };
    }

    public static VoucherRedeemRequestModel CreateVoucherRedeemRequest()
    {
        return new VoucherRedeemRequestModel()
        {
            ClientId = 10009107,
            DevicePlatform = "newweb",
            VoucherNumber = "36790374555124",
            VoucherReference = ""
        };
    }

    public static EligibleVoucherBonus SetActiveBonus()
    {
        return new EligibleVoucherBonus
        {
            Percentage = 10, // 10%
            MaxRedeemAmount = 100
        };
    }

    public static VpsControlCenterEndpoints SetVPSControlCenterEndpoints()
    {
        return new VpsControlCenterEndpoints
        {
            NotifyClientEndpoint = "https://vps-control-center-api-uat.betsolutions.net/Notify",
            NotifyAllEndpoint = "https://vps-control-center-api-uat.betsolutions.net/NotifyAll",
            GetSyxToken = "https://vps-control-center-api-uat.betsolutions.net/getSyxToken",
            ForceSyxTokenUpdate = "https://vps-control-center-api-uat.betsolutions.net/forceSyxTokenUpdate"
        };
    }

    public static DBSettings CreateDBSettings() => new Faker<DBSettings>()
        .RuleFor(r => r.LogStoreProcedureName, x => x.System.Random.AlphaNumeric(16));

    public static FlashConfiguration CreateFlashSettings() => new Faker<FlashConfiguration>()
        .RuleFor(r => r.MaxPolyRetry, x => x.System.Random.AlphaNumeric(2))
        .RuleFor(r => r.FlashConsumerSecret, x => x.System.Random.Words(10))
        .RuleFor(r => r.FlashAccountNumber, x => x.System.Random.AlphaNumeric(10))
        .RuleFor(r => r.FlashConsumerKey, x => x.System.Random.Words(10));
}
