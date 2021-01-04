using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsUserRolesHandling
{
    public class AadRole
    {

        public Guid Id { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public Guid RoleTemplateId { get; set; }
    }

    public class AadRoleMember
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
    }

    public class AadRoleMemberList
    {
        public List<AadRoleMember> Value { get; set; }

    }

    public class AadRoleList
    {
        public List<AadRole> Value { get; set; }

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
    public class AadUserCreation
    {
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string MailNickname { get; set; }
        public string Password { get; set; }
    }
}
