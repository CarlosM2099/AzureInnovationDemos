using AzureDevOpsUserManagementAPI.Models;
using AzureDevOpsUserManagementAPI.Utilities;
using AzureDevOpsUserManagementAPI.Utilities.DemoGenerator;
using AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Models;
using AzureInnovationDemosDAL;
using AzureInnovationDemosDAL.BusinessModels;
using AzureInnovationDemosDAL.Models;
using AzureInnovationDemosDAL.Utilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Helpers
{
    public class AzAppsDemoGenAPI
    {
        private readonly ModernAppDB modernAppDB;
        private readonly AdTenantOptions tenantOptions;
        private readonly AdDemosSkus adDemosSkus;
        private readonly AzureDemosDBManager demosDBManager;
        private readonly AzureDemosDBContext dBContext;
        private readonly DemoGenSettings demoGenSettings;
        private readonly DynamicsAPI dynamicsAPI;

        public AzAppsDemoGenAPI(AdTenantOptions tenantOptions, AdDemosSkus adDemosSkus, AzureDemosDBContext dBContext, DemoGenSettings demoGenSettings)
        {
            modernAppDB = new ModernAppDB();
            this.tenantOptions = tenantOptions;
            this.dBContext = dBContext;
            this.demosDBManager = new AzureDemosDBManager(this.dBContext);
            this.demoGenSettings = demoGenSettings;
            this.adDemosSkus = adDemosSkus;
            dynamicsAPI = new DynamicsAPI(tenantOptions);

        }

        public async Task<DemoEnvironment> CreateADOUserProject(string user, string organizationName, string projectName, string templateLocation)
        {
            HttpExec httpExec = new HttpExec();
            string result = "";
            DemoEnvironment projectResult = new DemoEnvironment();

            var projectRequest = new MultiProjects
            {
                AccessToken = tenantOptions.DemoGenADOToken,
                OrganizationName = organizationName,
                TemplatePath = templateLocation,
                InstallExtensions = true,
                Users = new RequestedProject[] {
                            new RequestedProject{
                                Email = user,
                                ProjectName = projectName}
                          }
            };

            Generator generator = new Generator(demoGenSettings);

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls |
                SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            try
            {
                var creationResult = await generator.Create(projectRequest);
                projectResult.TemplatePath = creationResult.TemplatePath;
                projectResult.TemplateName = creationResult.TemplateName;
            }

            catch (Exception e)
            {
                throw new Exception($"DemoGen API result : {result}", e);
            }

            projectResult.Users.Add(new DemoEnvironmentUser
            {
                Url = $"https://dev.azure.com/{organizationName}/{projectName}",
                Provisioned = true,
                Description = "Go to Project"
            });

            return projectResult;
        }

        public async Task<DemoEnvironment> CreateModernAppUser(Models.AadUser user)
        {
            GraphAPI graphAPI = new GraphAPI(tenantOptions);

            await graphAPI.SetUserLicenses(user, new string[] { adDemosSkus.EnterpriseLicense, adDemosSkus.Win10License });
            modernAppDB.InsertUser(user.GivenName, user.Surname, user.UserPrincipalName);

            return new DemoEnvironment()
            {
                Users = new List<DemoEnvironmentUser>() { new DemoEnvironmentUser() { Email = user.UserPrincipalName, Provisioned = true,
                    Description = "Go to Demo", Url = "http://portal.azure.com/" } }
            };
        }

        public async Task<DemoEnvironment> CreatePowerAppsUser(Models.AadUser user)
        {
            GraphAPI graphAPI = new GraphAPI(tenantOptions);

            await graphAPI.SetUserLicenses(user, new string[] { adDemosSkus.PowerAppsLicense });

            return new DemoEnvironment()
            {
                Users = new List<DemoEnvironmentUser>() { new DemoEnvironmentUser() { Email = user.UserPrincipalName, Provisioned = true,
                    Description = "Go to Demo", Url = "https://make.powerapps.com" } }
            };
        }

        public async Task<DemoEnvironment> CreateKubernetesUser(Models.AadUser user)
        {
            AzureGraphAPI azureGraphAPI = new AzureGraphAPI(tenantOptions);

            var dbUser = await demosDBManager.GetUser(user.MailNickname);
            var demos = await demosDBManager.GetDemos();
            var demo = demos.FirstOrDefault(r => r.Id == (int)DemoTypeEnum.Kubernetes);

            UserDemoAzureResource userDemoAzureResource = await demosDBManager.GetUserDemoAzureResource(dbUser.Id, demo.Id);

            var resourceGroups = await azureGraphAPI.GetResourceGroups();

            var k8ResourceGroup = resourceGroups.Value.
                FirstOrDefault(rg => rg.Name.Contains($"MC_{userDemoAzureResource.DemoAzureResource.Value}", StringComparison.InvariantCultureIgnoreCase));

            var resources = await azureGraphAPI.GetResourceGroupResources(k8ResourceGroup.Name);

            var site = resources.Value.FirstOrDefault(r => r.Type == "Microsoft.Web/sites");

            return new DemoEnvironment()
            {
                Users = new List<DemoEnvironmentUser>() { new DemoEnvironmentUser() { Email = user.UserPrincipalName, Provisioned = true,
                    Description = "Go to Site", Url = $"https://{site.Name}.azurewebsites.net" } }
            };
        }

        public async Task SetPowerAppsUserRoles(Models.AadUser adUser)
        {
            CRMUserList dynamicsUsers;
            CRMUser user;
            DynamicsAPI dynamicsAPI = new DynamicsAPI(tenantOptions);

            do
            {
                dynamicsUsers = await dynamicsAPI.GetUsers();
                user = dynamicsUsers.Value.FirstOrDefault(u => u.DomainName.Equals(adUser.UserPrincipalName, StringComparison.InvariantCultureIgnoreCase));

                if (user == null)
                {

                    Thread.Sleep(60 * 1000);
                }

            } while (user == null);

            var crmRoles = await dynamicsAPI.GetRoles();
            var environmentMakerRole = crmRoles.Value.FirstOrDefault(r =>
               r.Name.Equals(DynamicsAPI.EnvironmentMakerRoleName, StringComparison.InvariantCultureIgnoreCase));

            var cdsUserRole = crmRoles.Value.FirstOrDefault(r =>
                r.Name.Equals(DynamicsAPI.CommonDataServiceUserRoleName, StringComparison.InvariantCultureIgnoreCase));

            await dynamicsAPI.AssignUserRole(user, environmentMakerRole.Id);
            await dynamicsAPI.AssignUserRole(user, cdsUserRole.Id);
            await demosDBManager.UpdateDemoUserEnvironments(adUser.MailNickname, DemoTypeEnum.AutoGlass);
        }
    }
}