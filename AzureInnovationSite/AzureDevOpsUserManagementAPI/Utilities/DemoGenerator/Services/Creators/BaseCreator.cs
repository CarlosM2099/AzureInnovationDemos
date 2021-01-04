using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsAPI.Viewmodel.Extractor;

namespace AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Creators
{
   
    public class BaseCreator
    {
        public static ILog Logger = LogManager.GetLogger("ErrorLog", "ErrorLog");
        internal readonly ExtractorProjectSettings setting;
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Results { get; set; } = new List<string>();
        public BaseCreator(ExtractorProjectSettings setting)
        {
            this.setting = setting;
        }        
    }
}
