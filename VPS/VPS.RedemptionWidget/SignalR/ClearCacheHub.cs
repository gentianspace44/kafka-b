using Microsoft.AspNetCore.SignalR;
namespace VPS.RedemptionWidget.SignalR
{
    public class ClearCacheHub : Hub
    {

        public Task ClearCache(string message)
        {
            return Clients.All.SendAsync("ClearCache",message);
        }
    }
}
