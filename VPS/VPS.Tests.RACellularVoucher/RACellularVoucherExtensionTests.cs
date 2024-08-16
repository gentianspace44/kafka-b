using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.RACellularVoucher.Requests;
using VPS.Helpers;
using VPS.Provider.RACellularVoucher;
using VPS.Services.RACellularVoucher;
using Xunit;

namespace VPS.Tests.RACellularVoucher
{
    public static class RACellularVoucherExtensionTests
    {
        [Fact]
        public static void UseVoucherProcessor_ShouldCallMapGet()
        {
            // Arrange
            var endpoint = Substitute.For<IEndpointRouteBuilder>();

            // Act
            endpoint.UseVoucherProcessor();
            // Assert
            endpoint.Received().MapGet("/healthcheck", () =>
            {
                return "R & A Cellular Voucher is up and running";
            });
        }

        [Fact]
        public static void UseVoucherProcessor_ShouldCallMapPost()
        {
            // Arrange
            var endpoint = Substitute.For<IEndpointRouteBuilder>();

            // Act
            endpoint.UseVoucherProcessor();
            // Assert
            endpoint.Received().MapPost("/Redeem",
                 async (RaCellularVoucherRedeemRequest request,
                 RaCellularVoucherRedeemService _raCellularVoucherRedeemService) =>
                 {
                     request.VoucherReference = Guid.NewGuid().ToString();
                     request.Provider = EnumHelper.GetEnumDescription(VoucherType.RACellular);
                     return await _raCellularVoucherRedeemService.PerformRedeemAsync(request);
                 });
        }

        [Fact]
        public static void CannotCallUseVoucherProcessorWithNullEndpoint()
        {
            Assert.Throws<ArgumentNullException>(() => default(IEndpointRouteBuilder)!.UseVoucherProcessor());
        }
    }
}