using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSubstitute;
using VPS.API.Common;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.EasyLoad.Request;
using VPS.Domain.Models.Enums;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Infrastructure.Repository.Redis;
using VPS.Services.Common;
using VPS.Services.EasyLoad;
using VPS.Test.EasyLoad.Setup;

namespace VPS.Test.EasyLoad.Services
{
    public class EasyLoadConsumerTest : IClassFixture<Fixtures>
    {
        private readonly ILoggerAdapter<EasyLoadKafkaConsumer> _logger = Substitute.For<ILoggerAdapter<EasyLoadKafkaConsumer>>();
        private readonly IOptions<KafkaQueueConfiguration> _queueConfiguration;
        private readonly IClientBalanceService _clientBalanceService = Substitute.For<IClientBalanceService>();
        private readonly IVoucherBatchProcessingRepository _voucherBatchProcessingRepository = Substitute.For<IVoucherBatchProcessingRepository>();
        private readonly IHttpClientCommunication _httpClientCommunication = Substitute.For<IHttpClientCommunication>();
        private readonly IOptions<VpsControlCenterEndpoints> _vpsControlCenterEndpoints;
        private readonly IOptions<VoucherRedeemClientNotifications> _voucherRedeemClientNotifications;
        private readonly IOptions<DBSettings> _dbSettings;
        private readonly IRedisRepository _redisRepository = Substitute.For<IRedisRepository>();
        private readonly IOptions<RedisSettings> _redisSettings;
        private readonly IOptions<CountrySettings> _countrySettings;
        private readonly MetricsHelper _metricsHelper = Substitute.For<MetricsHelper>();


        public EasyLoadConsumerTest(Fixtures fixtures)
        {
            _queueConfiguration = Options.Create(fixtures.KafkaQueueConfiguration);
            _vpsControlCenterEndpoints = Options.Create(fixtures.VpsControlCenterEndpoints);
            _voucherRedeemClientNotifications = Options.Create(fixtures.VoucherRedeemClientNotifications);
            _dbSettings = Options.Create(fixtures.DBSettings);
            _redisSettings = Options.Create(fixtures.RedisSettings);
            _countrySettings = Options.Create(fixtures.CountrySettings);
        }

        [Fact]
        public void Construct_GivenIClientBalanceServiceIsNull_ShouldThrowException()
        {
            //Arrange
            //Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                return new EasyLoadKafkaConsumer(Arg.Any<IClientBalanceService>(),
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
            });

