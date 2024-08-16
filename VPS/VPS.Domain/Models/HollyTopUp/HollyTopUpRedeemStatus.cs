namespace VPS.Domain.Models.HollyTopUp
{
    public enum HollyTopUpRedeemStatus
    {
        ErrorInProcess = 0,
        RedeemSuccessful = 1,
        AlreadyRedeemed = 2,
        InvalidVoucher = 3,
        RedeemInProgress = 4,
        Expired = 5,
        Suspended = 6
    }
}
