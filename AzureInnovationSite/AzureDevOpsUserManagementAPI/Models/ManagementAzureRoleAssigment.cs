using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureDevOpsUserManagementAPI.Models
{
    public class ManagementAzureRoleAssigmentList
    {
        public List<ManagementAzureRoleAssigment> Value { get; set; }
    }

    public class ManagementAzureRoleAssigment
    {
        public ManagementAzureRoleAssigmentProperties Properties { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
    }

    public class ManagementAzureRoleAssigmentProperties
    {
        public string RoleDefinitionId { get; set; }
        public string PrincipalId { get; set; }
        public string PrincipalType { get; set; }
        public string Scope { get; set; }
        public object Condition { get; set; }
        public object ConditionVersion { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public object DelegatedManagedIdentityResourceId { get; set; }
    }
}