using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Models
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
}
