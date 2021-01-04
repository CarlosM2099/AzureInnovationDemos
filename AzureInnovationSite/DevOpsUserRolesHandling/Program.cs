using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsUserRolesHandling
{
    class Program
    {
        private static string MSGraphApiUrl = "https://graph.microsoft.com";
        private static string AzureDevAPIUrl = "https://dev.azure.com";
        private static string AzureDevAPIVersion = "api-version=5.0-preview.1";
        private static string VsspsAPIUrl = "https://vssps.dev.azure.com";
        private static string VsaexAPIUrl = "https://vsaex.dev.azure.com";
        private static string TenantDomain = "azureapps.onmicrosoft.com";

        private static string ADOResourceId = "499b84ac-1321-427f-aa17-267ca6975798";
        private static string AadAppClientId = "6fe1dbc8-0b8a-4cdf-a408-8e62fad195c0";
        private static string AdoAccountName = "MeganB0296";


        static void Main(string[] args)
        {

            Console.ReadLine();
            CallAPI();
            //GetAzureDevOps();
            //GetUsers();
        }
        private static void CallAPI()
        {
            AuthenticationResult authenticationResult = null;
            var authenticationContext = new AuthenticationContext($"https://login.microsoftonline.com/{TenantDomain}", false);
            ClientCredential clientCredential = new ClientCredential("cf5e7f97-89cc-4b19-809f-d7c5f0f8221a", "1z_-oO*ZzBo_KauRmE3og71ZE+RI[VS4");
            authenticationResult = authenticationContext.AcquireTokenAsync("https://azureapps.onmicrosoft.com/AzureDevOpsUserManagementAPI", clientCredential).Result;

            string accessToken = authenticationResult.AccessToken;

            HttpExec httpExec = new HttpExec(accessToken);

            AadUserCreation newUser = new AadUserCreation()
            {
                GivenName = "Test",
                Surname = "UserV",
                MailNickname = "testuv",
                Password = "TstU@1234"
            };

           string result = httpExec.ExecPost("https://localhost:44389/api/users", null,  newUser).Result;
        }


        private static void GetAzureDevOps()
        {
            AuthenticationResult authenticationResult = null;
            var authenticationContext = new AuthenticationContext($"https://login.microsoftonline.com/{TenantDomain}", false);
            UserPasswordCredential creds = new UserPasswordCredential($"MeganB@{TenantDomain}", "anupk@9363");
            ClientCredential clientCredential = new ClientCredential(AadAppClientId, "]+LO+j3oyZ-U9t4VkS3oYUWyqjyA_RhV");
            authenticationResult = authenticationContext.AcquireTokenAsync(ADOResourceId, clientCredential).Result;

            string accessToken = authenticationResult.AccessToken;

            authenticationResult = authenticationContext.AcquireTokenAsync(MSGraphApiUrl, clientCredential).Result;

            string graphAccessToken = authenticationResult.AccessToken;

            var groups = GetGraphGroups(accessToken);
            var projectAdminsGroup = groups.Value.FirstOrDefault(g => g.DisplayName == "Project Administrators");

            AadUserCreation newUser = new AadUserCreation()
            {
                GivenName = "Test",
                Surname = "UserIV",
                MailNickname = "testuiv",
                Password = "TstU@1234"
            };

            var addedUser = AddAadUser(newUser, graphAccessToken);

            var foundUser = FindUser(newUser.MailNickname, accessToken);

            if (string.IsNullOrEmpty(foundUser.DisplayName))
            {
                AadUser tenantUser = JsonConvert.DeserializeObject<AadUser>(addedUser);
                foundUser.DisplayName = tenantUser.DisplayName;
            }

            if (string.IsNullOrEmpty(foundUser.Mail))
            {
                AadUser tenantUser = JsonConvert.DeserializeObject<AadUser>(addedUser);
                foundUser.Mail = tenantUser.UserPrincipalName;
            }

            AddUserToOrganization(foundUser, accessToken);
            AddUserToProject(foundUser, projectAdminsGroup, accessToken);
        }

        public static void ADOCalls()
        {
            string account = "MeganB0296";            
            AuthenticationResult authenticationResult = null;
            var authenticationContext = new AuthenticationContext($"https://login.microsoftonline.com/{TenantDomain}", false);
            UserPasswordCredential creds = new UserPasswordCredential($"MeganB@{TenantDomain}", "anupk@9363");
            authenticationResult = authenticationContext.AcquireTokenAsync("499b84ac-1321-427f-aa17-267ca6975798", "6fe1dbc8-0b8a-4cdf-a408-8e62fad195c0", creds).Result;
            HttpClient httpClient = new HttpClient();

            string accessToken = authenticationResult.AccessToken;

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = httpClient.GetAsync($"{VsaexAPIUrl}/{account}/_apis/UserEntitlements?doNotSendInviteForNewUsers=false ").Result;
            var result = response.Content.ReadAsStringAsync().Result;

            response = httpClient.GetAsync($"{AzureDevAPIUrl}/{account}/_apis/projects?api-version=5.0").Result;
            result = response.Content.ReadAsStringAsync().Result;
            var projects = JsonConvert.DeserializeObject<DevOpsProjectsList>(result).Value;

            response = httpClient.GetAsync($"{AzureDevAPIUrl}//{account}/_apis/projects/{projects[0].Id}/teams/?{AzureDevAPIVersion}").Result;
            result = response.Content.ReadAsStringAsync().Result;
            var teams = JsonConvert.DeserializeObject<DevOpsTeamsList>(result).Value;

            foreach (var team in teams)
            {
                response = httpClient.GetAsync($"{team.Url}/members/?api-version=5.0").Result;
                result = response.Content.ReadAsStringAsync().Result;
                var teamMembers = JsonConvert.DeserializeObject<DevOpsTeamMemberList>(result).Value;
            }

            response = httpClient.GetAsync($"{VsspsAPIUrl}/{account}/_apis/graph/groups/?{AzureDevAPIVersion}").Result;
            result = response.Content.ReadAsStringAsync().Result;
            var groups = JsonConvert.DeserializeObject<GraphGroupsList>(result).Value;

            foreach (var group in groups)
            {
                Console.WriteLine(group.DisplayName);
                Console.WriteLine(group.Description);

                response = httpClient.GetAsync($"{group.Links.Memberships.Href}?direction=down&{AzureDevAPIVersion}").Result;
                result = response.Content.ReadAsStringAsync().Result;
                var groupMemberships = JsonConvert.DeserializeObject<GraphMemberShipList>(result).Value;

                Console.WriteLine($"Members ({groupMemberships.Count}):");

                foreach (var groupMembership in groupMemberships)
                {
                    response = httpClient.GetAsync($"{VsspsAPIUrl}/{account}/_apis/graph/users/{groupMembership.MemberDescriptor}?{AzureDevAPIVersion}").Result;
                    result = response.Content.ReadAsStringAsync().Result;
                    var user = JsonConvert.DeserializeObject<GraphUser>(result);
                    Console.WriteLine($"PN: {user.PrincipalName}, Id: {user.Descriptor}");
                }

                Console.WriteLine();
            }

            var projectAdmins = groups.FirstOrDefault(g => g.DisplayName == "Project Administrators");

            var foundUser = FindUser("TestU", accessToken);
            AddUserToOrganization(foundUser, accessToken);
            AddUserToProject(foundUser, projectAdmins, accessToken);

            response = httpClient.GetAsync($"{VsspsAPIUrl}/{account}/_apis/graph/users/?{AzureDevAPIVersion}").Result;
            result = response.Content.ReadAsStringAsync().Result;
            var users = JsonConvert.DeserializeObject<GraphUsersList>(result).Value;


            foreach (var user in users)
            {
                Console.WriteLine(user.DisplayName);
                Console.WriteLine("Groups:");
                response = httpClient.GetAsync($"{user.Links.Memberships.Href}?direction=up&{AzureDevAPIVersion}").Result;
                result = response.Content.ReadAsStringAsync().Result;
                var userGroups = JsonConvert.DeserializeObject<GraphMemberShipList>(result).Value;

                foreach (var userGroup in userGroups)
                {
                    response = httpClient.GetAsync($"{VsspsAPIUrl}/{account}/_apis/graph/groups/{userGroup.ContainerDescriptor}?{AzureDevAPIVersion}").Result;
                    result = response.Content.ReadAsStringAsync().Result;
                    var group = JsonConvert.DeserializeObject<GraphUser>(result);
                    Console.WriteLine(group.DisplayName);
                }

                Console.WriteLine();
            }

            //var reqContent = new { principalName = foundUser.Mail };
            //HttpContent content = new StringContent(JsonConvert.SerializeObject(reqContent), Encoding.UTF8, "application/json");

            //response = httpClient.PostAsync($"{VsspsAPIUrl}/{account}/_apis/graph/users?groupDescriptors=vssgp.Uy0xLTktMTU1MTM3NDI0NS0zOTcxNzUzNzcyLTE3NjQ1MzUzNjctMjQwMjExMjA5NC0yNzMxNjE5MTU0LTAtMC0wLTAtMQ&{AzureDevAPIVersion}", content).Result;
            //result = response.Content.ReadAsStringAsync().Result;
            //var createdUser = JsonConvert.DeserializeObject<GraphUser>(result);

            //response = httpClient.GetAsync($"{VsspsAPIUrl}/{account}/_apis/graph/users/?{AzureDevAPIVersion}").Result;
            //result = response.Content.ReadAsStringAsync().Result;
            //users = JsonConvert.DeserializeObject<GraphUsersList>(result).Value;

            //response = httpClient.GetAsync($"{AzureDevAPIUrl}/{account}/{project}/_apis/build/definitions/?api-version=5.0").Result;
            //result = response.Content.ReadAsStringAsync().Result;
            //var builds = JsonConvert.DeserializeObject<GraphUsersList>(result).Value;}
        }
        public static IdentityUser FindUser(string userAccount, string accessToken)
        {
            IdentityUser identity = null;
            HttpExec httpExec = new HttpExec(accessToken);

            var identitiesQuery = new
            {
                query = userAccount,
                identityTypes = new[] { "user" },
                operationScopes = new[] { "ims", "source" },
                properties = new[] { "Mail", "MailNickname" }
            };

            var url = $"{AzureDevAPIUrl}/{AdoAccountName}/_apis/IdentityPicker/Identities?{AzureDevAPIVersion}";
            var identitiesResult = JsonConvert.DeserializeObject<IdentityPickerQuery>(httpExec.ExecPost(url, null, identitiesQuery).Result).Results;

            if (identitiesResult != null && identitiesResult.Count > 0)
            {
                var identityResult = identitiesResult.FirstOrDefault();
                if (identityResult.Identities != null && identityResult.Identities.Count > 0)
                {
                    identity = identityResult.Identities.FirstOrDefault();
                }
            }

            return identity;
        }

        public static GraphGroupsList GetGraphGroups(string accessToken)
        {
            GraphGroupsList groups = null;
            HttpExec httpExec = new HttpExec(accessToken);

            var url = $"{VsspsAPIUrl}/{AdoAccountName}/_apis/graph/groups/?{AzureDevAPIVersion}";
            groups = JsonConvert.DeserializeObject<GraphGroupsList>(httpExec.ExecGet(url).Result);

            return groups;
        }

        public static void AddUserToOrganization(IdentityUser user, string accessToken)
        {
            HttpExec httpExec = new HttpExec(accessToken);

            var entitlementQuery = new object[] {
                new {
                    from = "",
                    op =0,
                    path ="",
                    value= new {
                        accessLevel = new {licensingSource =1,accountLicenseType = 2,msdnLicenseType = 0,licenseDisplayName="Basic",status =0,statusMessage="",assignmentSource=1},
                        extensions = new object[]{},
                        projectEntitlements= new object []{},
                        user = new { displayName=user.DisplayName,
                            origin =user.OriginDirectory,
                            originId =user.OriginId,
                            principalName =user.Mail,subjectKind="user"
                        }
                    }
                }
            };

            var url = $"{VsaexAPIUrl}/{AdoAccountName}/_apis/UserEntitlements?doNotSendInviteForNewUsers=false&{AzureDevAPIVersion}";
            var identitiesResult = JsonConvert.DeserializeObject<IdentityPickerQuery>(httpExec.ExecPatch(url, null, entitlementQuery, "application/json-patch+json").Result).Results;
        }

        public static void AddUserToProject(IdentityUser user, GraphGroup group, string accessToken)
        {
            HttpExec httpExec = new HttpExec(accessToken);

            var reqContent = new { principalName = user.Mail };
            var url = $"{VsspsAPIUrl}/{AdoAccountName}/_apis/graph/users?groupDescriptors={group.Descriptor}&{AzureDevAPIVersion}";
            var addedUser = JsonConvert.DeserializeObject<GraphUser>(httpExec.ExecPost(url, null, reqContent).Result);
        }

        public static string AddAadUser(AadUserCreation user, string accessToken)
        {
            HttpExec httpExec = new HttpExec(accessToken);
            AadUser tenantUser = new AadUser();
            tenantUser.GivenName = user.GivenName;
            tenantUser.Surname = user.Surname;
            tenantUser.DisplayName = $"{user.GivenName} {user.Surname}";
            tenantUser.UserPrincipalName = $"{user.MailNickname}@{TenantDomain}";
            tenantUser.UsageLocation = "US";
            tenantUser.MailNickname = user.MailNickname;
            tenantUser.AccountEnabled = true;
            tenantUser.CreatedDateTime = null;
            tenantUser.PasswordProfile = new AadUserPassword()
            {
                ForceChangePasswordNextSignInWithMfa = false,
                ForceChangePasswordNextSignIn = false,
                Password = user.Password
            };

            var requestResult = httpExec.ExecPost($"{MSGraphApiUrl}/v1.0/users", null, tenantUser).Result;
            return requestResult;
        }

        public static void GetUsers()
        {
            AuthenticationResult authenticationResult = null;
            var authenticationContext = new AuthenticationContext($"https://login.microsoftonline.com/{TenantDomain}", false);
            UserPasswordCredential creds = new UserPasswordCredential($"MeganB@{TenantDomain}", "anupk@9363");
            authenticationResult = authenticationContext.AcquireTokenAsync(MSGraphApiUrl, "6fe1dbc8-0b8a-4cdf-a408-8e62fad195c0", creds).Result;
            HttpExec httpExec = new HttpExec(authenticationResult.AccessToken);

            var roles = JsonConvert.DeserializeObject<AadRoleList>(httpExec.ExecGet($"{MSGraphApiUrl}/v1.0/directoryRoles").Result).Value;
            var users = JsonConvert.DeserializeObject<AadUsersList>(httpExec.ExecGet($"{MSGraphApiUrl}/v1.0/users?" +
               $"$select=createdDateTime,displayName,givenName,id,mail,surname,userPrincipalName&$top=999").Result).Value;

            foreach (var role in roles)
            {
                var roleMembers = JsonConvert.DeserializeObject<AadRoleMemberList>(httpExec.ExecGet($"{MSGraphApiUrl}/v1.0/directoryRoles/{role.Id}/members").Result).Value;
            }

            foreach (var user in users)
            {
                string memberOf = httpExec.ExecGet($"{MSGraphApiUrl}/v1.0/users/{user.UserPrincipalName}/memberOf").Result;

                Console.WriteLine($"User: {user.UserPrincipalName}");
                Console.WriteLine($"Member Of: {memberOf}");

                memberOf = httpExec.ExecPost($"{MSGraphApiUrl}/v1.0/users/{user.UserPrincipalName}/getMemberGroups", null, new { securityEnabledOnly = true }).Result;
                Console.WriteLine($"Member Groups: {memberOf}");

                var memberGroups = JsonConvert.DeserializeObject<AadRoleList>(memberOf).Value;

                Console.WriteLine();
            }
        }
    }
}
