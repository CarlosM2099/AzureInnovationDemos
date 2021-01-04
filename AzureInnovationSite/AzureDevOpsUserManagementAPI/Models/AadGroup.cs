using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureDevOpsUserManagementAPI.Models
{


    public class AadGroup
    {
        public Guid Id { get; set; }
        public DateTime? DeletedDateTime { get; set; }
        public string Classification { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public string[] CreationOptions { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string[] GroupTypes { get; set; }
        public string Mail { get; set; }
        public bool MailEnabled { get; set; }
        public string MailNickname { get; set; }
        public DateTime? OnPremisesLastSyncDateTime { get; set; }
        public string OnPremisesSecurityIdentifier { get; set; }
        public string OnPremisesSyncEnabled { get; set; }
        public string PreferredDataLocation { get; set; }
        public string[] ProxyAddresses { get; set; }
        public DateTime? RenewedDateTime { get; set; }
        public string[] ResourceBehaviorOptions { get; set; }
        public string[] ResourceProvisioningOptions { get; set; }
        public bool SecurityEnabled { get; set; }
        public string Visibility { get; set; }
        public string[] OnPremisesProvisioningErrors { get; set; }
    }


    public class AadGroupsList
    {
        public List<AadGroup> Value { get; set; }
    }
}