using AzureInnovationDemosDAL;
using AzureInnovationDemosDAL.BusinessModels;
using AzureInnovationDemosDAL.Models;
using AzureResourcesPoolManager.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AzureResourcesPoolManager.Helpers
{
    public class FreeKubernetesDemoAzureResource : IFreeAzureResource
    {
        private readonly string MSGraphApiUrl = "https://graph.microsoft.com";
        private readonly string AzureManagementApiUrl = "https://management.azure.com";

        AdOptions adOptions;
        AzureDemosDBContext dBContext;
        public FreeKubernetesDemoAzureResource(AdOptions adOptions, AzureDemosDBContext dBContext)
        {
            this.adOptions = adOptions;
            this.dBContext = dBContext;
        }

        public async Task FreeExpiredResource(DemoAzureResource demoAzureResource)
        {
            Console.Out.WriteLine($"Freeing {demoAzureResource.Value} azure demo res");
            GraphAPI graphAPI = new GraphAPI(adOptions);

            UserDemoAzureResource userDemoAzureResource = dBContext.UserDemoAzureResources
             .Include(u => u.DemoAzureResource)
             .Where(r => r.DemoAzureResourceId == demoAzureResource.Id)
             .First();

            var environments = dBContext.DemoUserEnvironments
              .Where(de => de.DemoId == demoAzureResource.DemoId && de.UserId == userDemoAzureResource.UserId)
              .ToList();

            User user = dBContext.Users.First(u => u.Id == userDemoAzureResource.UserId);

            var resources = dBContext.DemoUserResources
              .Where(de => de.DemoId == demoAzureResource.DemoId && de.UserId == userDemoAzureResource.UserId)
              .ToList();

            var aadUser = await GetUser(user.AccountName);
            string principalName = aadUser.UserPrincipalName;

            if (!string.IsNullOrEmpty(principalName))
            {
                var resourceGroups = await GetResourceGroups();
                var k8ResourceGroup = resourceGroups.Value.FirstOrDefault(rg => rg.Name.Contains($"MC_{userDemoAzureResource.DemoAzureResource.Value}", 
                    StringComparison.InvariantCultureIgnoreCase));

                if (k8ResourceGroup != null)
                {
                    await RemoveUserResourceGroupRole(aadUser, k8ResourceGroup.Name);
                }

                await RemoveUserResourceGroupRole(aadUser, userDemoAzureResource.DemoAzureResource.Value);
            }

            dBContext.UserDemoAzureResources.Remove(userDemoAzureResource);

            foreach (var environment in environments)
            {
                dBContext.DemoUserEnvironments.Remove(environment);
            }

            foreach (var resource in resources)
            {
                dBContext.DemoUserResources.Remove(resource);
            }

            var dbdemoAzureResource = dBContext.DemoAzureResources.First(dr => dr.Id == demoAzureResource.Id);

            dbdemoAzureResource.LockedUntil = null;
            dbdemoAzureResource.AttemptCount = 0;

            await dBContext.SaveChangesAsync();

            Console.Out.WriteLine($"Database updated res: {demoAzureResource.Value} freed");
        }

        private async Task<AadUser> GetUser(string userAccount)
        {
            AADTokenProvider tokenProvider = new AADTokenProvider(adOptions);
            var adToken = await tokenProvider.GetToken(MSGraphApiUrl);
            string accessToken = adToken.AccessToken;

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.GetAsync($"{MSGraphApiUrl}/v1.0/users?$filter=mailNickname eq '{userAccount}'");
                var result = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<AadUserResult>(result);

                if (user != null && user.Value.Count > 0)
                {
                    return user.Value[0];
                }
            }

            return null;
        }

        public async Task RemoveUserResourceGroupRole(AadUser user, string resourceGroupName)
        {
            AADTokenProvider tokenProvider = new AADTokenProvider(adOptions);
            var adToken = await tokenProvider.GetToken(AzureManagementApiUrl);
            string accessToken = adToken.AccessToken;

            string resourceGroupRoleAssignmentsUrl = $"{AzureManagementApiUrl}/subscriptions/{adOptions.SubscriptionId}/resourceGroups/{resourceGroupName}/" +
                $"providers/Microsoft.Authorization/roleAssignments?api-version=2019-04-01-preview";

            using (HttpClient httpClient = new HttpClient())
            {

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                httpClient.Timeout = new TimeSpan(0, 5, 0);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await httpClient.GetAsync(resourceGroupRoleAssignmentsUrl);
                var result = await response.Content.ReadAsStringAsync();

                var roleAssigments = JsonConvert.DeserializeObject<ManagementAzureRoleAssigmentList>(result);

                foreach (var rolesAssignment in roleAssigments.Value)
                {
                    string scope = rolesAssignment.Properties.Scope;
                    if (scope.ToLower().Contains($"resourceGroups/{resourceGroupName}".ToLower()))
                    {
                        if (rolesAssignment.Properties.PrincipalId == user.Id.ToString())
                        {
                            string userRoleAssignment = $"{AzureManagementApiUrl}/subscriptions/{adOptions.SubscriptionId}/resourceGroups/{resourceGroupName}/" +
                                $"providers/Microsoft.Authorization/roleAssignments/{rolesAssignment.Name}?api-version=2019-04-01-preview";

                            response = await httpClient.DeleteAsync(userRoleAssignment);
                            result = await response.Content.ReadAsStringAsync();
                        }
                    }
                }

            }
        }

        public async Task<AadResourceGroupList> GetResourceGroups()
        {             
            AADTokenProvider tokenProvider = new AADTokenProvider(adOptions);
            var adToken = await tokenProvider.GetToken(AzureManagementApiUrl);
            string accessToken = adToken.AccessToken;

            using HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            httpClient.Timeout = new TimeSpan(0, 5, 0);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await httpClient.GetAsync($"{AzureManagementApiUrl}/subscriptions/{adOptions.SubscriptionId}/resourceGroups?api-version=2019-10-01");
            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<AadResourceGroupList>(result);
        }
    }
}
