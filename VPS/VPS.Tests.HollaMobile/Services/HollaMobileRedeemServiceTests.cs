using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.HollaMobile;
using VPS.Domain.Models.HollaMobile.Requests;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Infrastructure.Repository.HollaMobile;
using VPS.Services.Common;
using VPS.Services.Common.Controllers;
using VPS.Services.HollaMobile;
using VPS.Test.Common.Setup;
using VPS.Tests.HollaMobile.Setup;
using Xunit;

namespace VPS.Tests.HollaMobile.Services
{
    public  class HollaMobileRedeemServiceTests : IClassFixture<Fixtures>
    {
        private readonly IRedisService _redisService = Substitute.For<IRedisService>();
        private readonly IOptions<RedisSettings> _redisSettings;
        private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        private readonly IHollaMobileRepository _hollaMobileRepository = Substitute.For<IHollaMobileRepository>();
        private readonly IVoucherLogRepository _voucherLogRepository = Substitute.For<IVoucherLogRepository>();
        private readonly ILoggerAdapter<HollaMobileRedeemService> _log = Substitute.For<ILoggerAdapter<HollaMobileRedeemService>>();
        private readonly HollaMobileRedeemService _hollaMobileRedeemService;
        private readonly IVoucherValidationService _voucherValidationService = Substitute.For<IVoucherValidationService>();
        private readonly HollaMobileRedeemRequest _voucherRedeemRequest;
        private readonly ILoggerAdapter<VoucherRedeemService<HollaMobileVoucher>> _vlog = Substitute.For<ILoggerAdapter<VoucherRedeemService<HollaMobileVoucher>>>();
        private readonly MetricsHelper _metricsHelper = Substitute.For<MetricsHelper>();
        private readonly IOptions<DBSettings> _dbSettings;
        private readonly IVoucherProviderService _voucherProviderService = Substitute.For<IVoucherProviderService>();

        public HollaMobileRedeemServiceTests(Fixtures fixtures)
        {
            _voucherRedeemRequest = HollaMobileArrangeCollection.CreateVoucherRedeemRequest();
            _redisSettings = Options.Create(fixtures.RedisSettings);
            _dbSettings = Options.Create(fixtures.DBSettings);
            _hollaMobileRedeemService = new HollaMobileRedeemService(_log,
                _voucherValidationService,
                _voucherLogRepository,
                _httpContextAccessor,
                _hollaMobileRepository,
                _vlog,
                _redisService,
                _redisSettings,
                _metricsHelper,
                _dbSettings,
                _voucherProviderService);
        }

        [Fact]
        public async Task GetRedeemOutcome_GivenVoucherIsRedeemed_ShouldReturnASuccessResponse()
        {
            //Arrange
            var hollyTopUpVoucherResponse = HollaMobileArrangeCollection.CreateSuccessHollaMobileVoucherResponse();
            var successRedeemOutcome = HollaMobileArrangeCollection.SuccessRedeemOutCome(hollyTopUpVoucherResponse.VoucherAmount, hollyTopUpVoucherResponse.VoucherID);

            _hollaMobileRepository.RedeemHollaMobileVoucher(_voucherRedeemRequest.VoucherNumber, _voucherRedeemRequest.ClientId).ReturnsForAnyArgs(hollyTopUpVoucherResponse);

            //Act
            var redeemOutCome = await _hollaMobileRedeemService.GetRedeemOutcome(_voucherRedeemRequest, VoucherType.HollaMobile);

            //Asert
            Assert.True(RedeemOutcomeObjectsAreEqualByValue(successRedeemOutcome, redeemOutCome));
        }

        [Fact]
        public async Task GetRedeemOutcome_GivenAVoucherHasAnError_ShouldReturnAFailResponse()
        {
            //Arrange
            var hollaMobileVoucherResponse = HollaMobileArrangeCollection.CreateErrorVoucherResponse();
            var failRedeemOutcome = HollaMobileArrangeCollection.FailVoucherRedeemOutCome(hollaMobileVoucherResponse.VoucherAmount, hollaMobileVoucherResponse.VoucherID);

            _hollaMobileRepository.RedeemHollaMobileVoucher(_voucherRedeemRequest.VoucherNumber, _voucherRedeemRequest.ClientId).ReturnsForAnyArgs(hollaMobileVoucherResponse);

            //Act
            var redeemOutCome = await _hollaMobileRedeemService.GetRedeemOutcome(_voucherRedeemRequest, VoucherType.HollaMobile);

            //Asert
            Assert.False(RedeemOutcomeObjectsAreEqualByValue(failRedeemOutcome, redeemOutCome));
        }

        [Fact]
        public async Task GetRedeemOutcome_GivenVoucherAlreadyRedeemed_ShouldReturnFailedResponse()
        {
            //Arrange
            var hollaMobileVoucherResponse = HollaMobileArrangeCollection.CreateAlreadyRedeemedHollaMobileVoucherResponse();
            var alreadyRedeemedOutcome = HollaMobileArrangeCollection.AlreadyRedeemedOutCome();

            _hollaMobileRepository.RedeemHollaMobileVoucher(_voucherRedeemRequest.VoucherNumber, _voucherRedeemRequest.ClientId).ReturnsForAnyArgs(hollaMobileVoucherResponse);

            //Act
            var redeemOutCome = await _hollaMobileRedeemService.GetRedeemOutcome(_voucherRedeemRequest, VoucherType.HollaMobile);

            //Asert
            Assert.True(RedeemOutcomeObjectsAreEqualByValue(alreadyRedeemedOutcome, redeemOutCome));
        }

        private static bool RedeemOutcomeObjectsAreEqualByValue(RedeemOutcome expected, RedeemOutcome actual)
        {
            return expected.OutcomeMessage == actual.OutcomeMessage &&
                   expected.OutComeTypeId == actual.OutComeTypeId &&
                   expected.VoucherAmount == actual.VoucherAmount &&
                   expected.VoucherID == actual.VoucherID;
        }
    }
}
