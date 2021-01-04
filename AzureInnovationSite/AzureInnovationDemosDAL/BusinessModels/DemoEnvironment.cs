using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL.BusinessModels
{
    public class DemoEnvironment
    {
        public string TemplateName { get; set; }
        public string TemplatePath { get; set; }
        public List<DemoEnvironmentUser> Users { get; set; } = new List<DemoEnvironmentUser>();
    }
    public class DemoEnvironmentUser
    {
        public string Email { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string TrackId { get; set; }
        public bool Provisioned { get; set; }
    }
}
