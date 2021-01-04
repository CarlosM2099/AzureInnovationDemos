using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Models.TemplateSelection;

namespace AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Interface
{
    public interface ITemplateService
    {
        List<TemplateDetails> GetAllTemplates();

        List<TemplateDetails> GetTemplatesByTags(string Tags);

        string GetTemplate(string TemplateName);

        string GetTemplateFromPath(string TemplateUrl, string ExtractedTemplate, string GithubToken, string UserID = "", string Password = "");

        bool checkTemplateDirectory(string dir);

        string FindPrivateTemplatePath(string privateTemplatePath);

        string checkSelectedTemplateIsPrivate(string templatePath);

        void DeletePrivateTemplate(string Template);
    }
}