using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Models
{
    public class AadUser
    {
        public bool AccountEnabled { get; set; }
        public Guid Id { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string MailNickname { get; set; }
        public string UserPrincipalName { get; set; }
        public string UsageLocation { get; set; }
        public AadUserPassword PasswordProfile { get; set; }
    }

    public class AadUsersList
    {
        public List<AadUser> Value { get; set; }
    }

    public class AadUserPassword
    {
        public bool ForceChangePasswordNextSignIn { get; set; } = false;
        public bool ForceChangePasswordNextSignInWithMfa { get; set; } = false;
        public string Password { get; set; }
    }

    public class AadUserCreation
    {
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string AccountName { get; set; }
        public string Password { get; set; }
    }
}
