using AzureDevOpsUserManagementAPI.Models;
using AzureDevOpsUserManagementAPI.Utilities;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Helpers
{
    public class DevOpsAPI
    {
        private readonly string ADOResourceId = "499b84ac-1321-427f-aa17-267ca6975798";
        private readonly string AzureDevAPIUrl = "https://dev.azure.com";
        private readonly string VsspsAPIUrl = "https://vssps.dev.azure.com";
        private readonly string VsaexAPIUrl = "https://vsaex.dev.azure.com";
        private readonly string AzureDevAPIVersion = "api-version=5.0-preview.1";        
        private AADToken adToken;
      
        private readonly AdTenantOptions tenantOptions;
        public DevOpsAPI(AdTenantOptions tenantOptions)
        {
            this.tenantOptions = tenantOptions;
            adToken = new AADToken();
        }

        public async Task GetToken()
        {
            if (string.IsNullOrEmpty(adToken.AccessToken))
            {
                AADTokenProvider tokenProvider = new AADTokenProvider(tenantOptions);
                adToken = await tokenProvider.GetToken(ADOResourceId);
            }
        }
        public async Task AddUserToProject(IdentityUser user, GraphGroup projectGroup, string accountName)
        {
            await GetToken();
            HttpExec httpExec = new HttpExec(adToken.AccessToken);

            var reqContent = new { principalName = user.Mail };
            var url = $"{VsspsAPIUrl}/{accountName}/_apis/graph/users?groupDescriptors={projectGroup.Descriptor}&{AzureDevAPIVersion}";
            var addedUser = JsonConvert.DeserializeObject<GraphUser>(await httpExec.ExecPost(url, null, reqContent));
        }


        public async Task CreateOrganization(string organizationName)
        {
            await GetToken();
            HttpExec httpExec = new HttpExec(adToken.AccessToken);

            var url = $"{VsaexAPIUrl}/_apis/HostAcquisition/NameAvailability/{organizationName}";
            var nameAvailability = JsonConvert.DeserializeObject<OrganizationAcquisitionNameAvailability>(await httpExec.ExecGet(url));

            if (nameAvailability.IsAvailable)
            {
                url = $"{VsaexAPIUrl}/_apis/HostAcquisition/Collections?collectionName={organizationName}&preferredRegion=CUS&{AzureDevAPIVersion}";
                var organizationResult = JsonConvert.DeserializeObject<OrganizationAcquisition>(await httpExec.ExecPost(url, null, new { }));
            }
        }


        public async Task InstallOrganizationExtension(string organizationName, string publisherName, string extensionName, string version)
        {
            await GetToken();
            HttpExec httpExec = new HttpExec(adToken.AccessToken);

            var url = $"{AzureDevAPIUrl}/_apis/extensionmanagement/NameAvailability/installedextensionsbyname/{publisherName}/{extensionName}/{version}?{AzureDevAPIVersion}";
            var installedExtension = JsonConvert.DeserializeObject<InstallExtensionResult>(await httpExec.ExecPost(url, ""));
        }


        public async Task InstallOrganizationExtension(string organizationName, string itemName)
        {
            await GetToken();
            HttpExec httpExec = new HttpExec(adToken.AccessToken);
            object nullObj = null;

            var installReq = new { assignmentType = 0, billingId = nullObj, itemId = itemName, operationType = 1, quantity = 0, properties = new { } };
            var url = $"https://{organizationName}.extmgmt.visualstudio.com/_apis/ExtensionManagement/AcquisitionRequests?{AzureDevAPIVersion}";

            var installedExtension = JsonConvert.DeserializeObject<ExtensionAcquisition>(await httpExec.ExecPost(url, null, installReq));
        }


        public async Task AddUserToOrganization(IdentityUser user, string organizationName)
        {
            await GetToken();
            HttpExec httpExec = new HttpExec(adToken.AccessToken);

            var entitlementQuery = new object[] {
                new {
                    from = "",
                    op = 0,
                    path = "",
                    value = new {
                        accessLevel = new { licensingSource = 1, accountLicenseType = 2, msdnLicenseType = 0, licenseDisplayName = "Basic", status = 0, statusMessage = "", assignmentSource = 1},
                        extensions = new object[] {},
                        projectEntitlements= new object [] {},
                        user = new {
                            displayName = user.DisplayName,
                            origin = user.OriginDirectory,
                            originId = user.OriginId,
                            principalName = user.Mail,
                            subjectKind = "user"
                        }
                    }
                }
            };

            var url = $"{VsaexAPIUrl}/{organizationName}/_apis/UserEntitlements?doNotSendInviteForNewUsers=false&{AzureDevAPIVersion}";
            var identitiesResult = JsonConvert.DeserializeObject<IdentityPickerQuery>(await httpExec.ExecPatch(url, null, entitlementQuery, "application/json-patch+json")).Results;
        }

        public async Task<GraphGroupsList> GetGraphGroups(string accountName)
        {
            await GetToken();
            GraphGroupsList groups = null;
            HttpExec httpExec = new HttpExec(adToken.AccessToken);

            var url = $"{VsspsAPIUrl}/{accountName}/_apis/graph/groups/?{AzureDevAPIVersion}";
            groups = JsonConvert.DeserializeObject<GraphGroupsList>(await httpExec.ExecGet(url));

            return groups;
        }

        public async Task<DevOpsProjectsList> GetProjects(string accountName)
        {
            await GetToken();
            DevOpsProjectsList projects = null;
            HttpExec httpExec = new HttpExec(adToken.AccessToken);

            var url = $"{AzureDevAPIUrl}/{accountName}/_apis/projects?{AzureDevAPIVersion}";
            projects = JsonConvert.DeserializeObject<DevOpsProjectsList>(await httpExec.ExecGet(url));

            return projects;
        }

        public async Task<GraphUsersList> GetUsers(string accountName)
        {
            await GetToken();
            GraphUsersList users = null;
            HttpExec httpExec = new HttpExec(adToken.AccessToken);

            var url = $"{VsspsAPIUrl}/{accountName}/_apis/graph/users/?{AzureDevAPIVersion}";
            users = JsonConvert.DeserializeObject<GraphUsersList>(await httpExec.ExecGet(url));

            return users;
        }

        public async Task<IdentityUser> GetUser(string userAccount, string organizationName)
        {
            await GetToken();
            IdentityUser identity = null;
            HttpExec httpExec = new HttpExec(adToken.AccessToken);

            var identitiesQuery = new
            {
                query = userAccount,
                identityTypes = new[] { "user" },
                operationScopes = new[] { "ims", "source" },
                properties = new[] { "Mail", "MailNickname" }
            };

            var url = $"{AzureDevAPIUrl}/{organizationName}/_apis/IdentityPicker/Identities?{AzureDevAPIVersion}";
            var identitiesResult = JsonConvert.DeserializeObject<IdentityPickerQuery>(await httpExec.ExecPost(url, null, identitiesQuery)).Results;

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

        public async Task<UserEntitlements> GetUserEntitlements(string accountName)
        {
            await GetToken();
            UserEntitlements usersEntitlements = null;
            HttpExec httpExec = new HttpExec(adToken.AccessToken);

            var url = $"{VsspsAPIUrl}/{accountName}/_apis/UserEntitlements?doNotSendInviteForNewUsers=false";
            usersEntitlements = JsonConvert.DeserializeObject<UserEntitlements>(await httpExec.ExecGet(url));

            return usersEntitlements;
        }
    }
}
