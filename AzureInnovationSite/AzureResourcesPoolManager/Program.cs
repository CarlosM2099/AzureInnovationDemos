using AzureInnovationDemosDAL;
using AzureResourcesPoolManager.HostServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AzureResourcesPoolManager
{
    public class Program
    {       
        private static async Task Main(string[] args)
        {
            var builder = new HostBuilder();
            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddAzureStorage();
                b.AddTimers();
            })
            .ConfigureAppConfiguration(b =>
            {                
                b.AddCommandLine(args);
            })
            .ConfigureLogging((context, b) =>
            {
                // here we can access context.HostingEnvironment.IsDevelopment() yet
                if (context.Configuration["environment"] == EnvironmentName.Development)
                {
                    b.SetMinimumLevel(LogLevel.Debug);
                    b.AddConsole();
                }
                else
                {
                    b.SetMinimumLevel(LogLevel.Information);
                }                
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(context.Configuration);
                services.AddDbContext<AzureDemosDBContext>(options =>
                {
                    options.UseSqlServer(context.Configuration.GetConnectionString("AzureDemosDB"));
                });
                
                services.AddLogging();
                services.AddHostedService<FreeExpiredDemoResourcesService>();               
                services.AddHostedService<ExecRemotePSCommandService>();
            })
            .UseConsoleLifetime();

            var host = builder.Build();
            
            using (host)
            {               
                await host.RunAsync();
            }
        }
    }
}
