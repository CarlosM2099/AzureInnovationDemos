using AzureInnovationDemosDAL;
using AzureInnovationDemosDAL.Models;
using AzureInnovationDemosDAL.Utilities;
using AzureResourcesPoolManager.Models;
using HtmlAgilityPack;
using MarkdownSharp;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AzureResourcesPoolManager.Utilities
{
    public class MDConverter
    {
        AzureDemosDBContext dBContext;
        AdOptions adOptions;
        public MDConverter(AzureDemosDBContext dBContext, AdOptions adOptions)
        {            
            this.dBContext = dBContext;
            this.adOptions = adOptions;
        }

        public async Task ConvertMDGuides()
        {
            AzureDemosDBManager demosDBManager = new AzureDemosDBManager(dBContext);
            GuideContentDB guideContentDB = new GuideContentDB(adOptions.GuideContentDB);
            var demoAssets = await demosDBManager.GetDemosAssets();

            demoAssets = demoAssets
                .Where(a => a.Type == DemoAssetTypeEnum.PrivateMD)
                .ToList();

            foreach (var asset in demoAssets)
            {
                Console.WriteLine($"Getting content for guide: {asset.Alias} ");

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

                Console.WriteLine($"Guide {asset.Alias} content converted to HTML");
            }

            Console.WriteLine($"Guide convertion finished");
        }

        public async Task<string> ConvertMDGuide(string guideUrl)
        {
            HtmlDocument htmlDocument = new HtmlDocument();

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", adOptions.GitHubToken);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3.raw");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "AzureInnovationDemoScripts");

                var response = await httpClient.GetAsync(guideUrl);
                var fileContent = await response.Content.ReadAsStringAsync();
                string htmlContent = ConvertMD(fileContent);

                htmlDocument.LoadHtml(htmlContent);

                response = await httpClient.GetAsync($"{guideUrl.Replace(Path.GetFileName(guideUrl), "")}Images");

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
                                    Console.WriteLine($"Image: {imgSrc} failed");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Image: {imgSrc} not found");
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
            string storageAccount = adOptions.StorageAccount;
            string storageAccountKey = adOptions.StorageAccountKey;
            CloudStorageAccount cloudStorageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(storageAccount, storageAccountKey), true);
            CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("guides");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(imageName);

            blockBlob.UploadFromByteArrayAsync(imageBytes, 0, imageBytes.Length);

            return blockBlob.StorageUri.PrimaryUri.AbsoluteUri;
        }
    }
}
