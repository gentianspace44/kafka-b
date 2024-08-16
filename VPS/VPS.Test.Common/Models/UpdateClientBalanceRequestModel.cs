namespace VPS.Test.Common.Models
{
    public class UpdateClientBalanceRequestModel
    {
        public string Platform { get; set; } = string.Empty;
        public string EligibleVoucherName { get; set; } = string.Empty;
        public long VoucherId { get; set; }
        public long UserId { get; set; }
        public string SessionToken { get; set; } = string.Empty;
        public long ClientId { get; set; }
        public int TransactionTypeId { get; set; }
        public decimal TransactionAmount { get; set; }
        public int BranchId { get; set; }
        public string ReferenceComments { get; set; } = string.Empty;
    }
}
