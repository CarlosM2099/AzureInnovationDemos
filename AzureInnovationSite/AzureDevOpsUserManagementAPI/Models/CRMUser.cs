using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Models
{
    public class CRMUserList
    {
        public List<CRMUser> Value { get; set; }
    }


    public class CRMUser
    {
        [JsonProperty("systemuserid")]
        public string UserId { get; set; }
        [JsonProperty("firstname")]
        public string FirstName { get; set; }
        [JsonProperty("lastname")]
        public string LastName { get; set; }
        [JsonProperty("fullname")]
        public string FullName { get; set; }
        [JsonProperty("internalemailaddress")]
        public string InternalEmail { get; set; }
        [JsonProperty("domainname")]
        public string DomainName { get; set; }
        [JsonProperty("_businessunitid_value")]
        public string BusinessUnitId { get; set; }
    }

    public class CRMRoleList
    {
        public List<CRMRole> Value { get; set; }
    }


    public class CRMRole
    {
        [JsonProperty("roleid")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("roleidunique")]
        public string UniqueId { get; set; }        
    }

}
