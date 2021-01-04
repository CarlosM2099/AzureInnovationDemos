using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureDevOpsUserManagementAPI.Models
{

    public class DemoOrganizationResult
    {
        public string Name { get; set; }        
    }

    public class ADODemoGenResult
    {
        public string TemplateName { get; set; }
        public string TemplatePath { get; set; }
        public List<ADODemoGenResultUser> Users { get; set; }
    }
    public class ADODemoGenResultUser
    {
        public string Email { get; set; }
        public string ProjectName { get; set; }
        public string ProjectUrl { get; set; }
        public string TrackId { get; set; }
        public string Status { get; set; }
    }
}