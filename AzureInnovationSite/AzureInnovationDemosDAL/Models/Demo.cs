using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL.Models
{
    public class Demo
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Categories { get; set; }
        public string Description { get; set; }
        public string Abstract { get; set; }
        public string Technologies { get; set; }
        public string Additional { get; set; }
        public bool IsSharedEnvironment { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsVisible { get; set; }
        public DemoType Type { get; set; }
        public ICollection<DemoAsset> Assets { get; set; }
        public ICollection<DemoAzureResource> AzureResources { get; set; }
        public ICollection<DemoVM> VMs { get; set; }
        public ICollection<DemoSharedCredentials> SharedCredentials { get; set; }
    }
}
