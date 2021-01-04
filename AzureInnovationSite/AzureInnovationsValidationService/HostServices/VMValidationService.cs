using AzureInnovationsValidationService.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureInnovationsValidationService.HostServices
{
    public class VMValidationService : BackgroundService
    {
        readonly Microsoft.Extensions.Logging.ILogger<VMValidationService> logger;
        readonly IConfiguration configuration;
        readonly IHostingEnvironment hostingEnvironment;

        public VMValidationService(Microsoft.Extensions.Logging.ILogger<VMValidationService> logger,
            IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.hostingEnvironment = hostingEnvironment;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return ExecVMValidation(stoppingToken);
        }


        [NoAutomaticTrigger]
        private async Task ExecVMValidation(CancellationToken cancellationToken)
        {
            SvcOptions adOptions = new SvcOptions();
            configuration.Bind("SvcOptions", adOptions);

            Console.Out.WriteLine($"Starting  VMValidation service");

            while (!cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(12 * 3600000);
            }
        }
    }
}
