using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.Net.Sockets;
using VPS.Domain.Models.BluVoucher.Requests;
using VPS.Domain.Models.BluVoucher.Responses;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Services.BluVoucher;
using VPS.Services.Common;
using VPS.Services.Common.Controllers;
using VPS.Test.BluVoucher.Setup;

namespace VPS.Test.BluVoucher.Services;

public class BluVoucherRedeemServiceTest : IClassFixture<Fixtures>
{

    private readonly ILoggerAdapter<BluVoucherRedeemService> _log = Substitute.For<ILoggerAdapter<BluVoucherRedeemService>>();
    private readonly IVoucherValidationService _voucherValidationService = Substitute.For<IVoucherValidationService>();
    private readonly IVoucherLogRepository _voucherLogRepository = Substitute.For<IVoucherLogRepository>();
    private readonly ITcpClient _tcpClient = Substitute.For<ITcpClient>();
    private readonly IAirtimeAuthentication _airtimeAuthentication = Substitute.For<IAirtimeAuthentication>();
    private readonly IGetStreamResults _getStreamResults = Substitute.For<IGetStreamResults>();
    private readonly BluVoucherRedeemService _bluVoucherRedeemService;
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<HttpContextAccessor>();
    private readonly ILoggerAdapter<VoucherRedeemService<Domain.Models.BluVoucher.BluVoucher>> _vlog = Substitute.For<ILoggerAdapter<VoucherRedeemService<Domain.Models.BluVoucher.BluVoucher>>>();
    private readonly IRemitBluVoucherService _remitBluVoucherService = Substitute.For<IRemitBluVoucherService>();
    private readonly Parameters _parameters;
    private readonly IBluVoucherKafkaProducer _bluVoucherKafkaProducer = Substitute.For<IBluVoucherKafkaProducer>();
    private readonly IOptions<RedisSettings> _redisSettings;
    private readonly IRedisService _redisService = Substitute.For<IRedisService>();
    private readonly MetricsHelper _metricsHelper = Substitute.For<MetricsHelper>();
    private readonly IOptions<DBSettings> _dbSettings;
    private readonly IVoucherProviderService _voucherProviderService = Substitute.For<IVoucherProviderService>();

    public BluVoucherRedeemServiceTest(Fixtures fixtures)
    {
        _parameters = Fixtures.CreateParameters();
        _dbSettings = Options.Create(fixtures.DBSettings);
        _redisSettings = Options.Create(fixtures.RedisSettings);

        _bluVoucherRedeemService = new BluVoucherRedeemService(
            _log,
            _voucherValidationService,
            _voucherLogRepository,
            _httpContextAccessor,
            _remitBluVoucherService,
            _bluVoucherKafkaProducer,
            _redisService,
            _redisSettings,
            _vlog,
            _metricsHelper,
            _dbSettings,
            _voucherProviderService);
    }

    [Fact]
    public async Task GetRedeemOutcome_Success()
    {
        //Arrange
        var parameter = _parameters;
        var bluVoucherRemitResponse = ArrangeCollections.CreateBluVoucherRemitResponse();
        var airtimeAuthResponse = ArrangeCollections.CreateAirtimeAuthenticationResponse(parameter);
        var bluVoucherRequest = ArrangeCollections.CreateBluVoucherRedeemRequest(parameter);
        var returnedStream = ArrangeCollections.CreateNetworkStream();

        var streamRedeemVoucherResult = ArrangeCollections.CreateSuccessStreamResultRedeemVoucher();
        var streamGetVoucherStatusResult = ArrangeCollections.CreateSuccessStreamResultGetVoucherStatus();
        _bluVoucherKafkaProducer.Produce(Arg.Any<BluVoucherRedeemRequest>(), VoucherType.BluVoucher, Arg.Any<RedeemOutcome>())
            .Returns(Task.CompletedTask);

        _airtimeAuthentication.Authenticate(Arg.Any<BluVoucherProviderAuthenticationRequest>()).Returns(airtimeAuthResponse);

        _getStreamResults.GetResults(Arg.Is<BluLabelProviderRequest>(x => x.EventType == "redeemVoucher"), Arg.Any<NetworkStream>()).Returns(streamRedeemVoucherResult);
        _getStreamResults.GetResults(Arg.Is<BluLabelProviderRequest>(x => x.EventType == "getVoucherStatus"), Arg.Any<NetworkStream>()).Returns(streamGetVoucherStatusResult);

        _remitBluVoucherService.RemitBluVoucher(Arg.Any<string>(), Arg.Any<string>()).Returns(bluVoucherRemitResponse);

        _tcpClient.When(x => x.ConnectAsync(Arg.Any<string>(), Arg.Any<int>()))
            .Do(x =>
            {
                Console.WriteLine($"Logged message: connected to provider");
            });

        _tcpClient.GetStream().Returns(returnedStream);

        _voucherLogRepository.When(x => x.UpdateVoucherLogAPIResponse(Arg.Any<string>(), "voucherRef123", Arg.Any<VoucherType>(), Arg.Any<VoucherStatus>(), Arg.Any<long>(), Arg.Any<decimal>(), Arg.Any<string>()))
        .Do(callInfo =>
        {
            var message = callInfo.Arg<string>();
            Console.WriteLine($"Logged message: {message}");
        });

        //Act
        var redeemResult = await _bluVoucherRedeemService.GetRedeemOutcome(bluVoucherRequest, VoucherType.BluVoucher);

        //Assert
        Assert.True(redeemResult.OutComeTypeId == 1);
    }

