using Microsoft.Extensions.Options;
using NSubstitute;
using VPS.API.Common;
using VPS.API.OTT;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.OTT.Requests;
using VPS.Domain.Models.OTT.Responses;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Test.OTTVoucher.Setup;

namespace VPS.Test.OTTVoucher.Services;

public class OTTVoucherServiceTest : IClassFixture<Fixture>
{

    private readonly OttVoucherRedeemRequest _oTTVoucherRedeemRequest;
    private readonly ILoggerAdapter<OttApiService> _log = Substitute.For<ILoggerAdapter<OttApiService>>();
    private readonly OttApiService _ottVoucherService;
    private readonly IHttpClientCommunication _httpClientCommunication;
    private readonly MetricsHelper _metricsHelper = Substitute.For<MetricsHelper>();

    public OTTVoucherServiceTest(Fixture fixture)
    {
        _oTTVoucherRedeemRequest = Fixture.CreateVoucherRedeemRequest();
        _httpClientCommunication = Substitute.For<IHttpClientCommunication>();
        _ottVoucherService = new OttApiService(_log, Options.Create(fixture.OttVoucherSettings), _httpClientCommunication, _metricsHelper);
    }

    [Fact]
    public async Task RedeemVoucher_Sucess_ReturnValidResponse()
    {
        //arrage
        var expectedOTTVoucherResponse = ArrangeCollection.CreateSuccessOTTVoucherResponse();
        var expectedRestResponse = ArrangeCollection.CreateSucessOTTVoucherRestResponse(expectedOTTVoucherResponse);

        _httpClientCommunication.SendRequestAsync(Arg.Any<string>(),
            Domain.Models.Enums.HttpMethod.POST,
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<List<KeyValuePair<string, string>>>())
            .Returns(Task.FromResult(expectedRestResponse));

        //act
        string uniqueReference = Guid.NewGuid().ToString();
        var result = await _ottVoucherService.RemitOTTVoucher(uniqueReference, _oTTVoucherRedeemRequest);

        //Assert
        //Assert that the result matches the expected response
        Assert.True(OTTVoucherResponseObjectAreEqualByValue(expectedOTTVoucherResponse, result));
    }

    private static bool OTTVoucherResponseObjectAreEqualByValue(OttProviderVoucherResponse expected, OttProviderVoucherResponse actual)
    {
        return expected.Success == actual.Success &&
               expected.VoucherAmount == actual.VoucherAmount &&
               expected.VoucherID == actual.VoucherID &&
               expected.Message == actual.Message &&
               expected.ErrorCode == actual.ErrorCode;
    }
}
