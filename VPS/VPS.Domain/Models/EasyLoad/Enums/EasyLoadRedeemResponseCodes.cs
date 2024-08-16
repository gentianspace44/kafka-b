namespace VPS.Domain.Models.EasyLoad.Enums
{
    public enum EasyLoadRedeemResponseCodes
    {
        VoucherRedeemSuccessful = 0,
        InvalidVoucher = 20,
        VoucherAlreadyRedeemed = 21,
        VoucherExpired = 22,
        VoucherNotActive = 23
    }
}
