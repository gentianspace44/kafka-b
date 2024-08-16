namespace VPS.ControlCenter.Logic.SignalrServices
{
    internal interface ISignalRHelperService
    {
        void AddUserConnection(string userId, string connectionId);
        Task<string> GetUserConnectionId(string userId);
    }
}
