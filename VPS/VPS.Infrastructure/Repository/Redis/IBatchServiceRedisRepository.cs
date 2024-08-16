using StackExchange.Redis;
using VPS.Domain.Models.Enums;

namespace VPS.Infrastructure.Repository.Redis
{
    public interface IBatchServiceRedisRepository
    {
        Task<RedisValue> Delete(string key, VoucherType voucherType);
    }
}
