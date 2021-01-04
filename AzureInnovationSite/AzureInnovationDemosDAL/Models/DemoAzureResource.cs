using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL.Models
{
    public class DemoAzureResource
    {
        public int Id { get; set; }
        public int DemoId { get; set; }
        public DemoAzureResourceType Type { get; set; }
     
        public string Description { get; set; }        
        public string Value { get; set; }
        public int AttemptCount { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;        
        public DateTime? LockedUntil { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
        public Demo Demo { get; set; }
    }
}
