using AzureInnovationDemosDAL.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL.Models
{
    public enum DemoTypeEnum
    {
        AutoGlass = 1,
        ADODemo = 2,
        AppModernization = 3,
        Kubernetes = 4
    }
    public class DemoType
    {
        public DemoType()
        {
        }
        private DemoType(DemoTypeEnum @enum)
        {
            Id = (int)@enum;
            Name = @enum.ToString();
            Description = @enum.GetEnumDescription();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public static implicit operator DemoType(DemoTypeEnum @enum) => new DemoType(@enum);

        public static implicit operator DemoTypeEnum(DemoType demoResourceType) => (DemoTypeEnum)demoResourceType.Id;
    }
}
