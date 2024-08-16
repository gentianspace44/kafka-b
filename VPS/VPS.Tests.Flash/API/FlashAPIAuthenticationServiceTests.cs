using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSubstitute;
using System.Net;
using VPS.API.Common;
using VPS.API.Flash;
using VPS.Domain.Models.Configurations.Flash;
using VPS.Domain.Models.Flash;
using VPS.Helpers;
using VPS.Helpers.Logging;
using Xunit;

namespace VPS.Tests.Flash.API;

public class FlashAPIAuthenticationServiceTests
{
    [Fact]
    public void Construct_GivenILoggerAdapterIsNull_ShouldThrowException()
    {
        //Arrange
        var metricsHelper = new MetricsHelper();

        //Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            return new FlashApiAuthenticationService(
                                                null!,
                                                Substitute.For<IOptions<FlashConfiguration>>(),
                                                Substitute.For<IHttpClientCommunication>(),
                                                metricsHelper,
                                                Substitute.For<IRedisServiceBridge>());
        });

        //Assert
        Assert.Equal("log", exception.ParamName);
    }

    [Fact]
    public void Construct_GivenFlashConfigurationIsNull_ShouldThrowException()
    {
        //Arrange
        var metricsHelper = new MetricsHelper();
        //Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            return new FlashApiAuthenticationService(
                                                Substitute.For<ILoggerAdapter<FlashApiAuthenticationService>>(),
                                                null,
                                                Substitute.For<IHttpClientCommunication>(),
                                                metricsHelper,
                                                Substitute.For<IRedisServiceBridge>());
        });

        //Assert
        Assert.Equal("flashSettings", exception.ParamName);
    }

    [Fact]
    public void Construct_GivenIHttpClientCommunicationIsNull_ShouldThrowException()
    {
        //Arrange
        var metricsHelper = new MetricsHelper();
        //Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            return new FlashApiAuthenticationService(
                                                Substitute.For<ILoggerAdapter<FlashApiAuthenticationService>>(),
                                                Substitute.For<IOptions<FlashConfiguration>>(),
                                                null,
                                                metricsHelper,
                                                Substitute.For<IRedisServiceBridge>());
        });

        //Assert
        Assert.Equal("flashSettings", exception.ParamName);
    }

    [Fact]
    public void IsFlashAccessTokenValid_GivenFlashAccessTokenIsNull_ShouldReturnFalse()
    {
        //Arrange
        var metricsHelper = new MetricsHelper();
        IOptions<FlashConfiguration> config = GetFlashConfig();
        var flashAPIAuthenticationService = new FlashApiAuthenticationService(
                                                Substitute.For<ILoggerAdapter<FlashApiAuthenticationService>>(),
                                                config,
                                                Substitute.For<IHttpClientCommunication>(),
                                                metricsHelper,
                                                Substitute.For<IRedisServiceBridge>());
        //Act
        var sut = flashAPIAuthenticationService.IsFlashAccessTokenValid("", "123");
        //Assert
        Assert.False(sut);
    }

    [Fact]
    public void IsFlashAccessTokenValid_GivenFlashAccessToken_ShouldReturnTrue()
    {
        //Arrange
        var metricsHelper = new MetricsHelper();
        IOptions<FlashConfiguration> config = GetFlashConfig();
        var flashAPIAuthenticationService = new FlashApiAuthenticationService(
                                                Substitute.For<ILoggerAdapter<FlashApiAuthenticationService>>(),
                                                config,
                                                Substitute.For<IHttpClientCommunication>(),
                                                metricsHelper,
                                                Substitute.For<IRedisServiceBridge>());
        //Act
        var sut = flashAPIAuthenticationService.IsFlashAccessTokenValid("123", "123");
        //Assert
        Assert.True(sut);
    }

    [Fact]
    public async Task GetFlashApiToken_GivenHttpStatusCodeIsOk_ShouldReturnFlashAccessToken()
    {
        //Arrange
        var content = new FlashAccessToken
        {
            Access_Token = "123",
            Token_Type = "token"
        };

        var redisMockResponse = new FlashAccessTokenCache() { IsTokenValid = false };

        var metricsHelper = new MetricsHelper();
        HttpResponseMessage mockResponse = new(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(content))
        };
        var requestContent = "grant_type=client_credentials";
        var httpClientCommunication = Substitute.For<IHttpClientCommunication>();
        var redisServiceBridge = Substitute.For<IRedisServiceBridge>();
        IOptions<FlashConfiguration> config = GetFlashConfig();
        var flashAPIAuthenticationService = new FlashApiAuthenticationService(
                                                Substitute.For<ILoggerAdapter<FlashApiAuthenticationService>>(),
                                                config,
                                                httpClientCommunication,
                                                metricsHelper,
                                                redisServiceBridge);

        httpClientCommunication
             .SendRequestAsync("http://localhost/flash/token", Domain.Models.Enums.HttpMethod.POST, requestContent, "application /x-www-form-urlencoded", default)
             .ReturnsForAnyArgs(mockResponse);

        redisServiceBridge
            .GetCachedFlashToken("token")
            .ReturnsForAnyArgs(redisMockResponse);
        //Act
        var sut = await flashAPIAuthenticationService.GetFlashApiToken("12343548");
        //Assert
        Assert.True(ObjectsAreEqualByValue(sut, content));
    }

    [Fact]
    public async Task GetFlashApiToken_GivenHttpStatusCodeIsNotOk_ShouldNotReturnFlashAccessToken()
    {
        //Arrange
        HttpResponseMessage mockResponse = new(HttpStatusCode.Ambiguous);
        var redisMockResponse = new FlashAccessTokenCache() { IsTokenValid = false };

        var timeout = new TimeSpan(0, 0, 30);
        var requestContent = "grant_type=client_credentials";
        var httpClientCommunication = Substitute.For<IHttpClientCommunication>();
        var metricsHelper = new MetricsHelper();
        var redisServiceBridge = Substitute.For<IRedisServiceBridge>();
        IOptions<FlashConfiguration> config = GetFlashConfig();
        var flashAPIAuthenticationService = new FlashApiAuthenticationService(
                                                Substitute.For<ILoggerAdapter<FlashApiAuthenticationService>>(),
                                                config,
                                                httpClientCommunication,
                                                metricsHelper,
                                                redisServiceBridge);

        httpClientCommunication
             .SendRequestAsync("http://localhost/flash/token", Domain.Models.Enums.HttpMethod.POST, requestContent, "application /x-www-form-urlencoded", default, default, timeout)
             .ReturnsForAnyArgs(mockResponse);

        redisServiceBridge
            .GetCachedFlashToken("token")
            .ReturnsForAnyArgs(redisMockResponse);
        //Act
        var sut = await flashAPIAuthenticationService.GetFlashApiToken("12343548");
        //Assert
        Assert.True(ObjectsAreEqualByValue(sut, new FlashAccessToken()));
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
            FlashAPITimeoutSeconds = 30,
            MaxPolyRetry = "3",
            FlashAccessTokenCacheKey = "Token",
            AccessTokenCacheLifespanSeconds = 100
        });
        return config;
    }
    private static bool ObjectsAreEqualByValue(FlashAccessToken? expected, FlashAccessToken actual)
    {
        return expected?.Access_Token == actual.Access_Token &&
               expected.Token_Type == actual.Token_Type &&
               expected.Expires_In == actual.Expires_In &&
               expected.Sscope == actual.Sscope;
    }
}
