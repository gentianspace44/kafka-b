namespace VPS.Domain.Models.Common
{
    public class ApiClient
    {

        public long ClientID { get; set; }

        public System.Guid ClientGuid { get; set; }

        public int FK_ClientTitleID { get; set; }

        public string? FirstName { get; set; }

        public string? ClientPassword { get; set; }

        public string? ClientPin { get; set; }

        public string? LastName { get; set; }

        public string? Initials { get; set; }

        public System.DateTime DateOfBirth { get; set; }

        public string? CellPhone { get; set; }

        public string? Email { get; set; }

        public int? FK_CountryID { get; set; }

        public string? PassportNumber { get; set; }

        public int FK_IdentityTypeID { get; set; }

        public string? IdentityTypeValue { get; set; }

        public decimal? CreditLimit { get; set; }

        public decimal CurrentStake { get; set; }

        public decimal BalanceAvailable { get; set; }

        public decimal BalancePending { get; set; }

        public int FK_ClientTypeID { get; set; }

        public int FK_AccountTypeID { get; set; }

        public bool ClientFICACompliant { get; set; }

        public int FK_CompanyID { get; set; }

        public bool? ChangePasswordAtNextLogin { get; set; }

        public int FailedLogins { get; set; }

        public int FK_ClientStatusID { get; set; }

        public int? FK_BranchID { get; set; }

        public int? FK_CountryOfOrigin { get; set; }

        public bool? NotifyOnBetStrike { get; set; }

        public decimal? MaximumLiabilitySingleBets { get; set; }

        public decimal? MaximumLiabilityMultipleBets { get; set; }

        public decimal? MaximumLiability { get; set; }

        public string? Gender { get; set; }

        public decimal? DepositLimit { get; set; }

        public int? PreferredContactID { get; set; }
    }
}
