using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using VPS.API.OTT;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.OTT.Requests;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Services.Common;
using VPS.Services.Common.Controllers;
using VPS.Services.OTT;
using VPS.Test.OTTVoucher.Setup;

namespace VPS.Test.OTTVoucher.Services;

public class OTTVoucherRedeemServiceTest : IClassFixture<Fixture>
{
    private readonly OttVoucherRedeemRequest _oTTVoucherRedeemRequest;
    private readonly OttVoucherRedeemService _oTTVoucherRedeemService;
    private readonly ILoggerAdapter<OttVoucherRedeemService> _log = Substitute.For<ILoggerAdapter<OttVoucherRedeemService>>();
    private readonly IVoucherLogRepository _voucherLogRepository = Substitute.For<IVoucherLogRepository>();
    private readonly IOttApiService _ottVoucherService = Substitute.For<IOttApiService>();
    private readonly IVoucherValidationService _voucherValidationService = Substitute.For<IVoucherValidationService>();
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    private readonly ILoggerAdapter<VoucherRedeemService<Domain.Models.OTT.OttVouchers>> _vlog = Substitute.For<ILoggerAdapter<VoucherRedeemService<Domain.Models.OTT.OttVouchers>>>();
    private readonly IOttKafkaProducer _kafkaProducer = Substitute.For<IOttKafkaProducer>();
    private readonly IOptions<RedisSettings> _redisSettings = Substitute.For<IOptions<RedisSettings>>();
    private readonly IRedisService _redisService = Substitute.For<IRedisService>();
    private readonly MetricsHelper _metricsHelper = Substitute.For<MetricsHelper>();
    private readonly IOptions<DBSettings> _dbSettings;
    private readonly IVoucherProviderService _voucherProviderService = Substitute.For<IVoucherProviderService>();

    public OTTVoucherRedeemServiceTest(Fixture fixtures)
    {
        _dbSettings = Options.Create(fixtures.DBSettings);

        _oTTVoucherRedeemRequest = Fixture.CreateVoucherRedeemRequest();

        _oTTVoucherRedeemService = new OttVoucherRedeemService(
            _log,
            _voucherValidationService,
            _ottVoucherService,
            _voucherLogRepository,
            _httpContextAccessor,
            _kafkaProducer,
            _redisService,
            _redisSettings,
            _vlog,
            _metricsHelper,
            _dbSettings,
            _voucherProviderService);
    }

    [Fact]
    public async Task GetRedeemOutCome_Sucess()
    {
        //Arrange
        var ottVoucherRedeemResponse = ArrangeCollection.CreateSuccessOTTVoucherResponse();
        var successRedeemOutcome = ArrangeCollection.SucessRedeemOutCome(ottVoucherRedeemResponse.VoucherAmount, ottVoucherRedeemResponse.VoucherID);

        string uniqueReference = Guid.NewGuid().ToString();
        _ottVoucherService.RemitOTTVoucher(uniqueReference, _oTTVoucherRedeemRequest).ReturnsForAnyArgs(ottVoucherRedeemResponse);

        //Act
        var redeemOutCome = await _oTTVoucherRedeemService.GetRedeemOutcome(_oTTVoucherRedeemRequest, VoucherType.OTT);

        //Asert
        Assert.True(RedeemOutcomeObjectsAreEqualByValue(successRedeemOutcome, redeemOutCome));

    }

    [Fact]
    public async Task GetRedeemOutcome_Fail()
    {
        //Arrange
        var ottVoucherRedeemVoucherResponse = ArrangeCollection.CreateErrorOTTVoucherResponse();
        var failRedeemOutcome = ArrangeCollection.FailVoucherRedeemOutCome();

        string uniqueReference = Guid.NewGuid().ToString();
        _ottVoucherService.RemitOTTVoucher(uniqueReference, _oTTVoucherRedeemRequest).ReturnsForAnyArgs(ottVoucherRedeemVoucherResponse);
        //Act
        var redeemOutCome = await _oTTVoucherRedeemService.GetRedeemOutcome(_oTTVoucherRedeemRequest, VoucherType.OTT);
        //Asert
        Assert.True(RedeemOutcomeObjectsAreEqualByValue(failRedeemOutcome, redeemOutCome));
    }

    [Fact]
    public async Task GetRedeemOutcome_Fail_Voucher_Already_Redeemed()
    {
        //Arrange
        var ottVoucherRedeemVoucherResponse = ArrangeCollection.CreateAlreadyRedeemedOTTVoucherResponse();
        var alreadyRedeemedOutcome = ArrangeCollection.AlreadyRedeemedOutCome();

        string uniqueReference = Guid.NewGuid().ToString();
        _ottVoucherService.RemitOTTVoucher(uniqueReference, _oTTVoucherRedeemRequest).ReturnsForAnyArgs(ottVoucherRedeemVoucherResponse);

        //Act
        var redeemOutCome = await _oTTVoucherRedeemService.GetRedeemOutcome(_oTTVoucherRedeemRequest, VoucherType.OTT);

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