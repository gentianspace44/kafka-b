using Microsoft.AspNetCore.SignalR;
using VPS.ControlCenter.Logic.SignalrServices;

namespace VPS.ControlCenter.Api.Hubs
{
    public class NotificationsHub : Hub
    {
        private SignalRHelperService _signalrHelperService;
        private readonly ILogger<NotificationsHub> _logger;
        public NotificationsHub(SignalRHelperService signalrHelperService, ILogger<NotificationsHub> logger)
        {
            _signalrHelperService = signalrHelperService;
            _logger = logger;
        }
        public Task Notify(string clientConnectionId, string message)
        {
            _logger.LogInformation("Notify.. client connection Id: {clientConnectionId}, message: {message}", args: new object[] { clientConnectionId, message });
            return Clients.Client(clientConnectionId).SendAsync("Notify", message);
        }

        public void SetUserId(string userId, string connectionId)
        {
            _logger.LogInformation("Set User ID. userId: {userId}, connectionId: {connectionId}", args: new object[] { userId, connectionId });
            _signalrHelperService.AddUserConnection(userId, connectionId);
        }

        public async override Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            _logger.LogInformation("Connection Established. connectionId: {connectionId}", args: new object[] { connectionId });
            // Trigger the custom "connected" event and pass the connection ID
            await Clients.Client(connectionId).SendAsync("connected", connectionId);
            await base.OnConnectedAsync();
        }


    }
}
