using AzureResourcesPoolManager.Models;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AzureResourcesPoolManager.Helpers
{
    public class GraphAPI
    {
        AdOptions tenantOptions;
        string AccessToken;
        string AzureDevAPIUrl = "https://dev.azure.com";
        string AdoResourceId = "499b84ac-1321-427f-aa17-267ca6975798";
        public GraphAPI(AdOptions adOptions)
        {
            tenantOptions = adOptions;           
        }

        public async Task GetToken()
        {
            AADTokenProvider tokenProvider = new AADTokenProvider(tenantOptions);
            var adToken = await tokenProvider.GetToken(AdoResourceId);
            AccessToken = adToken.AccessToken;
        }

        public async Task DeleteADODemoProject(string demoUrl)
        {
            await GetToken();

            Uri uri = new Uri(demoUrl);
            
            var orgName = uri.Segments.ElementAt(1);
            var projName = uri.LocalPath.Replace("/" + orgName, "");

            orgName = orgName.Replace("/", "");
            
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

                var response = await httpClient.GetAsync($"{AzureDevAPIUrl}/{orgName}/_apis/projects?api-version=5.0");
                var result = await response.Content.ReadAsStringAsync();
                var projects = JsonConvert.DeserializeObject<dynamic>(result);

                foreach (var project in projects.value)
                {
                    string projectName = project.name.ToString();

                    if (string.Compare(projectName, projName, CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace) == 0)
                    {
                        await httpClient.DeleteAsync($"{AzureDevAPIUrl}/{orgName}/_apis/projects/{project.id}?api-version=5.0");
                        Console.Out.WriteLine($"Project {projectName} deleted");
                        break;
                    }
                }
            }
        }        
    }
}
