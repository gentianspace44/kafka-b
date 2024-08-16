using System.Net.Sockets;
using Microsoft.Extensions.Options;
using NSubstitute;
using VPS.Domain.Models.BluVoucher.Requests;
using VPS.Domain.Models.BluVoucher.Responses;
using VPS.Domain.Models.Configurations.BluVoucher;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Services.BluVoucher;
using VPS.Test.BluVoucher.Setup;

namespace VPS.Test.BluVoucher.BluVoucher
{
    public class RemitBluVoucherServiceTests : IClassFixture<Fixtures>
    {
        private RemitBluVoucherService _remitBluVoucherService;
        private ILoggerAdapter<RemitBluVoucherService> _log;
        private IOptions<BluVoucherConfiguration> _bluVoucherSettings;
        private ITcpClient _tcpClient;
        private IAirtimeAuthentication _airtimeAuthentication;
        private IGetStreamResults _getStreamResults;
        private MetricsHelper _metricsHelper;

        public RemitBluVoucherServiceTests(Fixtures fixtures)
        {
            _bluVoucherSettings = Options.Create(fixtures.BluVoucherConfiguration);
            _log = Substitute.For<ILoggerAdapter<RemitBluVoucherService>>();
            _tcpClient = Substitute.For<ITcpClient>();
            _airtimeAuthentication = Substitute.For<IAirtimeAuthentication>();
            _getStreamResults = Substitute.For<IGetStreamResults>();
            _metricsHelper = new MetricsHelper();
            _remitBluVoucherService = new RemitBluVoucherService(_log, _bluVoucherSettings, _tcpClient, _airtimeAuthentication, _getStreamResults, _metricsHelper);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new RemitBluVoucherService(_log, _bluVoucherSettings, _tcpClient, _airtimeAuthentication, _getStreamResults, _metricsHelper);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CannotConstructWithNullLog()
        {
            Assert.Throws<ArgumentNullException>(() => new RemitBluVoucherService(default(ILoggerAdapter<RemitBluVoucherService>)!, _bluVoucherSettings, _tcpClient, _airtimeAuthentication, _getStreamResults, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullBluVoucherSettings()
        {
            Assert.Throws<ArgumentNullException>(() => new RemitBluVoucherService(_log, default(IOptions<BluVoucherConfiguration>)!, _tcpClient, _airtimeAuthentication, _getStreamResults, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullTcpClient()
        {
            Assert.Throws<ArgumentNullException>(() => new RemitBluVoucherService(_log, _bluVoucherSettings, default(ITcpClient)!, _airtimeAuthentication, _getStreamResults, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullGetStreamResults()
        {
            Assert.Throws<ArgumentNullException>(() => new RemitBluVoucherService(_log, _bluVoucherSettings, _tcpClient, _airtimeAuthentication, default(IGetStreamResults)!, _metricsHelper));
        }

        [Fact]
        public void CannotConstructWithNullMetricsHelper()
        {
            Assert.Throws<ArgumentNullException>(() => new RemitBluVoucherService(_log, _bluVoucherSettings, _tcpClient, _airtimeAuthentication, _getStreamResults, default(MetricsHelper)!));
        }

        [Fact]
        public async Task CanCallRemitBluVoucher()
        {
            // Arrange
            var reference = "410208932";
            var voucherPin = "1087050361";
            _airtimeAuthentication.Authenticate(Arg.Any<BluVoucherProviderAuthenticationRequest>()).Returns(new AirtimeAuthenticationResponse
            {
                SessionId = "1574342398",
                EventType = "2114618416",
                Event = new AuthenticationEvent { EventCode = "1766011321" },
                Data = new AuthenticationData
                {
                    TransTypes = new AuthenticationTransTypes { TransType = new List<string>() },
                    Reference = "153672252"
                }
            });
            _getStreamResults.GetResults<BluLabelProviderRequest>(Arg.Any<BluLabelProviderRequest>(), Arg.Any<NetworkStream>()).Returns("375894062");

            // Act
            _ = await _remitBluVoucherService.RemitBluVoucher(reference, voucherPin);

            // Assert
            await _tcpClient.Received().ConnectAsync(Arg.Any<string>(), Arg.Any<int>());
            _tcpClient.Received().GetStream();
            await _airtimeAuthentication.Received().Authenticate(Arg.Any<BluVoucherProviderAuthenticationRequest>());
            await _getStreamResults.Received().GetResults<BluLabelProviderRequest>(Arg.Any<BluLabelProviderRequest>(), Arg.Any<NetworkStream>());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallRemitBluVoucherWithInvalidReference(string value)
        {
            var results = await _remitBluVoucherService.RemitBluVoucher(value, "270840534");
            Assert.Equal("Invalid request", results.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallRemitBluVoucherWithInvalidVoucherPin(string value)
        {
            var results = await _remitBluVoucherService.RemitBluVoucher("2032865869", value);
            Assert.Equal("Invalid request", results.Message);
        }
    }
}