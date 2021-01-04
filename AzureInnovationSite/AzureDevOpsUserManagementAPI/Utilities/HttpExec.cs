using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AzureDevOpsUserManagementAPI.Utilities
{
    public class HttpExec
    {
        private string authToken = null;
        public HttpExec(string authToken = null)
        {
            this.authToken = authToken;
        }

        public async Task<string> ExecGet(string url)
        {
            int retryCount = 0;
            string result = "";

            while (retryCount < 5)
            {
                try
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        if (!string.IsNullOrWhiteSpace(authToken))
                        {
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                        }
                        httpClient.Timeout = new TimeSpan(0, 5, 0);
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = await httpClient.GetAsync(url);
                        result = await response.Content.ReadAsStringAsync();

                        return result;
                    }
                }
                catch (Exception e)
                {
                    retryCount++;

                    if (retryCount > 4)
                    {
                        throw e;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }

            return result;
        }

        public async Task<string> ExecPost(string url, string json = null, object value = null)
        {
            int retryCount = 0;
            string result = "";

            while (retryCount < 5)
            {
                try
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        if (!string.IsNullOrWhiteSpace(authToken))
                        {
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                        }

                        httpClient.Timeout = new TimeSpan(0, 5, 0);

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

                        var response = await httpClient.PostAsync(url, content);
                        result = await response.Content.ReadAsStringAsync();

                        return result;
                    }
                }
                catch (Exception e)
                {
                    retryCount++;

                    if (retryCount > 4)
                    {
                        throw e;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }

            return result;
        }

        public async Task<string> ExecPut(string url, string json = null, object value = null)
        {
            int retryCount = 0;
            string result = "";

            while (retryCount < 5)
            {
                try
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        if (!string.IsNullOrWhiteSpace(authToken))
                        {
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                        }

                        httpClient.Timeout = new TimeSpan(0, 5, 0);

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

                        var response = await httpClient.PutAsync(url, content);
                        result = await response.Content.ReadAsStringAsync();

                        return result;
                    }
                }
                catch (Exception e)
                {
                    retryCount++;

                    if (retryCount > 4)
                    {
                        throw e;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }

            return result;
        }

        public async Task<string> ExecPatch(string url, string json = null, object value = null, string applicationType = "application/json")
        {
            int retryCount = 0;
            string result = "";

            while (retryCount < 5)
            {
                try
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        HttpContent content = null;
                        httpClient.Timeout = new TimeSpan(0, 5, 0);
                        if (!string.IsNullOrWhiteSpace(authToken))
                        {
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                        }

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

                        HttpRequestMessage httpRequestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                        {
                            Content = content
                        };

                        var response = await httpClient.SendAsync(httpRequestMessage);
                        result = await response.Content.ReadAsStringAsync();

                        return result;
                    }
                }
                catch (Exception e)
                {
                    retryCount++;

                    if (retryCount > 4)
                    {
                        throw e;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }

            return result;
        }
    }
}