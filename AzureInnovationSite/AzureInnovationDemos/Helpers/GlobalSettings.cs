using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AzureInnovationDemos.Helpers
{
    public class GlobalSettings
    {
        [Required]
        public string SimpleAccountDomains { get; set; }
        [Required]
        public string AlertEmails { get; set; }
        [Required]
        public string StorageAccount { get; set; }
        [Required]
        public string StorageAccountKey { get; set; }
        [Required]
        public string GitHubToken { get; set; }
        [Required]
        public string GithubActionTemplate { get; set; }
    }
}
