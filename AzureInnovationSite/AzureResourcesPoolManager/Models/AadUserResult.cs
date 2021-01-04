using AzureInnovationDemosDAL.BusinessModels;
using System;
using System.Collections.Generic;

namespace AzureResourcesPoolManager.Models
{

    public class AadUserResult
    {
        public List<AadUser> Value { get; set; }
    }

    public class TenantSubscriptionSkuList
    {
        public List<TenantSubscriptionSku> Value { get; set; }
    }

    public class TenantSubscriptionSku
    {
        public string AppliesTo { get; set; }
        public string CapabilityStatus { get; set; }
        public int ConsumedUnits { get; set; }
        public string Id { get; set; }
        public Guid SkuId { get; set; }
        public string SkuPartNumber { get; set; }
        public TenantSubscriptionSkuPrepaidUnits PrepaidUnits { get; set; }
    }

    public class TenantSubscriptionSkuPrepaidUnits
    {
        public int Enabled { get; set; }
        public int Suspended { get; set; }
        public int Warning { get; set; }
    }

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



    public class AadResourceGroupList
    {
        public List<AadResourceGroup> Value { get; set; }
    }

    public class AadResourceGroup
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }
        public AadResourceGroupProperties Properties { get; set; }
    }

    public class AadResourceGroupProperties
    {
        public string ProvisioningState { get; set; }
    }
}
