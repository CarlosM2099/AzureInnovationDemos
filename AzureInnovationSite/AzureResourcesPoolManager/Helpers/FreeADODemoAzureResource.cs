using AzureInnovationDemosDAL;
using AzureInnovationDemosDAL.Models;
using AzureResourcesPoolManager.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AzureResourcesPoolManager.Helpers
{
    public class FreeADODemoAzureResource : IFreeAzureResource
    {
        AdOptions adOptions;
        AzureDemosDBContext dBContext;
        private readonly string AzureManagementApiUrl = "https://management.azure.com";
        public FreeADODemoAzureResource(AdOptions adOptions, AzureDemosDBContext dBContext)
        {
            this.adOptions = adOptions;
            this.dBContext = dBContext;
        }

        public async Task FreeExpiredResource(DemoAzureResource demoAzureResource)
        {
            Console.Out.WriteLine($"Freeing {demoAzureResource.Value} azure demo res");
            GraphAPI graphAPI = new GraphAPI(adOptions);

            List<string> demoEnvProjectsUrls = new List<string>();

            UserDemoAzureResource userDemoAzureResource = dBContext.UserDemoAzureResources
              .Where(r => r.DemoAzureResourceId == demoAzureResource.Id)
              .First();

            var environments = dBContext.DemoUserEnvironments
                .Where(de => de.DemoId == demoAzureResource.DemoId && de.UserId == userDemoAzureResource.UserId)
                .ToList();

            var resources = dBContext.DemoUserResources
             .Where(de => de.DemoId == demoAzureResource.DemoId && de.UserId == userDemoAzureResource.UserId)
             .ToList();

            foreach (var environment in environments)
            {
                demoEnvProjectsUrls.Add(environment.EnvironmentURL);
                dBContext.DemoUserEnvironments.Remove(environment);
            }

            foreach (var resource in resources)
            {
                dBContext.DemoUserResources.Remove(resource);
            }

            dBContext.UserDemoAzureResources.Remove(userDemoAzureResource);

            var dbdemoAzureResource = dBContext.DemoAzureResources.First(dr => dr.Id == demoAzureResource.Id);

            dbdemoAzureResource.LockedUntil = null;
            dbdemoAzureResource.AttemptCount = 0;

            foreach (var demoEnvProjectsUrl in demoEnvProjectsUrls)
            {
                await graphAPI.DeleteADODemoProject(demoEnvProjectsUrl);
            }

            await ResetAppPublishingPassword(demoAzureResource.Value);

            await dBContext.SaveChangesAsync();

            Console.Out.WriteLine($"Database updated res: {demoAzureResource.Value} freed");
        }

        private async Task ResetAppPublishingPassword(string siteName)
        {
            AADTokenProvider tokenProvider = new AADTokenProvider(adOptions);
            var adToken = await tokenProvider.GetToken(AzureManagementApiUrl);
            string accessToken = adToken.AccessToken;
            string siteId = siteName.Replace("tailwindtraders-s", "", StringComparison.InvariantCultureIgnoreCase);

            string resetPassUrl = $"{AzureManagementApiUrl}/subscriptions/{adOptions.SubscriptionId}/resourceGroups/" +
                $"AzureAppsADODemoS{siteId}/providers/Microsoft.Web/sites/{siteName}/newpassword?api-version=2019-08-01";

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                string valueJson = JsonConvert.SerializeObject(new { });
                var content = new StringContent(valueJson, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(resetPassUrl, content);
                var result = await response.Content.ReadAsStringAsync();
            }
        }
    }
}
