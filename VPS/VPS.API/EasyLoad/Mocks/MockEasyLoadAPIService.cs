using VPS.Domain.Models.EasyLoad.Enums;
using VPS.Domain.Models.EasyLoad.Response;

namespace VPS.API.EasyLoad.Mocks
{
    public class MockEasyLoadApiService : IEasyLoadApiService
    {
        public async Task<EasyLoadProviderVoucherResponse?> RedeemVoucher(string voucherPin, int clientId)
        {
            var random = new Random();
            //The type of response is dependent on the last pin of voucherPin for mock purposes
            var lastPin = voucherPin[voucherPin.Length - 1];
            return await GetResponseBasedOnPin(lastPin, voucherPin, random.Next(10000000, 99999999), 500);
        }

        private static async Task<EasyLoadProviderVoucherResponse> GetResponseBasedOnPin(char pin, string voucherPin, long voucherId, decimal amount)
        {
            await Task.Delay(100);

            switch (pin)
            {
                case '0':
                    return new EasyLoadProviderVoucherResponse
                    {
                        VoucherNumber = voucherPin,
                        VoucherId = voucherId,
                        Amount = amount,
                        ResponseCode = (int)EasyLoadRedeemResponseCodes.VoucherRedeemSuccessful,
                        ResponseMessage = "Voucher Successfully Redeemed"
                    };
                case '1':
                    return new EasyLoadProviderVoucherResponse
                    {
                        VoucherNumber = voucherPin,
                        VoucherId = voucherId,
                        Amount = amount,
                        ResponseCode = (int)EasyLoadRedeemResponseCodes.VoucherAlreadyRedeemed,
                        ResponseMessage = "Voucher Already Redeemed."
                    };
                default:
                    return new EasyLoadProviderVoucherResponse
                    {
                        VoucherNumber = voucherPin,
                        VoucherId = voucherId,
                        Amount = amount,
                        ResponseCode = -1, //-1 is an unhandled responseCode on easyload so we use it as unknown failure
                        ResponseMessage = "Redeem failed."
                    };
            }
        }
    }
}
