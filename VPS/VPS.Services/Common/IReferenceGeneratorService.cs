using VPS.Domain.Models.Enums;

namespace VPS.Services.Common
{
    public interface IReferenceGeneratorService
    {
        string Generate(VoucherType voucherType, string voucherId, string voucherPin, string platform);
        public string Generate(string voucherPrefix, string voucherId, string voucherPin, string platform);

    }
}
