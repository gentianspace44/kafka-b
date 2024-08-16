using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using VPS.API.EasyLoad;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.EasyLoad.Request;
using VPS.Domain.Models.Enums;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Services.Common;
using VPS.Services.Common.Controllers;
using VPS.Services.EasyLoad;
using VPS.Test.EasyLoad.Setup;

namespace VPS.Test.EasyLoad.Services;

public class EasyLoadVoucherRedeemServiceTest : IClassFixture<Fixtures>
{
    private readonly EasyLoadVoucherRedeemRequest _easyLoadVoucherRedeemRequest;
    private readonly EasyLoadVoucherRedeemService _easyLoadVoucherRedeemService;
    private readonly ILoggerAdapter<EasyLoadVoucherRedeemService> _log = Substitute.For<ILoggerAdapter<EasyLoadVoucherRedeemService>>();
    private readonly IVoucherLogRepository _voucherLogRepository = Substitute.For<IVoucherLogRepository>();
    private readonly IEasyLoadApiService _easyLoadService = Substitute.For<IEasyLoadApiService>();
    private readonly IVoucherValidationService _voucherValidationService = Substitute.For<IVoucherValidationService>();
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    private readonly ILoggerAdapter<VoucherRedeemService<Domain.Models.EasyLoad.EasyLoadVoucher>> _vlog = Substitute.For<ILoggerAdapter<VoucherRedeemService<Domain.Models.EasyLoad.EasyLoadVoucher>>>();
    private readonly IEasyLoadKafkaProducer _easyLoadKafkaProducer = Substitute.For<IEasyLoadKafkaProducer>();
    private readonly IOptions<RedisSettings> _redisSettings = Substitute.For<IOptions<RedisSettings>>();
    private readonly IRedisService _redisService = Substitute.For<IRedisService>();
    private readonly MetricsHelper _metricsHelper = Substitute.For<MetricsHelper>();
    private readonly IOptions<DBSettings> _dbSettings;
    private readonly IVoucherProviderService _voucherProviderService = Substitute.For<IVoucherProviderService>();

    public EasyLoadVoucherRedeemServiceTest(Fixtures fixtures)
    {
        _dbSettings = Options.Create(fixtures.DBSettings);
        _easyLoadVoucherRedeemRequest = Fixtures.CreateVoucherRedeemRequest();
        _easyLoadVoucherRedeemService = new EasyLoadVoucherRedeemService(_log, _voucherValidationService, _voucherLogRepository, _httpContextAccessor, _vlog, _easyLoadService, _redisService, _redisSettings, _metricsHelper, _dbSettings, _easyLoadKafkaProducer, _voucherProviderService);
    }

    [Fact]
    public async Task GetRedeemOutcome_Success()
    {
        //Arrange
        var easyLoadRedeemVoucherResponse = ArrangeCollection.CreateSuccessEasyLoadVoucherResponse(_easyLoadVoucherRedeemRequest.VoucherNumber);
        var successRedeemOutcome = ArrangeCollection.SuccessRedeemOutCome(easyLoadRedeemVoucherResponse.Amount, easyLoadRedeemVoucherResponse.VoucherId);

        _easyLoadService.RedeemVoucher(_easyLoadVoucherRedeemRequest.VoucherNumber, _easyLoadVoucherRedeemRequest.ClientId).ReturnsForAnyArgs(easyLoadRedeemVoucherResponse);

        //Act
        var redeemOutCome = await _easyLoadVoucherRedeemService.GetRedeemOutcome(_easyLoadVoucherRedeemRequest, VoucherType.EasyLoad);

        //Asert
        Assert.True(RedeemOutcomeObjectsAreEqualByValue(successRedeemOutcome, redeemOutCome));
    }

    [Fact]
    public async Task GetRedeemOutcome_Fail()
    {
        //Arrange
        var easyLoadRedeemVoucherResponse = ArrangeCollection.CreateErrorEasyLoadVoucherResponse(_easyLoadVoucherRedeemRequest.VoucherNumber);
        var failRedeemOutcome = ArrangeCollection.FailVoucherRedeemOutCome();

        _easyLoadService.RedeemVoucher(_easyLoadVoucherRedeemRequest.VoucherNumber, _easyLoadVoucherRedeemRequest.ClientId).ReturnsForAnyArgs(easyLoadRedeemVoucherResponse);

        //Act
        var redeemOutCome = await _easyLoadVoucherRedeemService.GetRedeemOutcome(_easyLoadVoucherRedeemRequest, VoucherType.EasyLoad);

        //Asert
        Assert.True(RedeemOutcomeObjectsAreEqualByValue(failRedeemOutcome, redeemOutCome));
    }

    [Fact]
    public async Task GetRedeemOutcome_Fail_Voucher_Already_Redeemed()
    {
        //Arrange
        var easyLoadRedeemVoucherResponse = ArrangeCollection.CreateAlreadyRedeemedEasyLoadVoucherResponse();
        var alreadyRedeemedOutcome = ArrangeCollection.AlreadyRedeemedOutCome();

        _easyLoadService.RedeemVoucher(_easyLoadVoucherRedeemRequest.VoucherNumber, _easyLoadVoucherRedeemRequest.ClientId).ReturnsForAnyArgs(easyLoadRedeemVoucherResponse);

        //Act
        var redeemOutCome = await _easyLoadVoucherRedeemService.GetRedeemOutcome(_easyLoadVoucherRedeemRequest, VoucherType.EasyLoad);

        //Asert
        Assert.True(RedeemOutcomeObjectsAreEqualByValue(alreadyRedeemedOutcome, redeemOutCome));
    }

    #region Private Utils
    private static bool RedeemOutcomeObjectsAreEqualByValue(RedeemOutcome expected, RedeemOutcome actual)
    {
        return expected.OutcomeMessage == actual.OutcomeMessage &&
               expected.OutComeTypeId == actual.OutComeTypeId &&
               expected.VoucherAmount == actual.VoucherAmount &&
               expected.VoucherID == actual.VoucherID;
    }
    #endregion
}
