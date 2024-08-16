using Confluent.Kafka;
using Microsoft.Extensions.Options;
using NSubstitute;
using VPS.API.Common;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.EasyLoad.Request;
using VPS.Domain.Models.Enums;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Infrastructure.Repository.Redis;
using VPS.Services.EasyLoad;
using VPS.Services.Kafka;
using VPS.Test.EasyLoad.Setup;

namespace VPS.Test.EasyLoad.Services
{
    public class EasyLoadKafkaProducerTests : IClassFixture<Fixtures>
    {
        private EasyLoadKafkaProducer _easyLoadKafkaProducer;
        private IVpsKafkaProducer _kafkaProducer;
        private IOptions<KafkaQueueConfiguration> _queueConfiguration;
        private ILoggerAdapter<EasyLoadKafkaProducer> _log;
        private IVoucherBatchProcessingRepository _voucherBatchProcessingRepository;
        private IHttpClientCommunication _httpClientCommunication;
        private IRedisRepository _redisRepository;
        private IOptions<VpsControlCenterEndpoints> _vpsControlCenterEndpoints;
        private IOptions<VoucherRedeemClientNotifications> _voucherRedeemClientNotifications;
        private IOptions<RedisSettings> _redisSettings;

        public EasyLoadKafkaProducerTests(Fixtures fixtures)
        {
            _redisSettings = Options.Create(fixtures.RedisSettings);
            _queueConfiguration = Options.Create(fixtures.KafkaQueueConfiguration);
            _vpsControlCenterEndpoints = Options.Create(fixtures.VpsControlCenterEndpoints);
            _voucherRedeemClientNotifications = Options.Create(fixtures.VoucherRedeemClientNotifications);

            _kafkaProducer = Substitute.For<IVpsKafkaProducer>();
            _log = Substitute.For<ILoggerAdapter<EasyLoadKafkaProducer>>();
            _voucherBatchProcessingRepository = Substitute.For<IVoucherBatchProcessingRepository>();
            _httpClientCommunication = Substitute.For<IHttpClientCommunication>();
            _redisRepository = Substitute.For<IRedisRepository>();
            _easyLoadKafkaProducer = new EasyLoadKafkaProducer(_kafkaProducer, _queueConfiguration, _log, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new EasyLoadKafkaProducer(_kafkaProducer, _queueConfiguration, _log, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CannotConstructWithNullKafkaProducer()
        {
            Assert.Throws<ArgumentNullException>(() => new EasyLoadKafkaProducer(default(IVpsKafkaProducer)!, _queueConfiguration, _log, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings));
        }

        [Fact]
        public void CannotConstructWithNullQueueConfiguration()
        {
            Assert.Throws<ArgumentNullException>(() => new EasyLoadKafkaProducer(_kafkaProducer, default(IOptions<KafkaQueueConfiguration>)!, _log, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings));
        }

        [Fact]
        public void CannotConstructWithNullLog()
        {
            Assert.Throws<ArgumentNullException>(() => new EasyLoadKafkaProducer(_kafkaProducer, _queueConfiguration, default(ILoggerAdapter<EasyLoadKafkaProducer>)!, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings));
        }

        [Fact]
        public void CannotConstructWithNullVoucherBatchProcessingRepository()
        {
            Assert.Throws<ArgumentNullException>(() => new EasyLoadKafkaProducer(_kafkaProducer, _queueConfiguration, _log, default(IVoucherBatchProcessingRepository)!, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings));
        }

        [Fact]
        public void CannotConstructWithNullHttpClientCommunication()
        {
            Assert.Throws<ArgumentNullException>(() => new EasyLoadKafkaProducer(_kafkaProducer, _queueConfiguration, _log, _voucherBatchProcessingRepository, default(IHttpClientCommunication)!, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings));
        }

        [Fact]
        public void CannotConstructWithNullRedisRepository()
        {
            Assert.Throws<ArgumentNullException>(() => new EasyLoadKafkaProducer(_kafkaProducer, _queueConfiguration, _log, _voucherBatchProcessingRepository, _httpClientCommunication, default(IRedisRepository)!, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings));
        }

        [Fact]
        public void CannotConstructWithNullVpsControlCenterEndpoints()
        {
            Assert.Throws<ArgumentNullException>(() => new EasyLoadKafkaProducer(_kafkaProducer, _queueConfiguration, _log, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, default(IOptions<VpsControlCenterEndpoints>)!, _voucherRedeemClientNotifications, _redisSettings));
        }

        [Fact]
        public void CannotConstructWithNullVoucherRedeemClientNotifications()
        {
            Assert.Throws<ArgumentNullException>(() => new EasyLoadKafkaProducer(_kafkaProducer, _queueConfiguration, _log, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, default(IOptions<VoucherRedeemClientNotifications>)!, _redisSettings));
        }

        [Fact]
        public void CannotConstructWithNullRedisSettings()
        {
            Assert.Throws<ArgumentNullException>(() => new EasyLoadKafkaProducer(_kafkaProducer, _queueConfiguration, _log, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, default(IOptions<RedisSettings>)!));
        }

        [Fact]
        public async Task CanCallProduceAsync()
        {
            // Arrange
            var raCellularVoucherRedeemRequest = new EasyLoadVoucherRedeemRequest
            {
                VoucherNumber = "12345678912",
                ClientId = 12345,
                VoucherReference = Guid.NewGuid().ToString(),
            };
            var voucherType = VoucherType.EasyLoad;
            var outcome = new RedeemOutcome
            {
                VoucherID = 496689284,
                VoucherAmount = 76.96M,
                OutcomeMessage = ""
            };

            _kafkaProducer.Produce(Arg.Any<string>(), Arg.Any<string>()).Returns(new DeliveryResult<Null, string>
            {
                Topic = "1927978816",
                Partition = new Partition(),
                Offset = new Offset(),
                TopicPartitionOffset = new TopicPartitionOffset(new TopicPartition("1028586546", new Partition()), new Offset()),
                Status = PersistenceStatus.PossiblyPersisted,
                Message = new Message<Null, string>
                {
                    Key = null!,
                    Value = "2114339789"
                },
                Key = null!,
                Value = "1305773186",
                Timestamp = new Timestamp(),
                Headers = new Headers()
            });

            _httpClientCommunication.SendRequestAsync(Arg.Any<string>(), Arg.Any<VPS.Domain.Models.Enums.HttpMethod>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, string>>>(), Arg.Any<CharsetEncoding>(), Arg.Any<TimeSpan?>()).Returns(new HttpResponseMessage());

            // Act
            await _easyLoadKafkaProducer.Produce(raCellularVoucherRedeemRequest, voucherType, outcome);

            // Assert
            await _kafkaProducer.Received().Produce(Arg.Any<string>(), Arg.Any<string>());
            await _voucherBatchProcessingRepository.Received().InsertPendingBatchVoucher(Arg.Any<PendingBatchVoucher>());
            await _httpClientCommunication.Received().SendRequestAsync(Arg.Any<string>(), Arg.Any<VPS.Domain.Models.Enums.HttpMethod>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, string>>>(), Arg.Any<CharsetEncoding>(), Arg.Any<TimeSpan?>());
        }

        [Fact]
        public void ConsumeAndProcessTransaction_ExceptionDuringProcessing_ThrowsException()
        {
            // Arrange
            // Act
            _ = _easyLoadKafkaProducer.Produce(new EasyLoadVoucherRedeemRequest(), VoucherType.EasyLoad, default(RedeemOutcome)!);
            // Act & Assert
            _log.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            _log.Received(1).LogCritical(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ConsumeAndProcessTransaction_ExceptionDuringProcessing_ShouldCallSendRequest()
        {
            // Arrange
            // Act
            _ = _easyLoadKafkaProducer.Produce(new EasyLoadVoucherRedeemRequest(), VoucherType.EasyLoad, default(RedeemOutcome)!);
            // Act & Assert
            _log.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            _log.Received(1).LogCritical(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            await _httpClientCommunication.SendRequestAsync(Arg.Any<string>(), Domain.Models.Enums.HttpMethod.POST, Arg.Any<string>(), "application/json");
        }
    }
}
