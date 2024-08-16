using VPS.Domain.Models.Configurations.Flash;

namespace VPS.API.Flash.Processes
{
    public static class FlashAuthenticationProcess
    {
        public static List<KeyValuePair<string, string>> CreateFlashAuthenticationHeaders(FlashConfiguration _flashSettings)
        {
            var consumerKey = _flashSettings.FlashConsumerKey;
            var consumerSecret = _flashSettings.FlashConsumerSecret;
            var encodedAuth = Base64Encode($"{consumerKey}:{consumerSecret}");
            return new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Authorization", $"Basic {encodedAuth}")
                };
        }

        public static string CreateFlashAuthenticationParameters()
        {
            var requestparams = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                };
            return string.Join("&", requestparams.Select(kv => kv.Key + "=" + kv.Value).ToArray());
        }

        public static TimeSpan CreateSecondsTimespan(int timeoutInSeconds)
        {
            return new TimeSpan(0,0, timeoutInSeconds);
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
