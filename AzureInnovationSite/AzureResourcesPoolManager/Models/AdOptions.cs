using System.ComponentModel.DataAnnotations;

namespace AzureResourcesPoolManager.Models
{
    public class AdOptions
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
        public string GuideContentDB { get; set; }
        [Required]
        public string ModernAppDB { get; set; }
        [Required]
        public string PSRemoteVMDNS { get; set; }
        [Required]
        public string PSRemoteVMPort { get; set; }
        [Required]
        public string PSRemoteVMUser { get; set; }
        [Required]
        public string PSRemoteVMPassword { get; set; }
        [Required]
        public string TradersSiteWebApp { get; set; }
        [Required]
        public string TradersSiteWebServer  { get; set; }
        [Required]
        public bool ExecHeyCommand { get; set; }
        [Required]
        public bool ExecMDConversion { get; set; }
        [Required]
        public string GitHubToken { get; set; }
    }
}
