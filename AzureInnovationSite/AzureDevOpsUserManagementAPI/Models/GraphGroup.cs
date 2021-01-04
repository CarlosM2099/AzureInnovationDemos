using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Models
{
    public class GrapHref
    {
        public string Href { get; set; }
    }

    public class GraphLink
    {
        public GrapHref Avatar { get; set; }
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
}
