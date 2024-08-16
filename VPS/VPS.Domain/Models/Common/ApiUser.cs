namespace VPS.Domain.Models.Common
{
    public class ApiUser
    {
        public long UserID { set; get; }

        public System.Guid UserGuid { set; get; }

        public string? UserName { set; get; }

        public string? FirstName { set; get; }

        public string? LastName { set; get; }

        public bool ChangePassword { set; get; }

        public System.DateTime? LastPasswordChange { set; get; }

        public bool Enabled { set; get; }

        public int FailedLogins { set; get; }

        public string? EmailAddress { set; get; }

        public string? IdentityTypeValue { set; get; }

        public int FK_IdentityTypeID { set; get; }

        public string? GBLicense { set; get; }

        public int FK_CompanyID { set; get; }

        public string? MessagingToken { set; get; }

        public byte FK_HoldingCompanyID { set; get; }

        public int FK_RoleID { set; get; }

        public int FK_BranchID { set; get; }

        public bool? IsInPlayChecked { set; get; }

    }
}
