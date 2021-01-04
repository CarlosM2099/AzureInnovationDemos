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
    public class FreeExpiredDemoResourcesService : BackgroundService
    {
        readonly Microsoft.Extensions.Logging.ILogger<FreeExpiredDemoResourcesService> logger;
        readonly IConfiguration configuration;
        readonly IHostingEnvironment hostingEnvironment;
        public FreeExpiredDemoResourcesService(Microsoft.Extensions.Logging.ILogger<FreeExpiredDemoResourcesService> logger,
            IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.hostingEnvironment = hostingEnvironment;
        }

        override protected async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await FreeExpiredDemoResources(cancellationToken);
        }
       
        [NoAutomaticTrigger]
        private async Task FreeExpiredDemoResources(CancellationToken cancellationToken)
        {
            IFreeAzureResource freeAzureResource = null;

            while (!cancellationToken.IsCancellationRequested)
            {
                AdOptions adOptions = new AdOptions();
                configuration.Bind("AdTenant", adOptions);

                DbContextOptionsBuilder options = new DbContextOptionsBuilder();
                options.UseSqlServer(configuration.GetConnectionString("AzureDemosDB"));

                var azureDemosDBContext = new AzureDemosDBContext(options.Options);

                AzureDemosDBManager demosDBManager = new AzureDemosDBManager(azureDemosDBContext);

                var expiredResources = await demosDBManager
                    .GetExpiredAzureDemoResources();

                Console.Out.WriteLine($"Cleaning {expiredResources.Count} expired resources");

                foreach (var expiredResource in expiredResources)
                {
                    Demo demo = await demosDBManager.GetDemo(expiredResource.DemoId);
                    DemoTypeEnum demoType = demo.Type;

                    switch (demoType)
                    {
                        case DemoTypeEnum.ADODemo:
                            freeAzureResource = new FreeADODemoAzureResource(adOptions, azureDemosDBContext);
                            break;
                        case DemoTypeEnum.AppModernization:
                            freeAzureResource = new FreeModernDemoAzureResource(adOptions, azureDemosDBContext);
                            break;
                        case DemoTypeEnum.Kubernetes:
                            freeAzureResource = new FreeKubernetesDemoAzureResource(adOptions, azureDemosDBContext);
                            break;
                        case DemoTypeEnum.AutoGlass:
                            freeAzureResource = new FreePowerAppsDemoAzureResource(adOptions, azureDemosDBContext);
                            break;
                        default:
                            freeAzureResource = null;
                            break;
                    }

                    if (freeAzureResource != null)
                    {
                        await freeAzureResource.FreeExpiredResource(expiredResource);
                    }
                }

                Thread.Sleep(TimeSpan.FromMinutes(30));
            }
        }

    }
}
