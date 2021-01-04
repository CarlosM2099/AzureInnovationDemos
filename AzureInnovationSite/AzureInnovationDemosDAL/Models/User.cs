using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Givenname { get; set; }
        public string Surname { get; set; }
        public string AccountName { get; set; }
        public string Password { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsVMAdmin { get; set; }
        public DateTime LastLoggin { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual UserDemoOrganization DemoOrganization { get; set; }
        
    }
}
