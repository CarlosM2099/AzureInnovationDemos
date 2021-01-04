
using AzureInnovationDemos.Extensions;
using AzureInnovationDemos.Helper;
using AzureInnovationDemos.Helpers;
using AzureInnovationDemosDAL;
using AzureInnovationDemosDAL.BusinessModels;
using AzureInnovationDemosDAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AzureInnovationDemos.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DemosController : ControllerBase
    {
        private readonly AzureDemosDBManager demosDBManager;
        private readonly AzAppsDemosAPIOptions demosAPIOptions;
        private readonly GlobalSettings settings;
        private readonly AzAppsDemosAPI azDemosApi;
        public DemosController(AzureDemosDBContext context, AzAppsDemosAPIOptions apiOptions, GlobalSettings globalSettings)
        {
            demosDBManager = new AzureDemosDBManager(context);
            demosAPIOptions = apiOptions;
            settings = globalSettings;
            azDemosApi = new AzAppsDemosAPI(demosAPIOptions, settings);
        }


        [HttpGet("~/api/demos")]
        public async Task<IEnumerable<Demo>> Get()
        {
            return await demosDBManager.GetVisibleDemos();
        }

        [HttpGet("~/api/demos/{id:int}")]
        public async Task<Demo> Get(int id)
        {
            return await demosDBManager.GetDemo(id);
        }

        [HttpPost("~/api/demos/{value}")]
        public void Post([FromBody]dynamic value)
        {
        }

        [HttpPost]
        [Route("~/api/demos/{id}/provisionenvuser")]
        public async Task<AadUser> CreateUserEnv(int id)
        {
            var claimsUser = (User.Identity as ClaimsIdentity);

            try
            {
                var demoUser = await demosDBManager.GetUser(claimsUser.GetAccount());

                var createdUser = await azDemosApi.CreateADODemoUser(demoUser);

                return createdUser;
            }
            catch (Exception ex)
            {
                string errorDescription = "User provisioning error: " +
                     "<br/>" + ex.Message +
                     "<br/>" + ex.StackTrace +
                     "<br/>" + "Demo ID: " + id +
                     "<br/>" + "User: " + claimsUser.Name +
                     "<br/>" + ex.InnerException ?? "";

                MailHelper.SendEmail("User Provisioning error", errorDescription, settings.AlertEmails);
                throw ex;
            }
        }

        [HttpPost]
        [Route("~/api/demos/{id}/provisionorg")]
        public async Task<DemoOrganization> CreateOrg(int id, [FromBody]AadUser user)
        {
            try
            {
                var claimsUser = (User.Identity as ClaimsIdentity);
                var demoUser = await demosDBManager.GetUser(claimsUser.GetAccount());
                var userDemoOrganization = await demosDBManager.GetUserDemoOrganization(demoUser.AccountName);

                if (userDemoOrganization == null)
                {
                    var createdDemoOrganization = await azDemosApi.CreateDemoOrganization(user);

                    await demosDBManager.AddUserDemoOrganization(demoUser.AccountName, createdDemoOrganization.Name);

                    return createdDemoOrganization;
                }

                return new DemoOrganization() { Name = userDemoOrganization.Name };
            }
            catch (Exception ex)
            {
                string errorDescription = "User Oranization provisioning error: " +
                    "<br/>" + ex.Message +
                     "<br/>" + ex.StackTrace +
                     "<br/>" + "Demo ID: " + id +
                     "<br/>" + "Request: <br/>" + JsonConvert.SerializeObject(user) +
                     "<br/>" + ex.InnerException ?? "";

                MailHelper.SendEmail("User Environment Provisioning error", errorDescription, settings.AlertEmails);
                throw ex;
            }
        }

        [HttpPost]
        [Route("~/api/demos/{id}/provisionenv/{organization}")]
        public async Task<DemoEnvironment> CreateEnv(int id, string organization, [FromBody]AadUser user)
        {
            try
            {
                var claimsUser = (User.Identity as ClaimsIdentity);
                var demoUser = await demosDBManager.GetUser(claimsUser.GetAccount());

                DemoAzureResource demoAzureResource = null;

                var userAzureResource = await demosDBManager.GetUserDemoAzureResource(demoUser.Id, id);

                if (userAzureResource == null)
                {
                    demoAzureResource = await demosDBManager.GetNextDemoAzureResource(id, TimeSpan.FromDays(5));
                }
                else
                {
                    demoAzureResource = userAzureResource.DemoAzureResource;
                }

                if (demoAzureResource != null)
                {
                    if (userAzureResource == null)
                    {
                        await demosDBManager.CreateUserDemoAzureResource(demoUser.Id, demoAzureResource.Id);
                    }

                    DemoEnvironment createdDemoEnvironment = null;
                    Demo demo = await demosDBManager.GetDemo(id);
                    DemoTypeEnum demoType = demo.Type;

                    switch (demoType)
                    {
                        case DemoTypeEnum.ADODemo:
                            var demoEnvironmentResources = await azDemosApi.GetDemoAzureResources(user, id, demoAzureResource.Value);
                            dynamic resources = JsonConvert.DeserializeObject(demoEnvironmentResources);

                            createdDemoEnvironment = await azDemosApi.HydrateADODemoEnvironment(user, id, organization, resources.templateLocation.ToString());

                            string githubAction = await azDemosApi.GeGitHubActionTemplate(demoAzureResource.Value);

                            XDocument publishingProfile = XDocument.Parse(resources.appPublishingProfile.ToString());

                            await demosDBManager.CreateDemoUserResource(id, demoUser.Id, "Web App Publishing Profile", publishingProfile.ToString(), DemoAssetTypeEnum.Code);
                            await demosDBManager.CreateDemoUserResource(id, demoUser.Id, "Github Action YAML", githubAction, DemoAssetTypeEnum.Code);                            
                            await demosDBManager.CreateDemoUserResource(id, demoUser.Id, "Azure Website", $"https://{demoAzureResource.Value}.azurewebsites.net", DemoAssetTypeEnum.Link);
                            await demosDBManager.CreateDemoUserResource(id, demoUser.Id, "TailwindTraders Website Repository", "https://github.com/microsoft/TailwindTraders-Website", DemoAssetTypeEnum.Link);
                            await demosDBManager.CreateDemoUserResource(id, demoUser.Id, "Github Secret Name", "TailwindtradersSecret", DemoAssetTypeEnum.AccessKeyToken);
                            await demosDBManager.CreateDemoUserEnvironment(createdDemoEnvironment, demoUser.Id, id);
                            break;

                        case DemoTypeEnum.AppModernization:
                            await azDemosApi.GetDemoAzureResources(user, id, demoAzureResource.Value);
                            createdDemoEnvironment = await azDemosApi.SetModernAppDemoUser(user, id);

                            await demosDBManager.CreateDemoUserEnvironment(createdDemoEnvironment, demoUser.Id, id);

                            break;
                        case DemoTypeEnum.Kubernetes:

                            var k8EnvironmentResources = await azDemosApi.GetDemoAzureResources(user, id, demoAzureResource.Value);
                            dynamic k8resources = JsonConvert.DeserializeObject(k8EnvironmentResources);

                            await demosDBManager.CreateDemoUserResource(id, demoUser.Id, "Resource Group", k8resources?.resourgeGroup.ToString(), DemoAssetTypeEnum.AccessKeyToken);
                            await demosDBManager.CreateDemoUserResource(id, demoUser.Id, "Resource Group MC", k8resources?.resourgeGroupMD.ToString(), DemoAssetTypeEnum.AccessKeyToken);
                            await demosDBManager.CreateDemoUserResource(id, demoUser.Id, "Container registry", k8resources?.registry.ToString(), DemoAssetTypeEnum.AccessKeyToken);
                            await demosDBManager.CreateDemoUserResource(id, demoUser.Id, "Kubernetes service", k8resources?.kService.ToString(), DemoAssetTypeEnum.AccessKeyToken);
                            await demosDBManager.CreateDemoUserResource(id, demoUser.Id, "Application Insights", k8resources?.insights.ToString(), DemoAssetTypeEnum.AccessKeyToken);

                            createdDemoEnvironment = await azDemosApi.SetKubernetesDemoUser(user);

                            await demosDBManager.CreateDemoUserEnvironment(createdDemoEnvironment, demoUser.Id, id);

                            break;

                        case DemoTypeEnum.AutoGlass:
                            await azDemosApi.GetDemoAzureResources(user, id, demoAzureResource.Value);
                            createdDemoEnvironment = await azDemosApi.SetAutoglassDemoUser(user, id);

                            await demosDBManager.CreateDemoUserEnvironment(createdDemoEnvironment, demoUser.Id, id);
                        
                            break;
                    }

                    return createdDemoEnvironment;
                }

                return null;
            }
            catch (Exception ex)
            {
                string errorDescription = "User Environment provisioning error: " +
                    "<br/>" + ex.Message +
                     "<br/>" + ex.StackTrace +
                     "<br/>" + "Demo ID: " + id +
                     "<br/>" + "Request: <br/>" + JsonConvert.SerializeObject(user) +
                     "<br/>" + ex.InnerException ?? "";

                MailHelper.SendEmail("User Environment Provisioning error", errorDescription, settings.AlertEmails);
                throw ex;
            }
        }


        [HttpGet]
        [Route("~/api/demos/{id}/vm")]
        public async Task<DemoVM> GetDemoVM(int id)
        {
            return await demosDBManager.GetDemoVM(id);
        }

        [HttpGet]
        [Route("~/api/demos/{id}/rdp")]
        public async Task<FileContentResult> GetDemoRDP(int id)
        {
            try
            {
                var claimsUser = (User.Identity as ClaimsIdentity);
                var demoVM = await demosDBManager.GetDemoVM(id);
                var demoUser = await demosDBManager.GetUser(claimsUser.GetAccount());
                var envDemoUser = await demosDBManager.GetDemoUserEnvironments(id, demoUser.Id);

                WebClient client = new WebClient();
                StreamReader reader = new StreamReader(client.OpenRead(demoVM.URL));
                string contents = reader.ReadToEnd();
                reader.Close();

                contents = $"{contents}\nusername:s:\\azuread\\{envDemoUser.First().EnvironmentUser}";

                await demosDBManager.SetUserRDPLog(demoUser.AccountName, id);

                return new FileContentResult(Encoding.Default.GetBytes(contents), "application/x-rdp")
                {
                    FileDownloadName = "demo.rdp"
                };

            }
            catch (Exception ex)
            {
                string errorDescription = "User VM error: " +
                    "<br/>" + ex.Message +
                     "<br/>" + ex.StackTrace +
                     "<br/>" + "Demo ID: " + id +
                     "<br/>" + ex.InnerException ?? "";

                MailHelper.SendEmail("User VM error", errorDescription, settings.AlertEmails);
                throw ex;
            }
        }



        [HttpGet]
        [Route("~/api/demos/user")]
        public async Task<User> GetDemoUser()
        {
            var claimsUser = (User.Identity as ClaimsIdentity);
            return await demosDBManager.GetUser(claimsUser.GetAccount());
        }


        [HttpGet]
        [Route("~/api/demos/{demoId}/user/{userId}/environments")]
        public async Task<List<DemoUserEnvironment>> GetDemoUserEnvironments(int demoId, int userId)
        {
            return await demosDBManager.GetDemoUserEnvironments(demoId, userId);
        }

        [HttpGet]
        [Route("~/api/demos/{demoId}/user/{userId}/resources")]
        public async Task<List<DemoUserResource>> GetDemoUserResources(int demoId, int userId)
        {
            return await demosDBManager.GetDemoUserResources(demoId, userId);
        }


        [HttpGet]
        [Route("~/api/demos/{demoId}/validateresources")]
        public async Task<ResourceValidation> ValidateDemoResources(int demoId)
        {
            return await azDemosApi.ValidateDemoAzureResources(demoId);
        }

        [HttpGet]
        [Route("~/api/demos/{demoId}/user/{userId}/resourceexpiration")]
        public async Task<object> GetUserDemoResourcesExpiration(int demoId, int userId)
        {
            var userAzureResource = await demosDBManager.GetUserDemoAzureResource(userId, demoId);

            return new { expirationDate = userAzureResource.DemoAzureResource.LockedUntil };
        }

        [HttpPut("demos/{value}")]
        public void Put(int id, [FromBody]dynamic value)
        {
        }

        [HttpDelete("demos/{id:int}")]
        // DELETE: api/Demos/5
        public void Delete(int id)
        {
        }
    }
}
