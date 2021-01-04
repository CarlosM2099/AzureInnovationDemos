using AzureInnovationDemos.Helpers;
using AzureInnovationDemosDAL.BusinessModels;
using AzureInnovationDemosDAL.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureInnovationDemos.Helper
{
    public class AzAppsDemosAPI
    {
        private string TenantDomain = "azureapps.onmicrosoft.com";
        private string ADODemosAppId;
        private string ADODemosAud;
        private string ADODemosAppSecret;
        private string ADODemosAPIURL;
        private string PowerAppRoleAzFunctionsURL;
        private string GithubActionFileURL;
        private string accessToken;
        public AzAppsDemosAPI(AzAppsDemosAPIOptions azAppsAPIOptions, GlobalSettings globalSettings)
        {
            ADODemosAppId = azAppsAPIOptions.DemosAppId;
            ADODemosAppSecret = azAppsAPIOptions.DemosAppSecret;
            ADODemosAud = azAppsAPIOptions.DemosAud;
            ADODemosAPIURL = azAppsAPIOptions.DemosAPIURL;
            PowerAppRoleAzFunctionsURL = azAppsAPIOptions.PowerAppRoleAzFunctionsURL;
            GithubActionFileURL = globalSettings.GithubActionTemplate;
        }

        public async Task GetToken()
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                AuthenticationResult authenticationResult = null;
                var authenticationContext = new AuthenticationContext($"https://login.microsoftonline.com/{TenantDomain}", false);
                ClientCredential clientCredential = new ClientCredential(ADODemosAppId, ADODemosAppSecret);

                authenticationResult = await authenticationContext.AcquireTokenAsync(ADODemosAud, clientCredential);
                accessToken = authenticationResult.AccessToken;
            }
        }

        public async Task<AadUser> CreateADODemoUser(User demoUser)
        {
            var result = await ExecPost($"{ADODemosAPIURL}/api/users", demoUser);

            return JsonConvert.DeserializeObject<AadUser>(result);
        }

        public async Task<DemoOrganization> CreateDemoOrganization(AadUser demoUser)
        {
            var result = await ExecPost($"{ADODemosAPIURL}/api/users/getdemoorganization", demoUser);

            return JsonConvert.DeserializeObject<DemoOrganization>(result);
        }

        public async Task<DemoEnvironment> HydrateADODemoEnvironment(AadUser demoUser, int demoId, string organization, string templateLocation)
        {
            DemoEnvironment result = new DemoEnvironment();

            await Task.Run(() =>
            {
                result.TemplateName = demoUser.DisplayName + " ADO Demo Project";
                result.TemplatePath = templateLocation;

                result.Users.Add(new DemoEnvironmentUser() { Email = demoUser.UserPrincipalName, Description = "Go To Project", Provisioned = false, Url = $"https://dev.azure.com/{organization}/{demoUser.DisplayName} ADO Demo Project" });
            });

            Task.Run(async () =>
            {
                DemoProjectRequest projectRequest = new DemoProjectRequest()
                {
                    DisplayName = demoUser.DisplayName,
                    UserPrincipalName = demoUser.UserPrincipalName,
                    ProjectName = demoUser.DisplayName + " ADO Demo Project",
                    TemplateLocation = templateLocation,
                    DemoId = demoId
                };

                await ExecPost($"{ADODemosAPIURL}/api/users/getdemoproject/{organization}", projectRequest, timeOut: 30);
            }).Forget();

            return result;
        }

        public async Task<DemoEnvironment> SetModernAppDemoUser(AadUser user, int id)
        {
            var result = await ExecPost($"{ADODemosAPIURL}/api/users/getmodernappuser", user);

            return JsonConvert.DeserializeObject<DemoEnvironment>(result);
        }

        public async Task<DemoEnvironment> SetAutoglassDemoUser(AadUser user, int id)
        {
            var result = await ExecPost($"{ADODemosAPIURL}/api/users/getpowerappsuser", user);

            return JsonConvert.DeserializeObject<DemoEnvironment>(result);
        }

        public async Task<string> GeGitHubActionTemplate(string webAppName)
        {
            string actionFileContent;

            var webRequest = WebRequest.Create(GithubActionFileURL);

            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                actionFileContent = await reader.ReadToEndAsync();
            }

            actionFileContent = actionFileContent.Replace("[AZURE_WEBAPP_NAME]", webAppName);

            return actionFileContent;
        }

        public void SetAutoglassUserRoles(AadUser user)
        {
            Task.Run(async () =>
            {
                await ExecPost($"{ADODemosAPIURL}/api/users/setpowerappsuserroles", user);
            }).Forget();
        }

        public async Task<DemoEnvironment> SetKubernetesDemoUser(AadUser user)
        {
            var result = await ExecPost($"{ADODemosAPIURL}/api/users/getkubernetesuser", user);

            return JsonConvert.DeserializeObject<DemoEnvironment>(result);
        }

        public async Task<string> GetDemoAzureResources(AadUser user, int demoId, string resourceName)
        {
            return await ExecPost($"{ADODemosAPIURL}/api/resources/{resourceName}/{demoId}", user);
        }

        public async Task<ResourceValidation> ValidateDemoAzureResources(int demoId)
        {
            var resources = await ExecGet($"{ADODemosAPIURL}/api/resources/validate/{demoId}");

            return JsonConvert.DeserializeObject<ResourceValidation>(resources);
        }

        public async Task<object> GetUserDemoAzureResourceExpiration(int demoId, int userId)
        {
            return await ExecGet($"{ADODemosAPIURL}/api/resources/demo/{demoId}/getexpiration/{userId}");
        }


        private async Task<string> ExecGet(string url, [CallerMemberName] string callerName = "")
        {
            var tryCount = 0;
            string result = "";

            await GetToken();

            while (tryCount < 5)
            {
                try
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        httpClient.Timeout = new TimeSpan(0, 5, 0);
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                        var response = await httpClient.GetAsync(url);
                        result = await response.Content.ReadAsStringAsync();
                        return result;
                    }
                }

                catch (Exception e)
                {
                    tryCount++;

                    Thread.Sleep(1000 * tryCount);

                    if (tryCount > 4)
                    {
                        throw new Exception($"{callerName} request result : {result}", e);
                    }
                }
            }
            return result;
        }

        private async Task<string> ExecPost(string url, object postLoad, [CallerMemberName] string callerName = "", int timeOut = 5)
        {
            var tryCount = 0;
            string result = "";
            while (tryCount < 5)
            {
                try
                {
                    await GetToken();
                    using (HttpClient httpClient = new HttpClient())
                    {
                        httpClient.Timeout = new TimeSpan(0, timeOut, 0);

                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                        HttpContent content = new StringContent(JsonConvert.SerializeObject(postLoad), Encoding.UTF8, "application/json");

                        var response = await httpClient.PostAsync(url, content);
                        result = await response.Content.ReadAsStringAsync();
                        
                        return result;
                    }
                }
                catch (Exception e)
                {
                    tryCount++;

                    Thread.Sleep(1000 * tryCount);

                    if (tryCount > 4)
                    {
                        throw new Exception($"{callerName} request result : {result}", e);
                    }
                }
            }

            return result;
        }
    }
}