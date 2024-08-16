using Confluent.Kafka;
using Microsoft.Extensions.Options;
using NSubstitute;
using VPS.API.Common;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.HollyTopUp.Requests;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Infrastructure.Repository.Redis;
using VPS.Services.HollyTopUp;
using VPS.Services.Kafka;
using VPS.Test.Common.Setup;
using Xunit;

namespace VPS.Tests.HollyTopUp.Services
{
    public class HollyTopUpKafkaProducerTest : IClassFixture<Fixtures>
    {
        private readonly HollyTopUpKafkaProducer _hollyTopUpKafkaProducer;
        private readonly IVpsKafkaProducer _kafkaProducer;
        private readonly ILoggerAdapter<HollyTopUpKafkaProducer> _log;
        private readonly IVoucherBatchProcessingRepository _voucherBatchProcessingRepository;
        private readonly IHttpClientCommunication _httpClientCommunication;
        private readonly IRedisRepository _redisRepository;
        private readonly IOptions<VpsControlCenterEndpoints> _vpsControlCenterEndpoints;
        private readonly IOptions<VoucherRedeemClientNotifications> _voucherRedeemClientNotifications;
        private readonly IOptions<RedisSettings> _redisSettings;
        private readonly IOptions<KafkaQueueConfiguration> _kafkaQueueConfiguration;

        public HollyTopUpKafkaProducerTest(Fixtures fixtures)
        {
            _kafkaProducer = Substitute.For<IVpsKafkaProducer>();
            _log = Substitute.For<ILoggerAdapter<HollyTopUpKafkaProducer>>();
            _voucherBatchProcessingRepository = Substitute.For<IVoucherBatchProcessingRepository>();
            _httpClientCommunication = Substitute.For<IHttpClientCommunication>();
            _redisRepository = Substitute.For<IRedisRepository>();

            _redisSettings = Options.Create(fixtures.RedisSettings);
            _kafkaQueueConfiguration = Options.Create(fixtures.KafkaQueueConfiguration);
            _vpsControlCenterEndpoints = Options.Create(fixtures.VPSControlCenterEndpoints);
            _voucherRedeemClientNotifications = Options.Create(fixtures.VoucherRedeemClientNotifications);

            _hollyTopUpKafkaProducer = new HollyTopUpKafkaProducer(_kafkaProducer, _kafkaQueueConfiguration, _log, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings);
        }


        [Fact]
        public void Construct_Instance_Test()
        {
            // Act
            var kafkaProducerInstance = new HollyTopUpKafkaProducer(_kafkaProducer, _kafkaQueueConfiguration, _log, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings);

            // Assert
            Assert.NotNull(kafkaProducerInstance);
        }

        [Fact]
        public void Constructor_When_KafkaProducer_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new HollyTopUpKafkaProducer(null, _kafkaQueueConfiguration, _log, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings));
        }


        [Fact]
        public void Constructor_When_QueueConfiguration_Is_Null()
        {
            Assert.Throws<NullReferenceException>(() => new HollyTopUpKafkaProducer(_kafkaProducer, default(IOptions<KafkaQueueConfiguration>)!, _log, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings));
        }

        [Fact]
        public void Constructor_When_Log_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new HollyTopUpKafkaProducer(_kafkaProducer, _kafkaQueueConfiguration, default(ILoggerAdapter<HollyTopUpKafkaProducer>)!, _voucherBatchProcessingRepository, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings));
        }

        [Fact]
        public void Constructor_When_BatchRepository_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new HollyTopUpKafkaProducer(_kafkaProducer, _kafkaQueueConfiguration, _log, default(IVoucherBatchProcessingRepository)!, _httpClientCommunication, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings));
        }

        [Fact]
        public void CannotConstructWithNullHttpClientCommunication()
        {
            Assert.Throws<ArgumentNullException>(() => new HollyTopUpKafkaProducer(_kafkaProducer, _kafkaQueueConfiguration, _log, _voucherBatchProcessingRepository, default(IHttpClientCommunication)!, _redisRepository, _vpsControlCenterEndpoints, _voucherRedeemClientNotifications, _redisSettings));
        }

        [Fact]
        public async Task CanCallProduceAsync()
        {
            // Arrange
            var htuRequest = new HollyTopUpRedeemRequest
            {
                VoucherNumber = "10005678912",
                ClientId = 1000791,
                VoucherReference = Guid.NewGuid().ToString(),
            };

            var voucherType = VoucherType.HollyTopUp;

            var outcome = new RedeemOutcome
            {
                VoucherID = 496689284,
                VoucherAmount = 76.96M,
                OutcomeMessage = ""
            };

            _kafkaProducer.Produce(Arg.Any<string>(), Arg.Any<string>()).Returns(new DeliveryResult<Null, string>
            {
                Topic = "1028586546",
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
            await _hollyTopUpKafkaProducer.Produce(htuRequest, voucherType, outcome);

            // Assert
            await _kafkaProducer.Received().Produce(Arg.Any<string>(), Arg.Any<string>());
            await _voucherBatchProcessingRepository.Received().InsertPendingBatchVoucher(Arg.Any<PendingBatchVoucher>());
            await _httpClientCommunication.Received().SendRequestAsync(Arg.Any<string>(), Arg.Any<VPS.Domain.Models.Enums.HttpMethod>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, string>>>(), Arg.Any<CharsetEncoding>(), Arg.Any<TimeSpan?>());
        }

    }
}
