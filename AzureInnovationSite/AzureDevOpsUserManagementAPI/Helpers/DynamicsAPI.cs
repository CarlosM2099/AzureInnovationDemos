using AzureDevOpsUserManagementAPI.Models;
using AzureDevOpsUserManagementAPI.Utilities;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AzureDevOpsUserManagementAPI.Helpers
{
    public class DynamicsAPI
    {
        private AADToken adToken;
        private readonly AdTenantOptions tenantOptions;
        public const string EnvironmentMakerRoleName = "Environment Maker";
        public const string CommonDataServiceUserRoleName = "Common Data Service User";
        public DynamicsAPI(AdTenantOptions tenantOptions)
        {
            this.tenantOptions = tenantOptions;
            adToken = new AADToken();
        }
        public async Task GetToken()
        {
            if (string.IsNullOrEmpty(adToken.AccessToken))
            {
                AADTokenProvider tokenProvider = new AADTokenProvider(tenantOptions);
                adToken = await tokenProvider.GetToken(this.tenantOptions.CRMResource);
            }
        }

        public async Task<CRMUserList> GetUsers()
        {
            await GetToken();

            HttpExec exec = new HttpExec(adToken.AccessToken);

            var response = await exec.ExecGet($"{tenantOptions.CRMResource}api/data/v9.1/systemusers");

            return JsonConvert.DeserializeObject<CRMUserList>(response);
        }

        public async Task<CRMRoleList> GetRoles()
        {
            await GetToken();

            HttpExec exec = new HttpExec(adToken.AccessToken);

            var response = await exec.ExecGet($"{tenantOptions.CRMResource}api/data/v9.1/roles");

            return JsonConvert.DeserializeObject<CRMRoleList>(response);
        }

        public async Task AssignUserRole(string userAccount, string roleId)
        {
            var users = await GetUsers();

            var user = users.Value.FirstOrDefault(u => u.DomainName.Equals(userAccount, StringComparison.InvariantCultureIgnoreCase));

            if (user != null)
            {
                await AssignUserRole(user, roleId);
            }
        }

        public async Task AssignUserRole(CRMUser user, string roleId)
        {
            await GetToken();

            HttpExec exec = new HttpExec(adToken.AccessToken);

            string ruserRoleReq = $"{{\"@odata.id\":\"{tenantOptions.CRMResource}api/data/v9.1/roles({roleId})\"}}";

            var response = await exec.ExecPost($"{tenantOptions.CRMResource}api/data/v9.1/systemusers({user.UserId})/systemuserroles_association/$ref", ruserRoleReq);
        }
    }
}
