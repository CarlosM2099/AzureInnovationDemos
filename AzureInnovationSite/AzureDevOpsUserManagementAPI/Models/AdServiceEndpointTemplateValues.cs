using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Models
{
    public class AdServiceEndpointTemplateValues
    {
        public string ServicePrincipalId { get; set; }
        public string ServicePrincipalKey { get; set; }
        public string Scope { get; set; }
        public string AzureSpnPermissions { get; set; }
        public string AzureSpnRoleAssignmentId { get; set; }
    }
}
