using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationVMValidationService.Models.Config
{
    public class AzureInnovationVMValidationServiceCfg : ConfigurationSection
    {
        public static Lazy<AzureInnovationVMValidationServiceCfg> Config { get; } = new Lazy<AzureInnovationVMValidationServiceCfg>
        (
            () =>
            {
                var o = ConfigurationManager.GetSection("azVMValService") as AzureInnovationVMValidationServiceCfg;
                return o ?? new AzureInnovationVMValidationServiceCfg();
            },
            true
        );

        [ConfigurationProperty("vmUsers")]
        [ConfigurationCollection(typeof(VmUserCollection), AddItemName = "add")]
        public VmUserCollection VmUsers
        {
            get
            {
                object o = this["vmUsers"];
                return o as VmUserCollection;
            }
        }

        [ConfigurationProperty("vmData")]
        public VmData VmData
        {
            get
            {
                object o = this["vmData"];
                return o as VmData;
            }
        }

        [ConfigurationProperty("mailSettings")]
        public MailSettings MailSettings
        {
            get
            {
                object o = this["mailSettings"];
                return o as MailSettings;
            }
        }

        [ConfigurationProperty("svcSettings")]
        public ServiceSettings ServiceSettings
        {
            get
            {
                object o = this["svcSettings"];
                return o as ServiceSettings;
            }
        }
    }
}