    [Fact]
    public async Task GetRedeemOutcome_Already_Redeem()
    {
        //Arrange
        var parameter = _parameters;
        var airtimeAUTHResponse = ArrangeCollections.CreateAirtimeAuthenticationResponse(parameter);
        var bluVoucherRequest = ArrangeCollections.CreateBluVoucherRedeemRequest(parameter);
        var returnedStream = ArrangeCollections.CreateNetworkStream();

        var streamRedeemVoucherResult = ArrangeCollections.CreateSuccessStreamResultRedeemVoucher();
        var streamGetVoucherStatusResult = ArrangeCollections.CreateSuccessStreamResultGetVoucherStatus();

        _bluVoucherKafkaProducer.Produce(Arg.Any<BluVoucherRedeemRequest>(), VoucherType.BluVoucher, Arg.Any<RedeemOutcome>()).ReturnsNull();
        _airtimeAuthentication.Authenticate(Arg.Any<BluVoucherProviderAuthenticationRequest>()).Returns(airtimeAUTHResponse);

        _getStreamResults.GetResults(Arg.Is<BluLabelProviderRequest>(x => x.EventType == "redeemVoucher"), Arg.Any<NetworkStream>()).Returns(streamRedeemVoucherResult);
        _getStreamResults.GetResults(Arg.Is<BluLabelProviderRequest>(x => x.EventType == "getVoucherStatus"), Arg.Any<NetworkStream>()).Returns(streamGetVoucherStatusResult);

        _tcpClient.When(x => x.ConnectAsync(Arg.Any<string>(), Arg.Any<int>()))
            .Do(x =>
            {
                Console.WriteLine($"Logged message: connected to provider");
            });

        _tcpClient.GetStream().Returns(returnedStream);

        _voucherLogRepository.When(x => x.UpdateVoucherLogAPIResponse(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<VoucherType>(), Arg.Any<VoucherStatus>(), Arg.Any<long>(), Arg.Any<decimal>(), Arg.Any<string>()))
       .Do(callInfo =>
        {
            var message = callInfo.Arg<string>();
            Console.WriteLine($"Logged message: {message}");
        });

        //Act
        var redeemResult = await _bluVoucherRedeemService.GetRedeemOutcome(bluVoucherRequest, VoucherType.BluVoucher);

        //Assert
        Assert.True(redeemResult.OutComeTypeId == -1);
    }

    [Fact]
    public async Task GetRedeemOutcome_Invalid_SessionId()
    {
        //Arrange
        var parameter = _parameters;
        var airtimeAUTHResponse = ArrangeCollections.CreateAirtimeAuthenticationResponse(parameter);
        var bluVoucherRequest = ArrangeCollections.CreateBluVoucherRedeemRequest(parameter);
        var returnedStream = ArrangeCollections.CreateNetworkStream();
        airtimeAUTHResponse.SessionId = string.Empty;

        var streamRedeemVoucherResult = ArrangeCollections.CreateSuccessStreamResultRedeemVoucher();
        var streamGetVoucherStatusResult = ArrangeCollections.CreateSuccessStreamResultGetVoucherStatus();

        _bluVoucherKafkaProducer.Produce(Arg.Any<BluVoucherRedeemRequest>(), VoucherType.BluVoucher, Arg.Any<RedeemOutcome>()).ReturnsNull();
        _airtimeAuthentication.Authenticate(Arg.Any<BluVoucherProviderAuthenticationRequest>()).Returns(airtimeAUTHResponse);

        _getStreamResults.GetResults(Arg.Is<BluLabelProviderRequest>(x => x.EventType == "redeemVoucher"), Arg.Any<NetworkStream>()).Returns(streamRedeemVoucherResult);
        _getStreamResults.GetResults(Arg.Is<BluLabelProviderRequest>(x => x.EventType == "getVoucherStatus"), Arg.Any<NetworkStream>()).Returns(streamGetVoucherStatusResult);

        _tcpClient.When(x => x.ConnectAsync(Arg.Any<string>(), Arg.Any<int>()))
            .Do(x =>
            {
                Console.WriteLine($"Logged message: connected to provider");
            });

        _tcpClient.GetStream().Returns(returnedStream);

        _voucherLogRepository.When(x => x.UpdateVoucherLogAPIResponse(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<VoucherType>(), Arg.Any<VoucherStatus>(), Arg.Any<long>(), Arg.Any<decimal>(), Arg.Any<string>()))
       .Do(callInfo =>
        {
            var message = callInfo.Arg<string>();
            Console.WriteLine($"Logged message: {message}");
        });

        //Act
        var redeemResult = await _bluVoucherRedeemService.GetRedeemOutcome(bluVoucherRequest, VoucherType.BluVoucher);

        //Assert
        Assert.True(redeemResult.OutComeTypeId == -1);
    }

