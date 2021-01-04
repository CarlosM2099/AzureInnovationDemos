using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureDevOpsUserManagementAPI.Models
{
    public class ManagementAzureResourceList
    {
        public List<ManagementAzureResource> Value { get; set; }
    }

    public class ManagementAzureResource
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Kind { get; set; }
        public string Location { get; set; }
        public ManagementAzureResourceTags Tags { get; set; }
    }

    public class ManagementAzureResourceTags
    {
    }

}