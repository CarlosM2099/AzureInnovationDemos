using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Models
{
    public class GitHubAccessDetails
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        public string Scope { get; set; }
    }
    public class GitHubUserDetail
    {
        public string Login { get; set; }
    }
}