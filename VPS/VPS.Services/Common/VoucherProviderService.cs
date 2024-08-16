using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VPS.API.Common;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.VRW.Voucher;
using VPS.Helpers;

namespace VPS.Services.Common
{
    public class VoucherProviderService : IVoucherProviderService
    {

        private readonly IHttpClientCommunication _httpClientCommunication;
        private readonly VpsControlCenterEndpoints _vpsControlCenterEndpoints;

        public VoucherProviderService(IHttpClientCommunication httpClientCommunication, IOptions<VpsControlCenterEndpoints> config)
        {          
            _httpClientCommunication = httpClientCommunication ?? throw new ArgumentNullException(nameof(httpClientCommunication));
            _vpsControlCenterEndpoints = config.Value;
        }

        public async Task<List<VoucherServiceEnabler>?> SetProviders()
        {
            var outputList = await Get(UrlHelper.CombineUrls(_vpsControlCenterEndpoints.BaseEndpoint, "getProviders"));

            if (outputList == null || !outputList.Any())
            {
                //In case redis isn't updated, the call below will force VPSC to update redis.
                outputList = await Get(_vpsControlCenterEndpoints.ForceRedisVoucherUpdate);
            }

            VoucherProviderHelper.SetProviders(outputList);
            return outputList;
        }
        public async Task<List<VoucherServiceEnabler>?> GetProviders()
        {
            return await Get(UrlHelper.CombineUrls(_vpsControlCenterEndpoints.BaseEndpoint, "getProviders"));
        }

        private async Task<List<VoucherServiceEnabler>?> Get(string url)
        {
            var response = await _httpClientCommunication.SendRequestAsync(url, Domain.Models.Enums.HttpMethod.GET);
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<VoucherServiceEnabler>>(responseBody);
        }
    }
}
