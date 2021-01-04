using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsUserRolesHandling
{
    public class HttpExec
    {
        private string authToken = null;
        public HttpExec(string authToken)
        {
            this.authToken = authToken;
        }

        public async Task<string> ExecGet(string url)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await httpClient.GetAsync(url);
                var result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }

        public async Task<string> ExecPost(string url, string json = null, object value = null)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = new TimeSpan(0, 0, 5);

                HttpContent content = null;

                if (value != null)
                {
                    string valueJson = JsonConvert.SerializeObject(value, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore });
                    content = new StringContent(valueJson, Encoding.UTF8, "application/json");
                }
                else if (json != null)
                {
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                var response = await httpClient.PostAsync(url, content);
                var result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }

        public async Task<string> ExecPatch(string url, string json = null, object value = null, string applicationType = "application/json")
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpContent content = null;

                if (value != null)
                {
                    string valueJson = JsonConvert.SerializeObject(value, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                    content = new StringContent(valueJson, Encoding.UTF8, applicationType);
                }
                else if (json != null)
                {
                    content = new StringContent(json, Encoding.UTF8, applicationType);
                }

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                {
                    Content = content
                };

                var response = await httpClient.SendAsync(httpRequestMessage);
                var result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }
    }
}
