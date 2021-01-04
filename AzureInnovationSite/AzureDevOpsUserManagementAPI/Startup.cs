using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AzureInnovationDemosDAL;
using Microsoft.EntityFrameworkCore;
using AzureDevOpsUserManagementAPI.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AzureDevOpsUserManagementAPI
{
    public class Startup
    {
        public Startup(Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(env.ContentRootPath)
             .AddJsonFile("appsettings.json")
             .AddJsonFile("demogensettings.json")
             .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AzureDemosDBContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("AzureDemosDB"));                
            });

            services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
                .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));
            services.AddControllers();

            services.AddSingleton(op =>
            {
                DemoGenSettings demoGenSettings = new DemoGenSettings();
                Configuration.Bind("DemoGen", demoGenSettings);
                return demoGenSettings;
            });

            services.AddSingleton(op => {
                AdTenantOptions apiOptions = new AdTenantOptions();
                Configuration.Bind("AdTenant", apiOptions);
                return apiOptions;
            });

            services.AddSingleton(op => {
                AdDemosSkus apiOptions = new AdDemosSkus();
                Configuration.Bind("AdDemosSkus", apiOptions);
                return apiOptions;
            });

            services.AddSingleton(op => {
                AzStorageOptions apiOptions = new AzStorageOptions();
                Configuration.Bind("AzStorage", apiOptions);
                return apiOptions;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;                
            }).AddJwtBearer(opts =>
            {
                opts.Audience = Configuration["AzureAd:Audience"];
                opts.Authority = $"https://login.microsoftonline.com/{Configuration["AzureAd:TenantId"]}";
                
            });

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            loggerFactory.AddLog4Net();

            LogManager.CreateRepository("ErrorLog");
        }
    }
}
