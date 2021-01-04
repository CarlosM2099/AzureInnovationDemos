using AzureDevOpsUserManagementAPI.Helpers;
using AzureDevOpsUserManagementAPI.Helpers.DemoAzureResourcesHandlers;
using AzureInnovationDemosDAL;
using AzureInnovationDemosDAL.BusinessModels;
using AzureInnovationDemosDAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Controllers
{
    [Authorize]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private readonly AzureDemosDBManager dBManager;
        private readonly AdTenantOptions adTenantOptions;
        public ResourcesController(AzureDemosDBContext context, AdTenantOptions tenantOptions)
        {
            dBManager = new AzureDemosDBManager(context);
            adTenantOptions = tenantOptions;
        }

        [HttpPost]
        [Route("~/api/resources/{resourceName}/{demoId}")]
        public async Task<object> SetDemoResources(string resourceName, int demoId, [FromBody] AadUser user)
        {
            IHandlerDemoAzureResources handlerDemoAzureResources = null;
            Demo demo = await dBManager.GetDemo(demoId);
            DemoTypeEnum demoType = demo.Type;

            switch (demoType)
            {
                case DemoTypeEnum.ADODemo:
                    handlerDemoAzureResources = new ADODemoAzureResources(dBManager, adTenantOptions);
                    break;
                case DemoTypeEnum.AppModernization:
                    handlerDemoAzureResources = new ModernAppDemoAzureResources(dBManager, adTenantOptions);
                    break;
                case DemoTypeEnum.Kubernetes:
                    handlerDemoAzureResources = new KubernetesDemoAzureResources(dBManager, adTenantOptions);
                    break;
                case DemoTypeEnum.AutoGlass:
                    handlerDemoAzureResources = new PowerAppsDemoAzureResources(dBManager, adTenantOptions);
                    break;
            }

            return await handlerDemoAzureResources.SetDemoAzureResources(user, demoId, resourceName);
        }

        [HttpGet]
        [Route("~/api/resources/validate/{demoId}")]
        public async Task<object> ValidateDemoResources(int demoId)
        {
            IHandlerDemoAzureResources handlerDemoAzureResources = null;
            Demo demo = await dBManager.GetDemo(demoId);
            DemoTypeEnum demoType = demo.Type;

            switch (demoType)
            {
                case DemoTypeEnum.ADODemo:
                    handlerDemoAzureResources = new ADODemoAzureResources(dBManager, adTenantOptions);
                    break;
                case DemoTypeEnum.AppModernization:
                    handlerDemoAzureResources = new ModernAppDemoAzureResources(dBManager, adTenantOptions);
                    break;
                case DemoTypeEnum.Kubernetes:
                    handlerDemoAzureResources = new KubernetesDemoAzureResources(dBManager, adTenantOptions);
                    break;
                case DemoTypeEnum.AutoGlass:
                    handlerDemoAzureResources = new PowerAppsDemoAzureResources(dBManager, adTenantOptions);
                    break;
            }

            return await handlerDemoAzureResources.ValidateDemoAzureResources(demoId);
        }

        [HttpGet]
        [Route("~/api/resources/demo/{demoId}/getexpiration/{userId}")]
        public async Task<object> GetUserDemoResourceExpiration(int demoId, int userId)
        {
            IHandlerDemoAzureResources handlerDemoAzureResources = null;
            Demo demo = await dBManager.GetDemo(demoId);
            DemoTypeEnum demoType = demo.Type;

            switch (demoType)
            {
                case DemoTypeEnum.ADODemo:
                    handlerDemoAzureResources = new ADODemoAzureResources(dBManager, adTenantOptions);
                    break;
                case DemoTypeEnum.AppModernization:
                    handlerDemoAzureResources = new ModernAppDemoAzureResources(dBManager, adTenantOptions);
                    break;
                case DemoTypeEnum.Kubernetes:
                    handlerDemoAzureResources = new KubernetesDemoAzureResources(dBManager, adTenantOptions);
                    break;
                case DemoTypeEnum.AutoGlass:
                    handlerDemoAzureResources = new PowerAppsDemoAzureResources(dBManager, adTenantOptions);
                    break;
            }

            return await handlerDemoAzureResources.GetUserDemoAzureResourceExpiration(demoId, userId);
        }
    }
}
