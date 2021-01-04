using AzureInnovationsValidationService.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureInnovationsValidationService.HostServices
{
    public class PingAzFuncsService : BackgroundService
    {
        readonly Microsoft.Extensions.Logging.ILogger<PingAzFuncsService> logger;
        readonly IConfiguration configuration;
        readonly IHostingEnvironment hostingEnvironment;
        public PingAzFuncsService(Microsoft.Extensions.Logging.ILogger<PingAzFuncsService> logger,
            IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.hostingEnvironment = hostingEnvironment;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return ExecPingAzFuncs(stoppingToken);
        }


        [NoAutomaticTrigger]
        private async Task ExecPingAzFuncs(CancellationToken cancellationToken)
        {
            SvcOptions adOptions = new SvcOptions();
            configuration.Bind("SvcOptions", adOptions);

            Console.Out.WriteLine($"Starting  ExecPingAzFuncs service");

            while (!cancellationToken.IsCancellationRequested)
            {
                using HttpClient httpClient = new HttpClient();

                Console.Out.WriteLine($"Calling  WindshieldAvailability az function");

                var result = await httpClient.GetAsync("https://autoglassfunctions.azurewebsites.net/api/WindshieldAvailability?argicnumber=000");
                var strResult = await result.Content.ReadAsStringAsync();
                Console.Out.WriteLine($"{strResult}");

                Console.Out.WriteLine($"Calling  CallOCRLogicApp az function");

                result = await httpClient.GetAsync("https://autoglassfunctions.azurewebsites.net/api/CallOCRLogicApp?imageURL=https://autoglassdemostorage.blob.core.windows.net/autoglassfiles/mylicenseplate.jpg");
                strResult = await result.Content.ReadAsStringAsync();
                Console.Out.WriteLine($"{strResult}");

                Thread.Sleep(TimeSpan.FromHours(4));
            }
        }
    }
}
