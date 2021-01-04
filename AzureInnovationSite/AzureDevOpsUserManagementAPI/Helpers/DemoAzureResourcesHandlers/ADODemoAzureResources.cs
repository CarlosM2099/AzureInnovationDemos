using AzureDevOpsUserManagementAPI.Models;
using AzureDevOpsUserManagementAPI.Utilities;
using AzureInnovationDemosDAL;
using AzureInnovationDemosDAL.BusinessModels;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Helpers.DemoAzureResourcesHandlers
{
    public class ADODemoAzureResources : IHandlerDemoAzureResources
    {
        private readonly AzureGraphAPI azureGraphAPI;
        private readonly GraphAPI graphAPI;
        private readonly AzureDemosDBManager dBManager;
        private readonly string DemoGenTemplate;
        private readonly string SubscriptionId;
        private readonly string SubscriptionName;
        private readonly string StorageAccount;
        private readonly string StorageAccountKey;
        public ADODemoAzureResources(AzureDemosDBManager demosDBManager, AdTenantOptions tenantOptions)
        {
            dBManager = demosDBManager;
            azureGraphAPI = new AzureGraphAPI(tenantOptions);
            graphAPI = new GraphAPI(tenantOptions);
            DemoGenTemplate = tenantOptions.DemoGenTemplate;
            SubscriptionId = tenantOptions.SubscriptionId;
            SubscriptionName = tenantOptions.SubscriptionName;

            StorageAccount = tenantOptions.StorageAccount;
            StorageAccountKey = tenantOptions.StorageAccountKey;
        }

        public async Task<dynamic> SetDemoAzureResources(AzureInnovationDemosDAL.BusinessModels.AadUser user, int demoId, string resourceName)
        {
            AdServiceEndpointTemplateValues templateValues = new AdServiceEndpointTemplateValues();            
            string appName = null, aadAppId = null, appPublishingProfile = null;
            var azureResourceSite = await azureGraphAPI.GetAzureResourceSite(resourceName);
            var azureSiteRolesAssignments = await azureGraphAPI.GetResourceGroupRoleAssignment(azureResourceSite.Properties.ResourceGroup);

            appName = azureResourceSite.Name;

            foreach (var rolesAssignment in azureSiteRolesAssignments.Value)
            {
                string scope = rolesAssignment.Properties.Scope;
                if (scope.ToLower().Contains($"resourceGroups/{azureResourceSite.Properties.ResourceGroup}".ToLower()))
                {
                    string appPrincipalId = rolesAssignment.Properties.PrincipalId;
                    var appDirectoryObject = await graphAPI.GetDirectoryObject(appPrincipalId);

                    aadAppId = appDirectoryObject.AppId;

                    if (appDirectoryObject != null && appDirectoryObject.DisplayName.Contains("ADODemoAadApp"))
                    {                      
                        appPublishingProfile = await azureGraphAPI.GetAppPublishingProfile(azureResourceSite.Properties.ResourceGroup, appName);
                    }
                }
            }

            if (string.IsNullOrEmpty(aadAppId))
            {
                throw new Exception($"Contributor AAD app wasn't found for web app {appName}");
            }            

            if (!string.IsNullOrEmpty(aadAppId) && !string.IsNullOrEmpty(appName))
            {                
                return new { templateLocation = DemoGenTemplate, appPublishingProfile };
            }

            return null;
        }

        private string CreateDemoTemplate(string appName, AdServiceEndpointTemplateValues templateValues)
        {
            Uri uri = new Uri(DemoGenTemplate);

            string fileName = Path.GetFileNameWithoutExtension(uri.LocalPath);
            string fileExtension = Path.GetExtension(uri.LocalPath);
            string newFileName = $"{fileName}-{appName}{fileExtension}";

            using (var a = ReplaceTemplateValues(appName, templateValues))
            {
                CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(StorageAccount, StorageAccountKey), true);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("templates");
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(newFileName);
                blockBlob.UploadFromStream(a);

                return DemoGenTemplate.Replace(fileName, $"{fileName}-{appName}");
            }
        }

        private Stream ReplaceTemplateValues(string webAppName, AdServiceEndpointTemplateValues templateValues)
        {
            using (var ms = GetResourceStream(DemoGenTemplate))
            {
                // using ZipArchive class via System.IO.Compression [4.3.0]
                using (var zip = new ZipArchive(ms, ZipArchiveMode.Update))
                {
                    var fileName = @"ReleaseDefinitions/Website-CD.json";
                    var jObject = JsonConvert.DeserializeObject<JObject>(zip.ReadTextFile(fileName));
                    jObject["environments"][0]["deployPhases"][0]["workflowTasks"][0]["inputs"]["WebAppName"] = webAppName;

                    // Remove the old zip entry.
                    zip.GetEntry(fileName).Delete();

                    var newFile = zip.CreateEntry(fileName);
                    using (var sw = new StreamWriter(newFile.Open()))
                    {
                        sw.Write(jObject.ToString(Formatting.Indented).Trim());
                    }

                    fileName = @"ServiceEndpoints/Tailwind Traders Site Azure Connection.json";
                    jObject = JsonConvert.DeserializeObject<JObject>(zip.ReadTextFile(fileName));
                    jObject["authorization"]["parameters"]["serviceprincipalid"] = templateValues.ServicePrincipalId;
                    jObject["authorization"]["parameters"]["serviceprincipalkey"] = templateValues.ServicePrincipalKey;
                    jObject["authorization"]["parameters"]["scope"] = templateValues.Scope;

                    jObject["data"]["subscriptionId"] = SubscriptionId;
                    jObject["data"]["subscriptionName"] = SubscriptionName;
                    jObject["data"]["azureSpnPermissions"] = templateValues.AzureSpnPermissions;
                    jObject["data"]["azureSpnRoleAssignmentId"] = templateValues.AzureSpnRoleAssignmentId;

                    // Remove the old zip entry.
                    zip.GetEntry(fileName).Delete();

                    newFile = zip.CreateEntry(fileName);
                    using (var sw = new StreamWriter(newFile.Open()))
                    {
                        sw.Write(jObject.ToString(Formatting.Indented).Trim());
                    }

                    ms.Flush();
                }

                // ms is technically disposed at this point, but the .ToArray() property continues to work.
                var bytes = ms.ToArray();

                // do not pass bytes into the constructor of memory stream, it will become a fixed size stream.
                var resultMs = new MemoryStream();
                resultMs.Write(bytes, 0, bytes.Length);
                resultMs.Flush();
                resultMs.Seek(0, SeekOrigin.Begin);
                return resultMs;
            }
        }

        private MemoryStream GetResourceStream(string fileName)
        {
            var ms = new MemoryStream();
            using (var wc = new System.Net.WebClient())
            {
                var dataStream = wc.OpenRead(fileName);
                dataStream.CopyTo(ms);
                ms.Flush();
            }

            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public async Task<object> ValidateDemoAzureResources(int demoId)
        {
            var demoAzureResources = await dBManager.GetDemoAzureResources(demoId);
            int totalResources = demoAzureResources.Count;
            int availableResourcesCount = 0;
            bool availableResources = true;

            availableResourcesCount = demoAzureResources.Count(r => r.LockedUntil == null);
            availableResources = availableResourcesCount > 0;

            if (!availableResources)
            {
                var nextAvailable = demoAzureResources
                    .OrderBy(rs => rs.LockedUntil)
                    .ThenBy(rs => rs.LockedUntil.Value.TimeOfDay)
                    .First();

                return new { availableResources, nextAvailable = nextAvailable.LockedUntil, availableResourcesCount };
            }

            return new { availableResources, availableResourcesCount };
        }

        public async Task<object> GetUserDemoAzureResourceExpiration(int demoId, int userId)
        {
            var userResource = await dBManager.GetUserDemoAzureResource(demoId, userId);

            return new { expirationDate = userResource.DemoAzureResource.LockedUntil };
        }
    }
}