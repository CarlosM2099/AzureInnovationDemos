using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL.Models
{
    public class DemoVM
    {
        public int Id { get; set; }
        public int DemoId { get; set; }
        public string URL { get; set; }
        public Demo Demo { get; set; }
    }
}
