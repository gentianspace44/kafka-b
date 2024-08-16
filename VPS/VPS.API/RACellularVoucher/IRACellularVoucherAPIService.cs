using VPS.Domain.Models.RACellularVoucher.Response;

namespace VPS.API.RACellularVoucher;

public interface IraCellularVoucherApiService
{
    Task<RaCellularVoucherRedeemResponse> RedeemVoucherAsync(string clientId, string voucherPin, string platform);
    Task<RaCellularVoucherRedeemResponse> LookUpVoucherAsync(string clientId, string voucherPin);
    Task<RaCellularVoucherRedeemResponse> AdviceAsync(string clientId, string voucherPin, string platform);
}
