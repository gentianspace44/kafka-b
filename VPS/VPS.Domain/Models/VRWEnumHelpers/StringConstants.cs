namespace VPS.Domain.Models.VRWEnumHelpers
{
    public static class StringConstants
    {
        public static string VPS_SESSION => "SYX_SESSION";

        public static string GENERIC_ERROR_MESSAGE => "Invalid voucher pin entered. Make sure you have selected the correct provider above.";

        public static string TRYAGAIN_ERROR_MESSAGE => "An error has occurred. Please try again later.";

        public static string DEFAULT_PROGRESS_MESSAGE => "Transaction process is still in progress.";

        public static string TIMEOUT_ERROR_MESSAGE => "Sorry, we’ve experienced a timeout connecting to your provider. Please try again.";

        public const string PROVIDER_LIST = "ProviderList";
    }
}
