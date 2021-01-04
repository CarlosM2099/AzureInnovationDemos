using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL.Models
{
   public class UserDemoAzureResource
    {
        [Column(Order = 0)]
        public int UserId { get; set; }

        [Column(Order = 1)]
        public int DemoAzureResourceId { get; set; }
       
        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("DemoAzureResourceId")]
        public DemoAzureResource DemoAzureResource { get; set; }
    }
}