            //Assert
            Assert.Equal("clientBalanceService", exception.ParamName);
        }


        [Fact]
        public void Construct_GivenILoggerAdapterIsNull_ShouldThrowException()
        {
            //Arrange
            //Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                return new EasyLoadKafkaConsumer(_clientBalanceService,
                                                Arg.Any<ILoggerAdapter<EasyLoadKafkaConsumer>>(),
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
            });

            //Assert
            Assert.Equal("logger", exception.ParamName);
        }


        [Fact]
        public void Construct_GivenKafkaQueueConfigurationIsNull_ShouldThrowException()
        {
            //Arrange
            //Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                return new EasyLoadKafkaConsumer(_clientBalanceService,
                                                _logger,
                                                Arg.Any<IOptions<KafkaQueueConfiguration>>(),
                                                _voucherBatchProcessingRepository,
                                                _httpClientCommunication,
                                                _redisRepository,
                                                _vpsControlCenterEndpoints,
                                                _voucherRedeemClientNotifications,
                                                _dbSettings,
                                                _redisSettings,
                                                _countrySettings,
                                                _metricsHelper);
            });

            //Assert
            Assert.Equal("queueConfiguration", exception.ParamName);
        }

        [Fact]
        public void Construct_GivenIVoucherBatchProcessingRepositoryIsNull_ShouldThrowException()
        {
            //Arrange
            //Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                return new EasyLoadKafkaConsumer(_clientBalanceService,
                                                _logger,
                                                 _queueConfiguration,
                                                Arg.Any<IVoucherBatchProcessingRepository>(),
                                                _httpClientCommunication,
                                                _redisRepository,
                                                _vpsControlCenterEndpoints,
                                                _voucherRedeemClientNotifications,
                                                _dbSettings,
                                                _redisSettings,
                                                _countrySettings,
                                                _metricsHelper);
            });

            //Assert
            Assert.Equal("voucherBatchProcessingRepository", exception.ParamName);
        }

        [Fact]
        public void Construct_GivenIHttpClientCommunicationIsNull_ShouldThrowException()
        {
            //Arrange
            //Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                return new EasyLoadKafkaConsumer(_clientBalanceService,
                                                _logger,
                                                 _queueConfiguration,
                                                _voucherBatchProcessingRepository,
                                                Arg.Any<IHttpClientCommunication>(),
                                                _redisRepository,
                                                _vpsControlCenterEndpoints,
                                                _voucherRedeemClientNotifications,
                                                _dbSettings,
                                                _redisSettings,
                                                _countrySettings,
                                                _metricsHelper);
            });

            //Assert
            Assert.Equal("httpClientCommunication", exception.ParamName);
        }

        [Fact]
        public void Construct_GivenIRedisRepositoryIsNull_ShouldThrowException()
        {
            //Arrange
            //Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                return new EasyLoadKafkaConsumer(_clientBalanceService,
                                                _logger,
                                                 _queueConfiguration,
                                                _voucherBatchProcessingRepository,
                                                _httpClientCommunication,
                                                Arg.Any<IRedisRepository>(),
                                                _vpsControlCenterEndpoints,
                                                _voucherRedeemClientNotifications,
                                                _dbSettings,
                                                _redisSettings,
                                                _countrySettings,
                                                _metricsHelper);
            });

            //Assert
            Assert.Equal("redisRepository", exception.ParamName);
        }


        [Fact]
        public void Construct_GivenVpsControlCenterEndpointsIsNull_ShouldThrowException()
        {
            //Arrange
            //Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                return new EasyLoadKafkaConsumer(_clientBalanceService,
                                                _logger,
                                                 _queueConfiguration,
                                                _voucherBatchProcessingRepository,
                                                _httpClientCommunication,
                                                _redisRepository,
                                                Arg.Any<IOptions<VpsControlCenterEndpoints>>(),
                                                _voucherRedeemClientNotifications,
                                                _dbSettings,
                                                _redisSettings,
                                                _countrySettings,
                                                _metricsHelper);
            });

            //Assert
            Assert.Equal("vpsControlCenterEndpoints", exception.ParamName);
        }

        [Fact]
        public void Construct_GivenVoucherRedeemClientNotificationsIsNull_ShouldThrowException()
        {
            //Arrange
            //Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                return new EasyLoadKafkaConsumer(_clientBalanceService,
                                                _logger,
                                                 _queueConfiguration,
                                                _voucherBatchProcessingRepository,
                                                _httpClientCommunication,
                                                _redisRepository,
                                                _vpsControlCenterEndpoints,
                                                Arg.Any<IOptions<VoucherRedeemClientNotifications>>(),
                                                _dbSettings,
                                                _redisSettings,
                                                _countrySettings,
                                                _metricsHelper);
            });

            //Assert
            Assert.Equal("voucherRedeemClientNotifications", exception.ParamName);
        }


        [Fact]
        public void Construct_GivenVpsDbSettingsIsNull_ShouldThrowException()
        {
            //Arrange
            //Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                return new EasyLoadKafkaConsumer(_clientBalanceService,
                                                _logger,
                                                 _queueConfiguration,
                                                _voucherBatchProcessingRepository,
                                                _httpClientCommunication,
                                                _redisRepository,
                                                _vpsControlCenterEndpoints,
                                                _voucherRedeemClientNotifications,
                                                Arg.Any<IOptions<DBSettings>>(),
                                                _redisSettings,
                                                _countrySettings,
                                                _metricsHelper);
            });

            //Assert
            Assert.Equal("dbSettings", exception.ParamName);
        }

        [Fact]
        public void Construct_GivenVpsRedisSettingsIsNull_ShouldThrowException()
        {
            //Arrange
            //Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                return new EasyLoadKafkaConsumer(_clientBalanceService,
                                                _logger,
                                                 _queueConfiguration,
                                                _voucherBatchProcessingRepository,
                                                _httpClientCommunication,
                                                _redisRepository,
                                                _vpsControlCenterEndpoints,
                                                _voucherRedeemClientNotifications,
                                                _dbSettings,
                                                Arg.Any<IOptions<RedisSettings>>(),
                                                _countrySettings,
                                                _metricsHelper);
            });

            //Assert
            Assert.Equal("redisSettings", exception.ParamName);
        }

        [Fact]
        public void Construct_GivenVpsCountrySettingsIsNull_ShouldThrowException()
        {
            //Arrange
            //Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                return new EasyLoadKafkaConsumer(_clientBalanceService,
                                                _logger,
                                                 _queueConfiguration,
                                                _voucherBatchProcessingRepository,
                                                _httpClientCommunication,
                                                _redisRepository,
                                                _vpsControlCenterEndpoints,
                                                _voucherRedeemClientNotifications,
                                                _dbSettings,
                                                _redisSettings,
                                                Arg.Any<IOptions<CountrySettings>>(),
                                                _metricsHelper);
            });

            //Assert
            Assert.Equal("countrySettings", exception.ParamName);
        }


        [Fact]
        public void Construct_GivenVpsMetricsHelperIsNull_ShouldThrowException()
        {
            //Arrange
            //Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                return new EasyLoadKafkaConsumer(_clientBalanceService,
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
                                                Arg.Any<MetricsHelper>());
            });

            //Assert
            Assert.Equal("metricsHelper", exception.ParamName);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ConsumeAndProcessTransaction_InvalidMessage_ReturnsFalse(string value)
        {
            var consumer = new EasyLoadKafkaConsumer(_clientBalanceService,
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

            var results = await consumer.ConsumeAndProcessTransaction(value);
            Assert.False(results);
        }

        [Fact]
        public async Task ConsumeAndProcessTransaction_NullMessage_ShouldLogMessage()
        {
            // Arrange
            var consumer = new EasyLoadKafkaConsumer(_clientBalanceService,
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

            string message = null!;

            // Act
            _ = await consumer.ConsumeAndProcessTransaction(message);

            // Assert
            _logger.Received(1).LogError(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Test_InvalidPayload_ReturnsFalse()
        {
            // Arrange
            var consumer = new EasyLoadKafkaConsumer(_clientBalanceService,
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

            var message = new EasyLoadVoucherRedeemRequest
            {
                ClientId = 123,
                VoucherNumber = "12345678965"
            };
            // Act
            var result = await consumer.ConsumeAndProcessTransaction(JsonConvert.SerializeObject(message));

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ConsumeAndProcessTransaction_ExceptionDuringProcessing_ThrowsException()
        {
            // Arrange
            var consumer = new EasyLoadKafkaConsumer(_clientBalanceService,
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

            string message = "valid_payload";

            // Act & Assert
            await Assert.ThrowsAsync<JsonReaderException>(async () => await consumer.ConsumeAndProcessTransaction(message));
            _logger.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            _logger.Received(1).LogCritical(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ConsumeAndProcessTransaction_ValidPayload_ButCreditOnSyxFails_ReturnsFalse()
        {
            // Arrange
            var consumer = new EasyLoadKafkaConsumer(_clientBalanceService,
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

            var expectedResponse = ArrangeCollection.FailSyxCreditOutCome();
            var request = new KafkaQueuePayload<EasyLoadVoucherRedeemRequest>
            {
                RedeemOutcome = new RedeemOutcome
                {
                    OutcomeMessage = "Invalid Voucher.",
                    OutComeTypeId = -1,
                },
                VoucherRedeemRequest = new EasyLoadVoucherRedeemRequest
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
            var result = await consumer.ConsumeAndProcessTransaction(message);

            // Assert
            Assert.False(result);
            _logger.Received().LogError(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ConsumeAndProcessTransaction_ValidPayload_ShouldReturnTrue()
        {
            // Arrange
            var consumer = new EasyLoadKafkaConsumer(_clientBalanceService,
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

            var request = new KafkaQueuePayload<EasyLoadVoucherRedeemRequest>
            {
                RedeemOutcome = new RedeemOutcome
                {
                    OutcomeMessage = "Invalid Voucher.",
                    OutComeTypeId = -1,
                },
                VoucherRedeemRequest = new EasyLoadVoucherRedeemRequest
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
            var result = await consumer.ConsumeAndProcessTransaction(message);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ConsumeAndProcessTransaction_ValidPayload_ButVoucherReferenceParsingFails_ReturnFalseDueToFormatException()
        {
            // Arrange
            // Arrange
            var consumer = new EasyLoadKafkaConsumer(_clientBalanceService,
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

            var request = new KafkaQueuePayload<EasyLoadVoucherRedeemRequest>
            {
                RedeemOutcome = new RedeemOutcome
                {
                    OutcomeMessage = "Invalid Voucher.",
                    OutComeTypeId = -1,
                },
                VoucherRedeemRequest = new EasyLoadVoucherRedeemRequest
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
            var result = await consumer.ConsumeAndProcessTransaction(message);

            // Assert
            Assert.False(result);
            _logger.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }
    }
}
