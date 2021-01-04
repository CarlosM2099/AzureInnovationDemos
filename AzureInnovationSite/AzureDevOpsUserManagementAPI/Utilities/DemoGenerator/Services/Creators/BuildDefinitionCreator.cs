using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Models;
using AzureDevOpsAPI.Build;
using AzureDevOpsAPI.Viewmodel.Extractor;

namespace AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Creators
{
    public class BuildDefinitionCreator : BaseCreator
    {
        public BuildDefinitionCreator(ExtractorProjectSettings settings) : base(settings)
        {

        }
        /// <summary>
        /// Creates Build Definitions
        /// </summary>
        /// <param name="model"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CreateBuildDefinition(Project model, AzureDevOpsAPI.AppConfiguration _buildConfig, string id, string templateUsed)
        {
            string buildDefinitionsPath = string.Empty;
            model.BuildDefinitions = new List<BuildDef>();
            // if the template is private && agreed to GitHubFork && GitHub Token is not null
            if (setting.IsPrivate == "true" && model.GitHubFork && !string.IsNullOrEmpty(model.GitHubToken))
            {
                buildDefinitionsPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\BuildDefinitions");
                if (Directory.Exists(buildDefinitionsPath))
                {
                    Directory.GetFiles(buildDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.BuildDefinitions.Add(new BuildDef() { FilePath = i }));
                }
                buildDefinitionsPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\BuildDefinitionGitHub");
                if (Directory.Exists(buildDefinitionsPath))
                {
                    Directory.GetFiles(buildDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.BuildDefinitions.Add(new BuildDef() { FilePath = i }));
                }
            }
            // if the template is private && not agreed to GitHubFork && GitHub Token is null
            else if (setting.IsPrivate == "true" && !model.GitHubFork && string.IsNullOrEmpty(model.GitHubToken))
            {
                buildDefinitionsPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\BuildDefinitions");
                if (Directory.Exists(buildDefinitionsPath))
                {
                    Directory.GetFiles(buildDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.BuildDefinitions.Add(new BuildDef() { FilePath = i }));
                }
            }
            // if the template is not private && agreed to GitHubFork && GitHub Token is not null
            else if (string.IsNullOrEmpty(setting.IsPrivate) && model.GitHubFork && !string.IsNullOrEmpty(model.GitHubToken))
            {
                buildDefinitionsPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\BuildDefinitionGitHub");
                if (Directory.Exists(buildDefinitionsPath))
                {
                    Directory.GetFiles(buildDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.BuildDefinitions.Add(new BuildDef() { FilePath = i }));
                }
            }
            // if the template is not private && not agreed to GitHubFork && GitHub Token is null
            else if (string.IsNullOrEmpty(setting.IsPrivate) && !model.GitHubFork && string.IsNullOrEmpty(model.GitHubToken))
            {
                buildDefinitionsPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\BuildDefinitions");
                if (Directory.Exists(buildDefinitionsPath))
                {
                    Directory.GetFiles(buildDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.BuildDefinitions.Add(new BuildDef() { FilePath = i }));
                }
            }

            try
            {
                foreach (BuildDef buildDef in model.BuildDefinitions)
                {
                    if (File.Exists(buildDef.FilePath))
                    {
                        BuildDefinition objBuild = new BuildDefinition(_buildConfig);
                        string jsonBuildDefinition = model.ReadJsonFile(buildDef.FilePath);
                        jsonBuildDefinition = jsonBuildDefinition.Replace("$ProjectName$", model.Environment.ProjectName)
                                             .Replace("$ProjectId$", model.Environment.ProjectId)
                                             .Replace("$username$", model.GitHubUserName);

                        if (model.Environment.VariableGroups.Count > 0)
                        {
                            foreach (var vGroupsId in model.Environment.VariableGroups)
                            {
                                string placeHolder = string.Format("${0}$", vGroupsId.Value);
                                jsonBuildDefinition = jsonBuildDefinition.Replace(placeHolder, vGroupsId.Key.ToString());
                            }
                        }

                        //update repositoryId 
                        foreach (string repository in model.Environment.RepositoryIdList.Keys)
                        {
                            string placeHolder = string.Format("${0}$", repository);
                            jsonBuildDefinition = jsonBuildDefinition.Replace(placeHolder, model.Environment.RepositoryIdList[repository]);
                        }
                        //update endpoint ids
                        foreach (string endpoint in model.Environment.ServiceEndpoints.Keys)
                        {
                            string placeHolder = string.Format("${0}$", endpoint);
                            jsonBuildDefinition = jsonBuildDefinition.Replace(placeHolder, model.Environment.ServiceEndpoints[endpoint]);
                        }

                        (string buildId, string buildName) buildResult = objBuild.CreateBuildDefinition(jsonBuildDefinition, model.ProjectName, model.SelectedTemplate);

                        if (!(string.IsNullOrEmpty(objBuild.LastFailureMessage)))
                        {
                            Errors.Add($"Error while creating build definition: { objBuild.LastFailureMessage }" + Environment.NewLine);
                        }
                        if (!string.IsNullOrEmpty(buildResult.buildId) && buildResult.buildId != "error")
                        {
                            buildDef.Id = buildResult.buildId;
                            buildDef.Name = buildResult.buildName;
                        }
                    }

                }
                return true;
            }

            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                Errors.Add($"Error while creating build definition: { ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Queue build after provisioning project
        /// </summary>
        /// <param name="model"></param>
        /// <param name="json"></param>
        /// <param name="_configuration"></param>
        public void QueueABuild(Project model, string json, AzureDevOpsAPI.AppConfiguration _buildConfig)
        {
            try
            {
                string jsonQueueABuild = json;
                if (File.Exists(jsonQueueABuild))
                {
                    string buildId = model.BuildDefinitions.FirstOrDefault().Id;

                    jsonQueueABuild = model.ReadJsonFile(jsonQueueABuild);
                    jsonQueueABuild = jsonQueueABuild.Replace("$buildId$", buildId.ToString());
                    BuildDefinition objBuild = new BuildDefinition(_buildConfig);
                    int queueId = objBuild.QueueBuild(jsonQueueABuild, model.ProjectName);

                    if (!string.IsNullOrEmpty(objBuild.LastFailureMessage))
                    {
                        Errors.Add("Error while Queueing build: " + objBuild.LastFailureMessage + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                Errors.Add("Error while Queueing Build: " + ex.Message);
            }
        }
    }
}
