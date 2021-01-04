using System;
using System.Collections.Generic;
using System.Text;

namespace AzureInnovationDemosDAL.BusinessModels
{
    public class ResourceValidation
    {
        public bool AvailableResources { get; set; }
        public int AvailableResourcesCount { get; set; }
        public string NextAvailable { get; set; }
    }
}
