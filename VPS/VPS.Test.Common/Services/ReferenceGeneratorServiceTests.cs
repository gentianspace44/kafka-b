using VPS.Domain.Models.Enums;
using VPS.Services.Common;

namespace VPS.Test.Common.Services
{
    public class ReferenceGeneratorServiceTests
    {
        [Theory]
        [InlineData(VoucherType.EasyLoad, "123456", "987654", "Android")]
        [InlineData(VoucherType.BluVoucher, "654321", "456789", "iOS")]
        public void Generate_WithVoucherType_ReturnsExpectedReference(
            VoucherType voucherType, string voucherId, string voucherPin, string platform)
        {
            // Arrange
            var service = new ReferenceGeneratorService();

            // Act
            var result = service.Generate(voucherType, voucherId, voucherPin, platform);

            // Assert
            string expectedReference = voucherType == VoucherType.EasyLoad ? $"{platform} Voucher Redeem - {voucherType} - {voucherId} - {voucherPin}" : $"{platform} Voucher Redeem - {voucherType} - {voucherId}";
            Assert.Equal(expectedReference, result);
        }

        [Theory]
        [InlineData("EasyLoad", "123456", "987654", "Android")]
        [InlineData("BluVoucher", "654321", "456789", "iOS")]
        public void Generate_WithStringVoucherPrefix_ReturnsExpectedReference(
            string voucherPrefix, string voucherId, string voucherPin, string platform)
        {
            // Arrange
            var service = new ReferenceGeneratorService();

            // Act
            var result = service.Generate(voucherPrefix, voucherId, voucherPin, platform);

            // Assert
            string expectedReference = voucherPrefix.ToLower() == VoucherType.EasyLoad.ToString().ToLower() ? $"{platform} Voucher Redeem - {voucherPrefix} - {voucherId} - {voucherPin}" : $"{platform} Voucher Redeem - {voucherPrefix} - {voucherId}";
            Assert.Equal(expectedReference, result);
        }
    }
}
