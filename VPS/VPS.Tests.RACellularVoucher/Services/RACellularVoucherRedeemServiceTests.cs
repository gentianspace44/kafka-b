using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using VPS.API.RACellularVoucher;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.RACellularVoucher.Requests;
using VPS.Domain.Models.RACellularVoucher.Response;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Services.Common;
using VPS.Services.Common.Controllers;
using VPS.Services.RACellularVoucher;
using VPS.Test.Common.Setup;
using VPS.Tests.RACellularVoucher.Setup;
using Xunit;

namespace VPS.Tests.RACellularVoucher.Services;

public class RACellularVoucherRedeemServiceTests : IClassFixture<Fixtures>
{
    private readonly ILoggerAdapter<RaCellularVoucherRedeemService> _log;
    private readonly IVoucherValidationService _voucherValidationService;
    private readonly IraCellularVoucherApiService _raCellularVoucherService;
    private readonly IVoucherLogRepository _voucherLogRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILoggerAdapter<VoucherRedeemService<Domain.Models.RACellularVoucher.RaCellularVoucher>> _vlog;
    private readonly IRaCellularVoucherKafkaProducer _raCellularVoucherKafkaProducer;
    private readonly RaCellularVoucherRedeemService _raCellularVoucherRedeemService;
    private readonly RaCellularVoucherRedeemRequest _voucherRedeemRequest;
    private readonly IOptions<RedisSettings> _redisSettings;
    private readonly IRedisService _redisService = Substitute.For<IRedisService>();
    private readonly MetricsHelper _metricsHelper = Substitute.For<MetricsHelper>();
    private readonly IOptions<DBSettings> _dbSettings;
    private readonly IVoucherProviderService _voucherProviderService = Substitute.For<IVoucherProviderService>();

    public RACellularVoucherRedeemServiceTests(Fixtures fixtures)
    {
        _voucherRedeemRequest = RACellularArrangeCollection.CreateVoucherRedeemRequest();
        _redisSettings = Options.Create(fixtures.RedisSettings);
        _dbSettings = Options.Create(fixtures.DBSettings);
        _log = Substitute.For<ILoggerAdapter<RaCellularVoucherRedeemService>>();
        _voucherValidationService = Substitute.For<IVoucherValidationService>();
        _raCellularVoucherService = Substitute.For<IraCellularVoucherApiService>();
        _voucherLogRepository = Substitute.For<IVoucherLogRepository>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _vlog = Substitute.For<ILoggerAdapter<VoucherRedeemService<Domain.Models.RACellularVoucher.RaCellularVoucher>>>();
        _raCellularVoucherKafkaProducer = Substitute.For<IRaCellularVoucherKafkaProducer>();

        _raCellularVoucherRedeemService = new RaCellularVoucherRedeemService(
                                _log,
                                _voucherValidationService,
                                _raCellularVoucherService,
                                _voucherLogRepository,
                                _httpContextAccessor,
                                _vlog,
                                _redisService,
                                _redisSettings,
                                _raCellularVoucherKafkaProducer,
                                _metricsHelper,
                                _dbSettings,
                                _voucherProviderService);
    }

