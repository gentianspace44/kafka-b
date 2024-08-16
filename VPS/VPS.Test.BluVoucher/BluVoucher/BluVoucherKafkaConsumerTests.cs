using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSubstitute;
using VPS.API.Common;
using VPS.Domain.Models.BluVoucher.Requests;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Infrastructure.Repository.Redis;
using VPS.Services.BluVoucher;
using VPS.Services.Common;
using VPS.Services.HollyTopUp;
using VPS.Test.Common.Setup;

namespace VPS.Test.BluVoucher.BluVoucher
{
    public class BluVoucherKafkaConsumerTests : IClassFixture<Fixtures>
    {
        private BluVoucherKafkaConsumer _bluVoucherKafkaConsumer;
        private IClientBalanceService _clientBalanceService;
        private ILoggerAdapter<HollyTopUpKafkaConsumer> _logger;
        private IOptions<KafkaQueueConfiguration> _queueConfiguration;
        private IVoucherBatchProcessingRepository _voucherBatchProcessingRepository;
        private IHttpClientCommunication _httpClientCommunication;
        private IRedisRepository _redisRepository;
        private IOptions<VpsControlCenterEndpoints> _vpsControlCenterEndpoints;
        private IOptions<VoucherRedeemClientNotifications> _voucherRedeemClientNotifications;
        private IOptions<DBSettings> _dbSettings;
        private IOptions<RedisSettings> _redisSettings;
        private IOptions<CountrySettings> _countrySettings;
        private MetricsHelper _metricsHelper;

