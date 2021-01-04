using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AzureInnovationDemos.Extensions;
using AzureInnovationDemos.Helpers;
using AzureInnovationDemos.Utilities;
using AzureInnovationDemosDAL;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AzureInnovationDemos
{
    public class Startup
    {
        AzureDemosDBManager demosManager;
        AzureDemosDBContext dBContext;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AzureDemosDBContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("AzureDemosDB"));
                dBContext = new AzureDemosDBContext(options.Options);
                demosManager = new AzureDemosDBManager(dBContext);
            });

            services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                .AddAzureAD(options =>
                {
                    Configuration.Bind("AzureAd", options);
                });

            services.AddSingleton<AzAppsDemosAPIOptions>(op =>
            {
                AzAppsDemosAPIOptions apiOptions = new AzAppsDemosAPIOptions();
                Configuration.Bind("AzDemosApi", apiOptions);
                return apiOptions;
            });

            services.AddSingleton<GlobalSettings>(op =>
            {
                GlobalSettings seetings = new GlobalSettings();
                Configuration.Bind("GlobalSettings", seetings);
                return seetings;
            });

            services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
            {
                options.Events.OnTicketReceived = async context =>
                    {
                       
                        GlobalSettings seetings = new GlobalSettings();
                        Configuration.Bind("GlobalSettings", seetings);

                        string[] simpleAccountDomainsArray = seetings.SimpleAccountDomains.Replace(" ", "").Split(',');
                        List<string> simpleAccountDomains = new List<string>(simpleAccountDomainsArray);
                        var claimsUser = context.Principal.Identity as ClaimsIdentity;
                        string userName = claimsUser.Name;
                        MailAddress mail = new MailAddress(userName.ToLower());
                        string account = mail.User;

                        if (simpleAccountDomains.Count(d => d.Equals(mail.Host, StringComparison.InvariantCultureIgnoreCase)) == 0)
                        {
                            string accountHost = Regex.Replace(mail.Host, @"\.[\W\w]*", "");
                            account = $"{mail.User}.{accountHost}";
                        }

                        claimsUser.SetClaimValue(KnownClaimTypes.MicrosoftAccountClaims.MicrosoftAccountId, account);

                        DbContextOptionsBuilder options = new DbContextOptionsBuilder();
                        options.UseSqlServer(Configuration.GetConnectionString("AzureDemosDB"));

                        var dbContext = new AzureDemosDBContext(options.Options);

                        demosManager = new AzureDemosDBManager(dbContext);


                        AzureDemosDBInitializer.Initialize(dbContext);


                        await demosManager.UpdateLogginUser(account, claimsUser.GetDisplayName(), userName);
                    };
            });

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddRazorPages();

            services.AddControllers()
                .AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;                
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller?}/{action=Index}/{id?}");
                endpoints.MapRazorPages();

                endpoints.MapHub<LogHub>("/GuideContentLog");
            });            
        }
    }
}
