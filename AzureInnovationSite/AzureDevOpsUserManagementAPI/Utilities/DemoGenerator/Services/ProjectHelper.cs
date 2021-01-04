using AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services
{
    public static class ProjectHelper
    {
        public static string ReadJsonFile(this Project file, string filePath)
        {
            string fileContents = string.Empty;

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using StreamReader sr = new StreamReader(fs);
                fileContents = sr.ReadToEnd();
            }

            return fileContents;
        }

        public static string ErrorId(this string str)
        {
            str = str + "_Errors";
            return str;
        }

        /// <summary>
        /// Get the path where we can file template related json files for selected template
        /// </summary>
        /// <param name="TemplateFolder"></param>
        /// <param name="TemplateName"></param>
        /// <param name="FileName"></param>
        public static string GetJsonFilePath(bool IsPrivate, string TemplateFolder, string TemplateName, string FileName = "")
        {
            string filePath;
            if (IsPrivate && !string.IsNullOrEmpty(TemplateFolder))
            {
                filePath = string.Format(TemplateFolder + @"\{0}", FileName);
            }
            else
            {
                filePath = string.Format(Path.GetFullPath(".") + @"\Utilities\DemoGenerator\Templates\" + @"{0}\{1}", TemplateName, FileName);
            }
            return filePath;
        }
    }
}