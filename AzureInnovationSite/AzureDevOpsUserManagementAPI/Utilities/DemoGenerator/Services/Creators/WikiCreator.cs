using AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsAPI;
using AzureDevOpsAPI.Viewmodel.Extractor;
using AzureDevOpsAPI.Viewmodel.Wiki;
using AzureDevOpsAPI.Wiki;

namespace AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Creators
{
    public class WikiCreator : BaseCreator
    {
        public WikiCreator(ExtractorProjectSettings settings) : base(settings)
        {

        }

        /// <summary>
        /// WIKI set up operations 
        /// Project as Wiki and Code as Wiki
        /// </summary>
        /// <param name="model"></param>
        /// <param name="_wikiConfiguration"></param>
        public void CreateProjetWiki(string templatesFolder, Project model, AppConfiguration _wikiConfiguration)
        {
            try
            {
                ManageWiki manageWiki = new ManageWiki(_wikiConfiguration);
                string projectWikiFolderPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, @"\Wiki\ProjectWiki");
                //templatesFolder + model.SelectedTemplate + "\\Wiki\\ProjectWiki";
                if (Directory.Exists(projectWikiFolderPath))
                {
                    string createWiki = string.Format(templatesFolder + "\\CreateWiki.json"); // check is path
                    if (File.Exists(createWiki))
                    {
                        string jsonString = File.ReadAllText(createWiki);
                        jsonString = jsonString.Replace("$ProjectID$", model.Environment.ProjectId)
                            .Replace("$Name$", model.Environment.ProjectName);
                        ProjectwikiResponse.Projectwiki projectWikiResponse = manageWiki.CreateProjectWiki(jsonString, model.Environment.ProjectId);
                        string[] subDirectories = Directory.GetDirectories(projectWikiFolderPath);
                        foreach (var dir in subDirectories)
                        {
                            //dirName==parentName//
                            string[] dirSplit = dir.Split('\\');
                            string dirName = dirSplit[dirSplit.Length - 1];
                            string sampleContent = File.ReadAllText(templatesFolder + "\\SampleContent.json");
                            sampleContent = sampleContent.Replace("$Content$", "Sample wiki content");
                            bool isPage = manageWiki.CreateUpdatePages(sampleContent, model.Environment.ProjectName, projectWikiResponse.Id, dirName);//check is created

                            if (isPage)
                            {
                                string[] getFiles = Directory.GetFiles(dir);
                                if (getFiles.Length > 0)
                                {
                                    List<string> childFileNames = new List<string>();
                                    foreach (var file in getFiles)
                                    {
                                        string[] fileNameExtension = file.Split('\\');
                                        string fileName = (fileNameExtension[fileNameExtension.Length - 1].Split('.'))[0];
                                        string fileContent = model.ReadJsonFile(file);
                                        bool isCreated = false;
                                        Dictionary<string, string> dic = new Dictionary<string, string>();
                                        dic.Add("content", fileContent);
                                        string newContent = JsonConvert.SerializeObject(dic);
                                        if (fileName == dirName)
                                        {
                                            manageWiki.DeletePage(model.Environment.ProjectName, projectWikiResponse.Id, fileName);
                                            isCreated = manageWiki.CreateUpdatePages(newContent, model.Environment.ProjectName, projectWikiResponse.Id, fileName);
                                        }
                                        else
                                        {
                                            isCreated = manageWiki.CreateUpdatePages(newContent, model.Environment.ProjectName, projectWikiResponse.Id, fileName);
                                        }
                                        if (isCreated)
                                        {
                                            childFileNames.Add(fileName);
                                        }
                                    }
                                    if (childFileNames.Count > 0)
                                    {
                                        foreach (var child in childFileNames)
                                        {
                                            if (child != dirName)
                                            {
                                                string movePages = File.ReadAllText(templatesFolder + @"\MovePages.json");
                                                if (!string.IsNullOrEmpty(movePages))
                                                {
                                                    movePages = movePages.Replace("$ParentFile$", dirName).Replace("$ChildFile$", child);
                                                    manageWiki.MovePages(movePages, model.Environment.ProjectId, projectWikiResponse.Id);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
        }
        public void CreateCodeWiki(Project model, AppConfiguration _wikiConfiguration)
        {
            try
            {
                string wikiFolder = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, @"\Wiki");
                //templatesFolder + model.SelectedTemplate + "\\Wiki";
                if (Directory.Exists(wikiFolder))
                {
                    string[] wikiFilePaths = Directory.GetFiles(wikiFolder);
                    if (wikiFilePaths.Length > 0)
                    {
                        ManageWiki manageWiki = new ManageWiki(_wikiConfiguration);

                        foreach (string wiki in wikiFilePaths)
                        {
                            string[] nameExtension = wiki.Split('\\');
                            string name = (nameExtension[nameExtension.Length - 1]).Split('.')[0];
                            string json = model.ReadJsonFile(wiki);
                            foreach (string repository in model.Environment.RepositoryIdList.Keys)
                            {
                                string placeHolder = string.Format("${0}$", repository);
                                json = json.Replace(placeHolder, model.Environment.RepositoryIdList[repository])
                                    .Replace("$Name$", name).Replace("$ProjectID$", model.Environment.ProjectId);
                            }
                            bool isWiki = manageWiki.CreateCodeWiki(json);
                          
                            if (!string.IsNullOrEmpty(manageWiki.LastFailureMessage))
                            {
                                Errors.Add("Error while creating wiki: " + manageWiki.LastFailureMessage);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                Errors.Add("Error while creating wiki: " + ex.Message);
            }
        }
    }
}