        public BluVoucherKafkaConsumerTests(Fixtures fixtures)
        {
            _queueConfiguration = Options.Create(fixtures.KafkaQueueConfiguration);
            _vpsControlCenterEndpoints = Options.Create(fixtures.VPSControlCenterEndpoints);
            _voucherRedeemClientNotifications = Options.Create(fixtures.VoucherRedeemClientNotifications);
            _dbSettings = Options.Create(fixtures.DBSettings);
            _redisSettings = Options.Create(fixtures.RedisSettings);
            _countrySettings = Options.Create(fixtures.CountrySettings);

            _clientBalanceService = Substitute.For<IClientBalanceService>();
            _logger = Substitute.For<ILoggerAdapter<HollyTopUpKafkaConsumer>>();
            _voucherBatchProcessingRepository = Substitute.For<IVoucherBatchProcessingRepository>();
            _httpClientCommunication = Substitute.For<IHttpClientCommunication>();
            _redisRepository = Substitute.For<IRedisRepository>();
            _metricsHelper = new MetricsHelper();
            _bluVoucherKafkaConsumer = new BluVoucherKafkaConsumer(_clientBalanceService, _logger, _queueConfiguration, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, _countrySettings, _metricsHelper);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new BluVoucherKafkaConsumer(_clientBalanceService, _logger, _queueConfiguration, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, _countrySettings, _metricsHelper);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CannotConstructWithNullClientBalanceService()
        {
            Assert.Throws<ArgumentNullException>(() => new BluVoucherKafkaConsumer(default(IClientBalanceService)!, _logger, _queueConfiguration, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullLogger()
        {
            Assert.Throws<ArgumentNullException>(() => new BluVoucherKafkaConsumer(_clientBalanceService, default(ILoggerAdapter<HollyTopUpKafkaConsumer>)!, _queueConfiguration, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullQueueConfiguration()
        {
            Assert.Throws<NullReferenceException>(() => new BluVoucherKafkaConsumer(_clientBalanceService, _logger, default(IOptions<KafkaQueueConfiguration>)!, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullVoucherBatchProcessingRepository()
        {
            Assert.Throws<ArgumentNullException>(() => new BluVoucherKafkaConsumer(_clientBalanceService, _logger, _queueConfiguration, default(IVoucherBatchProcessingRepository)!, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullHttpClientCommunication()
        {
            Assert.Throws<ArgumentNullException>(() => new BluVoucherKafkaConsumer(_clientBalanceService, _logger, _queueConfiguration, _voucherBatchProcessingRepository, default(IHttpClientCommunication)!, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullRedisRepository()
        {
            Assert.Throws<ArgumentNullException>(() => new BluVoucherKafkaConsumer(_clientBalanceService, _logger, _queueConfiguration, _voucherBatchProcessingRepository, _httpClientCommunication, default(IRedisRepository)!, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullVpsControlCenterEndpoints()
        {
            Assert.Throws<NullReferenceException>(() => new BluVoucherKafkaConsumer(_clientBalanceService, _logger, _queueConfiguration, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, default(IOptions<VpsControlCenterEndpoints>)!, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullVoucherRedeemClientNotifications()
        {
            Assert.Throws<NullReferenceException>(() => new BluVoucherKafkaConsumer(_clientBalanceService, _logger, _queueConfiguration, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, default(IOptions<VoucherRedeemClientNotifications>)!, _dbSettings, _redisSettings, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullDbSettings()
        {
            Assert.Throws<NullReferenceException>(() => new BluVoucherKafkaConsumer(_clientBalanceService, _logger, _queueConfiguration, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, default(IOptions<DBSettings>)!, _redisSettings, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullRedisSettings()
        {
            Assert.Throws<NullReferenceException>(() => new BluVoucherKafkaConsumer(_clientBalanceService, _logger, _queueConfiguration, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, default(IOptions<RedisSettings>)!, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullCountrySettings()
        {
            Assert.Throws<NullReferenceException>(() => new BluVoucherKafkaConsumer(_clientBalanceService, _logger, _queueConfiguration, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, default(IOptions<CountrySettings>)!, _metricsHelper));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallConsumeAndProcessTransactionWithInvalidMessage(string value)
        {
            var results = await _bluVoucherKafkaConsumer.ConsumeAndProcessTransaction(value);
            Assert.False(results);
        }

        [Fact]
        public async Task ConsumeAndProcessTransaction_NullMessage_ShouldLogMessage()
        {
            // Arrange
            string message = null!;

            // Act
            _ = await _bluVoucherKafkaConsumer.ConsumeAndProcessTransaction(message);

            // Assert
            _logger.Received(1).LogError(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ConsumeAndProcessTransaction_InvalidPayload_ReturnsFalse()
        {
            // Arrange
            var message = new BluLabelProviderRequest
            {
                SessionId = ""
            };
            // Act
            var result = await _bluVoucherKafkaConsumer.ConsumeAndProcessTransaction(JsonConvert.SerializeObject(message));

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ConsumeAndProcessTransaction_ExceptionDuringProcessing_ThrowsException()
        {
            // Arrange
            string message = "valid_payload";

            // Act & Assert
            await Assert.ThrowsAsync<JsonReaderException>(async () => await _bluVoucherKafkaConsumer.ConsumeAndProcessTransaction(message));
            _logger.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            _logger.Received(1).LogCritical(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task CanCallConsumeAndProcessTransaction()
        {
            // Arrange
            var expectedResponse = ArrangeCollection.FailSyxCreditOutCome();
            var request = new KafkaQueuePayload<BluLabelProviderRequest>
            {
                RedeemOutcome = new RedeemOutcome
                {
                    OutcomeMessage = "Invalid Voucher.",
                    OutComeTypeId = -1,
                },
                VoucherRedeemRequest = new BluLabelProviderRequest
                {
                    SessionId = "123"
                },
                VoucherType = VoucherType.BluVoucher
            };
            var message = JsonConvert.SerializeObject(request);

            _clientBalanceService.CreditOnSyX(default, Arg.Any<string>(), default, Arg.Any<string>(), default, Arg.Any<string>(), default, Arg.Any<string>(), Arg.Any<string>())
                .ReturnsForAnyArgs(expectedResponse);

            _httpClientCommunication.SendRequestAsync(Arg.Any<string>(), Arg.Any<Domain.Models.Enums.HttpMethod>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, string>>>(), Arg.Any<CharsetEncoding>(), Arg.Any<TimeSpan?>()).Returns(new HttpResponseMessage());

            // Act
            _ = await _bluVoucherKafkaConsumer.ConsumeAndProcessTransaction(message);

            // Assert
            await _clientBalanceService.Received().CreditOnSyX(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<VoucherType>(), Arg.Any<string>(), Arg.Any<string>());
            await _httpClientCommunication.Received().SendRequestAsync(Arg.Any<string>(), Arg.Any<Domain.Models.Enums.HttpMethod>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, string>>>(), Arg.Any<CharsetEncoding>(), Arg.Any<TimeSpan?>());
        }

        [Fact]
        public async Task ConsumeAndProcessTransaction_ValidPayload_ButCreditOnSyxFails_ReturnsFalse()
        {
            // Arrange
            var expectedResponse = ArrangeCollection.FailSyxCreditOutCome();
            var request = new KafkaQueuePayload<BluLabelProviderRequest>
            {
                RedeemOutcome = new RedeemOutcome
                {
                    OutcomeMessage = "Invalid Voucher.",
                    OutComeTypeId = -1,
                },
                VoucherRedeemRequest = new BluLabelProviderRequest
                {
                    SessionId = "123"
                },
                VoucherType = VoucherType.BluVoucher
            };
            var message = JsonConvert.SerializeObject(request);

            _clientBalanceService.CreditOnSyX(default, Arg.Any<string>(), default, Arg.Any<string>(), default, Arg.Any<string>(), default, Arg.Any<string>(), Arg.Any<string>())
                .ReturnsForAnyArgs(expectedResponse);

            // Act
            var result = await _bluVoucherKafkaConsumer.ConsumeAndProcessTransaction(message);

            // Assert
            Assert.False(result);
            _logger.Received().LogError(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ConsumeAndProcessTransaction_ValidPayload_ShouldReturnTrue()
        {
            // Arrange
            var request = new KafkaQueuePayload<BluLabelProviderRequest>
            {
                RedeemOutcome = new RedeemOutcome
                {
                    OutcomeMessage = "Invalid Voucher.",
                    OutComeTypeId = -1,
                },
                VoucherRedeemRequest = new BluLabelProviderRequest
                {
                    SessionId = "123"
                },
                VoucherType = VoucherType.BluVoucher
            };
            var message = JsonConvert.SerializeObject(request);
            var expectedResponse = ArrangeCollection.SuccessSyxCreditOutCome();

            _clientBalanceService.CreditOnSyX(default, Arg.Any<string>(), default, Arg.Any<string>(), default, Arg.Any<string>(), default, Arg.Any<string>(), Arg.Any<string>())
               .ReturnsForAnyArgs(expectedResponse);

            // Act
            var result = await _bluVoucherKafkaConsumer.ConsumeAndProcessTransaction(message);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ConsumeAndProcessTransaction_ValidPayload_ButVoucherReferenceParsingFails_ReturnFalseDueToFormatException()
        {
            // Arrange
            var request = new KafkaQueuePayload<BluLabelProviderRequest>
            {
                RedeemOutcome = new RedeemOutcome
                {
                    OutcomeMessage = "Invalid Voucher.",
                    OutComeTypeId = -1,
                },
                VoucherRedeemRequest = new BluLabelProviderRequest
                {
                    SessionId = "123"
                },
                VoucherType = VoucherType.BluVoucher
            };
            var message = JsonConvert.SerializeObject(request);
            var expectedResponse = ArrangeCollection.FailSyxCreditOutCome();

            _clientBalanceService.CreditOnSyX(default, Arg.Any<string>(), default, Arg.Any<string>(), default, Arg.Any<string>(), default, Arg.Any<string>(), Arg.Any<string>())
               .ReturnsForAnyArgs(expectedResponse);

            // Act
            var result = await _bluVoucherKafkaConsumer.ConsumeAndProcessTransaction(message);

            // Assert
            Assert.False(result);
            _logger.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }
    }
}