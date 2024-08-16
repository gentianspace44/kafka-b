using Microsoft.AspNetCore.SignalR;

namespace VPS.ControlCenter.Api.Hubs
{
    public class FeatureToggleHub:Hub
    {
        public Task UpdateFeature(string featureToggleName, string toggleValue)
        {
            return Clients.All.SendAsync("UpdateFeature", featureToggleName, toggleValue);
        }
    }
}
