using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzureDevOpsUserManagementAPI.Helpers;
using Microsoft.Extensions.Configuration;
using System.Linq;
using AzureDevOpsUserManagementAPI.Models;
using System.Threading;
using AzureInnovationDemosDAL;
using Microsoft.EntityFrameworkCore;
using AzureInnovationDemosDAL.Models;

namespace AzureInnovationDemosAzFunctions
{
    public static class AssignPAUserRoles
    {
        [FunctionName("AssignPAUserRoles")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("AssignPAUserRoles request started");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string userName = data?.userName;

            if (userName != null)
            {
                var currentDir = (Directory.Exists(@"D:\home\site\wwwroot") ? @"D:\home\site\wwwroot" : @Directory.GetCurrentDirectory()) + "/appsettings.json";
                IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(currentDir).Build();
                AdTenantOptions apiOptions = new AdTenantOptions();

                configuration.Bind("AdTenant", apiOptions);

                DynamicsAPI dynamicsAPI = new DynamicsAPI(apiOptions);
                CRMUserList dynamicsUsers;
                CRMUser user;

                log.LogInformation($"Validating user account {userName} is in the dynamics org");

                do
                {
                    dynamicsUsers = await dynamicsAPI.GetUsers();
                    user = dynamicsUsers.Value.FirstOrDefault(u => u.DomainName.Equals($"{userName}@{apiOptions.TenantDomain}", StringComparison.InvariantCultureIgnoreCase));

                    if (user == null)
                    {
                        log.LogInformation($"User account {userName} not in the dynamics org, waiting 1 min");
                        Thread.Sleep(60 * 1000);
                    }

                } while (user == null);

                log.LogInformation($"User account {userName} found in the dynamics org, assigning roles");

                var crmRoles = await dynamicsAPI.GetRoles();

                var environmentMakerRole = crmRoles.Value.FirstOrDefault(r =>
                    r.Name.Equals(DynamicsAPI.EnvironmentMakerRoleName, StringComparison.InvariantCultureIgnoreCase));

                var cdsUserRole = crmRoles.Value.FirstOrDefault(r =>
                    r.Name.Equals(DynamicsAPI.CommonDataServiceUserRoleName, StringComparison.InvariantCultureIgnoreCase));

                await dynamicsAPI.AssignUserRole(user, environmentMakerRole.Id);
                await dynamicsAPI.AssignUserRole(user, cdsUserRole.Id);

                log.LogInformation($"User account {userName} got roles assigned, updating environment status");

                DbContextOptionsBuilder options = new DbContextOptionsBuilder();

                options.UseSqlServer(configuration.GetConnectionString("AzureDemosDB"));

                var dBContext = new AzureDemosDBContext(options.Options);
                var demosManager = new AzureDemosDBManager(dBContext);

                await demosManager.UpdateDemoUserEnvironments(userName, DemoTypeEnum.AutoGlass);

                log.LogInformation($"User account {userName} environment status updated");
            }

            return userName != null
                ? (ActionResult)new OkObjectResult($"Dynamics roles assigned to user: {userName}")
                : new BadRequestObjectResult("Please pass a user name in the request body");
        }
    }
}
