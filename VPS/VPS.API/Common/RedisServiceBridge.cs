using Newtonsoft.Json;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.Flash;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Redis;

namespace VPS.API.Common
{
    public class RedisServiceBridge : IRedisServiceBridge
    {
        private readonly IRedisRepository _redisRepository;
        private readonly ILoggerAdapter<RedisServiceBridge> _log;
        public RedisServiceBridge(IRedisRepository redisRepository, ILoggerAdapter<RedisServiceBridge> log)
        {
            this._redisRepository = redisRepository ?? throw new ArgumentNullException(nameof(redisRepository));
            this._log = log ?? throw new ArgumentNullException(nameof(log));
        }
        public async Task SaveFlashTokenCache(string flashAccessTokenKey, FlashAccessToken token, int timetoLiveInSec)
        {
            var redisToken = new FlashAccessTokenCache() { AccessToken = token.Access_Token, ExpiresIn = token.Expires_In };
            await _redisRepository.AddWithTTL(flashAccessTokenKey, redisToken, RedisStoreType.TokenRefreshStore, timetoLiveInSec);
        }

        public async Task<FlashAccessTokenCache> GetCachedFlashToken(string flashAccessTokenKey)
        {
            var tokenCache = await _redisRepository.Get(flashAccessTokenKey, RedisStoreType.TokenRefreshStore);

            if (tokenCache.HasValue)
            {
                var token = JsonConvert.DeserializeObject<FlashAccessTokenCache>(tokenCache!)!;
                token.IsTokenValid = IsTokenValid(token);
                return token;
            }
            _log.LogInformation(flashAccessTokenKey, "Flash Access Token not found in Redis");
            return new FlashAccessTokenCache() { IsTokenValid = false };
        }

        public async Task DeleteIdempotency(string flashIdempotencyKey)
        {
            await _redisRepository.Delete(flashIdempotencyKey, RedisStoreType.IdempotencyStore);

            _log.LogInformation(flashIdempotencyKey, "Flash idempotency deleted from Redis");
        }

        private static bool IsTokenValid(FlashAccessTokenCache token)
        {
            var secondsCreated = (DateTime.UtcNow - token.CreatedOn.ToUniversalTime()).TotalSeconds;
            return (int.Parse(token.ExpiresIn) - 60) > Convert.ToInt32(secondsCreated);
        }
    }
}
