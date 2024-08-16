using Microsoft.Extensions.Options;
using NSubstitute;
using VPS.API.Common;
using VPS.API.Syx;
using VPS.Domain.Models.Configurations;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Test.Common.Setup;

namespace VPS.Test.Common.Services;

public class SyxAPIServiceTests : IClassFixture<Fixtures>
{
    private readonly IOptions<SyxSettings> _syxSettings;
    private readonly ILoggerAdapter<SyxApiService> _log = Substitute.For<ILoggerAdapter<SyxApiService>>();
    private readonly IHttpClientCommunication _httpClientCommunication = Substitute.For<IHttpClientCommunication>();
    private readonly SyxApiService _syxService;
    private readonly MetricsHelper _metricsHelper = Substitute.For<MetricsHelper>();
    private readonly IOptions<VpsControlCenterEndpoints> _vpsControlCenterEndpoints;

    public SyxAPIServiceTests(Fixtures fixtures)
    {
        _syxSettings = Options.Create(fixtures.SyxSettings);
        _vpsControlCenterEndpoints = Options.Create(fixtures.VPSControlCenterEndpoints);
        _syxService = new SyxApiService(_syxSettings, _log, _httpClientCommunication, _metricsHelper, _vpsControlCenterEndpoints);
    }

    [Fact]
    public async Task UpdateClientBalance_Success()
    {
        //Arrange
        var clientBalanceUpdateRequest = ArrangeCollection.CreateUpdateClientBalanceRequest();
        var sucessClientBalanceResponse = ArrangeCollection.SuccessApiClientBalanceUpdateResponse();
        var expectedRestResponse = ArrangeCollection.CreateSuccessApiClientBalanceUpdateResponse(sucessClientBalanceResponse);
        _httpClientCommunication.SendRequestAsync(Arg.Any<string>(), default, default, Arg.Any<string>(), default, default).ReturnsForAnyArgs(expectedRestResponse);

        //Act
        var result = await _syxService.UpdateClientBalance(clientBalanceUpdateRequest.ClientId, clientBalanceUpdateRequest.TransactionTypeId, clientBalanceUpdateRequest.TransactionAmount, clientBalanceUpdateRequest.BranchId, clientBalanceUpdateRequest.ReferenceComments);

        //Assert
        Assert.True(result?.ResponseType == 1);
    }

    [Fact]
    public async Task UpdateClientBalance_Fail()
    {
        //Arrange
        var clientBalanceUpdateRequest = ArrangeCollection.CreateUpdateClientBalanceRequest();
        var failClientBalanceResponse = ArrangeCollection.FailApiClientBalanceUpdateResponse();
        var expectedRestResponse = ArrangeCollection.CreateFailApiClientBalanceUpdateResponse(failClientBalanceResponse);
        var syxToken = ArrangeCollection.GetSyxToken();

        _httpClientCommunication.SendRequestAsync(Arg.Any<string>(), default, default, Arg.Any<string>(), default, default).ReturnsForAnyArgs(expectedRestResponse);

        _httpClientCommunication
            .SendRequestAsync("https://vps-control-center-api-uat.betsolutions.net/getSyxToken", Domain.Models.Enums.HttpMethod.GET)
            .Returns(syxToken);

        //Act
        var result = await _syxService.UpdateClientBalance(clientBalanceUpdateRequest.ClientId, clientBalanceUpdateRequest.TransactionTypeId, clientBalanceUpdateRequest.TransactionAmount, clientBalanceUpdateRequest.BranchId, clientBalanceUpdateRequest.ReferenceComments);

        //Assert
        Assert.True(result?.ResponseType == -1);
    }

    [Fact]
    public async Task CheckVoucherExists_Success()
    {
        //Arrange
        var request = ArrangeCollection.CreateCheckVoucherExistsRequest();
        var sucessApiVoucherExistsResponse = ArrangeCollection.SuccessApiVoucherExistsResponse();
        var expectedRestResponse = ArrangeCollection.CreateSuccessApiVoucherExistsResponse(sucessApiVoucherExistsResponse);
        _httpClientCommunication.SendRequestAsync(Arg.Any<string>(), default, default, Arg.Any<string>(), default, default).ReturnsForAnyArgs(expectedRestResponse);

        //Act
        var result = await _syxService.CheckVoucherExists(request.ClientId, request.Reference);

        //Assert
        Assert.True(result?.ResponseType == 1);
    }

    [Fact]
    public async Task CheckVoucherExists_Fail()
    {
        //Arrange
        var request = ArrangeCollection.CreateCheckVoucherExistsRequest();
        var failApiVoucherExistsResponse = ArrangeCollection.FailApiVoucherExistsResponse();
        var syxToken = ArrangeCollection.GetSyxToken();
        var expectedRestResponse = ArrangeCollection.CreateFailApiVoucherExistsResponse(failApiVoucherExistsResponse);
        _httpClientCommunication.SendRequestAsync(Arg.Any<string>(), default, default, Arg.Any<string>(), default, default).ReturnsForAnyArgs(expectedRestResponse);

        _httpClientCommunication
                .SendRequestAsync("https://vps-control-center-api-uat.betsolutions.net/getSyxToken", Domain.Models.Enums.HttpMethod.GET)
                .Returns(syxToken);

        //Act
        var result = await _syxService.CheckVoucherExists(request.ClientId, request.Reference);

        //Assert
        Assert.True(result?.ResponseType == -1);
    }
}
