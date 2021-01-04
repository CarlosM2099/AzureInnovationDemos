using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 

namespace AzureInnovationDemosDAL.Models
{
    public class UserRDPLog
    {         
        [Key, Column(Order = 0)]
        public int UserId { get; set; }

        public string UserAccount { get; set; }

        public int DemoId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
        
        public Demo Demo { get; set; }
    }
}
