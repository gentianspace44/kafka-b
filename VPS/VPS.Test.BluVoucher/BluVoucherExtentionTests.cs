using Microsoft.AspNetCore.Routing;
using NSubstitute;
using VPS.Domain.Models.BluVoucher.Requests;
using VPS.Provider.BluVoucher;
using VPS.Services.BluVoucher;
using Microsoft.AspNetCore.Builder;
using VPS.Domain.Models.Enums;
using VPS.Helpers;

namespace VPS.Test.BluVoucher
{
    public static class BluVoucherExtentionTests
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
                return "BluVoucher is up and running!";
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
                 async (BluVoucherRedeemRequest request,
                 BluVoucherRedeemService _bluVoucherRedeemService) =>
                 {
                     request.VoucherReference = Guid.NewGuid().ToString();
                     request.Provider = EnumHelper.GetEnumDescription(VoucherType.BluVoucher);
                     return await _bluVoucherRedeemService.PerformRedeem(request);
                 });
        }

        [Fact]
        public static void CannotCallUseVoucherProcessorWithNullEndpoint()
        {
            Assert.Throws<ArgumentNullException>(() => default(IEndpointRouteBuilder)!.UseVoucherProcessor());
        }
    }
}