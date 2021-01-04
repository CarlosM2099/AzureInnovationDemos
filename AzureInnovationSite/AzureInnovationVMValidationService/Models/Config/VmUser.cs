using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationVMValidationService.Models.Config
{
    /// <summary>
    /// Represents an "add" entry from the "vmPool" element in app.config.
    /// </summary>
    /// <seealso cref="ConfigurationElement" />
    public class VmUser : ConfigurationElement
    { 

        [ConfigurationProperty("vmUser", IsRequired = true)]
        public string RemoteDesktopUserName
        {
            get { return base["vmUser"] as string; }
            set { base["vmUser"] = value; }
        }

        [ConfigurationProperty("vmPwd", IsRequired = true)]
        public string RemoteDesktopPassword
        {
            get { return base["vmPwd"] as string; }
            set { base["vmPwd"] = value; }
        }
         

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return RemoteDesktopUserName + ":" + RemoteDesktopPassword;
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