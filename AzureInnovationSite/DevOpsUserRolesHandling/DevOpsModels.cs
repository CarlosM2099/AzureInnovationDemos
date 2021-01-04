using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsUserRolesHandling
{
    public class DevOpsProjectsList
    {
        public List<DevOpsProject> Value { get; set; }
    }

    public class DevOpsProject
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string State { get; set; }
        public int Revision { get; set; }
        public string Visibility { get; set; }
        public string LastUpdateTime { get; set; }
    }

    public class DevOpsTeamsList
    {
        public List<DevOpsTeam> Value { get; set; }
    }

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

    public class GrapHref
    {
        public string Href { get; set; }
    }

    public class GraphLink
    {
        public GrapHref Self { get; set; }
        public GrapHref Memberships { get; set; }
        public GrapHref MembershipState { get; set; }
        public GrapHref StorageKey { get; set; }
    }
    public class GraphGroup
    {
        public string SubjectKind { get; set; }
        public string Description { get; set; }
        public string Domain { get; set; }
        public string PrincipalName { get; set; }
        public string MailAddress { get; set; }
        public string Origin { get; set; }
        public Guid OriginId { get; set; }
        public string DisplayName { get; set; }

        [JsonProperty("_links")]
        public GraphLink Links { get; set; }
        public string Url { get; set; }
        public string Descriptor { get; set; }
    }

    public class GraphGroupsList
    {
        public List<GraphGroup> Value { get; set; }
    }
    public class GraphUsersList
    {
        public List<GraphUser> Value { get; set; }
    }


    public class GraphMemberShipList
    {
        public List<GraphMemberShip> Value { get; set; }
    }

    public class GraphMemberShip
    {
        public string ContainerDescriptor { get; set; }
        public string MemberDescriptor { get; set; }
    }
    public class GraphUser
    {
        public string SubjectKind { get; set; }
        public string Description { get; set; }
        public string Domain { get; set; }
        public string PrincipalName { get; set; }
        public string MailAddress { get; set; }
        public string Origin { get; set; }
        public Guid OriginId { get; set; }
        public string DisplayName { get; set; }

        [JsonProperty("_links")]
        public GraphLink Links { get; set; }
        public string Url { get; set; }
        public string Descriptor { get; set; }
    }

    public class IdentityPickerQuery
    {
        public List<IdentityPickerResult> Results { get; set; }
    }
    public class IdentityPickerResult
    {
        public List<IdentityUser> Identities { get; set; }
        public string QueryToken { get; set; }
    }
    public class IdentityUser
    {
        public Guid? LocalId { get; set; }
        public Guid OriginId { get; set; }
        public string EntityId { get; set; }
        public string EntityType { get; set; }
        public string OriginDirectory { get; set; }
        public string LocalDirectory { get; set; }
        public string DisplayName { get; set; }
        public string ScopeName { get; set; }
        public string SamAccountName { get; set; }
        public string Active { get; set; }
        public string SubjectDescriptor { get; set; }
        public string Department { get; set; }
        public string JobTitle { get; set; }
        public string Mail { get; set; }
        public string MailNickname { get; set; }
        public string PhysicalDeliveryOfficeName { get; set; }
        public string SignInAddress { get; set; }
        public string Surname { get; set; }
        public bool Guest { get; set; }
        public bool IsMru { get; set; }
        public string TelephoneNumber { get; set; }
        public string Description { get; set; }
    }
}
