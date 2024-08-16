namespace VPS.Domain.Models.Enums
{
    public enum RedisStoreType
    {
        ConcurencyStore = 0,
        DelayStore = 1,
        InProgressStore = 2,
        IdempotencyStore = 3,
        TokenRefreshStore = 4
    }
}
