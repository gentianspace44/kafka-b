namespace VPS.Domain.Models.HollaMobile
{
    public enum HollaMobileRedeemStatus
    {
        ErrorInProcess = 0,
        RedeemSuccessful = 1,
        AlreadyRedeemed = 2,
        InvalidAirtime = 3,
        RedeemInProgress = 4,
        Expired = 5,
        Suspended = 6
    }
}
