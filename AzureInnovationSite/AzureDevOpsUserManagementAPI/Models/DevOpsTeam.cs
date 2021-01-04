using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Models
{
    public class DevOpsTeam
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string IdentityUrl { get; set; }
        public string ProjectName { get; set; }
        public Guid ProjectId { get; set; }
    }

    public class DevOpsTeamsList
    {
        public List<DevOpsTeam> Value { get; set; }
    }

    public class DevOpsTeamMemberList
    {
        public List<DevOpsTeamMember> Value { get; set; }
    }

    public class DevOpsTeamMember
    {
        public bool IsTeamAdmin { get; set; }
        public DevOpsTeamMemberIdentity Identity { get; set; }
    }

    public class DevOpsTeamMemberIdentity
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string UniqueName { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
    }
}
