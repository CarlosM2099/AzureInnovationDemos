using Newtonsoft.Json;

namespace AzureResourcesPoolManager.Helpers
{
    public class AADToken
    {
        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }
        public string Scope { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public string ExpiresIn { get; set; }

        [JsonProperty(PropertyName = "ext_expires_in")]
        public string ExtExpiresIn { get; set; }

        [JsonProperty(PropertyName = "expires_on")]
        public string ExpiresOn { get; set; }

        [JsonProperty(PropertyName = "not_before")]
        public string NotBefore { get; set; }
        public string Resource { get; set; }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "refresh_token")]
        public string RefreshToken { get; set; }
    }

}
