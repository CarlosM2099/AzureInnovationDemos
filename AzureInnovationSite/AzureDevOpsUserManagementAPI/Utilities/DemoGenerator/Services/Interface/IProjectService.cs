using AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Interface
{
    public interface IProjectService
    {
        void RemoveKey(string id);

        void AddMessage(string id, string message);

        JObject GetStatusMessage(string id);

        HttpResponseMessage GetprojectList(string accname, string pat);        

        (string projectId, string accountName, string templateUsed) CreateProjectEnvironment(Project model);

        bool CheckForInstalledExtensions(string extensionJsonFile, string token, string account);

        bool InstallExtensions(Project model, string accountName, string PAT);
    }
}