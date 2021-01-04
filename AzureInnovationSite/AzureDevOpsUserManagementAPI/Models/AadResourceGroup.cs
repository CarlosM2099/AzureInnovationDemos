using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Models
{
    public class AadResourceGroupList
    {
        public List<AadResourceGroup> Value { get; set; }
    }

    public class AadResourceGroup
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }
        public AadResourceGroupProperties Properties { get; set; }
    }

    public class AadResourceGroupProperties
    {
        public string ProvisioningState { get; set; }
    }
}
