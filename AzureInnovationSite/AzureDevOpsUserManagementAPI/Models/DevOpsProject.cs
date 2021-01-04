using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Models
{
    public class DevOpsProject
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string State { get; set; }
        public int Revision { get; set; }
        public string Visibility { get; set; }
        public string LastUpdateTime { get; set; }
    }

    public class DevOpsProjectsList
    {
        public List<DevOpsProject> Value { get; set; }
    } 
}
