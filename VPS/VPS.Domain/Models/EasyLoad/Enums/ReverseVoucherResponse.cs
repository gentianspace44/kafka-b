namespace VPS.Domain.Models.EasyLoad.Enums
{
    /// <summary>
    ///     30 Invalid Customer(Redeem Customer & Reverse Customer must be the same)
    ///     31 Reversal Period Expired(Default 5 minutes)
    ///     32 Voucher Not Found
    ///     33 Update Error
    ///     34 Unknown Error(Error in Message)
    ///     35 Database Error
    /// </summary>
    public enum ReverseVoucherResponse
    {
        InvalidCustomer = 30,
        ReversalExpired = 31,
        VoucherNotFound = 32,
        UpdateError = 33,
        UnknownError = 34,
        DatabaseError = 35

    }
}
