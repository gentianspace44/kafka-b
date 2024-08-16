using Microsoft.Extensions.Options;
using NSubstitute;
using VPS.API.Common;
using VPS.API.RACellularVoucher;
using VPS.Domain.Models.Configurations.RACellularVoucher;
using VPS.Helpers;
using VPS.Helpers.Logging;
using Xunit;

namespace VPS.Tests.RACellularVoucher.API;

public class RACellularVoucherAPIServiceTests
{

    [Fact]
    public void Construct_GivenILoggerAdapterIsNull_ShouldThrowException()
    {
        //Arrange
        //Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            return new RaCellularVoucherApiService(
                                                null!,
                                                Substitute.For<IHttpClientCommunication>(),
                                                Substitute.For<IOptions<RACellularVoucherConfiguration>>(),
                                                Substitute.For<MetricsHelper>());
        });

        //Assert
        Assert.Equal("log", exception.ParamName);
    }

    [Fact]
    public void Construct_GivenIHttpClientCommunicationIsNull_ShouldThrowException()
    {
        //Arrange
        //Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            return new RaCellularVoucherApiService(Substitute.For<ILoggerAdapter<RaCellularVoucherApiService>>(),
                                                null!,
                                                Substitute.For<IOptions<RACellularVoucherConfiguration>>(),
                                                Substitute.For<MetricsHelper>());
        });

        //Assert
        Assert.Equal("httpClientCommunication", exception.ParamName);
    }

    [Fact]
    public void Construct_GivenRACellularVoucherConfigurationIsNull_ShouldThrowException()
    {
        //Arrange
        //Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            return new RaCellularVoucherApiService(Substitute.For<ILoggerAdapter<RaCellularVoucherApiService>>(),
                                                Substitute.For<IHttpClientCommunication>(),
                                                null!,
                                                Substitute.For<MetricsHelper>());
        });

        //Assert
        Assert.Equal("raCellularVoucherConfiguration", exception.ParamName);
    }

    [Fact]
    public async Task RedeemVoucherAsync_GivenInvalidClientId_ShouldThrowException()
    {
        //Arrange
        var clientId = "";
        var voucherPin = Guid.NewGuid().ToString();
        var platform = "Mob";

        var config = Substitute.For<IOptions<RACellularVoucherConfiguration>>();

        SetUpConfig(config);

        var sut = CreateRACellularVoucherService(config);

        //Act
        var results = await sut.RedeemVoucherAsync(clientId, voucherPin, platform);

        //Assert
        Assert.Equal("R&A Redeem Request Failed.", results.FaultMsg);
    }

    [Fact]
    public async Task RedeemVoucherAsync_GivenInvalidVoucherPin_ShouldThrowException()
    {
        //Arrange
        var clientId = Guid.NewGuid().ToString();
        var voucherPin = "";
        var platform = "Mob";

        var config = Substitute.For<IOptions<RACellularVoucherConfiguration>>();

        SetUpConfig(config);

        var sut = CreateRACellularVoucherService(config);

        //Act
        var results = await sut.RedeemVoucherAsync(clientId, voucherPin, platform);

        //Assert
        Assert.Equal("R&A Redeem Request Failed.", results.FaultMsg);
    }

    [Fact]
    public async Task RedeemVoucherAsync_GivenValidInputs_ShouldCallSendRequestAsync()
    {
        //Arrange
        var voucherPin = "12345678965";
        var platform = "Mob";
        var contentType = "application/json";
        var clientId = Guid.NewGuid().ToString();

        var httpClient = Substitute.For<IHttpClientCommunication>();
        var config = Substitute.For<IOptions<RACellularVoucherConfiguration>>();
        var metricsHelper = Substitute.For<MetricsHelper>();

        SetUpConfig(config);

        var sut = new RaCellularVoucherApiService(Substitute.For<ILoggerAdapter<RaCellularVoucherApiService>>(),
                                               httpClient,
                                               config,
                                               metricsHelper);
        //Act
        _ = await sut.RedeemVoucherAsync(clientId, voucherPin, platform);

        //Assert
        await httpClient.Received(1).SendRequestAsync(
            "http://localhost/RedeemVoucherUrl",
            Domain.Models.Enums.HttpMethod.POST,
            Arg.Any<string>(),
            contentType,
            Arg.Any<List<KeyValuePair<string, string>>>());
    }

    [Fact]
    public async Task AdviceAsync_GivenInvalidClientId_ShouldThrowException()
    {
        //Arrange
        var clientId = "";
        var voucherPin = Guid.NewGuid().ToString();
        var platform = "Mob";

        var config = Substitute.For<IOptions<RACellularVoucherConfiguration>>();

        SetUpConfig(config);

        var sut = CreateRACellularVoucherService(config);

        //Act
        var results = await sut.AdviceAsync(clientId, voucherPin, platform);

        //Assert
        Assert.Equal("R&A Advice Request Failed.", results.FaultMsg);
    }

    [Fact]
    public async Task AdviceAsync_GivenInvalidVoucherPin_ShouldThrowException()
    {
        //Arrange
        var clientId = Guid.NewGuid().ToString();
        var voucherPin = "";
        var platform = "Mob";

        var config = Substitute.For<IOptions<RACellularVoucherConfiguration>>();

        SetUpConfig(config);

        var sut = CreateRACellularVoucherService(config);

        //Act
        var results = await sut.AdviceAsync(clientId, voucherPin, platform);

        //Assert
        Assert.Equal("R&A Advice Request Failed.", results.FaultMsg);
    }

    [Fact]
    public async Task AdviceAsync_GivenValidInputs_ShouldCallSendRequestAsync()
    {
        //Arrange
        var voucherPin = "12345678965";
        var platform = "Mob";
        var contentType = "application/json";
        var clientId = Guid.NewGuid().ToString();

        var httpClient = Substitute.For<IHttpClientCommunication>();
        var config = Substitute.For<IOptions<RACellularVoucherConfiguration>>();
        var metricsHelper = Substitute.For<MetricsHelper>();

        SetUpConfig(config);

        var sut = new RaCellularVoucherApiService(Substitute.For<ILoggerAdapter<RaCellularVoucherApiService>>(),
                                               httpClient,
                                               config,
                                               metricsHelper);
        //Act
        _ = await sut.AdviceAsync(clientId, voucherPin, platform);

        //Assert
        await httpClient.Received(1).SendRequestAsync(
            "http://localhost/AdviceUrl",
            Domain.Models.Enums.HttpMethod.POST,
            Arg.Any<string>(),
            contentType,
            Arg.Any<List<KeyValuePair<string, string>>>());
    }

    [Fact]
    public async Task LookUpVoucherAsync_GivenInvalidClientId_ShouldThrowException()
    {
        //Arrange
        var clientId = "";
        var voucherPin = Guid.NewGuid().ToString();

        var config = Substitute.For<IOptions<RACellularVoucherConfiguration>>();

        SetUpConfig(config);

        var sut = CreateRACellularVoucherService(config);

        //Act
        var results = await sut.LookUpVoucherAsync(clientId, voucherPin);

        //Assert
        Assert.Equal("R&A Lookup Request Failed.", results.FaultMsg);
    }

    [Fact]
    public async Task LookUpVoucherAsync_GivenInvalidVoucherPin_ShouldThrowException()
    {
        //Arrange
        var clientId = Guid.NewGuid().ToString();
        var voucherPin = "";

        var config = Substitute.For<IOptions<RACellularVoucherConfiguration>>();

        SetUpConfig(config);

        var sut = CreateRACellularVoucherService(config);

        //Act
        var results = await sut.LookUpVoucherAsync(clientId, voucherPin);

        //Assert
        Assert.Equal("R&A Lookup Request Failed.", results.FaultMsg);
    }

    [Fact]
    public async Task LookUpVoucherAsync_GivenValidInputs_ShouldCallSendRequestAsync()
    {
        //Arrange
        var voucherPin = "12345678965";
        var contentType = "application/json";
        var clientId = Guid.NewGuid().ToString();

        var httpClient = Substitute.For<IHttpClientCommunication>();
        var config = Substitute.For<IOptions<RACellularVoucherConfiguration>>();
        var metricsHelper = Substitute.For<MetricsHelper>();

        SetUpConfig(config);

        var sut = new RaCellularVoucherApiService(Substitute.For<ILoggerAdapter<RaCellularVoucherApiService>>(),
                                               httpClient,
                                               config,
                                               metricsHelper);
        //Act
        _ = await sut.LookUpVoucherAsync(clientId, voucherPin);

        //Assert
        await httpClient.Received(1).SendRequestAsync(
            "http://localhost/VoucherLookupUrl",
            Domain.Models.Enums.HttpMethod.POST,
            Arg.Any<string>(),
            contentType,
            Arg.Any<List<KeyValuePair<string, string>>>());
    }

    private static void SetUpConfig(IOptions<RACellularVoucherConfiguration> config)
    {
        config.Value.Returns(new RACellularVoucherConfiguration
        {
            BaseUrl = "http://localhost",
            AdviceUrl = "/AdviceUrl",
            RedeemVoucherUrl = "/RedeemVoucherUrl",
            VoucherLookupUrl = "/VoucherLookupUrl",
            MaxPolyRetry = "3"
        });
    }

    private static RaCellularVoucherApiService CreateRACellularVoucherService(IOptions<RACellularVoucherConfiguration> config)
       => new(Substitute.For<ILoggerAdapter<RaCellularVoucherApiService>>(),
                                                Substitute.For<IHttpClientCommunication>(),
                                                config,
                                                Substitute.For<MetricsHelper>());
}
