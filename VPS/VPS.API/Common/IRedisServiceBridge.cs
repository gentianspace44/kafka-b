using VPS.Domain.Models.Flash;

namespace VPS.API.Common
{
    public interface IRedisServiceBridge
    {
        Task<FlashAccessTokenCache> GetCachedFlashToken(string flashAccessTokenKey);
        Task SaveFlashTokenCache(string flashAccessTokenKey, FlashAccessToken token, int timetoLiveInSec);
        Task DeleteIdempotency(string flashIdempotencyKey);
    }
}
