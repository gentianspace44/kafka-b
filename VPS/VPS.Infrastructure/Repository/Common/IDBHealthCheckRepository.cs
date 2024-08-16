namespace VPS.Infrastructure.Repository.Common
{
    public interface IDBHealthCheckRepository
    {
        Task<Tuple<bool,string>> CheckDBStatusAsync();
    }
}
