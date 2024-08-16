namespace VPS.Domain.Models.BluVoucher.Enums
{
    public enum BluVoucherStatus
    {
        Inactive = 1,
        Active,
        BlackListed,
        Expired,
        Redeemed,
        Cancelled,
        Pending,
    }
}
