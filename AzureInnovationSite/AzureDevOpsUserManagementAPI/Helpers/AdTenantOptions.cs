using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AzureDevOpsUserManagementAPI.Helpers
{
    public class AdTenantOptions
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string UserPassword { get; set; }
        [Required]
        public string AccountName { get; set; }
        [Required]
        public string TenantDomain { get; set; }
        [Required]
        public string TenantId { get; set; }
        [Required]
        public string AppClientId { get; set; }
        [Required]
        public string DemoGenAPI { get; set; }
        [Required]
        public string DemoGenTemplate { get; set; }
        [Required]
        public string DemoGenADOToken { get; set; }
        [Required]
        public string DemoADOVMSecGroup { get; set; }
        [Required]
        public string SubscriptionId { get; set; }
        [Required]
        public string SubscriptionName { get; set; }
        [Required]
        public string StorageAccount { get; set; }
        [Required]
        public string StorageAccountKey { get; set; }
        [Required]
        public string CRMResource { get; set; }
    }
}