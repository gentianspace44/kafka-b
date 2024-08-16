using Bogus;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Configurations.EasyLoad;
using VPS.Domain.Models.EasyLoad.Request;
using VPS.Helpers;

namespace VPS.Test.EasyLoad.Setup;

public class Fixtures
{
    public EasyLoadConfiguration EasyLoadSettings { get; set; }
    public DBSettings DBSettings { get; set; }
    public KafkaQueueConfiguration KafkaQueueConfiguration { get; set; }
    public VpsControlCenterEndpoints VpsControlCenterEndpoints { get; set; }
    public VoucherRedeemClientNotifications VoucherRedeemClientNotifications { get; set; }
    public RedisSettings RedisSettings { get; set; }
    public CountrySettings CountrySettings { get; set; }
    public Fixtures()
    {
        DBSettings = CreateDBSettings();
        EasyLoadSettings = SetEasyLoadSettings();
        KafkaQueueConfiguration = CreateKafkaQueueConfiguration();
        VpsControlCenterEndpoints = CreateVpsControlCenterEndpoints();
        VoucherRedeemClientNotifications = CreateVoucherRedeemClientNotifications();
        RedisSettings = CreateRedisSettings();
        CountrySettings = CreateCountrySettings();
    }

    public static EasyLoadConfiguration SetEasyLoadSettings()
    {
        return new EasyLoadConfiguration
        {
            BaseUrl = "https://qa-voucher-service.shoptoshop.co.za/",
            RedeemVoucherUrl = "/api/voucher/redeemVoucher",
            ReverseVoucherUrl = "/api/voucher/reverseVoucher",
            ApiKey = "keJ3HUVNmwMD77yj",
            MaxPolyRetry = "3"
        };
    }

    public static EasyLoadVoucherRedeemRequest CreateVoucherRedeemRequest()
    {
        return new EasyLoadVoucherRedeemRequest()
        {
            ClientId = 10009107,
            DevicePlatform = "newweb",
            VoucherNumber = "36790374555124",
            VoucherReference = ""
        };
    }

    public static KafkaQueueConfiguration CreateKafkaQueueConfiguration()
    {
        return new KafkaQueueConfiguration
        {
            MessageTopic = "easyload-provider",
            ProducerName = "easyload-provider-producer",
            ConsumerName = "easyload-provider-consumer",
            Broker = "kafka_container:9093",
            Group = "easyload-group",
            ProducerMaximumRetryCount = 3,
            ConsumerMaximumRetryCount = 3,
            MaxPollIntervalMs = 900000,
            SessionTimeoutMs = 10000,
            BufferSize = "100"
        };
    }

    public static VpsControlCenterEndpoints CreateVpsControlCenterEndpoints()
    {
        return new VpsControlCenterEndpoints
        {
            NotifyClientEndpoint = "test",
            NotifyAllEndpoint = "test",
            GetSyxToken = "test",
            ForceSyxTokenUpdate = "test",
            ForceRedisVoucherUpdate = "test",
            BaseEndpoint = "test",
        };
    }

    public static VoucherRedeemClientNotifications CreateVoucherRedeemClientNotifications()
    {
        return new VoucherRedeemClientNotifications
        {
            VoucherRedeemInProgressMessage = "test",
            VoucherRedeemCriticalFailOnProducer = "test",
            VoucherRedeemSuccess = "test",
            VoucherRedeemCriticalFailOnConsumer = "test",
            VocherRedeemFailPendingManualProcessing = "test",
            VoucherAlreadyCreditedOnSyx = "test"
        };
    }
    public static RedisSettings CreateRedisSettings()
    {
        return new RedisSettings
        {
            ConcurrencyRedisServer = "test",
            DelayRedisServer = "test",
            InProgressRedisServer = "test",
            ConcurrencyConnectionClientName = "test",
            DelayConnectionClientName = "test",
            InProgressConnectionClientName = "test",
            ConcurrencyRedisDb = 1,
            DelayRedisDb =1,
            InProgressRedisDb = 1,
            EnableConcurrencyCheck = true,
            EnableDelayCheck = true,
            EnableInProgressCheck = true,
            CachePolicyTimeToLiveInSeconds = 1,
            InProgressPolicyTimeToLiveInHours = 1,
            ConcurrencyDelayInSeconds =1,
            UseSSL = true,
            MaxDelayPolyRetry = 1,
            MaxConcurrencyPolyRetry = 1,
            MaxInProgressPolyRetry = 1
        };
    }

    public static CountrySettings CreateCountrySettings()
    {
        return new CountrySettings
        {
            CurrencyCode = "R"
        };
    }

    public static string GetEasyLoadQuePayload()
    {
        return "{\"VoucherRedeemRequest\":{\"VoucherNumber\":\"038879351503083\",\"ClientId\":10009107,\"DevicePlatform\":\"newweb\",\"VoucherReference\":\"69d31e19-b35f-4f4b-b872-41bb0a84e02b\",\"Provider\":\"EasyLoad\",\"CreatedDate\":\"2024-04-07T20:20:37.3207693+00:00\",\"ServiceVersion\":\"VPS.Provider.EasyLoad 1.0.0\"},\"VoucherType\":5,\"RedeemOutcome\":{\"VoucherID\":45673827,\"VoucherAmount\":10.00,\"OutComeTypeId\":1,\"OutcomeMessage\":\"Voucher redeem successfully\"}}";
    }

    public static DBSettings CreateDBSettings() => new Faker<DBSettings>()
        .RuleFor(r => r.LogStoreProcedureName, x => x.System.Random.AlphaNumeric(16));
}
