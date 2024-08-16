using Microsoft.Extensions.Options;
using NSubstitute;
using VPS.API.Common;
using VPS.Domain.Models.Configurations;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Infrastructure.Repository.Redis;
using VPS.Services.Common;
using VPS.Services.HollyTopUp;
using VPS.Test.Common.Setup;
using Xunit;

namespace VPS.Tests.HollyTopUp.Services
{

    public class HollyTopUpKafkaConsumerTest : IClassFixture<Fixtures>
    {
        private readonly ILoggerAdapter<HollyTopUpKafkaConsumer> _logger = Substitute.For<ILoggerAdapter<HollyTopUpKafkaConsumer>>();
        private readonly IClientBalanceService _clientBalanceService = Substitute.For<IClientBalanceService>();
        private readonly IVoucherBatchProcessingRepository _voucherBatchProcessingRepository = Substitute.For<IVoucherBatchProcessingRepository>();
        private readonly IHttpClientCommunication _httpClientCommunication = Substitute.For<IHttpClientCommunication>();
        private readonly IOptions<VoucherRedeemClientNotifications> _voucherRedeemClientNotifications;
        private readonly IRedisRepository _redisRepository = Substitute.For<IRedisRepository>();
        private readonly IOptions<CountrySettings> _countrySettings;
        private readonly MetricsHelper _metricsHelper = Substitute.For<MetricsHelper>();
        private readonly IOptions<DBSettings> _dbSettings;
        private readonly IOptions<RedisSettings> _redisSettings;
        private readonly IOptions<KafkaQueueConfiguration> _queueConfiguration;
        private readonly IOptions<VpsControlCenterEndpoints> _vpsControlCenterEndpoints;
        private readonly HollyTopUpKafkaConsumer _hollyTopUpKafkaConsumer;



        public HollyTopUpKafkaConsumerTest(Fixtures fixtures)
        {
            _dbSettings = Options.Create(fixtures.DBSettings);
            _countrySettings = Options.Create(fixtures.CountrySettings);
            _redisSettings = Options.Create(fixtures.RedisSettings);
            _queueConfiguration = Options.Create(fixtures.KafkaQueueConfiguration);
            _vpsControlCenterEndpoints = Options.Create(fixtures.VPSControlCenterEndpoints);
            _voucherRedeemClientNotifications = Options.Create(fixtures.VoucherRedeemClientNotifications);
        }

        [Fact]
        public void Validate_Constructor()
        {
            //Arrange
            //Act
            var hollyTopUpKafkaConsumer = new HollyTopUpKafkaConsumer(_clientBalanceService,
               _logger,
               _queueConfiguration,
               _voucherBatchProcessingRepository,
               _httpClientCommunication,
               _redisRepository,
               _vpsControlCenterEndpoints,
               _voucherRedeemClientNotifications,
               _dbSettings,
               _redisSettings,
               _countrySettings,
               _metricsHelper);

            //Assert
            Assert.NotNull(hollyTopUpKafkaConsumer);
        }

        [Fact]
        public async Task HollyTopUpKafkaConsumer_ConsumeAndProcessTransaction_When_Message_Is_Empty()
        {
            //Arrange
            var kafkaMessage = string.Empty;
            var hollyTopUpKafkaConsumer = new HollyTopUpKafkaConsumer(_clientBalanceService,
              _logger,
              _queueConfiguration,
              _voucherBatchProcessingRepository,
              _httpClientCommunication,
              _redisRepository,
              _vpsControlCenterEndpoints,
              _voucherRedeemClientNotifications,
              _dbSettings,
              _redisSettings,
              _countrySettings,
              _metricsHelper);

            //Act
            var response = await hollyTopUpKafkaConsumer.ConsumeAndProcessTransaction(kafkaMessage);

            //Assert
            Assert.False(response);
        }

        [Fact]
        public async Task HollyTopUpKafkaConsumer_ConsumeAndProcessTransaction_When_Message_Is_Invalid()
        {
            //Arrange
            var kafkaMessage = "Invalid Kafka Message";

            //Act           
            //Assert
            await Assert.ThrowsAsync<System.NullReferenceException>(async () => await _hollyTopUpKafkaConsumer.ConsumeAndProcessTransaction(kafkaMessage));
        }

        [Fact]
        public async Task HollyTopUpKafkaConsumer_ConsumeAndProcessTransaction_When_RedeemOutCome_Is_Null()
        {
            //Arrange
            var kafkaMessage = "{\"VoucherRedeemRequest\":{\"VoucherNumber\":\"701669166754237\",\"ClientId\":10009107,\"DevicePlatform\":\"newweb\",\"VoucherReference\":\"a110e907-0931-425c-b5ff-d1715b12b515\",\"Provider\":\"HollyTopUp\",\"CreatedDate\":\"2024-04-11T10:16:44.9904242+02:00\",\"ServiceVersion\":\"VPS.Provider.HollyTopUp 1.0.0\"},\"VoucherType\":1,\"RedeemOutcome\":null}";


            //Act
            //Assert
            await Assert.ThrowsAsync<System.NullReferenceException>(async () => await _hollyTopUpKafkaConsumer.ConsumeAndProcessTransaction(kafkaMessage));
        }

        [Fact]
        public async Task HollyTopUpKafkaConsumer_ConsumeAndProcessTransaction_When_VoucherRedeemRequest_Is_Null()
        {
            //Arrange
            var kafkaMessage = "{\"VoucherRedeemRequest\":null,\"VoucherType\":1,\"RedeemOutcome\":{\"VoucherID\":45673970,\"VoucherAmount\":10.00,\"OutComeTypeId\":1,\"OutcomeMessage\":\"Voucher redeem successfully\"}}";


            //Act
            //Assert
            await Assert.ThrowsAsync<System.NullReferenceException>(async () => await _hollyTopUpKafkaConsumer.ConsumeAndProcessTransaction(kafkaMessage));

        }

        [Fact]
        public async Task HollyTopUpKafkaConsumer_ConsumeAndProcessTransaction_When_Payload_Is_Null()
        {
            //Arrange
            var kafkaMessage = "{}";


            //Act
            //Assert
            await Assert.ThrowsAsync<System.NullReferenceException>(async () => await _hollyTopUpKafkaConsumer.ConsumeAndProcessTransaction(kafkaMessage));
        }


    }
}
