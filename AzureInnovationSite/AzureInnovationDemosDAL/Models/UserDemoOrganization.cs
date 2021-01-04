using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureInnovationDemosDAL.Models
{
    public class UserDemoOrganization
    {
        [Key, ForeignKey("User")]
        public int Id { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }
    }
}
