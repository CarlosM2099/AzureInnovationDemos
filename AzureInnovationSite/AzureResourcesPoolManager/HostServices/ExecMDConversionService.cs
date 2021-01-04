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
    public class ExecMDConversionService : BackgroundService
    {
        readonly Microsoft.Extensions.Logging.ILogger<ExecMDConversionService> logger;
        readonly IConfiguration configuration;
        readonly IHostingEnvironment hostingEnvironment;
        public ExecMDConversionService(Microsoft.Extensions.Logging.ILogger<ExecMDConversionService> logger,
            IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.hostingEnvironment = hostingEnvironment;
        }

        override protected async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await ExecMDConversion(cancellationToken);
        }

        
        [NoAutomaticTrigger]
        private async Task ExecMDConversion(CancellationToken cancellationToken)
        {
            AdOptions adOptions = new AdOptions();
            configuration.Bind("AdTenant", adOptions);

            if (adOptions.ExecMDConversion)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Console.Out.WriteLine($"Executing MD conversion");                   

                    DbContextOptionsBuilder options = new DbContextOptionsBuilder();
                    options.UseSqlServer(configuration.GetConnectionString("AzureDemosDB"));

                    var azureDemosDBContext = new AzureDemosDBContext(options.Options);

                    MDConverter converter = new MDConverter(azureDemosDBContext, adOptions);

                    Console.Out.WriteLine($"Converting guides");

                    await converter.ConvertMDGuides();

                    Thread.Sleep(12 * 3600000);
                }
            }
        }
    }
}
