using VPS.Domain.Models.BluVoucher.Responses;

namespace VPS.Services.BluVoucher.Mocks
{
    public class MockRemitBluVoucherService : IRemitBluVoucherService
    {
        public async Task<BluVoucherProviderResponse> RemitBluVoucher(string reference, string voucherPin)
        {
            var random = new Random();
            var lastPin = voucherPin[voucherPin.Length - 1];
            return await GetResponseBasedOnPin(lastPin, random.Next(10000000, 99999999), 500);
        }

        private static Task<BluVoucherProviderResponse> GetResponseBasedOnPin(char pin, long voucherId, decimal amount)
        {
            BluVoucherProviderResponse response;
            switch (pin)
            {
                case '0':
                    response = new BluVoucherProviderResponse
                    {
                        ErrorCode = 1,
                        Message = "Voucher redeem successfully",
                        Success = true,
                        VoucherAmount = amount,
                        VoucherID = voucherId,
                    };
                    break;
                case '1':
                    response = new BluVoucherProviderResponse
                    {
                        ErrorCode = 1,
                        Message = "Voucher Already Redeemed.",
                        Success = false,
                        VoucherAmount = amount,
                        VoucherID = voucherId,
                    };
                    break;
                default:
                    response = new BluVoucherProviderResponse
                    {
                        ErrorCode = 1,
                        Message = "Redeem failed.",
                        Success = false,
                        VoucherAmount = amount,
                        VoucherID = voucherId,
                    };
                    break;
            }
            return Task.FromResult(response);
        }
    }
}
