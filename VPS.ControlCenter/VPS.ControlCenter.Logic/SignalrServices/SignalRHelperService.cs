using VPS.ControlCenter.Logic.Models;
using VPS.ControlCenter.Logic.RedisServices;

namespace VPS.ControlCenter.Logic.SignalrServices
{
    public class SignalRHelperService:ISignalRHelperService
    {
        private static readonly Dictionary<long, string> userConnectionIds = new Dictionary<long, string>();

        private IRedisRepository _redis;

        public SignalRHelperService(IRedisRepository redis)
        {
            _redis = redis;
        }

        public void AddUserConnection(string userId, string connectionId)
        {
            _redis.Add(userId, new SignalRConnectionModel(connectionId));
        }

        public async Task<string> GetUserConnectionId(string userId)
        {
            var redisEntry = await _redis.Get(userId);
            var connection = Newtonsoft.Json.JsonConvert.DeserializeObject<SignalRConnectionModel>(redisEntry.ToString());
            return connection?.ConnectionId;
        }
    }
}
