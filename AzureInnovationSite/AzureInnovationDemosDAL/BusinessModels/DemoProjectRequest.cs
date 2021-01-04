using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL.BusinessModels
{
    public class DemoProjectRequest
    {
        public int DemoId { get; set; }
        public string DisplayName { get; set; }
        public string UserPrincipalName { get; set; }
        public string TemplateLocation { get; set; }
        public string ProjectName { get; set; }
    }
}
