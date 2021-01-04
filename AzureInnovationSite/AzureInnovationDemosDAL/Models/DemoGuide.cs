using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL.Models
{
    public class DemoGuide
    {
        [Key]
        public int DemoAssetId { get; set; }

        public string GuideContent { get; set; }

        [ForeignKey("DemoAssetId")]
        public DemoAsset DemoAsset { get; set; }
    }
}
