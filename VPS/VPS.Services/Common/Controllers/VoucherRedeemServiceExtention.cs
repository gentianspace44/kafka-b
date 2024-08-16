using Polly;
using Polly.Retry;
using System.Reflection;
using VPS.Domain.Models.Common.Request;
using VPS.Domain.Models.Configurations;
using VPS.Helpers.Logging;

namespace VPS.Services.Common.Controllers
{
    public static class VoucherRedeemServiceExtension
    {

        public static async Task<bool> DoesConcurrencyExist<T>(this VoucherRedeemService<T> voucherRedeemService,
            RedisSettings redisSettings,
            VoucherRedeemRequestBase voucherRedeemRequest,
            IRedisService redisService,
            ILoggerAdapter<VoucherRedeemService<T>> log
            )
        {

            int noOfAttempts = 0;

            AsyncRetryPolicy _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    redisSettings.MaxConcurrencyPolyRetry,
                    retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(10, noOfAttempts)),
                    onRetry: (exception, timeElapsed) =>
                    {
                        noOfAttempts++;
                        log.LogInformation(voucherRedeemRequest.VoucherNumber, "Redis Concurrency check retry. message: {message}. Retry Attempt: {noOfAttempts}",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            voucherRedeemRequest, noOfAttempts);

                        log.LogError(voucherRedeemRequest.VoucherNumber, "Redis Concurrency check retry. message: {message}. Retry Attempt: {noOfAttempts}. Error: {error}",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            voucherRedeemRequest, noOfAttempts, exception);
                    }
                );


            if (redisSettings.EnableConcurrencyCheck && await _retryPolicy.ExecuteAsync(() => redisService.DoesConcurrencyExist(voucherRedeemRequest.VoucherNumber, voucherRedeemRequest)))
            {
                log.LogInformation(voucherRedeemRequest.VoucherNumber, "Redeem failed, Duplicate voucher request found in Redis. Voucher PIN: {voucherPin}. Request Object:{request}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    voucherRedeemRequest.VoucherNumber, voucherRedeemRequest);

                return true;
            }

            return false;
        }

        public static async Task<bool> DoesConcurrencyAndIdempotencyExist<T>(this VoucherRedeemService<T> voucherRedeemService,
           RedisSettings redisSettings,
           VoucherRedeemRequestBase voucherRedeemRequest,
           IRedisService redisService,
           ILoggerAdapter<VoucherRedeemService<T>> log,
            int idempotencyTTL
           )
        {

            int noOfAttempts = 0;

            AsyncRetryPolicy _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    redisSettings.MaxConcurrencyPolyRetry,
                    retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(10, noOfAttempts)),
                    onRetry: (exception, timeElapsed) =>
                    {
                        noOfAttempts++;
                        log.LogInformation(voucherRedeemRequest.VoucherNumber, "Redis Concurrency check retry. message: {message}. Retry Attempt: {noOfAttempts}",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            voucherRedeemRequest, noOfAttempts);

                        log.LogError(voucherRedeemRequest.VoucherNumber, "Redis Concurrency check retry. message: {message}. Retry Attempt: {noOfAttempts}. Error: {error}",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            voucherRedeemRequest, noOfAttempts, exception);
                    }
                );


            if (redisSettings.EnableConcurrencyCheck && await _retryPolicy.ExecuteAsync(() => redisService.ConcurrencyWithIdempotencyCheckExists(voucherRedeemRequest.VoucherNumber, voucherRedeemRequest, idempotencyTTL)))
            {
                log.LogInformation(voucherRedeemRequest.VoucherNumber, "Redeem failed, Duplicate voucher request found in Redis. Voucher PIN: {voucherPin}. Request Object:{request}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    voucherRedeemRequest.VoucherNumber, voucherRedeemRequest);

                return true;
            }

            return false;
        }


        public static async Task<bool> IsDelayStillAlive<T>(this VoucherRedeemService<T> voucherRedeemService,
            RedisSettings redisSettings,
            VoucherRedeemRequestBase voucherRedeemRequest,
            IRedisService redisService,
            ILoggerAdapter<VoucherRedeemService<T>> log
            )
        {

            int noOfAttempts = 0;

            AsyncRetryPolicy _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    redisSettings.MaxDelayPolyRetry,
                    retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(10, noOfAttempts)),
                    onRetry: (exception, timeElapsed) =>
                    {
                        noOfAttempts++;
                        log.LogInformation(voucherRedeemRequest.VoucherNumber, "Redis Delay check retry. message: {message}. Retry Attempt: {noOfAttempts}",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            voucherRedeemRequest, noOfAttempts);

                        log.LogError(voucherRedeemRequest.VoucherNumber, "Redis Delay check retry. message: {message}. Retry Attempt: {noOfAttempts}. Error: {error}",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            voucherRedeemRequest, noOfAttempts, exception);
                    }
                );


            if (redisSettings.EnableDelayCheck && await _retryPolicy.ExecuteAsync(() => redisService.IsDelayStillAlive(voucherRedeemRequest.VoucherNumber, voucherRedeemRequest, redisSettings.CachePolicyTimeToLiveInSeconds)))
            {
                log.LogInformation(voucherRedeemRequest.VoucherNumber, "Redeem failed, Failed to pass delay policy in Redis. Voucher PIN: {voucherPin}. Request Object:{request}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    voucherRedeemRequest.VoucherNumber, voucherRedeemRequest);

                return true;
            }

            return false;
        }

        public static async Task<bool> IsRedemptionInProgress<T>(this VoucherRedeemService<T> voucherRedeemService,
          RedisSettings redisSettings,
          string voucherNumber,
          IRedisService redisService,
          ILoggerAdapter<VoucherRedeemService<T>> log
          )
        {

            int noOfAttempts = 0;

            AsyncRetryPolicy _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    redisSettings.MaxInProgressPolyRetry,
                    retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(10, noOfAttempts)),
                    onRetry: (exception, timeElapsed) =>
                    {
                        noOfAttempts++;
                        log.LogInformation(voucherNumber, "Redis InProgress check retry. voucher: {voucher}. Retry Attempt: {noOfAttempts}",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            voucherNumber, noOfAttempts);

                        log.LogError(voucherNumber, "Redis InProgress check retry. voucher: {voucher}. Retry Attempt: {noOfAttempts}. Error: {error}",
                            MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                            voucherNumber, noOfAttempts, exception);
                    }
                );


            if (redisSettings.EnableInProgressCheck && await _retryPolicy.ExecuteAsync(() => redisService.CheckInProgressRedemption(voucherNumber)))
            {
                log.LogInformation(voucherNumber, "Redemption In Progress. Voucher PIN: {voucherPin}. ",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    voucherNumber);

                return true;
            }

            return false;
        }

    }


}
