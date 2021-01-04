using AzureDevOpsUserManagementAPI.Models;
using AzureDevOpsUserManagementAPI.Utilities;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AzureDevOpsUserManagementAPI.Helpers
{
    public class AzureGraphAPI
    {
        private readonly string AzureManagementApiUrl = "https://management.azure.com";        
        private AADToken adToken;
        private readonly AdTenantOptions tenantOptions;

        public const string AADContributorRole = "b24988ac-6180-42a0-ab88-20f7382dd24c";
        public const string AADSnapshotDebuggerRole = "08954f03-6346-4c2e-81c0-ec3a5cfae23b";

        public AzureGraphAPI(AdTenantOptions tenantOptions)
        {
            this.tenantOptions = tenantOptions;
            adToken = new AADToken();
        }

        public async Task GetToken()
        {
            if (string.IsNullOrEmpty(adToken.AccessToken))
            {               
                AADTokenProvider tokenProvider = new AADTokenProvider(tenantOptions);
                adToken = await tokenProvider.GetToken(AzureManagementApiUrl);
            }
        }

        public async Task<ManagementAzureResourceList> GetResourceGroupResources(string resourceGroup)
        {
            await GetToken();
            HttpExec httpExec = new HttpExec(adToken.AccessToken);

            string rscsUrl = $"{AzureManagementApiUrl}/subscriptions/{tenantOptions.SubscriptionId}/resourceGroups/{resourceGroup}/resources?api-version=2019-11-01";
            string managementAzureResourceListString = await httpExec.ExecGet(rscsUrl);

            var azureResourcesList = JsonConvert.DeserializeObject<ManagementAzureResourceList>(managementAzureResourceListString);
             
            return azureResourcesList;
        }

        public async Task<ManagementAzureSite> GetAzureResourceSite(string resourceName)
        {
            await GetToken();
            HttpExec httpExec = new HttpExec(adToken.AccessToken);

            string rscsUrl = $"{AzureManagementApiUrl}/subscriptions/{tenantOptions.SubscriptionId}/resources?$filter=name eq '{resourceName}'&$select=id,identity,kind,location,managedBy,name,plan,properties,sku,tags,type&api-version=2019-05-10";
            string managementAzureResourceListString = await httpExec.ExecGet(rscsUrl);

            var azureResourceSiteApp = JsonConvert.DeserializeObject<ManagementAzureResourceList>(managementAzureResourceListString);

            foreach (var web in azureResourceSiteApp.Value)
            {
                if (web.Type == "Microsoft.Web/sites")
                {
                    List<string> v_20181101 = new List<string>(new string[] { "centralus", "northeurope", "westeurope", "southeastasia", "koreacentral", "koreasouth", "westus", "eastus", "japanwest", "japaneast", "eastasia", "eastus2", "northcentralus", "southcentralus", "brazilsouth", "australiaeast", "australiasoutheast", "westindia", "centralindia", "southindia", "canadacentral", "canadaeast", "ukwest", "uksouth", "westus2", "msftwestus", "msfteastus", "msfteastasia", "msftnortheurope", "eastus2stage", "centralusstage", "northcentralusstage", "francecentral", "southafricanorth", "australiacentral", "westcentralus", "eastasiastage" });

                    string apiVersion = "";

                    if (v_20181101.Contains(web.Location.ToString().ToLower()))
                    {
                        apiVersion = "2018-11-01";
                    }

                    string siteResourceUrl = $"{AzureManagementApiUrl}/{web.Id}?api-version={apiVersion}";
                    var siteResourceString = await httpExec.ExecGet(siteResourceUrl);
                    return JsonConvert.DeserializeObject<ManagementAzureSite>(siteResourceString);
                }
            }

            return null;
        }

        public async Task<ManagementAzureRoleAssigmentList> GetResourceGroupRoleAssignment(string resourceGroupName)
        {
            await GetToken();
            HttpExec httpExec = new HttpExec(adToken.AccessToken);

            string resourceGroupRoleAssignmentUrl = $"{AzureManagementApiUrl}/subscriptions/{tenantOptions.SubscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Authorization/roleAssignments?api-version=2019-04-01-preview";
            var resourceGroupRoleAssignmentString = await httpExec.ExecGet(resourceGroupRoleAssignmentUrl);

            return JsonConvert.DeserializeObject<ManagementAzureRoleAssigmentList>(resourceGroupRoleAssignmentString);
        }

        public async Task<string> GetAppPublishingProfile(string resourceGroupName, string appName)
        {
            await GetToken();
            HttpExec httpExec = new HttpExec(adToken.AccessToken);

            var format = new { format = "WebDeploy", includeDisasterRecoveryEndpoints = true };             
            string appPublishingProfileUrl = $"{AzureManagementApiUrl}/subscriptions/{tenantOptions.SubscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Web//sites/{appName}/publishxml?api-version=2019-08-01";
            var publishingProfile = await httpExec.ExecPost(appPublishingProfileUrl, null, format);

            return publishingProfile;
        }

        public async Task<AadResourceGroupList> GetResourceGroups()
        {
            await GetToken();
            HttpExec httpExec = new HttpExec(adToken.AccessToken);

            string resourceGroupsUrl = $"{AzureManagementApiUrl}/subscriptions/{tenantOptions.SubscriptionId}/resourceGroups?api-version=2019-10-01";
            var resourceGroupsResult = await httpExec.ExecGet(resourceGroupsUrl);

            return JsonConvert.DeserializeObject<AadResourceGroupList>(resourceGroupsResult);
        }      

        public async Task<ManagementAzureRoleAssigmentList> SetResourceGroupRoleAssignment(string resourceGroupName, string roleId, string principalId)
        {
            await GetToken();

            HttpExec httpExec = new HttpExec(adToken.AccessToken);
            string roleUrl = $"/subscriptions/{tenantOptions.SubscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{roleId}";
            var roleAssignmentName = Guid.NewGuid();
            string resourceGroupRoleAssignmentUrl = $"{AzureManagementApiUrl}/subscriptions/{tenantOptions.SubscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Authorization/roleAssignments/{roleAssignmentName}?api-version=2019-04-01-preview";

            var roleAssignmentRequest = new
            {
                properties = new
                {
                    roleDefinitionId = roleUrl,
                    principalId
                }
            };

            var resourceGroupRoleAssignmentString = await httpExec.ExecPut(resourceGroupRoleAssignmentUrl, null, roleAssignmentRequest);

            return JsonConvert.DeserializeObject<ManagementAzureRoleAssigmentList>(resourceGroupRoleAssignmentString);
        }
    }
}