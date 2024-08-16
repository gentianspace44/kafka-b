namespace VPS.ControlCenter.Core.HollyTopUp
{
    public class User
    {
        public User()
        {
        }

        public int UserID { get; set; }
        public int UserTypeID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }
        public string? UserEmailAddress { get; set; }
        public string UserMobileNumber { get; set; }
        public int? ManagerID { get; set; }
        public bool UserEnabled { get; set; }
        public System.DateTime CreateDatetime { get; set; }
        public System.DateTime? ModifiedDate { get; set; }
        public int? FailedLoginAttempts { get; set; }
        public string IdNumber { get; set; }
        public string? EmployeeCode { get; set; }
        public bool UserChangePassword { get; set; }

    }
}
