using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationVMValidationService.Models.Config
{
    public class ServiceSettings : ConfigurationElement
    {
        [ConfigurationProperty("storageAccount", IsRequired = true)]
        public string StorageAccount
        {
            get { return base["storageAccount"] as string; }
            set { base["storageAccount"] = value; }
        }

        [ConfigurationProperty("exeName", IsRequired = true)]
        public string ExeName
        {
            get { return base["exeName"] as string; }
            set { base["exeName"] = value; }
        }

        [ConfigurationProperty("storageAccountKey", IsRequired = true)]
        public string StorageAccountKey
        {
            get { return base["storageAccountKey"] as string; }
            set { base["storageAccountKey"] = value; }
        }
         
    }
}