using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Models
{
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