    [Fact]
    public async Task GetRedeemOutcome_Invalid_Request()
    {
        //Arrange
        var parameter = _parameters;
        var airtimeAUTHResponse = ArrangeCollections.CreateAirtimeAuthenticationResponse(parameter);
        var bluVoucherRequest = ArrangeCollections.CreateBluVoucherRedeemRequest(parameter);
        var returnedStream = ArrangeCollections.CreateNetworkStream();

        var streamRedeemVoucherResult = ArrangeCollections.CreateFailedStreamResult();
        var streamGetVoucherStatusResult = ArrangeCollections.CreateSuccessStreamResultGetVoucherStatus();

        _bluVoucherKafkaProducer.Produce(Arg.Any<BluVoucherRedeemRequest>(), VoucherType.BluVoucher, Arg.Any<RedeemOutcome>()).ReturnsNull();
        _airtimeAuthentication.Authenticate(Arg.Any<BluVoucherProviderAuthenticationRequest>()).Returns(airtimeAUTHResponse);

        _getStreamResults.GetResults(Arg.Is<BluLabelProviderRequest>(x => x.EventType == "redeemVoucher"), Arg.Any<NetworkStream>()).Returns(streamRedeemVoucherResult);
        _getStreamResults.GetResults(Arg.Is<BluLabelProviderRequest>(x => x.EventType == "getVoucherStatus"), Arg.Any<NetworkStream>()).Returns(streamGetVoucherStatusResult);

        _tcpClient.When(x => x.ConnectAsync(Arg.Any<string>(), Arg.Any<int>()))
            .Do(x =>
            {
                Console.WriteLine($"Logged message: connected to provider");
            });

        _tcpClient.GetStream().Returns(returnedStream);

        _voucherLogRepository.When(x => x.UpdateVoucherLogAPIResponse(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<VoucherType>(), Arg.Any<VoucherStatus>(), Arg.Any<long>(), Arg.Any<decimal>(), Arg.Any<string>()))
       .Do(callInfo =>
        {
            var message = callInfo.Arg<string>();
            Console.WriteLine($"Logged message: {message}");
        });

        //Act
        var redeemResult = await _bluVoucherRedeemService.GetRedeemOutcome(bluVoucherRequest, VoucherType.BluVoucher);

        //Assert
        Assert.True(redeemResult.OutComeTypeId == -1);
    }

    [Fact]
    public async Task GetRedeemOutcome_Throws_Exception()
    {
        //Arrange
        var parameter = _parameters;
        var airtimeAUTHResponse = ArrangeCollections.CreateAirtimeAuthenticationResponse(parameter);
        var bluVoucherRequest = ArrangeCollections.CreateBluVoucherRedeemRequest(parameter);
        var returnedStream = ArrangeCollections.CreateNetworkStream();

        var streamRedeemVoucherResult = ArrangeCollections.CreateSuccessStreamResultRedeemVoucher();
        var streamGetVoucherStatusResult = ArrangeCollections.CreateSuccessStreamResultGetVoucherStatus();

        _bluVoucherKafkaProducer.Produce(Arg.Any<BluVoucherRedeemRequest>(), VoucherType.BluVoucher, Arg.Any<RedeemOutcome>()).ReturnsNull();
        _airtimeAuthentication.Authenticate(Arg.Any<BluVoucherProviderAuthenticationRequest>()).Returns(airtimeAUTHResponse);

        _getStreamResults.GetResults(Arg.Is<BluLabelProviderRequest>(x => x.EventType == "redeemVoucher"), Arg.Any<NetworkStream>()).Returns(streamRedeemVoucherResult);
        _getStreamResults.GetResults(Arg.Is<BluLabelProviderRequest>(x => x.EventType == "getVoucherStatus"), Arg.Any<NetworkStream>()).Returns(streamGetVoucherStatusResult);

        _tcpClient.When(x => x.ConnectAsync(Arg.Any<string>(), Arg.Any<int>()))
            .Do(x =>
            {
                Console.WriteLine($"Logged message: connected to provider");
            });

        _tcpClient.GetStream().Returns(returnedStream);

        _voucherLogRepository.When(x => x.UpdateVoucherLogAPIResponse(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<VoucherType>(), Arg.Any<VoucherStatus>(), Arg.Any<long>(), Arg.Any<decimal>(), Arg.Any<string>()))
       .Do(callInfo =>
        {
            var message = callInfo.Arg<string>();
            Console.WriteLine($"Logged message: {message}");
        });

        //Act
        var redeemResult = await _bluVoucherRedeemService.GetRedeemOutcome(bluVoucherRequest, VoucherType.BluVoucher);

        //Assert
        Assert.True(redeemResult.OutComeTypeId == -1);
    }

