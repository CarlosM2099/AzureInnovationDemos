using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;

namespace AzureInnovationVMValidationService
{
    public static class Program
    {
        public const string ServiceName = "AzureInnovationVMValSvc";
        public const string ServiceDisplayName = "Azure Innovation VM Validation Service";
        public const string ServiceDescription = "Service for Azure Innovation VM Validation";
        public static int Main(string[] args)
        {
            ThreadPool.SetMinThreads(128, 128);
            ThreadPool.SetMaxThreads(256, 256);

            // Hack to load log4net config from app.config file.
            //log4net.LogManager.GetLogger(typeof(Program));

            var rc = HostFactory.Run(x =>
            {
                //x.UseLog4Net();

                x.Service<ValidationService>();
                x.RunAsNetworkService();

                x.SetServiceName(ServiceName);
                x.SetDisplayName(ServiceDisplayName);
                x.SetDescription(ServiceDescription);
                x.ApplyCommandLine();
            });

            // Set Error Level.
            return (int)Convert.ChangeType(rc, rc.GetTypeCode());
        }
    }
}
