using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using VPS.Domain.Models.Common.Request;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Services.Common;
using VPS.Services.Common.Controllers;
using VPS.Test.Common.Mocks;
using VPS.Test.Common.Models;
using VPS.Test.Common.Setup;

namespace VPS.Test.Common.Services;

public class VoucherRedeemServiceTests : IClassFixture<Fixtures>
{
    private readonly VoucherRedeemRequestModel _voucherRedeemRequest;
    private readonly IClientBalanceService _clientBalanceBonusUpdate = Substitute.For<IClientBalanceService>();
    private readonly IVoucherValidationService _voucherValidationService = Substitute.For<IVoucherValidationService>();
    private readonly IVoucherLogRepository _voucherLogRepository = Substitute.For<IVoucherLogRepository>();
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    private readonly VoucherRedeemServiceBaseImplementation _voucherRedeemService;
    private readonly ILoggerAdapter<VoucherRedeemService<ProviderVoucher>> vlog = Substitute.For<ILoggerAdapter<VoucherRedeemService<ProviderVoucher>>>();
    private readonly IOptions<RedisSettings> _redisSettings;
    private readonly IRedisService _redisService = Substitute.For<IRedisService>();
    private readonly MetricsHelper _metricsHelper = Substitute.For<MetricsHelper>();
    private readonly IOptions<DBSettings> _dbSettings;
    private readonly IVoucherProviderService _voucherProviderService = Substitute.For<IVoucherProviderService>();

    public VoucherRedeemServiceTests(Fixtures fixtures)
    {
        _dbSettings = Options.Create(fixtures.DBSettings);
        _voucherRedeemRequest = Fixtures.CreateVoucherRedeemRequest();
        _redisSettings = _redisSettings = Options.Create(fixtures.RedisSettings);
        _voucherRedeemService = new VoucherRedeemServiceBaseImplementation(vlog,
            _voucherValidationService,
            _voucherLogRepository,
            _httpContextAccessor,
            _redisService,
            _redisSettings,
            _metricsHelper,
            _dbSettings,
            _voucherProviderService
            );
    }

    [Fact]
    public async Task PerformRedeem_Success()
    {
        //Arrange
        string validationOutcome = string.Empty;
        _voucherValidationService.IsVoucherRequestValid(Arg.Any<VoucherRedeemRequestBase>()).ReturnsForAnyArgs(validationOutcome);

        _voucherValidationService.GetVoucherNumberLength(Arg.Any<VoucherType>()).Returns(14);

        var successSyxCreditOutCome = ArrangeCollection.SuccessSyxCreditOutCome();
        _clientBalanceBonusUpdate.CreditOnSyX(default, Arg.Any<string>(), default, Arg.Any<string>(), default, Arg.Any<string>(), default, Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(successSyxCreditOutCome);

        //Act
        var result = await _voucherRedeemService.ProcessRedeemRequest(_voucherRedeemRequest, VoucherType.EasyLoad);

        //Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task PerformRedeem_Fail_RedeemOutcome_Fail()
    {
        //Arrange
        string validationOutcome = string.Empty;
        _voucherValidationService.IsVoucherRequestValid(Arg.Any<VoucherRedeemRequestBase>()).ReturnsForAnyArgs(validationOutcome);

        var successSyxCreditOutCome = ArrangeCollection.SuccessSyxCreditOutCome();
        _clientBalanceBonusUpdate.CreditOnSyX(default, Arg.Any<string>(), default, Arg.Any<string>(), default, Arg.Any<string>(), default, Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(successSyxCreditOutCome);

        //Act
        var result = await _voucherRedeemService.ProcessRedeemRequest(_voucherRedeemRequest, VoucherType.BluVoucher);

        //Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task PerformRedeem_Fail_Voucher_Already_Redeemed()
    {
        //Arrange
        string validationOutcome = string.Empty;
        _voucherValidationService.IsVoucherRequestValid(Arg.Any<VoucherRedeemRequestBase>()).ReturnsForAnyArgs(validationOutcome);

        //Act
        var result = await _voucherRedeemService.ProcessRedeemRequest(_voucherRedeemRequest, VoucherType.EasyLoad);

        //Assert
        Assert.False(result.IsSuccess);
    }
}
