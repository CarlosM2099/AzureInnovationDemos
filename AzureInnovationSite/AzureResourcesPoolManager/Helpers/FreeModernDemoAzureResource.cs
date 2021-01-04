using AzureInnovationDemosDAL;
using AzureInnovationDemosDAL.BusinessModels;
using AzureInnovationDemosDAL.Models;
using AzureInnovationDemosDAL.Utilities;
using AzureResourcesPoolManager.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AzureResourcesPoolManager.Helpers
{
    public class FreeModernDemoAzureResource : IFreeAzureResource
    {
        private readonly string MSGraphApiUrl = "https://graph.microsoft.com";
        private readonly string AzureManagementApiUrl = "https://management.azure.com";
        private readonly string resourceGroup = "AzureAppsAppModernizationDemo";
        AdOptions adOptions;
        AzureDemosDBContext dBContext;
        public FreeModernDemoAzureResource(AdOptions adOptions, AzureDemosDBContext dBContext)
        {
            this.adOptions = adOptions;
            this.dBContext = dBContext;            
        }
        public async Task FreeExpiredResource(DemoAzureResource demoAzureResource)
        {
            Console.Out.WriteLine($"Freeing {demoAzureResource.Value} azure demo res");
            GraphAPI graphAPI = new GraphAPI(adOptions);
            ModernAppDB modernAppDB = new ModernAppDB(adOptions.ModernAppDB);            

            UserDemoAzureResource userDemoAzureResource = dBContext.UserDemoAzureResources
             .Where(r => r.DemoAzureResourceId == demoAzureResource.Id)
             .First();

            var environments = dBContext.DemoUserEnvironments
              .Where(de => de.DemoId == demoAzureResource.DemoId && de.UserId == userDemoAzureResource.UserId)
              .ToList();

            User user = dBContext.Users.First(u => u.Id == userDemoAzureResource.UserId);

            var aadUser = await GetUser(user.AccountName);
            string principalName = aadUser.UserPrincipalName;

            if (!string.IsNullOrEmpty(principalName))
            {
                await RemoveUserResourceGroupRole(aadUser);
                await DeleteLicenses(principalName);
                modernAppDB.DeleteUserByEmail(principalName);
            }

            dBContext.UserDemoAzureResources.Remove(userDemoAzureResource);

            foreach (var environment in environments)
            {
                dBContext.DemoUserEnvironments.Remove(environment);
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

        private async Task<string> DeleteLicenses(string userPrincipalName)
        {
            AADTokenProvider tokenProvider = new AADTokenProvider(adOptions);
            var adToken = await tokenProvider.GetToken(MSGraphApiUrl);
            string accessToken = adToken.AccessToken;

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.GetAsync($"{MSGraphApiUrl}/v1.0/subscribedSkus");
                var result = await response.Content.ReadAsStringAsync();
                var licenses = JsonConvert.DeserializeObject<TenantSubscriptionSkuList>(result).Value;

                var newLicenses = new
                {
                    addLicenses = new object[] { },
                    removeLicenses = licenses.Where(l => l.SkuPartNumber == "ENTERPRISEPACK" || l.SkuPartNumber == "Win10_VDA_E3").Select(l => l.SkuId).ToArray()
                };

                string valueJson = JsonConvert.SerializeObject(newLicenses, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore });
                var content = new StringContent(valueJson, Encoding.UTF8, "application/json");

                response = await httpClient.PostAsync($"{MSGraphApiUrl}/v1.0/users/{userPrincipalName}/assignLicense", content);
                result = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<dynamic>(result);
                if (user != null)
                {
                    return user.userPrincipalName;
                }
            }

            return string.Empty;
        }

        public async Task RemoveUserResourceGroupRole(AadUser user)
        {            
            AADTokenProvider tokenProvider = new AADTokenProvider(adOptions);
            var adToken = await tokenProvider.GetToken(AzureManagementApiUrl);
            string accessToken = adToken.AccessToken;

            string resourceGroupRoleAssignmentsUrl = $"{AzureManagementApiUrl}/subscriptions/{adOptions.SubscriptionId}/resourceGroups/{resourceGroup}/" +
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
                    if (scope.ToLower().Contains($"resourceGroups/{resourceGroup}".ToLower()))
                    {
                        if (rolesAssignment.Properties.PrincipalId == user.Id.ToString())
                        {
                            string userRoleAssignment = $"{AzureManagementApiUrl}/subscriptions/{adOptions.SubscriptionId}/resourceGroups/{resourceGroup}/" +
                                $"providers/Microsoft.Authorization/roleAssignments/{rolesAssignment.Name}?api-version=2019-04-01-preview";

                            response = await httpClient.DeleteAsync(userRoleAssignment);
                            result = await response.Content.ReadAsStringAsync();                            
                        }
                    }
                }

            }
        }
    }
}
