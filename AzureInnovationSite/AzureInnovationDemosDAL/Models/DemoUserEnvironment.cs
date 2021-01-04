using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL.Models
{
    public class DemoUserEnvironment
    {
        public int Id { get; set; }
        public string EnvironmentUser { get; set; }
        public string EnvironmentPassword { get; set; }
        public string EnvironmentURL { get; set; }
        public string EnvironmentDescription { get; set; }
        public bool EnvironmentProvisioned { get; set; }         
        public DateTime CreatedDate { get; set; }
        public int DemoId { get; set; }
        public Demo Demo { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }        
    }
}
