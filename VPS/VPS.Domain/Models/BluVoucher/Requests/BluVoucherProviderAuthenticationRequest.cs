using System.Net.Sockets;
using System.Text.Json.Serialization;

namespace VPS.Domain.Models.BluVoucher.Requests
{
    public class BluVoucherProviderAuthenticationRequest : BluVoucherAuthenticationRequestBase
    {
        [JsonIgnore]
        public NetworkStream? NetworkStream { get; set; }
    }
}
