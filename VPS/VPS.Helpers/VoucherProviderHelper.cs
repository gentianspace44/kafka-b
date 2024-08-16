using VPS.Domain.Models.Enums;
using VPS.Domain.Models.VRW.Voucher;

namespace VPS.Helpers
{
    public static class VoucherProviderHelper
    {
        public static List<VoucherServiceEnabler>? Providers { get; set; }


        public static void SetProviders(List<VoucherServiceEnabler>? providers)
        {
            Providers = providers;
        }

        public static List<VoucherServiceEnabler>? GetProviders()
        {
            return Providers;
        }

        public static string? IsVoucherLengthValid(string voucherNumber, string expectedLength)
        {
            var expectedLengths = expectedLength.Split(',')
                          .Select(value => int.Parse(value.Trim()));

            if (string.IsNullOrWhiteSpace(voucherNumber) || !expectedLengths.Any(z => z == voucherNumber.Length))
            {
                return $"Invalid Voucher number length.";
            }
            return null;
        }

        public static string? IsVoucherLengthValid(string voucherNumber, VoucherType voucherType)
        {
            VoucherServiceEnabler? voucherServiceEnabler = (Providers?.Find(c => c.VoucherType.VoucherTypeId == (int)voucherType)) ?? throw new FormatException("No matching voucher type found.");
            var voucherLengths = voucherServiceEnabler.VoucherLength;

            return IsVoucherLengthValid(voucherNumber, voucherLengths);
        }
        public static List<VoucherServiceEnabler>? GetProvidersByLength(int length)
        {
            //We get only enabled providers
            var voucherServiceEnabler = Providers?.Where(c => c.IsEnabled && c.VoucherType.VoucherLength.Split(',')
                          .Select(value => int.Parse(value.Trim()))
                          .Any(x => x == length)).ToList();
            return voucherServiceEnabler;
        }

        public static bool IsProviderEnabled(VoucherType voucherType)
        {
            var provider = Providers?.Find(c => c.VoucherType.VoucherTypeId == (int)voucherType);
            if (provider != null)
            {
                return provider.IsEnabled;
            }
            return false;
        }
    }
}
