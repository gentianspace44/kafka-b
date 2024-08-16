using Newtonsoft.Json;
using System.Reflection;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.Flash.Requests;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Redis;

namespace VPS.Services.Common
{
    public class RedisService : IRedisService
    {
        private readonly IRedisRepository _redisRepository;
        private readonly ILoggerAdapter<RedisService> _log;

        public RedisService(IRedisRepository redisRepository, ILoggerAdapter<RedisService> log)
        {
            this._redisRepository = redisRepository ?? throw new ArgumentNullException(nameof(redisRepository));
            this._log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public static SemaphoreSlim SEMAPHORE { get { return ConcurrencyHelper.GetSemaphorInstance(); } }

        public async Task<Tuple<bool, string>> CheckRedisHealth(RedisStoreType storeType)
        {
            try
            {

                var healthStatus = await _redisRepository.CheckRedisHealth(storeType);

                return Tuple.Create(healthStatus.Item1, healthStatus.Item2);

            }
            catch (Exception ex)
            {
                _log.LogError(ex, null, ex.Message);
                return Tuple.Create(false, ex.Message);
            }
        }

        public async Task DeleteConcurrencyEntry(string voucherNumber)
        {
            await _redisRepository.Delete(voucherNumber, RedisStoreType.ConcurencyStore);
        }

        public async Task<bool> DoesConcurrencyExist(string voucherPin, dynamic voucherRequestObject)
        {
            await SEMAPHORE.WaitAsync();
            try
            {
                var fetchData = await _redisRepository.Get(voucherPin, RedisStoreType.ConcurencyStore);

                if (fetchData.HasValue)
                {
                    _log.LogInformation(voucherPin, "Duplicate entry found in Redis. PIN: {voucherPin} - Original Request: {fetchData}",
                       MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                        voucherPin, fetchData);
                    return true;
                }
                else
                {
                    //PIN does not exist in Redis so add it
                    _log.LogInformation(voucherPin, "Redis Cache insert. PIN: {voucherPin} - Original Request {voucherRequestObject}",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty, voucherPin, JsonConvert.SerializeObject(voucherRequestObject));

                    await _redisRepository.Add(voucherPin, voucherRequestObject, RedisStoreType.ConcurencyStore);

                    return false;
                }
            }
            catch (Exception ex)
            {
                _log.LogInformation(voucherPin, "Silent insertion fail. PIN:  {voucherPin} - Original Request {voucherRequestObject}. Error: {message}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty, voucherPin, JsonConvert.SerializeObject(voucherRequestObject), ex.Message);
                _log.LogError(ex, voucherPin, "Silent redis insertion fail.");
                return false;
            }
            finally
            {
                SEMAPHORE.Release();
            }
        }

        public async Task<bool> IsDelayStillAlive(string voucherPin, dynamic voucherRequestObject, int timeToLive)
        {
            try
            {

                var fetchData = await _redisRepository.Get(voucherPin, RedisStoreType.DelayStore);

                if (fetchData.HasValue)
                {
                    _log.LogInformation(voucherPin, "Entry found in Redis for Delay Check. PIN: {voucherPin} - Original Request: {fetchData}",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty, voucherPin, fetchData);
                    return true;
                }
                else
                {
                    //PIN does not exist in Redis so add it
                    _log.LogInformation(voucherPin, "Redis Cache insert into Delay Store. PIN: {voucherPin} - Original Request {voucherRequestObject}",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty, voucherPin, JsonConvert.SerializeObject(voucherRequestObject));

                    await _redisRepository.AddWithTTL(voucherPin, voucherRequestObject, RedisStoreType.DelayStore, timeToLive);

                    return false;
                }
            }
            catch (Exception ex)
            {
                _log.LogInformation(voucherPin, "Silent insertion fail. PIN:  {voucherPin} - Original Request {voucherRequestObject}. Error: {message}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty, voucherPin, JsonConvert.SerializeObject(voucherRequestObject));
                _log.LogError(ex, voucherPin, "Silent redis insertion fail.");
                return false;
            }
        }

        public async Task<bool> CheckInProgressRedemption(string voucherNumber)
        {
            try
            {

                var fetchData = await _redisRepository.Get(voucherNumber, RedisStoreType.InProgressStore);

                if (fetchData.HasValue)
                {
                    _log.LogInformation(voucherNumber, "Entry found in Redis for In-Progress Processing. PIN: {voucherPin} - Original Request: {fetchData}",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty, voucherNumber, fetchData);
                    return true;
                }

                return false;

            }
            catch (Exception ex)
            {
                _log.LogError(ex, voucherNumber, "Failed to get in-progress status.");
                return false;
            }
        }

        public async Task<bool> ConcurrencyWithIdempotencyCheckExists(string voucherPin, dynamic voucherRequest, int idempotencyTTL)
        {
            await SEMAPHORE.WaitAsync();
            try
            {
                var fetchData = await _redisRepository.Get(voucherPin, RedisStoreType.ConcurencyStore);

                if (fetchData.HasValue)
                {
                    _log.LogInformation(voucherPin, "Duplicate entry found in Redis. PIN: {voucherPin} - Original Request: {fetchData}",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                        voucherPin, fetchData);
                    return true;
                }
                else
                {
                    var key = voucherPin + "_" + voucherRequest.ClientId.ToString();
                    _log.LogInformation(voucherPin, "Redis Cache insert. PIN: {voucherPin} - Original Request {voucherRequest}",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                        voucherPin, JsonConvert.SerializeObject(voucherRequest));
                    _log.LogInformation(voucherPin, "Checking for Idempotency entry in Redis. PIN: {voucherPin} - Original Request: {voucherRequest}",
                        MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                        voucherPin, JsonConvert.SerializeObject(voucherRequest));
                    var voucherIdempotentCache = await _redisRepository.Get(key, RedisStoreType.IdempotencyStore);
                    if (voucherIdempotentCache.HasValue)
                    {
                        _log.LogInformation(voucherPin, "Idempotency entry in Redis. Key: {key} found",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            key);
                        var voucherRetryModel = JsonConvert.DeserializeObject<FlashRedeemRequest>(voucherIdempotentCache);
                        _log.LogInformation(voucherPin, "Idempotency replacing{voucherRequest} with {voucherIdempotentCache}",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            JsonConvert.SerializeObject(voucherRequest), JsonConvert.SerializeObject(voucherIdempotentCache));

                        voucherRequest.VoucherReference = voucherRetryModel.VoucherReference;
                    }
                    else
                    {
                        await _redisRepository.AddWithTTL(key, voucherRequest, RedisStoreType.IdempotencyStore, idempotencyTTL);
                    }
                    await _redisRepository.Add(voucherPin, voucherRequest, RedisStoreType.ConcurencyStore);

                    return false;
                }
            }
            catch (Exception ex)
            {
                _log.LogInformation(voucherPin, "Silent insertion fail. PIN:  {voucherPin} - Original Request {voucherRequest}. Error: {message}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    voucherPin, JsonConvert.SerializeObject(voucherRequest), ex.Message);
                _log.LogError(ex, voucherPin, "Silent redis insertion fail.", MethodBase.GetCurrentMethod()?.Name ?? string.Empty);
                return false;
            }
            finally
            {
                SEMAPHORE.Release();
            }
        }
    }
}
