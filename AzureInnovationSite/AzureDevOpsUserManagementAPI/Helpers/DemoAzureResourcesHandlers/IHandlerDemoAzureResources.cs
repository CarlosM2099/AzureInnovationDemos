using AzureInnovationDemosDAL.BusinessModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Helpers.DemoAzureResourcesHandlers
{
    interface IHandlerDemoAzureResources
    {
        Task<object> SetDemoAzureResources(AadUser user,int demoId, string resourceName);
        Task<object> ValidateDemoAzureResources(int demoId);
        Task<object> GetUserDemoAzureResourceExpiration(int demoId, int userId);
    }
}
