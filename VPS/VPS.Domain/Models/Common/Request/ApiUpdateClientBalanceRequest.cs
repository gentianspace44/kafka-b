namespace VPS.Domain.Models.Common.Request
{
    public class ApiUpdateClientBalanceRequest
    {
        public string? SessionToken { set; get; }

        public long ClientID { get; set; }

        public int ClientTransactionTypeID { get; set; }

        public long? BetID { get; set; }

        public decimal TransactionAmount { get; set; }

        public decimal? PendingBalanceChange { get; set; }

        public decimal? TaxRate { get; set; }

        public int BranchID { get; set; }

        public long UserID { get; set; }

        public bool UpdateClientBalances { get; set; }

        public string? ReferenceComment { get; set; }

    }
}
