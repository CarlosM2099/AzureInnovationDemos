using AzureInnovationDemosDAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureInnovationDemos.Models
{
    public class DemoGuideContent
    {

        public int DemoId { get; set; }
        public int DemoAssetId { get; set; }
        public string GuideContent { get; set; }
        public DemoUserEnvironment Environment { get; set; }
        public DemoVM VM { get; internal set; }
        public List<DemoAsset> Assets { get; internal set; }
    }
}
