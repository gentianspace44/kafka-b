namespace VPS.Domain.Models.BluVoucher.Enums
{
    public enum BluVoucherRedeemStatus
    {
        Successful,
        FunctionNotAvailable,
        InvalidSession,
        TechnicalError,
        InvalidInput = 6,
        InvalidVoucher,
        InvalidPin,
    }
}
