using AzureDevOpsUserManagementAPI.Utilities;
using AzureInnovationDemosDAL;
using AzureInnovationDemosDAL.BusinessModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AzureDevOpsUserManagementAPI.Helpers.DemoAzureResourcesHandlers
{
    public class ModernAppDemoAzureResources : IHandlerDemoAzureResources
    {
        private readonly AzureGraphAPI azureGraphAPI;
        private readonly GraphAPI graphAPI;
        private readonly AzureDemosDBManager dBManager;
     

        public ModernAppDemoAzureResources(AzureDemosDBManager demosDBManager, AdTenantOptions tenantOptions)
        {
            dBManager = demosDBManager;
            azureGraphAPI = new AzureGraphAPI(tenantOptions);
            graphAPI = new GraphAPI(tenantOptions);            
        }

        public async Task<dynamic> SetDemoAzureResources(AadUser user, int demoId, string resourceName)
        {
            await azureGraphAPI.SetResourceGroupRoleAssignment(resourceName, AzureGraphAPI.AADSnapshotDebuggerRole, user.Id.ToString());
            return await azureGraphAPI.SetResourceGroupRoleAssignment(resourceName, AzureGraphAPI.AADContributorRole, user.Id.ToString());
        }


        public async Task<object> ValidateDemoAzureResources(int demoId)
        {
            var demoAzureResources = await dBManager.GetDemoAzureResources(demoId);
            int totalResources = demoAzureResources.Count;
            int availableResourcesCount = 0;
            bool availableResources = true;

            availableResourcesCount = demoAzureResources.Count(r => r.LockedUntil == null);
            availableResources = availableResourcesCount > 0;

            if (!availableResources)
            {
                var nextAvailable = demoAzureResources
                    .OrderBy(rs => rs.LockedUntil)
                    .ThenBy(rs => rs.LockedUntil.Value.TimeOfDay)
                    .First();

                return new { availableResources, nextAvailable = nextAvailable.LockedUntil, availableResourcesCount };
            }

            return new { availableResources, availableResourcesCount };
        }

        public async Task<object> GetUserDemoAzureResourceExpiration(int demoId, int userId)
        {         
            var userResource = await dBManager.GetUserDemoAzureResource(demoId, userId);

            return new { expirationDate = userResource.DemoAzureResource.LockedUntil };
        }
    }
}