    [Fact]
    public void Construct_GivenILoggerAdapterIsNull_ShouldThrowException()
    {
        //Arrange
        //Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            return new RaCellularVoucherRedeemService(
                                                null!,
                                                _voucherValidationService,
                                                _raCellularVoucherService,
                                                _voucherLogRepository,
                                                _httpContextAccessor,
                                                _vlog,
                                                _redisService,
                                                _redisSettings,
                                                _raCellularVoucherKafkaProducer,
                                                _metricsHelper,
                                                _dbSettings,
                                                _voucherProviderService);
        });

        //Assert
        Assert.Equal("log", exception.ParamName);
    }

    [Fact]
    public void Construct_GivenVoucherLogRepositoryIsNull_ShouldThrowException()
    {
        //Arrange
        //Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            return new RaCellularVoucherRedeemService(
                                    _log,
                                    null!,
                                    _raCellularVoucherService,
                                    _voucherLogRepository,
                                    _httpContextAccessor,
                                    _vlog,
                                    _redisService,
                                    _redisSettings,
                                    _raCellularVoucherKafkaProducer,
                                    _metricsHelper,
                                    _dbSettings,
                                    _voucherProviderService);
        });

        //Assert
        Assert.Equal("voucherValidationService", exception.ParamName);
    }

    [Fact]
    public void Construct_GivenRACellularVoucherServiceIsNull_ShouldThrowException()
    {
        //Arrange
        //Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            return new RaCellularVoucherRedeemService(
                                    _log,
                                    _voucherValidationService,
                                    null!,
                                    _voucherLogRepository,
                                    _httpContextAccessor,
                                    _vlog,
                                    _redisService,
                                    _redisSettings,
                                    _raCellularVoucherKafkaProducer,
                                    _metricsHelper,
                                    _dbSettings,
                                    _voucherProviderService);
        });

        //Assert
        Assert.Equal("raCellularVoucherService", exception.ParamName);
    }

    [Fact]
    public async Task PerformRedeemAsync_GivenRACellularVoucherRedeemRequest_ShouldCallProcessRedeemRequest()
    {
        //Arrange
        var voucherRedeemRequest = new RaCellularVoucherRedeemRequest();
        var sut = new RaCellularVoucherRedeemService(
                                    _log,
                                    _voucherValidationService,
                                    _raCellularVoucherService,
                                    _voucherLogRepository,
                                    _httpContextAccessor,
                                    _vlog,
                                    _redisService,
                                    _redisSettings,
                                    _raCellularVoucherKafkaProducer,
                                    _metricsHelper,
                                    _dbSettings,
                                    _voucherProviderService);
        //Act
        var result = await sut.PerformRedeemAsync(voucherRedeemRequest);
        //Assert

        Assert.IsType<ServiceResponse>(result);
    }

    [Fact]
    public async Task GetRedeemOutcome_GivenVoucherIsRedeemed_ShouldReturnASuccessResponse()
    {
        //Arrange
        var raCellularVoucherResponse = RACellularArrangeCollection.CreateSuccessVoucherResponse();
        var successRedeemOutcome = RACellularArrangeCollection.SuccessRedeemOutCome(raCellularVoucherResponse.Amount, raCellularVoucherResponse.PinNumber);

        _raCellularVoucherService.RedeemVoucherAsync(_voucherRedeemRequest.ClientId.ToString(), _voucherRedeemRequest.VoucherNumber, _voucherRedeemRequest.DevicePlatform).ReturnsForAnyArgs(raCellularVoucherResponse);

        //Act
        var redeemOutCome = await _raCellularVoucherRedeemService.GetRedeemOutcome(_voucherRedeemRequest, VoucherType.RACellular);

        //Asert
        Assert.True(RedeemOutcomeObjectsAreEqualByValue(successRedeemOutcome, redeemOutCome));
    }

    [Fact]
    public async Task GetRedeemOutcome_GivenAVoucherHasAnError_ShouldReturnAFailResponse()
    {
        //Arrange
        var raCellularVoucherResponse = RACellularArrangeCollection.CreateErrorVoucherResponse();
        var failRedeemOutcome = RACellularArrangeCollection.FailVoucherRedeemOutCome(raCellularVoucherResponse.Amount, raCellularVoucherResponse.PinNumber);

        _raCellularVoucherService
            .RedeemVoucherAsync(_voucherRedeemRequest.ClientId.ToString(), _voucherRedeemRequest.VoucherNumber, _voucherRedeemRequest.DevicePlatform)
            .ReturnsForAnyArgs(raCellularVoucherResponse);

        //Act
        var redeemOutCome = await _raCellularVoucherRedeemService.GetRedeemOutcome(_voucherRedeemRequest, VoucherType.RACellular);

        //Asert
        Assert.True(RedeemOutcomeObjectsAreEqualByValue(failRedeemOutcome, redeemOutCome));
    }

    [Fact]
    public async Task GetRedeemOutcome_GivenVoucherAlreadyRedeemed_ShouldReturnFailedResponse()
    {
        //Arrange
        var raCellularVoucherResponse = RACellularArrangeCollection.CreateAlreadyVoucherResponse();
        var failRedeemOutcome = RACellularArrangeCollection.FailVoucherRedeemOutCome(0, "0");

        _raCellularVoucherService
            .RedeemVoucherAsync(_voucherRedeemRequest.ClientId.ToString(), _voucherRedeemRequest.VoucherNumber, _voucherRedeemRequest.DevicePlatform)
            .ReturnsForAnyArgs(raCellularVoucherResponse);

        //Act
        var redeemOutCome = await _raCellularVoucherRedeemService.GetRedeemOutcome(_voucherRedeemRequest, VoucherType.RACellular);

        //Asert
        Assert.True(RedeemOutcomeObjectsAreEqualByValue(failRedeemOutcome, redeemOutCome));
    }

    [Fact]
    public async Task GetRedeemOutcome_GivenRedeemResultHasFault_ShouldReturnFailedResponse()
    {
        //Arrange
        var raCellularVoucherResponse = new RaCellularVoucherRedeemResponse
        {
            HasFault = true,
            PinNumber = "12345678965",
            FaultNumber = "",
            FaultMsg = "R&A redeem failed, Please try again.",
            Reference = "12345"
        };
        var failRedeemOutcome = RACellularArrangeCollection.FailVoucherRedeemOutCome(0, "0");

        _raCellularVoucherService
            .RedeemVoucherAsync(_voucherRedeemRequest.ClientId.ToString(), _voucherRedeemRequest.VoucherNumber, _voucherRedeemRequest.DevicePlatform)
            .ReturnsForAnyArgs(raCellularVoucherResponse);

        //Act
        var redeemOutCome = await _raCellularVoucherRedeemService.GetRedeemOutcome(_voucherRedeemRequest, VoucherType.RACellular);

        //Asert
        Assert.True(RedeemOutcomeObjectsAreEqualByValue(failRedeemOutcome, redeemOutCome));
        _log.Received().LogInformation(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<object[]>());
    }

    private static bool RedeemOutcomeObjectsAreEqualByValue(RedeemOutcome expected, RedeemOutcome actual)
    {
        return expected.OutcomeMessage == actual.OutcomeMessage &&
               expected.OutComeTypeId == actual.OutComeTypeId &&
               expected.VoucherAmount == actual.VoucherAmount &&
               expected.VoucherID == actual.VoucherID;
    }

}
