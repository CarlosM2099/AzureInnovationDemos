﻿using AzureResourcesPoolManager.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureResourcesPoolManager.Helpers
{
    public class AADTokenProvider
    {
        private readonly AdOptions adTenantOptions;
        public AADTokenProvider(AdOptions adTenantOptions)
        {
            this.adTenantOptions = adTenantOptions;
        }
        public async Task<AADToken> GetToken(string resource)
        {
            AADToken accessToken;

            using (HttpClient client = new HttpClient())
            {
                HttpContent content = new FormUrlEncodedContent(new[]{
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("resource", resource),
                    new KeyValuePair<string, string>("username", $"{adTenantOptions.UserName}@{adTenantOptions.TenantDomain}"),
                    new KeyValuePair<string, string>("password", adTenantOptions.UserPassword),
                    new KeyValuePair<string, string>("client_id", adTenantOptions.AppClientId)
                });
                
                var response = await client.PostAsync($"https://login.microsoftonline.com/{adTenantOptions.TenantId}/oauth2/token", content);
                var contentStr = await response.Content.ReadAsStringAsync();
                accessToken = JsonConvert.DeserializeObject<AADToken>(contentStr);
            }

            return accessToken;
        }

    }
}