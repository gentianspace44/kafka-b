using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Net;
using VPS.API.Common;
using VPS.API.Flash;
using VPS.Domain.Models.Configurations.Flash;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.Flash;
using VPS.Domain.Models.Flash.Requests;
using VPS.Domain.Models.Flash.Responses;
using VPS.Helpers;
using VPS.Helpers.Logging;
using Xunit;

namespace VPS.Tests.Flash.API;
public class FlashAPIServiceTests
{
    [Fact]
    public void Construct_GivenILoggerAdapterIsNull_ShouldThrowException()
    {
        //Arrange
        //Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            return new FlashApiService(
                        null!,
                        Substitute.For<IOptions<FlashConfiguration>>(),
                        Substitute.For<IHttpClientCommunication>(),
                        Substitute.For<MetricsHelper>(),
                        Substitute.For<IFlashApiAuthenticationService>(),
                        Substitute.For<IRedisServiceBridge>());
        });
        
        //Assert
        Assert.Equal("log", exception.ParamName);
    }

    [Fact]
    public void Construct_GivenFlashConfigurationIsNull_ShouldThrowException()
    {
        //Arrange
        //Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            return new FlashApiService(
                        Substitute.For<ILoggerAdapter<FlashApiService>>(),
                        null,
                        Substitute.For<IHttpClientCommunication>(),
                        Substitute.For<MetricsHelper>(),
                        Substitute.For<IFlashApiAuthenticationService>(),
                        Substitute.For<IRedisServiceBridge>());
        });

        //Assert
        Assert.Equal("flashSettings", exception.ParamName);
    }

    [Fact]
    public void Construct_GivenIHttpClientCommunicationIsNull_ShouldThrowException()
    {
        //Arrange
        IOptions<FlashConfiguration> flashConfig = GetFlashConfig();
        //Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            return new FlashApiService(
                        Substitute.For<ILoggerAdapter<FlashApiService>>(),
                        flashConfig,
                        null,
                        Substitute.For<MetricsHelper>(),
                        Substitute.For<IFlashApiAuthenticationService>(),
                        Substitute.For<IRedisServiceBridge>());
        });

        //Assert
        Assert.Equal("httpClientCommunication", exception.ParamName);
    }

    [Fact]
    public void Construct_GivenMetricsHelperIsNull_ShouldThrowException()
    {
        //Arrange
        IOptions<FlashConfiguration> flashConfig = GetFlashConfig();
        //Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            return new FlashApiService(
                        Substitute.For<ILoggerAdapter<FlashApiService>>(),
                        flashConfig,
                        Substitute.For<IHttpClientCommunication>(),
                        null,
                        Substitute.For<IFlashApiAuthenticationService>(),
                        Substitute.For<IRedisServiceBridge>());
        });

        //Assert
        Assert.Equal("metricsHelper", exception.ParamName);
    }

    [Fact]
    public void Construct_GivenIFlashAPIAuthenticationServiceIsNull_ShouldThrowException()
    {
        //Arrange
        IOptions<FlashConfiguration> flashConfig = GetFlashConfig();
        //Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            return new FlashApiService(
                        Substitute.For<ILoggerAdapter<FlashApiService>>(),
                        flashConfig,
                        Substitute.For<IHttpClientCommunication>(),
                        Substitute.For<MetricsHelper>(),
                        null,
                        Substitute.For<IRedisServiceBridge>());
        });

        //Assert
        Assert.Equal("flashAPIAuthenticationService", exception.ParamName);
    }

    [Fact]
    public async Task RedeemVoucher_GivenIsFlashAccessTokenValidIsNotValid_ShouldFailToRedeem()
    {
        //Arrange
        IOptions<FlashConfiguration> flashConfig = GetFlashConfig();

        var flashAPIAuthenticationService = Substitute.For<IFlashApiAuthenticationService>();
        var redisServiceBridge = Substitute.For<IRedisServiceBridge>();

        flashAPIAuthenticationService
            .GetFlashApiToken("123")
            .ReturnsForAnyArgs(new FlashAccessToken());

        flashAPIAuthenticationService
            .IsFlashAccessTokenValid("123", "123")
            .ReturnsForAnyArgs(false);
        redisServiceBridge
            .GetCachedFlashToken("123")
            .ReturnsForAnyArgs(new FlashAccessTokenCache() { IsTokenValid = false });

        var service = new FlashApiService(
                        Substitute.For<ILoggerAdapter<FlashApiService>>(),
                        flashConfig,
                        Substitute.For<IHttpClientCommunication>(),
                        Substitute.For<MetricsHelper>(),
                        flashAPIAuthenticationService,
                        redisServiceBridge);

        var request = new FlashRequest
        {
            pin = "123",
            metadata = new Metadata { clientId =  "123" }
        };
        //Act
        var sut = await service.RedeemVoucher(request);

        //Assert
        Assert.Equal(sut.ResponseMessage, $"Failed to get flash token for voucher {request.pin}");
    }

    [Fact]
    public async Task RedeemVoucher_GivenIsFlashAccessTokenValidIsValid_ShouldRedeemVoucher()
    {
        //Arrange
        var content = new FlashRedeemResponse
        {
            Amount = 10,
            AccountNumber = "123",
            ResponseMessage = "success"
        };

        HttpResponseMessage mockResponse = new(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(content))
        };

        IOptions<FlashConfiguration> flashConfig = GetFlashConfig();
        var timeout = new TimeSpan(0, 0, 30);
        var flashAPIAuthenticationService = Substitute.For<IFlashApiAuthenticationService>();

        flashAPIAuthenticationService
            .GetFlashApiToken("123")
            .ReturnsForAnyArgs(new FlashAccessToken
            {
                Access_Token = "token"
            });

        flashAPIAuthenticationService
            .IsFlashAccessTokenValid("123", "123")
            .ReturnsForAnyArgs(true);

        var httpClientCommunication = Substitute.For<IHttpClientCommunication>();

        httpClientCommunication.SendRequestAsync(Arg.Any<string>(), Domain.Models.Enums.HttpMethod.POST, default, "application/json", default, default, timeout)
            .ReturnsForAnyArgs(mockResponse);

        var redisServiceBridge = Substitute.For<IRedisServiceBridge>();

        redisServiceBridge
            .GetCachedFlashToken("123")
            .ReturnsForAnyArgs(new FlashAccessTokenCache() { IsTokenValid = true, AccessToken = "123" });

        var service = new FlashApiService(
                        Substitute.For<ILoggerAdapter<FlashApiService>>(),
                        flashConfig,
                        httpClientCommunication,
                        Substitute.For<MetricsHelper>(),
                        flashAPIAuthenticationService,
                        redisServiceBridge);

        var request = new FlashRequest
        {
            pin = "123",
            metadata = new Metadata { clientId = "123" }
        };
        //Act
        var sut = await service.RedeemVoucher(request);

        //Assert
        Assert.True(ObjectsAreEqualByValue(sut, content));
    }

    private static IOptions<FlashConfiguration> GetFlashConfig()
    {
        var config = Substitute.For<IOptions<FlashConfiguration>>();
        config.Value.Returns(new FlashConfiguration
        {
            FlashAccountNumber = "1",
            FlashEndpoint = "http://localhost/flash",
            FlashConsumerKey = "123",
            FlashConsumerSecret = "123",
            MaxPolyRetry = "1",
            AccessTokenCacheLifespanSeconds = 10,
            FlashAccessTokenCacheKey = "123",
            FlashAPITimeoutSeconds = 10,
            IdempotencyLifespanSeconds = 10,
        });
        return config;
    }

    private static bool ObjectsAreEqualByValue(FlashRedeemResponse expected, FlashRedeemResponse actual)
    {
        return expected.ResponseMessage == actual.ResponseMessage &&
               expected.AccountNumber == actual.AccountNumber &&
               expected.Amount == actual.Amount &&
               expected.Reference == actual.Reference &&
               expected.Voucher == actual.Voucher &&
               expected.ResponseCode == actual.ResponseCode &&
               expected.StoreId == actual.StoreId &&
               expected.TerminalId == actual.TerminalId;
    }
}
