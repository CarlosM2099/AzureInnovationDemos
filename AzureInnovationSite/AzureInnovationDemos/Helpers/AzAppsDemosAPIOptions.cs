using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AzureInnovationDemos.Helpers
{
    public class AzAppsDemosAPIOptions
    {
        [Required]
        public string DemosAppId { get; set; }
        [Required]
        public string DemosAppSecret { get; set; }
        [Required]
        public string DemosAud { get; set; }
        [Required]
        public string DemosAPIURL { get; set; }
        [Required]
        public string DemosGuideDB { get; set; }
        [Required]
        public string PowerAppRoleAzFunctionsURL { get; set; }
    }
}
