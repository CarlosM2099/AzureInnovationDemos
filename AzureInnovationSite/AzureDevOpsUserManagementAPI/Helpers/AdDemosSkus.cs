using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Helpers
{
    public class AdDemosSkus
    {
        [Required]
        public string PowerAppsLicense { get; set; } = "POWERAPPS_PER_USER";
        [Required]
        public string EnterpriseLicense { get; set; } = "ENTERPRISEPACK";
        [Required]
        public string Win10License { get; set; } = "Win10_VDA_E3";
    }
}
