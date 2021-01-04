using AzureDevOpsUserManagementAPI.Models;
using AzureDevOpsUserManagementAPI.Utilities;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Helpers
{
    public class GraphAPI
    {
        private readonly string MSGraphApiUrl = "https://graph.microsoft.com";
        private AADToken adToken;


        private readonly AdTenantOptions tenantOptions;
        public GraphAPI(AdTenantOptions tenantOptions)
        {
            this.tenantOptions = tenantOptions;
            adToken = new AADToken();
        }

        public async Task GetToken()
        {
            if (string.IsNullOrEmpty(adToken.AccessToken))
            {
                AADTokenProvider tokenProvider = new AADTokenProvider(tenantOptions);
                adToken = await tokenProvider.GetToken(MSGraphApiUrl);
            }
        }

        public async Task<string> SetAadAppSecret(string appId)
        {
            await GetToken();
            HttpExec httpExec = new HttpExec(adToken.AccessToken);

            string addAppUrl = $"https://graph.microsoft.com/beta/applications/{appId}/addPassword";

            var passwordResult = await httpExec.ExecPost(addAppUrl, null, new { passwordCredentials = new { displayName = $"App Access Secret ({DateTime.Now.ToLongDateString()})" } });
            var password = JsonConvert.DeserializeObject<AadAppPasswordcredential>(passwordResult);

            return password.SecretText;
        }

        private string RandomString(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_#$%^&*()?";
            StringBuilder res = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }

            return res.ToString();
        }

        public async Task<AadApp> GetAadApp(string appId)
        {
            await GetToken();
            HttpExec httpExec = new HttpExec(adToken.AccessToken);

            string addAppUrl = $"https://graph.microsoft.com/beta/applications?$filter=appId eq '{appId}'";
            var addAppResult = await httpExec.ExecGet(addAppUrl);

            var addApps = JsonConvert.DeserializeObject<AadAppList>(addAppResult);

            if (addApps.Value.Count > 0)
            {
                return addApps.Value.First();
            }

            return null;
        }

        public async Task<AadObject> GetDirectoryObject(string objectId)
        {
            await GetToken();
            HttpExec httpExec = new HttpExec(adToken.AccessToken);

            string dirObjUrl = $"https://graph.microsoft.com/beta/directoryObjects/{objectId}";
            var dirObjResult = await httpExec.ExecGet(dirObjUrl);

            return JsonConvert.DeserializeObject<AadObject>(dirObjResult);
        }

        public async Task<AadUser> AddAadUser(AadUserCreation user)
        {
            await GetToken();

            AadUser aadUser = null;
            HttpExec httpExec = new HttpExec(adToken.AccessToken);

            aadUser = await GetAadUser(user.AccountName);

            if (aadUser == null)
            {

                aadUser = new AadUser
                {
                    GivenName = user.GivenName,
                    Surname = user.Surname,
                    DisplayName = $"{user.GivenName} {user.Surname}",
                    UserPrincipalName = $"{user.AccountName}@{tenantOptions.TenantDomain}",
                    UsageLocation = "US",
                    MailNickname = user.AccountName,
                    AccountEnabled = true,
                    CreatedDateTime = null,
                    PasswordProfile = new AadUserPassword()
                    {
                        ForceChangePasswordNextSignInWithMfa = false,
                        ForceChangePasswordNextSignIn = false,
                        Password = user.Password
                    }
                };

                var requestResult = await httpExec.ExecPost($"{MSGraphApiUrl}/v1.0/users", null, aadUser);
                var newAadUser = JsonConvert.DeserializeObject<dynamic>(requestResult);

                aadUser.Id = newAadUser.id;
            }
            else
            {
                AadUserPassword pass = new AadUserPassword() { ForceChangePasswordNextSignIn = false, ForceChangePasswordNextSignInWithMfa = false, Password = user.Password };
                var requestResult = await httpExec.ExecPatch($"{MSGraphApiUrl}/v1.0/users/{aadUser.Id}", null, new { PasswordProfile = pass });
            }

            return aadUser;
        }

        public async Task<AadUser> GetAadUser(string userMailNickname)
        {
            await GetToken();
            HttpExec httpExec = new HttpExec(adToken.AccessToken);
            var usersResult = await httpExec.ExecGet($"{MSGraphApiUrl}/v1.0/users?$filter=userPrincipalName eq '{userMailNickname}@{tenantOptions.TenantDomain}'&$select=id,givenName,mailNickname,displayName,surname,userPrincipalName,accountEnabled");
            var users = JsonConvert.DeserializeObject<AadUsersList>(usersResult);

            if (users.Value.Count > 0)
            {
                return users.Value.First();
            }

            return null;
        }


        public async Task SetUserGroups(AadUser user, params string[] groupsNames)
        {
            await GetToken();
            HttpExec httpExec = new HttpExec(adToken.AccessToken);
            var groupsResult = await httpExec.ExecGet($"{MSGraphApiUrl}/v1.0/groups");
            var addGroups = JsonConvert.DeserializeObject<AadGroupsList>(groupsResult);
            var odatauser = $"{{\"@odata.id\":\"https://graph.microsoft.com/v1.0/users/{user.Id}\"}}";

            foreach (var groupName in groupsNames)
            {
                var group = addGroups.Value.FirstOrDefault(g => g.DisplayName.Replace(" ", "").Equals(groupName, StringComparison.InvariantCultureIgnoreCase));
                if (group != null)
                {
                    var response = await httpExec.ExecPost($"{MSGraphApiUrl}/v1.0/groups/{group.Id}/members/$ref", odatauser, null);
                }
            }
        }

        public async Task SetUserRoles(AadUser user, params string[] rolesNames)
        {
            await GetToken();
            HttpExec httpExec = new HttpExec(adToken.AccessToken);
            var groupsResult = await httpExec.ExecGet($"{MSGraphApiUrl}/v1.0/directoryRoles");
            var addRoles = JsonConvert.DeserializeObject<AadGroupsList>(groupsResult);
            var odatauser = $"{{\"@odata.id\":\"https://graph.microsoft.com/v1.0/users/{user.Id}\"}}";

            foreach (var roleName in rolesNames)
            {
                var role = addRoles.Value.FirstOrDefault(r => r.DisplayName.Replace(" ", "").Equals(roleName, StringComparison.InvariantCultureIgnoreCase));
                if (role != null)
                {
                    var response = await httpExec.ExecPost($"{MSGraphApiUrl}/v1.0/directoryRoles/{role.Id}/members/$ref", odatauser, null);
                }
            }
        }

        public async Task SetUserLicenses(AadUser user, params string[] skusNames)
        {
            await GetToken();
            HttpExec httpExec = new HttpExec(adToken.AccessToken);
            Guid skuId = Guid.Empty;

            var skusResult = await httpExec.ExecGet($"{MSGraphApiUrl}/v1.0/subscribedSkus");
            var skus = JsonConvert.DeserializeObject<AadSubscriptionSkuList>(skusResult);

            foreach (var skuName in skusNames)
            {
                var sku = skus.Value.FirstOrDefault(r => r.SkuPartNumber.Replace(" ", "").Equals(skuName, StringComparison.InvariantCultureIgnoreCase));
                if (sku != null)
                {
                    skuId = sku.SkuId;

                    var addLicenses = new List<AadLicense>(){ new AadLicense {
                        DisabledPlans = new object [] { },
                        SkuId  = skuId
                    }};

                    var licenses = new
                    {
                        addLicenses,
                        removeLicenses = new List<AadLicense>()
                    };

                    var requestResult = await httpExec.ExecPost($"{MSGraphApiUrl}/v1.0/users/{user.Id}/assignLicense", null, licenses);
                }
            }
        }
    }
}
