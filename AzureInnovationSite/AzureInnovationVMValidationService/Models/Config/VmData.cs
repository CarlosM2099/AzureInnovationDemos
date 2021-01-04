using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationVMValidationService.Models.Config
{
    public class VmData : ConfigurationElement
    {
        [ConfigurationProperty("host", IsRequired = true, IsKey = true)]
        public string HostName
        {
            get { return base["host"] as string; }
            set { base["host"] = value; }
        }

        [ConfigurationProperty("rdpPort", IsRequired = true)]
        public ushort RemoteDesktopPort
        {
            get { return (ushort)base["rdpPort"]; }
            set { base["rdpPort"] = value; }
        }

        [ConfigurationProperty("validDesktopContentText", IsRequired = true)]
        public string ValidDesktopText
        {
            get { return (string)base["validDesktopContentText"]; }
            set { base["validDesktopContentText"] = value; }
        }

        

        public override string ToString()
        {
            return HostName + ":" + RemoteDesktopPort;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data
        /// structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode() ^ GetType().ToString().GetHashCode();
        }
    }
}
