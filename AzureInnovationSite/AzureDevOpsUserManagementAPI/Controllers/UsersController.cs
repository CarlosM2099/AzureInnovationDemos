using AzureDevOpsUserManagementAPI.Helpers;
using AzureDevOpsUserManagementAPI.Models;
using AzureDevOpsUserManagementAPI.Utilities.DemoGenerator;
using AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Models;
using AzureInnovationDemosDAL;
using AzureInnovationDemosDAL.BusinessModels;
using AzureInnovationDemosDAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Controllers
{

    [Authorize]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly GraphAPI graphAPI;
        private readonly DevOpsAPI devOpsAPI;
        private readonly AzAppsDemoGenAPI azDemoGenAPI;
        private readonly AdTenantOptions adTenantOptions;
        private readonly DemoGenSettings demoGenSettings;
        private readonly AzureDemosDBContext dbContext;
        public UsersController(AdTenantOptions adTenantOptions, AdDemosSkus adDemosSkus, AzureDemosDBContext context, DemoGenSettings demoGenSettings)
        {
            graphAPI = new GraphAPI(adTenantOptions);
            devOpsAPI = new DevOpsAPI(adTenantOptions);
            azDemoGenAPI = new AzAppsDemoGenAPI(adTenantOptions, adDemosSkus, context, demoGenSettings);
            this.adTenantOptions = adTenantOptions;
            this.demoGenSettings = demoGenSettings;
            this.dbContext = context;
        }

        [HttpGet]
        [Route("~/api/users")]
        public async Task<GraphUsersList> Get()
        {
            return await devOpsAPI.GetUsers(adTenantOptions.AccountName);
        }

        [HttpPost]
        [Route("~/api/users")]
        public async Task<Models.AadUser> Post(AadUserCreation newUser)
        {
            Models.AadUser aadUser = await graphAPI.AddAadUser(newUser);
            string userVMSecGroup = adTenantOptions.DemoADOVMSecGroup;

            await graphAPI.SetUserGroups(aadUser, userVMSecGroup);

            return aadUser;
        }

        [HttpPost]
        [Route("~/api/users/getdemoorganization")]
        public async Task<DemoOrganizationResult> CreateUserOrganization(Models.AadUser user)
        {
            string userOrganization = "DemoOrg" + user.MailNickname.Replace(".", "");

            await devOpsAPI.CreateOrganization(userOrganization);

            await devOpsAPI.InstallOrganizationExtension(userOrganization, "keesschollaart.arm-outputs");
            await devOpsAPI.InstallOrganizationExtension(userOrganization, "ms-devlabs.TeamProjectHealth");

            var identityUser = new IdentityUser();

            var aadUser = await graphAPI.GetAadUser(user.MailNickname);

            while (aadUser == null)
            {
                aadUser = await graphAPI.GetAadUser(user.MailNickname);
                Thread.Sleep(1000);
            }

            identityUser.OriginId = aadUser.Id;
            identityUser.OriginDirectory = "aad";

            if (string.IsNullOrEmpty(identityUser.DisplayName))
            {
                identityUser.DisplayName = user.DisplayName;
            }

            if (string.IsNullOrEmpty(identityUser.Mail))
            {
                identityUser.Mail = user.UserPrincipalName;
            }

            await devOpsAPI.AddUserToOrganization(identityUser, userOrganization);

            return new DemoOrganizationResult() { Name = userOrganization };
        }


        [HttpPost]
        [Route("~/api/users/getdemoproject/{organization}")]
        public async Task<DemoEnvironment> CreateUserProject(string organization, [FromBody]DemoProjectRequest request)
        {
            var result = await azDemoGenAPI.CreateADOUserProject(request.UserPrincipalName, organization, request.ProjectName, request.TemplateLocation);
            MailAddress mail = new MailAddress(request.UserPrincipalName);
            var demosManager = new AzureDemosDBManager(dbContext);
            await demosManager.UpdateDemoUserEnvironments(mail.User, DemoTypeEnum.ADODemo);

            return result;
        }

        [HttpPost]
        [Route("~/api/users/getmodernappuser")]
        public async Task<DemoEnvironment> CreateModernAppUser([FromBody]Models.AadUser user)
        {
            return await azDemoGenAPI.CreateModernAppUser(user);
        }

        [HttpPost]
        [Route("~/api/users/getpowerappsuser")]
        public async Task<DemoEnvironment> CreatePowerAppsUser([FromBody]Models.AadUser user)
        {
            return await azDemoGenAPI.CreatePowerAppsUser(user);
        }

        [HttpPost]
        [Route("~/api/users/getkubernetesuser")]
        public async Task<DemoEnvironment> CreateKubernetesUser([FromBody]Models.AadUser user)
        {
            return await azDemoGenAPI.CreateKubernetesUser(user);
        }


        [HttpPost]
        [Route("~/api/users/setpowerappsuserroles")]
        public async Task SetPowerAppsUserRoles([FromBody]Models.AadUser user)
        {
            await azDemoGenAPI.SetPowerAppsUserRoles(user);
        }
    }
}
