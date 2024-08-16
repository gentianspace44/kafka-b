using NSubstitute;
using VPS.Helpers.Logging;
using VPS.Services.Common;
using VPS.Test.Common.Models;
using VPS.Test.Common.Setup;

namespace VPS.Test.Common.Services
{
    public class VoucherValidationServiceTests
    {
        private readonly ILoggerAdapter<VoucherValidationService> _log = Substitute.For<ILoggerAdapter<VoucherValidationService>>();
        private readonly VoucherValidationService _voucherValidationService;
        public VoucherValidationServiceTests()
        {
            _voucherValidationService = new VoucherValidationService(_log);
        }

        [Fact]
        public void IsVoucherRequestValid_NullRequest_ReturnsErrorMessage()
        {
            // Arrange
            var voucherRedeemRequest = ArrangeCollection.GetNullVoucherRedeemRequestModel();

            // Act
            var result = _voucherValidationService.IsVoucherRequestValid(voucherRedeemRequest);

            // Assert
            Assert.Equal("Invalid Request", result);
        }


        [Theory]
        [InlineData("", 1, "Missing voucher pin/number")]
        [InlineData("123", 1, "Invalid voucher pin entered")]
        [InlineData("invalid", 1, "Invalid voucher pin entered")]
        [InlineData("1234567890123456789012345678901", 1, "Invalid voucher pin entered")]
        public void IsVoucherRequestValid_InvalidVoucherNumber_ReturnsErrorMessage(string voucherNumber, int clientId, string expectedError)
        {
            // Arrange
            var voucherRedeemRequest = ArrangeCollection.GetVoucherRedeemRequestModel(voucherNumber, clientId);

            // Act
            var result = _voucherValidationService.IsVoucherRequestValid(voucherRedeemRequest);

            // Assert
            Assert.Equal(expectedError, result);
        }


        [Fact]
        public void IsVoucherRequestValid_InvalidClientId_ReturnsErrorMessage()
        {
            // Arrange
            var voucherRedeemRequest = ArrangeCollection.GetVoucherRedeemRequestModel("1234567890", 0);

            // Act
            var result = _voucherValidationService.IsVoucherRequestValid(voucherRedeemRequest);


            // Assert
            Assert.Equal("Invalid Client ID", result);
        }


        [Fact]
        public void IsVoucherRequestValid_ValidRequest_ReturnsEmptyString()
        {
            // Arrange
            var voucherRedeemRequest = new VoucherRedeemRequestModel
            {
                VoucherNumber = "1234567890",
                ClientId = 1,
                DevicePlatform = "Platform",
            };


            // Act
            var result = _voucherValidationService.IsVoucherRequestValid(voucherRedeemRequest);


            // Assert
            Assert.Equal("", result);
            _log.DidNotReceiveWithAnyArgs().LogError(null, null);
        }
    }
}
