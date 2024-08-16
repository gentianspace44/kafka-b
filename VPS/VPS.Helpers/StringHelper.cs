using VPS.Domain.Models.Enums;

namespace VPS.Helpers
{
    public static class StringHelper
    {
        public static long ConvertStringToLong(string input)
        {
            if (Int64.TryParse(input, out long converted))
            {
                return converted;
            }
            else
            {
                throw new FormatException("Invalid number provided");
            }
        }

        public static string ComposeRedisKey(string redisKey, RedisStoreType redisStore)
        {
            if (redisStore == RedisStoreType.ConcurencyStore)
            {
                return $"{redisKey}_concurrency";
            }
            else if (redisStore == RedisStoreType.DelayStore)
            {
                return $"{redisKey}_delay";
            }
            else if (redisStore == RedisStoreType.InProgressStore)
            {
                return $"{redisKey}_in_progress";
            }
            else if (redisStore == RedisStoreType.IdempotencyStore)
            {
                return $"{redisKey}_idempotency";
            }
            else
            {
                return redisKey;
            }
        }
    }
}
