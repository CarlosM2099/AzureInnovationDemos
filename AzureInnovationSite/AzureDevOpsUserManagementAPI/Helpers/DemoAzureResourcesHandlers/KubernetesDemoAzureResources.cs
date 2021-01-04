using AzureInnovationDemosDAL;
using AzureInnovationDemosDAL.BusinessModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AzureDevOpsUserManagementAPI.Helpers.DemoAzureResourcesHandlers
{
    public class KubernetesDemoAzureResources : IHandlerDemoAzureResources
    {
        private readonly AzureGraphAPI azureGraphAPI;
        private readonly GraphAPI graphAPI;
        private readonly AzureDemosDBManager dBManager;


        public KubernetesDemoAzureResources(AzureDemosDBManager demosDBManager, AdTenantOptions tenantOptions)
        {
            dBManager = demosDBManager;
            azureGraphAPI = new AzureGraphAPI(tenantOptions);
            graphAPI = new GraphAPI(tenantOptions);
        }

        public async Task<object> GetUserDemoAzureResourceExpiration(int demoId, int userId)
        {
            var userResource = await dBManager.GetUserDemoAzureResource(demoId, userId);

            return new { expirationDate = userResource.DemoAzureResource.LockedUntil };
        }

        public async Task<object> SetDemoAzureResources(AadUser user, int demoId, string resourceName)
        {
            var resourceGroups = await azureGraphAPI.GetResourceGroups();

            var k8ResourceGroup = resourceGroups.Value.FirstOrDefault(rg => rg.Name.Contains($"MC_{resourceName}", StringComparison.InvariantCultureIgnoreCase));

            if (k8ResourceGroup != null)
            {
                await azureGraphAPI.SetResourceGroupRoleAssignment(k8ResourceGroup.Name, AzureGraphAPI.AADContributorRole, user.Id.ToString());
            }

            await azureGraphAPI.SetResourceGroupRoleAssignment(resourceName, AzureGraphAPI.AADContributorRole, user.Id.ToString());

            var resources = await azureGraphAPI.GetResourceGroupResources(resourceName);

            var registry = resources.Value.FirstOrDefault(r => r.Type == "Microsoft.ContainerRegistry/registries")?.Name;
            var kService = resources.Value.FirstOrDefault(r => r.Type == "Microsoft.ContainerService/managedClusters")?.Name;
            var insights = resources.Value.FirstOrDefault(r => r.Type == "Microsoft.Insights/components")?.Name;
            var resourgeGroup = resourceName;
            var resourgeGroupMD = k8ResourceGroup.Name;

            return new { registry, kService, insights, resourgeGroup, resourgeGroupMD };
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
    }
}