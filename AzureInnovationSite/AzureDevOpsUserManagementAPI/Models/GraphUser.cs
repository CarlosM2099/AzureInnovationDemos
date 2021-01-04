using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Models
{
    public class GraphUser
    {
        public string SirectoryAlia { get; set; }
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
}
