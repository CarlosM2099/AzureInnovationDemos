using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL.Models
{
    public class DemoUserResource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public DateTime CreatedDate { get; set; }
        public DemoAssetType Type { get; set; }
        public int DemoId { get; set; }
        public Demo Demo { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
