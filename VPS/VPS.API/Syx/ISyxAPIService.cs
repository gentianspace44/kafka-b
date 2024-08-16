using VPS.Domain.Models.Common.Response;

namespace VPS.API.Syx
{
    public interface ISyxApiService
    {
        Task<ApiLoginResponse?> Login(string username,string password);
        Task<ApiClientLoginResponse?> LoginClient(string accountNumber, string password);
        Task<ApiClientBalanceUpdateResponse?> UpdateClientBalance(long clientId, int transactionTypeId, decimal transactionAmount, int branchId, string referenceComments);
        Task<ApiVoucherExistsResponse?> CheckVoucherExists(long clientId, string reference);
        Task<bool> HealthCheck();
    }
}
