using System.ComponentModel;
using System.Reflection;
using VPS.Domain.Models.HollyTopUp;

namespace VPS.Helpers
{
    public static class EnumHelper
    {
        public static string GetEnumDescription(Enum value)
        {
            try
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                // Get the Description attribute value for the enum value
                FieldInfo? fi = value.GetType().GetField(value.ToString());
                if (fi == null) throw new ArgumentException("Field not found");
                DescriptionAttribute[]? attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes.Length > 0)
                    return attributes[0].Description;
                else
                    return value.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetHollyTopUpRedeemStatusString(int value)
        {
            switch (value)
            {
                case (int)HollyTopUpRedeemStatus.ErrorInProcess:
                    return HollyTopUpRedeemStatus.ErrorInProcess.ToString();
                case (int)HollyTopUpRedeemStatus.RedeemSuccessful:
                    return HollyTopUpRedeemStatus.RedeemSuccessful.ToString();
                case (int)HollyTopUpRedeemStatus.RedeemInProgress:
                    return HollyTopUpRedeemStatus.RedeemInProgress.ToString();
                case (int)HollyTopUpRedeemStatus.InvalidVoucher:
                    return HollyTopUpRedeemStatus.InvalidVoucher.ToString();
                case (int)HollyTopUpRedeemStatus.AlreadyRedeemed:
                    return HollyTopUpRedeemStatus.AlreadyRedeemed.ToString();
                default: return "Unknown Status";
            }
        }
    }
}
