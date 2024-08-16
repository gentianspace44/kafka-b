using VPS.Domain.Models.Enums;

namespace VPS.Services.Common
{
    public interface IRedisService
    {
        Task DeleteConcurrencyEntry(string voucherNumber);
        Task<bool> DoesConcurrencyExist(string voucherPin, dynamic voucherRequestObject);
        Task<bool> IsDelayStillAlive(string voucherPin, dynamic voucherRequestObject, int timeToLive);
        Task<Tuple<bool, string>> CheckRedisHealth(RedisStoreType storeType);
        Task<bool> CheckInProgressRedemption(string voucherNumber);
        Task<bool> ConcurrencyWithIdempotencyCheckExists(string voucherPin, dynamic voucherRequest, int idempotencyTTL);
    }
}
