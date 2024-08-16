using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using VPS.API.Flash;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Configurations.Flash;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.Flash;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Services.Common;
using VPS.Services.Common.Controllers;
using VPS.Services.Flash;
using VPS.Test.Common.Setup;
using VPS.Tests.Flash.Setup;
using Xunit;

namespace VPS.Tests.Flash.Services;
public class FlashRedeemServiceTests : IClassFixture<Fixtures>
{
    private readonly FlashRedeemService _flashRedeemService;
    private readonly ILoggerAdapter<FlashRedeemService> _log = Substitute.For<ILoggerAdapter<FlashRedeemService>>();
    private readonly IVoucherLogRepository _voucherLogRepository = Substitute.For<IVoucherLogRepository>();
    private readonly IFlashApiService _flashAPIService = Substitute.For<IFlashApiService>();
    private readonly IFlashKafkaProducer _flashKafkaProducer = Substitute.For<IFlashKafkaProducer>();
    private readonly MetricsHelper _metricsHelper = Substitute.For<MetricsHelper>();
    private readonly IOptions<FlashConfiguration> _flashSettings;
    private readonly IOptions<DBSettings> _dbSettings;
    private readonly IVoucherValidationService _voucherValidationService = Substitute.For<IVoucherValidationService>();
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    private readonly ILoggerAdapter<VoucherRedeemService<FlashVoucher>> _vlog = Substitute.For<ILoggerAdapter<VoucherRedeemService<FlashVoucher>>>();
    private readonly IRedisService _redisService = Substitute.For<IRedisService>();
    private readonly IOptions<RedisSettings> _redisSettings = Substitute.For<IOptions<RedisSettings>>();
    private readonly IVoucherProviderService _voucherProviderService = Substitute.For<IVoucherProviderService>();

    public FlashRedeemServiceTests(Fixtures fixtures)
    {
        _dbSettings = Options.Create(fixtures.DBSettings);
        _flashSettings = Options.Create(fixtures.FlashSettings);
        _flashRedeemService = new FlashRedeemService(_log, _voucherValidationService,
            _voucherLogRepository,
            _httpContextAccessor,
            _vlog,
            _flashAPIService,
            _redisService,
            _redisSettings,
            _metricsHelper,
            _flashSettings,
            _flashKafkaProducer,
            _dbSettings,
            _voucherProviderService);
    }

    [Fact]
    public async Task GetRedeemOutcome_Success()
    {
        //Arrange
        var flashRequest = FlashArrangeCollection.CreateFlashRedeemRequest();
        var request = FlashArrangeCollection.CreateVoucherRedeemRequest();

        var flashRedeemVoucherResponse = FlashArrangeCollection.CreateSuccessFlashVoucherResponse(flashRequest.VoucherNumber);
        var successRedeemOutcome = await ArrangeCollection.SuccessRedeemOutCome(flashRedeemVoucherResponse.Amount, 0);

        _flashAPIService.RedeemVoucher(request).ReturnsForAnyArgs(flashRedeemVoucherResponse);

        //Act
        var redeemOutCome = await _flashRedeemService.GetRedeemOutcome(flashRequest, VoucherType.Flash);

        //Asert
        Assert.True(RedeemOutcomeObjectsAreEqualByValue(successRedeemOutcome, redeemOutCome));
    }

    [Fact]
    public async Task GetRedeemOutcome_Fail()
    {
        //Arrange
        var flashRequest = FlashArrangeCollection.CreateFlashRedeemRequest();
        var request = FlashArrangeCollection.CreateVoucherRedeemRequest();

        var flashRedeemVoucherResponse = FlashArrangeCollection.CreateFailFlashVoucherResponse(flashRequest.VoucherNumber);
        var failRedeemOutcome = await ArrangeCollection.FailVoucherRedeemOutCome();

        _flashAPIService.RedeemVoucher(request).ReturnsForAnyArgs(flashRedeemVoucherResponse);

        //Act
        var redeemOutCome = await _flashRedeemService.GetRedeemOutcome(flashRequest, VoucherType.Flash);

        //Asert
        Assert.True(RedeemOutcomeObjectsAreEqualByValue(failRedeemOutcome, redeemOutCome));
    }

    [Fact]
    public async Task GetRedeemOutcome_Fail_Voucher_Already_Redeemed()
    {
        //Arrange
        var flashRedeemVoucherResponse = FlashArrangeCollection.CreateAlreadyRedeemedVoucherResponse();
        var alreadyRedeemedOutcome = FlashArrangeCollection.AlreadyRedeemedOutCome();
        var request = FlashArrangeCollection.CreateVoucherRedeemRequest();
        var flashRequest = FlashArrangeCollection.CreateFlashRedeemRequest();

        _flashAPIService.RedeemVoucher(request).ReturnsForAnyArgs(flashRedeemVoucherResponse);

        //Act
        var redeemOutCome = await _flashRedeemService.GetRedeemOutcome(flashRequest, VoucherType.Flash);

        //Asert
        Assert.True(RedeemOutcomeObjectsAreEqualByValue(alreadyRedeemedOutcome, redeemOutCome));
    }
    private static bool RedeemOutcomeObjectsAreEqualByValue(RedeemOutcome expected, RedeemOutcome actual)
    {
        return expected.OutcomeMessage == actual.OutcomeMessage &&
               expected.OutComeTypeId == actual.OutComeTypeId;
    }
}
