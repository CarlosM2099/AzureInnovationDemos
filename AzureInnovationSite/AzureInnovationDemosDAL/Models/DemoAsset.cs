using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL.Models
{   
    public class DemoAsset
    {
        public int Id { get; set; }
        public int DemoId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Alias { get; set; }
        public DemoAssetType Type { get; set; }        
        public Demo Demo { get; set; }
    }
}
