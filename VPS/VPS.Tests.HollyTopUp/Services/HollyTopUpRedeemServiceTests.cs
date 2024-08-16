using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.HollyTopUp;
using VPS.Domain.Models.HollyTopUp.Requests;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Infrastructure.Repository.HollyTopUp;
using VPS.Services.Common;
using VPS.Services.Common.Controllers;
using VPS.Services.HollyTopUp;
using VPS.Test.Common.Setup;
using VPS.Tests.HollyTopUp.Setup;
using Xunit;

namespace VPS.Tests.HollyTopUp.Services;

public class HollyTopUpRedeemServiceTests : IClassFixture<Fixtures>
{
    private readonly IRedisService _redisService = Substitute.For<IRedisService>();
    private readonly IOptions<RedisSettings> _redisSettings;
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    private readonly IHollyTopUpRepository _hollyTopUpRepository = Substitute.For<IHollyTopUpRepository>();
    private readonly IVoucherLogRepository _voucherLogRepository = Substitute.For<IVoucherLogRepository>();
    private readonly ILoggerAdapter<HollyTopUpRedeemService> _log = Substitute.For<ILoggerAdapter<HollyTopUpRedeemService>>();
    private readonly HollyTopUpRedeemService _hollyTopUpRedeemService;
    private readonly IHollyTopUpKafkaProducer _hollyTopUpKafkaProducer = Substitute.For<IHollyTopUpKafkaProducer>();
    private readonly IVoucherValidationService _voucherValidationService = Substitute.For<IVoucherValidationService>();
    private readonly HollyTopUpRedeemRequest _voucherRedeemRequest;
    private readonly ILoggerAdapter<VoucherRedeemService<HollyTopUpVoucher>> _vlog = Substitute.For<ILoggerAdapter<VoucherRedeemService<HollyTopUpVoucher>>>();
    private readonly MetricsHelper _metricsHelper = Substitute.For<MetricsHelper>();
    private readonly IOptions<DBSettings> _dbSettings;
    private readonly IVoucherProviderService _voucherProviderService = Substitute.For<IVoucherProviderService>();

    public HollyTopUpRedeemServiceTests(Fixtures fixtures)
    {
        _voucherRedeemRequest = HollyTopUpArrangeCollection.CreateVoucherRedeemRequest();
        _redisSettings = Options.Create(fixtures.RedisSettings);
        _dbSettings = Options.Create(fixtures.DBSettings);
        _hollyTopUpRedeemService = new HollyTopUpRedeemService(_log,
            _voucherValidationService,
            _voucherLogRepository,
            _httpContextAccessor,
            _hollyTopUpRepository,
            _vlog,
            _redisService,
            _redisSettings,
            _metricsHelper,
            _hollyTopUpKafkaProducer,
            _dbSettings,
            _voucherProviderService);
    }

    [Fact]
    public void Validate_Constructor()
    {
        //Arrange
        //Act
         var hollyTopUpRedeemService = new HollyTopUpRedeemService(_log,
            _voucherValidationService,
            _voucherLogRepository,
            _httpContextAccessor,
            _hollyTopUpRepository,
            _vlog,
            _redisService,
            _redisSettings,
            _metricsHelper,
            _hollyTopUpKafkaProducer,
            _dbSettings,
            _voucherProviderService);

        //Assert
        Assert.NotNull(hollyTopUpRedeemService);
    }

   

    [Fact]
    public async Task GetRedeemOutcome_GivenVoucherIsRedeemed_ShouldReturnASuccessResponse()
    {
        //Arrange
        var hollyTopUpVoucherResponse = HollyTopUpArrangeCollection.CreateSuccessHollyTopUpVoucherResponse();
        var successRedeemOutcome = HollyTopUpArrangeCollection.SuccessRedeemOutCome(hollyTopUpVoucherResponse.VoucherAmount, hollyTopUpVoucherResponse.VoucherID);

        _hollyTopUpRepository.RedeemHollyTopUpVoucher(_voucherRedeemRequest.VoucherNumber, _voucherRedeemRequest.ClientId).ReturnsForAnyArgs(hollyTopUpVoucherResponse);

        //Act
        var redeemOutCome = await _hollyTopUpRedeemService.GetRedeemOutcome(_voucherRedeemRequest, VoucherType.HollyTopUp);

        //Assert
        Assert.True(RedeemOutcomeObjectsAreEqualByValue(successRedeemOutcome, redeemOutCome));
    }

    [Fact]
    public async Task GetRedeemOutcome_GivenAVoucherHasAnError_ShouldReturnAFailResponse()
    {

        //Arrange
        var hollyTopUpVoucherResponse = HollyTopUpArrangeCollection.CreateErrorVoucherResponse();
        var failRedeemOutcome = HollyTopUpArrangeCollection.FailVoucherRedeemOutCome(hollyTopUpVoucherResponse.VoucherAmount, hollyTopUpVoucherResponse.VoucherID);

        _hollyTopUpRepository.RedeemHollyTopUpVoucher(_voucherRedeemRequest.VoucherNumber, _voucherRedeemRequest.ClientId).ReturnsForAnyArgs(hollyTopUpVoucherResponse);

        //Act
        var redeemOutCome = await _hollyTopUpRedeemService.GetRedeemOutcome(_voucherRedeemRequest, VoucherType.HollyTopUp);

        //Assert
        Assert.True(RedeemOutcomeObjectsAreEqualByValue(failRedeemOutcome, redeemOutCome));
    }

    [Fact]
    public async Task GetRedeemOutcome_GivenVoucherAlreadyRedeemed_ShouldReturnFailedResponse()
    {
        //Arrange
        var hollyTopUpVoucherResponse = HollyTopUpArrangeCollection.CreateAlreadyRedeemedHollyTopUpVoucherResponse();
        var alreadyRedeemedOutcome = HollyTopUpArrangeCollection.AlreadyRedeemedOutCome();

        _hollyTopUpRepository.RedeemHollyTopUpVoucher(_voucherRedeemRequest.VoucherNumber, _voucherRedeemRequest.ClientId).ReturnsForAnyArgs(hollyTopUpVoucherResponse);

        //Act
        var redeemOutCome = await _hollyTopUpRedeemService.GetRedeemOutcome(_voucherRedeemRequest, VoucherType.EasyLoad);

        //Assert
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
