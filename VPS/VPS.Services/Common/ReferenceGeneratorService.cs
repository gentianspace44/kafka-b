using VPS.Domain.Models.Enums;

namespace VPS.Services.Common
{
    public class ReferenceGeneratorService : IReferenceGeneratorService
    {
        public string Generate(VoucherType voucherType, string voucherId, string voucherPin, string platform)
        {
            string? voucherPrefix = Enum.GetName(typeof(VoucherType), voucherType); 
            switch (voucherType)
            {
                case VoucherType.EasyLoad:
                    return $"{platform} Voucher Redeem - {voucherPrefix} - {voucherId} - {voucherPin}";
                default:
                    return $"{platform} Voucher Redeem - {voucherPrefix} - {voucherId}";

            }
        }

        public string Generate(string voucherPrefix, string voucherId, string voucherPin, string platform)
        {

            switch (voucherPrefix.ToLower())
            {
                case "easyload":
                    return $"{platform} Voucher Redeem - {voucherPrefix} - {voucherId} - {voucherPin}";
                default:
                    return $"{platform} Voucher Redeem - {voucherPrefix} - {voucherId}";

            }
        }
    }
}
