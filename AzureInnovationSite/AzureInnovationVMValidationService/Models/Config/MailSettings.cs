using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationVMValidationService.Models.Config
{
    public class MailSettings : ConfigurationElement
    {
        [ConfigurationProperty("smtpServer", IsRequired = true, IsKey = true)]
        public string SmtpServer
        {
            get { return base["smtpServer"] as string; }
            set { base["smtpServer"] = value; }
        }

        [ConfigurationProperty("useSsl", IsRequired = true)]
        public bool UseSsl
        {
            get { return (bool)base["useSsl"]; }
            set { base["useSsl"] = value; }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public ushort Port
        {
            get { return (ushort)base["port"]; }
            set { base["port"] = value; }
        }

        [ConfigurationProperty("userName", IsRequired = true)]
        public string UserName
        {
            get { return base["userName"] as string; }
            set { base["userName"] = value; }
        }

        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get { return base["password"] as string; }
            set { base["password"] = value; }
        }

        [ConfigurationProperty("from", IsRequired = true)]
        public string FromAddressRaw
        {
            get { return base["from"] as string; }
            set { base["from"] = value; }
        }

        [ConfigurationProperty("to", IsRequired = true)]
        public string ToAddressesRaw
        {
            get { return base["to"] as string; }
            set { base["to"] = value; }
        }

        [ConfigurationProperty("outageTimeout", IsRequired = true)]
        public TimeSpan OutageTimeout
        {
            get { return (TimeSpan)base["outageTimeout"]; }
            set { base["outageTimeout"] = value; }
        }

        [ConfigurationProperty("repeatEmailTimeout", IsRequired = true)]
        public TimeSpan RepeatEmailTimeout
        {
            get { return (TimeSpan)base["repeatEmailTimeout"]; }
            set { base["repeatEmailTimeout"] = value; }
        }

        public MailAddress FromAddress => new MailAddress(FromAddressRaw);

        public IEnumerable<MailAddress> ToAddresses
        {
            get
            {
                var addys = ToAddressesRaw?.Split(";,|\t\r\n\v".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                if (addys != null)
                {
                    foreach (var addy in addys)
                    {
                        var a = addy?.Trim();
                        if (!string.IsNullOrEmpty(a) && !a.StartsWith("#"))
                        {
                            yield return new MailAddress(a);
                        }
                    }
                }
            }
        }
    }
}
