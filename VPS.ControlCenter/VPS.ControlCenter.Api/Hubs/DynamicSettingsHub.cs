using Microsoft.AspNetCore.SignalR;

namespace VPS.ControlCenter.Api.Hubs
{
    public class DynamicSettingsHub:Hub
    {
        public Task ClearCache(string message)
        {
            return Clients.All.SendAsync("ClearCache", message);
        }
    }
}
