using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureDevOpsUserManagementAPI.Models
{
    public class UserEntitlements
    {
        public int TotalCount { get; set; }
        public List<UserEntitlement> Items { get; set; }
        public List<UserEntitlement> Members { get; set; }
    }

    public class UserEntitlement
    {
        public string Id { get; set; }
        public GraphUser User { get; set; }
        public Accesslevel AccessLevel { get; set; }
        public DateTime LastAccessedDate { get; set; }
        public dynamic[] ProjectEntitlements { get; set; }
        public dynamic[] Extensions { get; set; }
        public dynamic[] GroupAssignments { get; set; }
    }

    public class Accesslevel
    {
        public string LicensingSource { get; set; }
        public string AccountLicenseType { get; set; }
        public string MsdnLicenseType { get; set; }
        public string LicenseDisplayName { get; set; }
        public string Status { get; set; }
        public string StatusMessage { get; set; }
        public string AssignmentSource { get; set; }
    }
    public class OrganizationAcquisition
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class OrganizationAcquisitionNameAvailability
    {
        public bool IsAvailable { get; set; }
        public string Name { get; set; }
        public string UnavailabilityReason { get; set; }
    }


    public class InstallExtensionResult
    {
        public string ExtensionId { get; set; }
        public string ExtensionName { get; set; }
        public string PublisherId { get; set; }
        public string PublisherName { get; set; }
        public string Version { get; set; }
        public Installstate InstallState { get; set; }
        public DateTime LastPublished { get; set; }
    }

    public class Installstate
    {
        public string Flags { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class ExtensionAcquisition
    {
        public string ItemId { get; set; }
        public string OperationType { get; set; }
        public object BillingId { get; set; }
        public int Quantity { get; set; }
        public string AssignmentType { get; set; }
        public ExtensionAcquisitionProperties Properties { get; set; }
    }

    public class ExtensionAcquisitionProperties
    {
    }

}