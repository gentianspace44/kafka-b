using Newtonsoft.Json;

namespace VPS.Domain.Models.Flash
{
    public class FlashAccessTokenCache
    {
        public string AccessToken { get; set; } = string.Empty;
        public string ExpiresIn { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        [JsonIgnore]
        public bool IsTokenValid { get; set; }
    }
}