    [Fact]
    public async Task PerformRedeem_ShouldIncrementRequestCount()
    {
        // Arrange
        var parameter = _parameters;
        var bluVoucherRequest = ArrangeCollections.CreateBluVoucherRedeemRequest(parameter);
        // Act
        var sut = await _bluVoucherRedeemService.PerformRedeem(bluVoucherRequest);
        // Assert
        _metricsHelper.Received(1).IncBluVoucherRequestCounter(_log);
        Assert.NotNull(sut);
    }

    [Fact]
    public async Task GetRedeemOutcome_GivenErrorWhileRedeeming_ShouldReturnFalse()
    {

        //Arrange
        var parameter = _parameters;
        var bluVoucherRemitResponse = new BluVoucherProviderResponse()
        {
            ErrorCode = 1,
            Message = "Already redeemed",
            Success = false,
            VoucherAmount = 500,
            VoucherID = 1,
        };
        var airtimeAuthResponse = ArrangeCollections.CreateAirtimeAuthenticationResponse(parameter);
        var bluVoucherRequest = ArrangeCollections.CreateBluVoucherRedeemRequest(parameter);
        var returnedStream = ArrangeCollections.CreateNetworkStream();

        var streamRedeemVoucherResult = ArrangeCollections.CreateSuccessStreamResultRedeemVoucher();
        var streamGetVoucherStatusResult = ArrangeCollections.CreateSuccessStreamResultGetVoucherStatus();
        _bluVoucherKafkaProducer.Produce(Arg.Any<BluVoucherRedeemRequest>(), VoucherType.BluVoucher, Arg.Any<RedeemOutcome>())
            .Returns(Task.CompletedTask);

        _airtimeAuthentication.Authenticate(Arg.Any<BluVoucherProviderAuthenticationRequest>()).Returns(airtimeAuthResponse);

        _getStreamResults.GetResults(Arg.Is<BluLabelProviderRequest>(x => x.EventType == "redeemVoucher"), Arg.Any<NetworkStream>()).Returns(streamRedeemVoucherResult);
        _getStreamResults.GetResults(Arg.Is<BluLabelProviderRequest>(x => x.EventType == "getVoucherStatus"), Arg.Any<NetworkStream>()).Returns(streamGetVoucherStatusResult);

        _remitBluVoucherService.RemitBluVoucher(Arg.Any<string>(), Arg.Any<string>()).Returns(bluVoucherRemitResponse);

        _tcpClient.When(x => x.ConnectAsync(Arg.Any<string>(), Arg.Any<int>()))
            .Do(x =>
            {
                Console.WriteLine($"Logged message: connected to provider");
            });

        _tcpClient.GetStream().Returns(returnedStream);

        _voucherLogRepository.When(x => x.UpdateVoucherLogAPIResponse(Arg.Any<string>(), "voucherRef123", Arg.Any<VoucherType>(), Arg.Any<VoucherStatus>(), Arg.Any<long>(), Arg.Any<decimal>(), Arg.Any<string>()))
        .Do(callInfo =>
        {
            var message = callInfo.Arg<string>();
            Console.WriteLine($"Logged message: {message}");
        });

        //Act
        var redeemResult = await _bluVoucherRedeemService.GetRedeemOutcome(bluVoucherRequest, VoucherType.BluVoucher);

        //Assert
        Assert.True(redeemResult.OutComeTypeId == -1);
        _log.Received(1).LogInformation(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<object[]>());
    }

}
