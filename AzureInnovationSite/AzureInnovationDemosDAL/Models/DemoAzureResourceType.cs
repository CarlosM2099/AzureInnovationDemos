using AzureInnovationDemosDAL.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL.Models
{

    public enum DemoAzureResourceTypeEnum
    {
        WebApp = 1,
        License = 2,
        ResourceGroup = 3
    }

    public class DemoAzureResourceType
    {
        public DemoAzureResourceType()
        {
        }
        private DemoAzureResourceType(DemoAzureResourceTypeEnum @enum)
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

        public static implicit operator DemoAzureResourceType(DemoAzureResourceTypeEnum @enum) => new DemoAzureResourceType(@enum);

        public static implicit operator DemoAzureResourceTypeEnum(DemoAzureResourceType demoResourceType) => (DemoAzureResourceTypeEnum)demoResourceType.Id;
    }
}
