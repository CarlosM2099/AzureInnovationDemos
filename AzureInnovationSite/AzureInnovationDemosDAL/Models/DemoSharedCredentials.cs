using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL.Models
{
    public class DemoSharedCredentials
    {
        public int Id { get; set; }
        public int DemoId { get; set; }
        public string DemoUser { get; set; }
        public string DemoPassword { get; set; }
        public string DemoURL { get; set; }
        public Demo Demo { get; set; }
    }
}
