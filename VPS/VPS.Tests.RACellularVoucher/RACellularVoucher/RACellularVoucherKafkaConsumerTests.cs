using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSubstitute;
using VPS.API.Common;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.RACellularVoucher.Requests;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Infrastructure.Repository.Redis;
using VPS.Services.Common;
using VPS.Services.RACellularVoucher;
using VPS.Test.Common.Setup;
using Xunit;

namespace VPS.Tests.RACellularVoucher.RACellularVoucher
{
    public class RaCellularVoucherKafkaConsumerTests : IClassFixture<Fixtures>
    {
        private RaCellularVoucherKafkaConsumer _raCellularVoucherKafkaConsumer;
        private IClientBalanceService _clientBalanceService;
        private IOptions<KafkaQueueConfiguration> _queueConfiguration;
        private ILoggerAdapter<RaCellularVoucherKafkaConsumer> _logger;
        private IVoucherBatchProcessingRepository _voucherBatchProcessingRepository;
        private IHttpClientCommunication _httpClientCommunication;
        private IRedisRepository _redisRepository;
        private IOptions<VpsControlCenterEndpoints> _vpsControlCenterEndpoints;
        private IOptions<VoucherRedeemClientNotifications> _voucherRedeemClientNotifications;
        private IOptions<DBSettings> _dbSettings;
        private IOptions<RedisSettings> _redisSettings;
        private IOptions<CountrySettings> _countrySettings;
        private MetricsHelper _metricsHelper;

