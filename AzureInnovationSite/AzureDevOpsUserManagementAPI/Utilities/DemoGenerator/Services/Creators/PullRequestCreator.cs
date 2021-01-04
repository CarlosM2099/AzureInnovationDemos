using AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsAPI.Git;
using AzureDevOpsAPI.Viewmodel.Extractor;
using AzureDevOpsAPI.Viewmodel.Repository;

namespace AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Creators
{
    public class PullRequestCreator : BaseCreator
    {
        public PullRequestCreator(ExtractorProjectSettings settings) : base(settings)
        {

        }

        /// <summary>
        /// Creates pull request
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pullRequestJsonPath"></param>
        /// <param name="_configuration3_0"></param>
        public void CreatePullRequest(Project model, string pullRequestJsonPath, AzureDevOpsAPI.AppConfiguration _workItemConfig)
        {
            try
            {
                if (File.Exists(pullRequestJsonPath))
                {
                    string commentFile = Path.GetFileName(pullRequestJsonPath);
                    string repositoryId = string.Empty;
                    if (model.SelectedTemplate == "MyHealthClinic") { repositoryId = model.Environment.RepositoryIdList["MyHealthClinic"]; }
                    if (model.SelectedTemplate == "SmartHotel360") { repositoryId = model.Environment.RepositoryIdList["PublicWeb"]; }
                    else { repositoryId = model.Environment.RepositoryIdList[model.SelectedTemplate]; }

                    pullRequestJsonPath = model.ReadJsonFile(pullRequestJsonPath);
                    pullRequestJsonPath = pullRequestJsonPath.Replace("$reviewer$", model.Environment.UserUniqueId);
                    Repository objRepository = new Repository(_workItemConfig);
                    string[] pullReqResponse = new string[2];

                    pullReqResponse = objRepository.CreatePullRequest(pullRequestJsonPath, repositoryId);
                    if (pullReqResponse.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(pullReqResponse[0]) && !string.IsNullOrEmpty(pullReqResponse[1]))
                        {
                            model.Environment.PullRequests.Add(pullReqResponse[1], pullReqResponse[0]);
                            commentFile = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, @"\PullRequests\Comments\" + commentFile);
                            //string.Format(templatesFolder + @"{0}\PullRequests\Comments\{1}", model.SelectedTemplate, commentFile);
                            if (File.Exists(commentFile))
                            {
                                commentFile = model.ReadJsonFile(commentFile);
                                PullRequestComments.Comments commentsList = JsonConvert.DeserializeObject<PullRequestComments.Comments>(commentFile);
                                if (commentsList.Count > 0)
                                {
                                    foreach (PullRequestComments.Value thread in commentsList.Value)
                                    {
                                        string threadID = objRepository.CreateCommentThread(repositoryId, pullReqResponse[0], JsonConvert.SerializeObject(thread));
                                        if (!string.IsNullOrEmpty(threadID))
                                        {
                                            if (thread.Replies != null && thread.Replies.Count > 0)
                                            {
                                                foreach (var reply in thread.Replies)
                                                {
                                                    objRepository.AddCommentToThread(repositoryId, pullReqResponse[0], threadID, JsonConvert.SerializeObject(reply));
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
                Errors.Add("Error while creating pull Requests: " + ex.Message);
            }
        }
    }
}
