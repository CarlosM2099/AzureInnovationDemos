using AzureInnovationDemos.Models;
using AzureInnovationDemos.Utilities;
using AzureInnovationDemosDAL;
using AzureInnovationDemosDAL.Models;
using AzureInnovationDemosDAL.Utilities;
using HtmlAgilityPack;
using MarkdownSharp;
using Microsoft.AspNetCore.SignalR;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AzureInnovationDemos.Helpers
{
    public class MDConverter
    {
        private readonly AzureDemosDBContext dBContext;
        private readonly AzAppsDemosAPIOptions demosAPIOptions;
        private readonly GlobalSettings globalSettings;
        private readonly IHubContext<LogHub> logHubContext;
        public MDConverter(AzureDemosDBContext dBContext, AzAppsDemosAPIOptions demosAPIOptions, GlobalSettings globalSettings, IHubContext<LogHub> hubcontext)
        {
            this.dBContext = dBContext;
            this.demosAPIOptions = demosAPIOptions;
            this.globalSettings = globalSettings;
            this.logHubContext = hubcontext;
        }

        public async Task ConvertMDGuides()
        {
            AzureDemosDBManager demosDBManager = new AzureDemosDBManager(dBContext);
            GuideContentDB guideContentDB = new GuideContentDB(demosAPIOptions.DemosGuideDB);
            var demoAssets = await demosDBManager.GetDemosAssets();

            await logHubContext.Clients.All.SendAsync("ReceiveMessage", $"Starting guide content sync");

            demoAssets = demoAssets
                .Where(a => a.Type == DemoAssetTypeEnum.PrivateMD)
                .ToList();

            await logHubContext.Clients.All.SendAsync("ReceiveMessage", $"Guide content sync started");

            foreach (var asset in demoAssets)
            {
                await logHubContext.Clients.All.SendAsync("ReceiveMessage", $"Getting content for guide: {asset.Alias} ");

                var mdHTMLContent = await ConvertMDGuide(asset.Value);
                var demoGuide = await guideContentDB.GuideExists(asset.Id);

                if (!demoGuide)
                {
                    await guideContentDB.InsertGuide(asset.Id, mdHTMLContent);
                }
                else
                {
                    await guideContentDB.UpdateGuide(asset.Id, mdHTMLContent);
                }

                await logHubContext.Clients.All.SendAsync("ReceiveMessage", $"Guide {asset.Alias} content converted to HTML");
            }

            await logHubContext.Clients.All.SendAsync("ReceiveMessage", $"Guide sync completed");
        }

        public async Task<string> ConvertMDGuide(string guideUrl)
        {
            HtmlDocument htmlDocument = new HtmlDocument();

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", globalSettings.GitHubToken);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3.raw");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "AzureInnovationDemoScripts");

                var response = await httpClient.GetAsync(guideUrl);
                var fileContent = await response.Content.ReadAsStringAsync();
                string htmlContent = ConvertMD(fileContent);

                htmlDocument.LoadHtml(htmlContent);

                Uri guideUri = new Uri(guideUrl);

                response = await httpClient.GetAsync($"{guideUrl.Replace(Path.GetFileName(guideUrl), "")}Images{guideUri.Query}");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    fileContent = await response.Content.ReadAsStringAsync();

                    List<GitHubFile> gitHubFiles = new List<GitHubFile>();
                    Dictionary<string, string> imagesUrls = new Dictionary<string, string>();

                    gitHubFiles = JsonConvert.DeserializeObject<List<GitHubFile>>(fileContent);

                    foreach (var file in gitHubFiles)
                    {
                        imagesUrls[file.Name.ToLower()] = file.Url;
                    }

                    var documentImages = htmlDocument.DocumentNode.SelectNodes("//img");
                    if (documentImages != null)
                    {
                        foreach (var img in documentImages)
                        {
                            string imgSrc = img.Attributes["src"].Value.ToLower().Replace("images/", "");

                            if (imagesUrls.ContainsKey(imgSrc))
                            {
                                try
                                {
                                    var imgUrl = imagesUrls[imgSrc];
                                    var bytes = await httpClient.GetByteArrayAsync(imgUrl);
                                    Uri imgUri = new Uri(imgUrl);
                                    var uploadedImage = UploadGuideImage(Path.Combine(imgUri.Segments[6].Replace("%20", " "), imgSrc), bytes);
                                    img.Attributes["src"].Value = uploadedImage;
                                }
                                catch
                                {
                                    await logHubContext.Clients.All.SendAsync("ReceiveMessage", $"Image: {imgSrc} failed");
                                }
                            }
                            else
                            {
                                await logHubContext.Clients.All.SendAsync("ReceiveMessage", $"Image: {imgSrc} not found");
                            }
                        }
                    }
                }
            }

            return htmlDocument.DocumentNode.InnerHtml;
        }

        public string ConvertMD(string inputMDText)
        {
            var markdown = new Markdown();
            var actual = markdown.Transform(inputMDText);

            return actual;
        }

        public string UploadGuideImage(string imageName, byte[] imageBytes)
        {
            string storageAccount = globalSettings.StorageAccount;
            string storageAccountKey = globalSettings.StorageAccountKey;
            CloudStorageAccount cloudStorageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(storageAccount, storageAccountKey), true);
            CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("guides");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(imageName);

            blockBlob.UploadFromByteArrayAsync(imageBytes, 0, imageBytes.Length);

            return blockBlob.StorageUri.PrimaryUri.AbsoluteUri;
        }
    }
}
