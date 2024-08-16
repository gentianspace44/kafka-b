using Confluent.Kafka;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using VPS.API.Common;
using VPS.Domain.Models.BluVoucher.Requests;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Infrastructure.Repository.Redis;
using VPS.Services.BluVoucher;
using VPS.Services.HollyTopUp;
using VPS.Services.Kafka;
using VPS.Test.Common.Setup;

namespace VPS.Test.BluVoucher.BluVoucher
{
    public class BluVoucherKafkaProducerTests : IClassFixture<Fixtures>
    {
        private BluVoucherKafkaProducer _bluVoucherKafkaProducer;
        private IVpsKafkaProducer _kafkaProducer;
        private IOptions<KafkaQueueConfiguration> _queueConfiguration;
        private ILoggerAdapter<HollyTopUpKafkaProducer> _log;
        private IVoucherBatchProcessingRepository _voucherBatchProcessingRepository;
        private IHttpClientCommunication _httpClientCommunication;
        private IRedisRepository _redisRepository;
        private IOptions<VpsControlCenterEndpoints> _vpsControlCenterEndpoints;
        private IOptions<VoucherRedeemClientNotifications> _voucherRedeemClientNotifications;
        private IOptions<RedisSettings> _redisSettings;

        public BluVoucherKafkaProducerTests(Fixtures fixtures)
        {
            _redisSettings = Options.Create(fixtures.RedisSettings);
            _queueConfiguration = Options.Create(fixtures.KafkaQueueConfiguration);
            _vpsControlCenterEndpoints = Options.Create(fixtures.VPSControlCenterEndpoints);
            _voucherRedeemClientNotifications = Options.Create(fixtures.VoucherRedeemClientNotifications);

            _kafkaProducer = Substitute.For<IVpsKafkaProducer>();

            _log = Substitute.For<ILoggerAdapter<HollyTopUpKafkaProducer>>();
            _voucherBatchProcessingRepository = Substitute.For<IVoucherBatchProcessingRepository>();
            _httpClientCommunication = Substitute.For<IHttpClientCommunication>();
            _redisRepository = Substitute.For<IRedisRepository>();

            _bluVoucherKafkaProducer = new BluVoucherKafkaProducer(_kafkaProducer, _queueConfiguration, _log, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new BluVoucherKafkaProducer(_kafkaProducer, _queueConfiguration, _log, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CannotConstructWithNullKafkaProducer()
        {
            Assert.Throws<ArgumentNullException>(() => new BluVoucherKafkaProducer(default(IVpsKafkaProducer)!, _queueConfiguration, _log, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings));
        }

        [Fact]
        public void CannotConstructWithNullQueueConfiguration()
        {
            Assert.Throws<NullReferenceException>(() => new BluVoucherKafkaProducer(_kafkaProducer, default(IOptions<KafkaQueueConfiguration>)!, _log, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings));
        }

        [Fact]
        public void CannotConstructWithNullLog()
        {
            Assert.Throws<ArgumentNullException>(() => new BluVoucherKafkaProducer(_kafkaProducer, _queueConfiguration, default(ILoggerAdapter<HollyTopUpKafkaProducer>)!, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings));
        }

        [Fact]
        public void CannotConstructWithNullVoucherBatchProcessingRepository()
        {
            Assert.Throws<ArgumentNullException>(() => new BluVoucherKafkaProducer(_kafkaProducer, _queueConfiguration, _log, default(IVoucherBatchProcessingRepository)!, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings));
        }

        [Fact]
        public void CannotConstructWithNullHttpClientCommunication()
        {
            Assert.Throws<ArgumentNullException>(() => new BluVoucherKafkaProducer(_kafkaProducer, _queueConfiguration, _log, _voucherBatchProcessingRepository, default(IHttpClientCommunication)!, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings));
        }

        [Fact]
        public void CannotConstructWithNullRedisRepository()
        {
            Assert.Throws<ArgumentNullException>(() => new BluVoucherKafkaProducer(_kafkaProducer, _queueConfiguration, _log, _voucherBatchProcessingRepository, _httpClientCommunication, default(IRedisRepository)!, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings));
        }

        [Fact]
        public void CannotConstructWithNullVpsControlCenterEndpoints()
        {
            Assert.Throws<NullReferenceException>(() => new BluVoucherKafkaProducer(_kafkaProducer, _queueConfiguration, _log, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, default(IOptions<VpsControlCenterEndpoints>)!, _voucherRedeemClientNotifications, _redisSettings));
        }

        [Fact]
        public void CannotConstructWithNullVoucherRedeemClientNotifications()
        {
            Assert.Throws<NullReferenceException>(() => new BluVoucherKafkaProducer(_kafkaProducer, _queueConfiguration, _log, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, default(IOptions<VoucherRedeemClientNotifications>)!, _redisSettings));
        }

        [Fact]
        public void CannotConstructWithNullRedisSettings()
        {
            Assert.Throws<NullReferenceException>(() => new BluVoucherKafkaProducer(_kafkaProducer, _queueConfiguration, _log, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, default(IOptions<RedisSettings>)!));
        }

        [Fact]
        public async Task CanCallProduce()
        {
            // Arrange
            var bluVoucherRedeemRequest = new BluVoucherRedeemRequest
            {
                VoucherNumber = "12345678912",
                ClientId = 12345,
                VoucherReference = Guid.NewGuid().ToString(),
            };

            var voucherType = VoucherType.BluVoucher;
            var outcome = new RedeemOutcome
            {
                VoucherID = 1620971880L,
                VoucherAmount = 695034541.08M,
                OutcomeMessage = ""
            };

            _kafkaProducer.Produce(Arg.Any<string>(), Arg.Any<string>()).Returns(new DeliveryResult<Null, string>
            {
                Topic = "1927978816",
                Partition = new Partition(),
                Offset = new Offset(),
                TopicPartitionOffset = new TopicPartitionOffset(new TopicPartition("686987211", new Partition()), new Offset()),
                Status = PersistenceStatus.PossiblyPersisted,
                Message = new Message<Null, string>
                {
                    Key = null!,
                    Value = "2093928187"
                },
                Key = null!,
                Value = "927229250",
                Timestamp = new Timestamp(),
                Headers = new Headers()
            });

            _httpClientCommunication.SendRequestAsync(Arg.Any<string>(), Arg.Any<Domain.Models.Enums.HttpMethod>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, string>>>(), Arg.Any<CharsetEncoding>(), Arg.Any<TimeSpan?>()).Returns(new HttpResponseMessage());

            // Act
            await _bluVoucherKafkaProducer.Produce(bluVoucherRedeemRequest, voucherType, outcome);

            // Assert
            await _kafkaProducer.Received().Produce(Arg.Any<string>(), Arg.Any<string>());
            await _voucherBatchProcessingRepository.Received().InsertPendingBatchVoucher(Arg.Any<PendingBatchVoucher>());
            await _httpClientCommunication.Received().SendRequestAsync(Arg.Any<string>(), Arg.Any<Domain.Models.Enums.HttpMethod>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, string>>>(), Arg.Any<CharsetEncoding>(), Arg.Any<TimeSpan?>());
        }

        [Fact]
        public async Task Produce_ExceptionDuringProcessing_ThrowsException()
        {
            // Arrange
            // Act
            await _bluVoucherKafkaProducer.Produce(new BluVoucherRedeemRequest(), VoucherType.BluVoucher, default(RedeemOutcome)!);
            // Act & Assert
            _log.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            _log.Received(1).LogCritical(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Produce_ExceptionDuringProcessing_ShouldCallSendRequest()
        {
            // Arrange
            // Act
            await _bluVoucherKafkaProducer.Produce(new BluVoucherRedeemRequest(), VoucherType.BluVoucher, default(RedeemOutcome)!);
            // Act & Assert
            _log.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            _log.Received(1).LogCritical(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            await _httpClientCommunication.SendRequestAsync(Arg.Any<string>(), Domain.Models.Enums.HttpMethod.POST, Arg.Any<string>(), "application/json");
        }

        [Fact]
        public async Task Produce_GivenREquestIsNull_ShouldThrowException()
        {
            // Arrange
            var bluVoucherRedeemRequest = new BluVoucherRedeemRequest
            {
                VoucherNumber = "12345678912",
            };

            // Act
            await _bluVoucherKafkaProducer.Produce(bluVoucherRedeemRequest, VoucherType.BluVoucher, default(RedeemOutcome)!);

            // Assert
            _log.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            _log.Received().LogInformation(bluVoucherRedeemRequest.VoucherNumber, "Inserting voucher {voucherNumber} to VoucherBatchProcessing", Arg.Any<string>(), bluVoucherRedeemRequest.VoucherNumber);
            await _httpClientCommunication.SendRequestAsync(Arg.Any<string>(), Domain.Models.Enums.HttpMethod.POST, Arg.Any<string>(), "application/json");
        }

        [Fact]
        public async Task Produce_GivenReedemOutcome_ShouldWaitAndRetryAsync()
        {
            // Arrange
            // Arrange
            var bluVoucherRedeemRequest = new BluVoucherRedeemRequest
            {
                VoucherNumber = "12345678912",
                ClientId = 12345,
                VoucherReference = Guid.NewGuid().ToString(),
            };
            // Act
            await _bluVoucherKafkaProducer.Produce(bluVoucherRedeemRequest, VoucherType.BluVoucher, default(RedeemOutcome)!);
            // Act & Assert
            _log.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            _log.Received(1).LogCritical(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            await _httpClientCommunication.SendRequestAsync(Arg.Any<string>(), Domain.Models.Enums.HttpMethod.POST, Arg.Any<string>(), "application/json");
            await Assert.ThrowsAsync<NullReferenceException>(async () => await _bluVoucherKafkaProducer.Produce(default(BluVoucherRedeemRequest)!, VoucherType.BluVoucher, default(RedeemOutcome)!));
        }

        [Fact]
        public async Task Produce_GivenResponseRequestIsNull_ShouldThrowException()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () => await _bluVoucherKafkaProducer.Produce(default(BluVoucherRedeemRequest)!, VoucherType.BluVoucher, new RedeemOutcome() { OutcomeMessage = "" }));
        }
    }
}