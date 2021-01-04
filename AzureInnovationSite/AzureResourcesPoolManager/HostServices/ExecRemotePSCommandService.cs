using AzureInnovationDemosDAL;
using AzureInnovationDemosDAL.Models;
using AzureResourcesPoolManager.Helpers;
using AzureResourcesPoolManager.Models;
using AzureResourcesPoolManager.Utilities;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RemotingFramework.PowerShell;
using RemotingFramework.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureResourcesPoolManager.HostServices
{
    public class ExecRemotePSCommandService : BackgroundService
    {
        readonly Microsoft.Extensions.Logging.ILogger<ExecRemotePSCommandService> logger;
        readonly IConfiguration configuration;
        readonly IHostingEnvironment hostingEnvironment;
        public ExecRemotePSCommandService(Microsoft.Extensions.Logging.ILogger<ExecRemotePSCommandService> logger,
            IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.hostingEnvironment = hostingEnvironment;
        }

        override protected async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await ExecRemotePSCMDs(cancellationToken);
        }

        [NoAutomaticTrigger]
        private async Task ExecRemotePSCMDs(CancellationToken cancellationToken)
        {
            AdOptions adOptions = new AdOptions();
            configuration.Bind("AdTenant", adOptions);

            if (adOptions.ExecHeyCommand)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Console.Out.WriteLine($"Executing Hey command");

                    using (IRemotePowerShell rps = new RemotePowerShell())
                    {
                        rps.Open
                        (
                            RemotePowerShellConnectionInfo.Create
                            (
                                adOptions.PSRemoteVMDNS,
                                ushort.Parse(adOptions.PSRemoteVMPort),
                                adOptions.PSRemoteVMUser,
                                adOptions.PSRemoteVMPassword.ToSecureString()
                            )
                        );

                        rps.ConfigureNonInteractiveConsoleHost();

                        for (int c = 0; c < 10; c++)
                        {
                            await rps.InvokeScriptAsync(@$"hey -z 15m -c 1000 {adOptions.TradersSiteWebApp}");
                            Console.Out.WriteLine($"'Hey' command executed ({c + 1 }) on {adOptions.TradersSiteWebApp}");
                        }
                        for (int c = 0; c < 10; c++)
                        {
                            await rps.InvokeScriptAsync(@$"hey -z 15m -c 1000 {adOptions.TradersSiteWebServer}");
                            Console.Out.WriteLine($"'Hey' command executed ({c + 1}) on {adOptions.TradersSiteWebServer}");
                        }
                    }

                    Console.Out.WriteLine($"Hey command executed");

                    Thread.Sleep(TimeSpan.FromHours(10));
                }
            }
        }
    }
}
