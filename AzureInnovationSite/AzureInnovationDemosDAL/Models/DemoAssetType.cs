using AzureInnovationDemosDAL.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL.Models
{
    public enum DemoAssetTypeEnum
    {
        ClickThrough = 1,
        LiveDemo = 2,        
        Video = 3,
        AccessKeyToken = 4,
        Link = 5,
        PrivateMD = 6,
        Code = 7
    }
    public class DemoAssetType
    {
        public DemoAssetType()
        {
        }
        private DemoAssetType(DemoAssetTypeEnum @enum)
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

        public static implicit operator DemoAssetType(DemoAssetTypeEnum @enum) => new DemoAssetType(@enum);

        public static implicit operator DemoAssetTypeEnum(DemoAssetType demoResourceType) => (DemoAssetTypeEnum)demoResourceType.Id;
    }
}
