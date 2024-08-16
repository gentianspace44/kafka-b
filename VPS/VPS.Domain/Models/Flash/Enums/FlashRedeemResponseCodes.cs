namespace VPS.Domain.Models.Flash.Enums
{
    public enum FlashRedeemResponseCodes
    {
        VoucherRedeemSuccessfull = 0,
        VoucherInvalid = -1,
        VoucherAlreadyRedeemed = 2401,
        VoucherNotFound = 2402,
        VoucherCancelled = 2403,
        VoucherExpired = 2405,
        VoucherDuplicateTransaction = 2201
    }
}