        public RaCellularVoucherKafkaConsumerTests(Fixtures fixtures)
        {
            _queueConfiguration = Options.Create(fixtures.KafkaQueueConfiguration);
            _vpsControlCenterEndpoints = Options.Create(fixtures.VPSControlCenterEndpoints);
            _voucherRedeemClientNotifications = Options.Create(fixtures.VoucherRedeemClientNotifications);
            _dbSettings = Options.Create(fixtures.DBSettings);
            _redisSettings = Options.Create(fixtures.RedisSettings);
            _countrySettings = Options.Create(fixtures.CountrySettings);

            _clientBalanceService = Substitute.For<IClientBalanceService>();
            _logger = Substitute.For<ILoggerAdapter<RaCellularVoucherKafkaConsumer>>();
            _voucherBatchProcessingRepository = Substitute.For<IVoucherBatchProcessingRepository>();
            _httpClientCommunication = Substitute.For<IHttpClientCommunication>();
            _redisRepository = Substitute.For<IRedisRepository>();
            _metricsHelper = new MetricsHelper();
            _raCellularVoucherKafkaConsumer = new RaCellularVoucherKafkaConsumer(_clientBalanceService, _queueConfiguration, _logger, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, _countrySettings, _metricsHelper);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new RaCellularVoucherKafkaConsumer(
                _clientBalanceService,
                _queueConfiguration,
                _logger,
                _voucherBatchProcessingRepository,
                _httpClientCommunication,
                _redisRepository,
                _vpsControlCenterEndpoints,
                _voucherRedeemClientNotifications,
                _dbSettings,
                _redisSettings,
                _countrySettings,
                _metricsHelper);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CannotConstructWithNullClientBalanceService()
        {
            Assert.Throws<ArgumentNullException>(() => new RaCellularVoucherKafkaConsumer(default(IClientBalanceService)!, _queueConfiguration, _logger, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullQueueConfiguration()
        {
            Assert.Throws<ArgumentNullException>(() => new RaCellularVoucherKafkaConsumer(_clientBalanceService, default(IOptions<KafkaQueueConfiguration>)!, _logger, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullLogger()
        {
            Assert.Throws<ArgumentNullException>(() => new RaCellularVoucherKafkaConsumer(_clientBalanceService, _queueConfiguration, default(ILoggerAdapter<RaCellularVoucherKafkaConsumer>)!, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullVoucherBatchProcessingRepository()
        {
            Assert.Throws<ArgumentNullException>(() => new RaCellularVoucherKafkaConsumer(_clientBalanceService, _queueConfiguration, _logger, default(IVoucherBatchProcessingRepository)!, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullHttpClientCommunication()
        {
            Assert.Throws<ArgumentNullException>(() => new RaCellularVoucherKafkaConsumer(_clientBalanceService, _queueConfiguration, _logger, _voucherBatchProcessingRepository, default(IHttpClientCommunication)!, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullRedisRepository()
        {
            Assert.Throws<ArgumentNullException>(() => new RaCellularVoucherKafkaConsumer(_clientBalanceService, _queueConfiguration, _logger, _voucherBatchProcessingRepository, _httpClientCommunication, default(IRedisRepository)!, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullVpsControlCenterEndpoints()
        {
            Assert.Throws<ArgumentNullException>(() => new RaCellularVoucherKafkaConsumer(_clientBalanceService, _queueConfiguration, _logger, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, default(IOptions<VpsControlCenterEndpoints>)!, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullVoucherRedeemClientNotifications()
        {
            Assert.Throws<ArgumentNullException>(() => new RaCellularVoucherKafkaConsumer(_clientBalanceService, _queueConfiguration, _logger, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, default(IOptions<VoucherRedeemClientNotifications>)!, _dbSettings, _redisSettings, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullDbSettings()
        {
            Assert.Throws<ArgumentNullException>(() => new RaCellularVoucherKafkaConsumer(_clientBalanceService, _queueConfiguration, _logger, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, default(IOptions<DBSettings>)!, _redisSettings, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullRedisSettings()
        {
            Assert.Throws<ArgumentNullException>(() => new RaCellularVoucherKafkaConsumer(_clientBalanceService, _queueConfiguration, _logger, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, default(IOptions<RedisSettings>)!, _countrySettings, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullCountrySettings()
        {
            Assert.Throws<ArgumentNullException>(() => new RaCellularVoucherKafkaConsumer(_clientBalanceService, _queueConfiguration, _logger, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, default(IOptions<CountrySettings>)!, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullMetricsHelper()
        {
            Assert.Throws<ArgumentNullException>(() => new RaCellularVoucherKafkaConsumer(_clientBalanceService, _queueConfiguration, _logger, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _dbSettings, _redisSettings, _countrySettings, default(MetricsHelper)!));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ConsumeAndProcessTransaction_InvalidMessage_ReturnsFalse(string value)
        {
            var results = await _raCellularVoucherKafkaConsumer.ConsumeAndProcessTransaction(value);
            Assert.False(results);
        }

        [Fact]
        public async Task ConsumeAndProcessTransaction_NullMessage_ShouldLogMessage()
        {
            // Arrange
            string message = null!;

            // Act
            _ = await _raCellularVoucherKafkaConsumer.ConsumeAndProcessTransaction(message);

            // Assert
            _logger.Received(1).LogError(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ConsumeAndProcessTransaction_InvalidPayload_ReturnsFalse()
        {
            // Arrange
            var message = new RaCellularVoucherRedeemRequest
            {
                ClientId = 123,
                VoucherNumber = "12345678965"
            };
            // Act
            var result = await _raCellularVoucherKafkaConsumer.ConsumeAndProcessTransaction(JsonConvert.SerializeObject(message));

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ConsumeAndProcessTransaction_ExceptionDuringProcessing_ThrowsException()
        {
            // Arrange
            string message = "valid_payload";

            // Act & Assert
            await Assert.ThrowsAsync<JsonReaderException>(async () => await _raCellularVoucherKafkaConsumer.ConsumeAndProcessTransaction(message));
            _logger.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            _logger.Received(1).LogCritical(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ConsumeAndProcessTransaction_ValidPayload_ButCreditOnSyxFails_ReturnsFalse()
        {
            // Arrange
            var expectedResponse = ArrangeCollection.FailSyxCreditOutCome();
            var request = new KafkaQueuePayload<RaCellularVoucherRedeemRequest>
            {
                RedeemOutcome = new RedeemOutcome
                {
                    OutcomeMessage = "Invalid Voucher.",
                    OutComeTypeId = -1,
                },
                VoucherRedeemRequest = new RaCellularVoucherRedeemRequest
                {
                    ClientId = 123,
                    VoucherNumber = "12345678965"
                },
                VoucherType = VoucherType.RACellular
            };
            var message = JsonConvert.SerializeObject(request);

            _clientBalanceService.CreditOnSyX(default, Arg.Any<string>(), default, Arg.Any<string>(), default, Arg.Any<string>(), default, Arg.Any<string>(), Arg.Any<string>())
                .ReturnsForAnyArgs(expectedResponse);

            // Act
            var result = await _raCellularVoucherKafkaConsumer.ConsumeAndProcessTransaction(message);

            // Assert
            Assert.False(result);
            _logger.Received().LogError(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ConsumeAndProcessTransaction_ValidPayload_ShouldReturnTrue()
        {
            // Arrange
            var request = new KafkaQueuePayload<RaCellularVoucherRedeemRequest>
            {
                RedeemOutcome = new RedeemOutcome
                {
                    OutcomeMessage = "Invalid Voucher.",
                    OutComeTypeId = -1,
                },
                VoucherRedeemRequest = new RaCellularVoucherRedeemRequest
                {
                    ClientId = 123,
                    VoucherNumber = "12345678965"
                },
                VoucherType = VoucherType.RACellular
            };
            var message = JsonConvert.SerializeObject(request);
            var expectedResponse = ArrangeCollection.SuccessSyxCreditOutCome();

            _clientBalanceService.CreditOnSyX(default, Arg.Any<string>(), default, Arg.Any<string>(), default, Arg.Any<string>(), default, Arg.Any<string>(), Arg.Any<string>())
               .ReturnsForAnyArgs(expectedResponse);

            // Act
            var result = await _raCellularVoucherKafkaConsumer.ConsumeAndProcessTransaction(message);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ConsumeAndProcessTransaction_ValidPayload_ButVoucherReferenceParsingFails_ReturnFalseDueToFormatException()
        {
            // Arrange
            var request = new KafkaQueuePayload<RaCellularVoucherRedeemRequest>
            {
                RedeemOutcome = new RedeemOutcome
                {
                    OutcomeMessage = "Invalid Voucher.",
                    OutComeTypeId = -1,
                },
                VoucherRedeemRequest = new RaCellularVoucherRedeemRequest
                {
                    ClientId = 123,
                    VoucherNumber = "12345678965",
                    VoucherReference = "#4$"
                },
                VoucherType = VoucherType.RACellular
            };
            var message = JsonConvert.SerializeObject(request);
            var expectedResponse = ArrangeCollection.FailSyxCreditOutCome();

            _clientBalanceService.CreditOnSyX(default, Arg.Any<string>(), default, Arg.Any<string>(), default, Arg.Any<string>(), default, Arg.Any<string>(), Arg.Any<string>())
               .ReturnsForAnyArgs(expectedResponse);

            // Act
            var result = await _raCellularVoucherKafkaConsumer.ConsumeAndProcessTransaction(message);

            // Assert
            Assert.False(result);
            _logger.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }
    }
}