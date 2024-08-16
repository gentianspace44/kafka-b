using Microsoft.Extensions.Options;
using NSubstitute;
using VPS.API.Common;
using VPS.API.EasyLoad;
using VPS.Domain.Models.Configurations.EasyLoad;
using VPS.Domain.Models.EasyLoad.Request;
using VPS.Domain.Models.EasyLoad.Response;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Test.EasyLoad.Setup;

namespace VPS.Test.EasyLoad.Services
{
    public class EasyLoadServiceTest : IClassFixture<Fixtures>
    {
        private readonly EasyLoadVoucherRedeemRequest _easyLoadVoucherRedeemRequest;
        private readonly ILoggerAdapter<EasyLoadApiService> _log = Substitute.For<ILoggerAdapter<EasyLoadApiService>>();
        private readonly IHttpClientCommunication _httpClientCommunication = Substitute.For<IHttpClientCommunication>();
        private readonly IOptions<EasyLoadConfiguration> _easyLoadSettings;
        private readonly EasyLoadApiService _easyLoadService;
        private readonly MetricsHelper _metricsHelper = Substitute.For<MetricsHelper>();

        public EasyLoadServiceTest(Fixtures fixtures)
        {
            _easyLoadVoucherRedeemRequest = Fixtures.CreateVoucherRedeemRequest();
            _easyLoadSettings = Options.Create(fixtures.EasyLoadSettings);
            _easyLoadService = new EasyLoadApiService(_log, _easyLoadSettings, _httpClientCommunication, _metricsHelper);
        }

        [Fact]
        public async Task RedeemVoucher_Success_ReturnsValidResponse()
        {
            // Arrange
            var expectedEasyLoadVoucherResponse = ArrangeCollection.CreateSuccessEasyLoadVoucherResponse(_easyLoadVoucherRedeemRequest.VoucherNumber);
            var expectedRestResponse = ArrangeCollection.CreateSuccessEasyLoadRestResponse(expectedEasyLoadVoucherResponse);

            _httpClientCommunication.SendRequestAsync(Arg.Any<string>(), default, default, Arg.Any<string>(), default, default).ReturnsForAnyArgs(expectedRestResponse);

            // Act
            var result = await _easyLoadService.RedeemVoucher(_easyLoadVoucherRedeemRequest.VoucherNumber, _easyLoadVoucherRedeemRequest.ClientId);

            // Assert
            // Assert that the result matches the expected response
            Assert.True(EasyLoadVoucherResponseObjectsAreEqualByValue(expectedEasyLoadVoucherResponse, result));
        }



        #region Private Utils
        private static bool EasyLoadVoucherResponseObjectsAreEqualByValue(EasyLoadProviderVoucherResponse expected, EasyLoadProviderVoucherResponse? actual)
        {
            return expected.VoucherNumber == actual?.VoucherNumber &&
                   expected.Amount == actual.Amount &&
                   expected.ResponseCode == actual.ResponseCode &&
                   expected.ResponseMessage == actual.ResponseMessage;
        }
        #endregion
    }

}
