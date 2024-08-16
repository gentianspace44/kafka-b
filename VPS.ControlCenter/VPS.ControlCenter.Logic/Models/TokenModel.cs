namespace VPS.ControlCenter.Logic.Models
{
    public class TokenModel
    {
        public TokenModel()
        {

        }
        public TokenModel(string jwt, DateTime expiryTime)
        {
            Token = jwt;
            ExpiryTime = expiryTime;
        }

        public string Token { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}
