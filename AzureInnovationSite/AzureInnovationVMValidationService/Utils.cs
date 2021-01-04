using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AzureInnovationVMValidationService.Models;
using Microsoft.MD;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace AzureInnovationVMValidationService
{
    public static class Utils
    {
        public static string HtmlEscape(string input)
        {
            input = input?.Replace("<", "&lt;")?.Replace(">", "&gt;")
                ?.Replace("\r\n", "\n")?.Replace("\n\r", "\n")
                ?.Replace("\r", "\n")?.Replace("\n", "\n<br />");

            return input ?? string.Empty;
        }

        public static void SendEmail(MailPriority priority, string subject, IRenderable body)
        {
            var cfg = Models.Config.AzureInnovationVMValidationServiceCfg.Config.Value;
            var html = body.ToHtml();

            var email = new MailMessage
            {
                From = cfg.MailSettings.FromAddress,
                Subject = subject,
                Body = html,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                Priority = priority
            };

            email.To.AddRange(cfg.MailSettings.ToAddresses);

            using
            (
                var client = new SmtpClient(cfg.MailSettings.SmtpServer)
                {
                    Port = 587,
                    EnableSsl = cfg.MailSettings.UseSsl,
                    Credentials = new NetworkCredential
                    (
                        cfg.MailSettings.UserName,
                        cfg.MailSettings.Password
                    )
                }
            )
            {
                client.Send(email);
            }
        }

        public static double ParsePercent(string input)
        {
            input = (input ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(input)) { return 0; }

            return input.Contains("%")
                ? (double.Parse(input.Replace("%", string.Empty)) / 100d)
                : double.Parse(input);
        }

        public static async Task<string> GetImageContent(Image img, string imageName)
        {
            var cfg = Models.Config.AzureInnovationVMValidationServiceCfg.Config.Value;
            var storedImageUrl = StoreImage(img, cfg.ServiceSettings.StorageAccount, cfg.ServiceSettings.StorageAccountKey, imageName);
            string strResult;
            dynamic ocrResult;

            using (HttpClient httpClient = new HttpClient())
            {
                Console.Out.WriteLine($"Calling CallOCRLogicApp az function");

                var result = await httpClient.GetAsync($"https://autoglassfunctions.azurewebsites.net/api/CallOCRLogicApp?imageURL={storedImageUrl}");
                strResult = await result.Content.ReadAsStringAsync();
                Console.Out.WriteLine($"{strResult}");

                ocrResult = JsonConvert.DeserializeObject<dynamic>(strResult);
            }

            return ocrResult.RecognizedText;
        }

        public static string StoreImage(Image image, string storageAccount, string storageAccountKey, string newFileName)
        {
            CloudStorageAccount cloudStorageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(storageAccount, storageAccountKey), true);
            CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("assets");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(newFileName);
            string storedUrl;

            using (var stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                var array = stream.ToArray();

                blockBlob.UploadFromByteArray(array, 0, array.Length);

                storedUrl = blockBlob.StorageUri.PrimaryUri.AbsoluteUri;
            }

            return storedUrl;
        }
    }
}
