using AzureDevOpsAPI;
using AzureDevOpsAPI.DeploymentGRoup;
using AzureDevOpsAPI.Extractor;
using AzureDevOpsAPI.Git;
using AzureDevOpsAPI.ProjectsAndTeams;
using AzureDevOpsAPI.Queues;
using AzureDevOpsAPI.Service;
using AzureDevOpsAPI.Services;
using AzureDevOpsAPI.TestManagement;
using AzureDevOpsAPI.Viewmodel.Extractor;
using AzureDevOpsAPI.Viewmodel.GitHub;
using AzureDevOpsAPI.Viewmodel.ProjectAndTeams;
using AzureDevOpsAPI.WorkItemAndTracking;
using AzureDevOpsUserManagementAPI.Helpers;
using AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Creators;
using AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Interface;
using AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Models;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ExtensionManagement.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services
{
    public class ProjectService : IProjectService
    {
        private readonly DemoGenSettings demoGenSettings;
        public static readonly object objLock = new object();
        public static Dictionary<string, string> statusMessages;
        public static ILog Logger = LogManager.GetLogger("ErrorLog", "ErrorLog");

        public bool isDefaultRepoTodetele = true;
        public string websiteUrl = string.Empty;
        public string templateUsed = string.Empty;
        public static string projectName = string.Empty;
        public static AccessDetails accessDetails = new AccessDetails();

        public string templateVersion = string.Empty;
        public static string enableExtractor = "";

        public ProjectService(DemoGenSettings demoGenSettings)
        {
            this.demoGenSettings = demoGenSettings;
        }

        public static Dictionary<string, string> StatusMessages
        {
            get
            {
                if (statusMessages == null)
                {
                    statusMessages = new Dictionary<string, string>();
                }

                return statusMessages;
            }
            set
            {
                statusMessages = value;
            }
        }
        public void AddMessage(string id, string message)
        {
            lock (objLock)
            {
                if (id.EndsWith("_Errors"))
                {
                    StatusMessages[id] = (StatusMessages.ContainsKey(id) ? StatusMessages[id] : string.Empty) + message;
                }
                else
                {
                    StatusMessages[id] = message;
                }
            }
        }
        public void RemoveKey(string id)
        {
            lock (objLock)
            {
                StatusMessages.Remove(id);
            }
        }
        public JObject GetStatusMessage(string id)
        {
            lock (ProjectService.objLock)
            {
                string message = string.Empty;
                JObject obj = new JObject();
                if (id.EndsWith("_Errors"))
                {
                    //RemoveKey(id);
                    obj["status"] = "Error: \t" + ProjectService.StatusMessages[id];
                }
                if (ProjectService.StatusMessages.Keys.Count(x => x == id) == 1)
                {
                    obj["status"] = ProjectService.StatusMessages[id];
                }
                else
                {
                    obj["status"] = "Successfully Created";

                }
                return obj;
            }
        }

        public HttpResponseMessage GetprojectList(string accname, string pat)
        {
            string defaultHost = demoGenSettings.DefaultHost;
            string ProjectCreationVersion = demoGenSettings.ProjectCreationVersion;

            AppConfiguration config = new AppConfiguration() { AccountName = accname, PersonalAccessToken = pat, UriString = defaultHost + accname, VersionNumber = ProjectCreationVersion };
            Projects projects = new Projects(config);
            HttpResponseMessage response = projects.GetListOfProjects();
            return response;
        }



        #region Project Setup Operations

        /// <summary>
        /// start provisioning project - calls required
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pat"></param>
        /// <param name="accountName"></param>
        /// <returns></returns>
        public (string projectId, string accountName, string templateUsed) CreateProjectEnvironment(Project model)
        {
            string accountName = model.AccountName;
            if (model.IsPrivatePath)
            {
                templateUsed = model.PrivateTemplateName;
            }
            else
            {
                templateUsed = model.SelectedTemplate;
            }
            Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "Project Name: " + model.ProjectName + "\t Template Selected: " + templateUsed + "\t Organization Selected: " + accountName);
            string pat = model.AccessToken;
            //define versions to be use
            string projectCreationVersion = demoGenSettings.ProjectCreationVersion;
            string repoVersion = demoGenSettings.RepoVersion;
            string buildVersion = demoGenSettings.BuildVersion;
            string releaseVersion = demoGenSettings.ReleaseVersion;
            string wikiVersion = demoGenSettings.WikiVersion;
            string boardVersion = demoGenSettings.BoardVersion;
            string workItemsVersion = demoGenSettings.WorkItemsVersion;
            string queriesVersion = demoGenSettings.QueriesVersion;
            string endPointVersion = demoGenSettings.EndPointVersion;
            string extensionVersion = demoGenSettings.ExtensionVersion;
            string dashboardVersion = demoGenSettings.DashboardVersion;
            string agentQueueVersion = demoGenSettings.AgentQueueVersion;
            string getSourceCodeVersion = demoGenSettings.GetSourceCodeVersion;
            string testPlanVersion = demoGenSettings.TestPlanVersion;
            string releaseHost = demoGenSettings.ReleaseHost;
            string defaultHost = demoGenSettings.DefaultHost;
            string deploymentGroup = demoGenSettings.DeloymentGroup;
            string graphApiVersion = demoGenSettings.GraphApiVersion;
            string graphAPIHost = demoGenSettings.GraphAPIHost;
            string gitHubBaseAddress = demoGenSettings.GitHubBaseAddress;
            string variableGroupsApiVersion = demoGenSettings.VariableGroupsApiVersion;

            string processTemplateId = Default.SCRUM;
            model.Environment = new EnvironmentValues
            {
                ServiceEndpoints = new Dictionary<string, string>(),
                RepositoryIdList = new Dictionary<string, string>(),
                PullRequests = new Dictionary<string, string>(),
                GitHubRepos = new Dictionary<string, string>()
            };
            ProjectTemplate template = null;
            Models.ProjectSettings settings = null;
            List<WiMapData> wiMapping = new List<WiMapData>();
            AccountMembers.Account accountMembers = new AccountMembers.Account();
            model.AccountUsersForWi = new List<string>();
            websiteUrl = model.WebsiteUrl;
            projectName = model.ProjectName;

            //string logWIT = System.Configuration.ConfigurationManager.AppSettings["LogWIT"];
            //if (logWIT == "true")
            //{
            //    string patBase64 = System.Configuration.ConfigurationManager.AppSettings["PATBase64"];
            //    string url = System.Configuration.ConfigurationManager.AppSettings["URL"];
            //    string projectId = System.Configuration.ConfigurationManager.AppSettings["PROJECTID"];
            //    string reportName = string.Format("{0}", "AzureDevOps_Analytics-DemoGenerator");
            //    IssueWi objIssue = new IssueWi();
            //    objIssue.CreateReportWi(patBase64, "4.1", url, websiteUrl, reportName, "", templateUsed, projectId, model.Region);
            //}

            AppConfiguration gitHubConfig = new AppConfiguration() { GitBaseAddress = gitHubBaseAddress, GitCredential = model.GitHubToken, MediaType = "application/json", Scheme = "Bearer" };

            if (model.GitHubFork && model.GitHubToken != null)
            {
                GitHubImportRepo gitHubImport = new GitHubImportRepo(gitHubConfig);
                HttpResponseMessage userResponse = gitHubImport.GetUserDetail();
                GitHubUserDetail userDetail = new GitHubUserDetail();
                if (userResponse.IsSuccessStatusCode)
                {
                    userDetail = JsonConvert.DeserializeObject<GitHubUserDetail>(userResponse.Content.ReadAsStringAsync().Result);
                    gitHubConfig.UserName = userDetail.Login;
                    model.GitHubUserName = userDetail.Login;
                }
            }
            //configuration setup
            string _credentials = model.AccessToken;
            AppConfiguration _projectCreationVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = projectCreationVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _releaseVersion = new AppConfiguration() { UriString = releaseHost + accountName + "/", VersionNumber = releaseVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _buildVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = buildVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName, GitBaseAddress = gitHubBaseAddress, GitCredential = model.GitHubToken };
            AppConfiguration _workItemsVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = workItemsVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _queriesVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = queriesVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _boardVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = boardVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _wikiVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = wikiVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _endPointVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = endPointVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName, GitBaseAddress = gitHubBaseAddress, GitCredential = model.GitHubToken };
            AppConfiguration _extensionVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = extensionVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _dashboardVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = dashboardVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _repoVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = repoVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName, GitBaseAddress = gitHubBaseAddress, GitCredential = model.GitHubToken };
            AppConfiguration _getSourceCodeVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = getSourceCodeVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName, GitBaseAddress = gitHubBaseAddress, GitCredential = model.GitHubToken };
            AppConfiguration _agentQueueVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = agentQueueVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _testPlanVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = testPlanVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _deploymentGroup = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = deploymentGroup, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _graphApiVersion = new AppConfiguration() { UriString = graphAPIHost + accountName + "/", VersionNumber = graphApiVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };
            AppConfiguration _variableGroupApiVersion = new AppConfiguration() { UriString = defaultHost + accountName + "/", VersionNumber = variableGroupsApiVersion, PersonalAccessToken = pat, Project = model.ProjectName, AccountName = accountName };

            string projTemplateFile = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "ProjectTemplate.json");
            string projectSettingsFile = string.Empty;
            string _checkIsPrivate = string.Empty;
            ExtractorProjectSettings xtProjSettings = new ExtractorProjectSettings();
            if (File.Exists(projTemplateFile))
            {
                _checkIsPrivate = File.ReadAllText(projTemplateFile);
            }
            if (_checkIsPrivate != "")
            {
                xtProjSettings = JsonConvert.DeserializeObject<ExtractorProjectSettings>(_checkIsPrivate);
            }

            //initialize project template and settings
            try
            {
                if (File.Exists(projTemplateFile))
                {
                    string templateItems = model.ReadJsonFile(projTemplateFile);
                    template = JsonConvert.DeserializeObject<ProjectTemplate>(templateItems);
                    projectSettingsFile = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.ProjectSettings);

                    if (File.Exists(projectSettingsFile))
                    {
                        settings = JsonConvert.DeserializeObject<Models.ProjectSettings>(model.ReadJsonFile(projectSettingsFile));

                        if (!string.IsNullOrWhiteSpace(settings.Type))
                        {
                            if (settings.Type.ToLower() == TemplateType.Scrum.ToString().ToLower())
                            {
                                processTemplateId = Default.SCRUM;
                            }
                            else if (settings.Type.ToLower() == TemplateType.Agile.ToString().ToLower())
                            {
                                processTemplateId = Default.Agile;
                            }
                            else if (settings.Type.ToLower() == TemplateType.CMMI.ToString().ToLower())
                            {
                                processTemplateId = Default.CMMI;
                            }
                            else if (settings.Type.ToLower() == TemplateType.Basic.ToString().ToLower())
                            {
                                processTemplateId = Default.BASIC;
                            }
                            else if (!string.IsNullOrEmpty(settings.Id))
                            {
                                processTemplateId = settings.Id;
                            }
                            else
                            {
                                AddMessage(model.Id.ErrorId(), "Could not recognize process template. Make sure that the exported project template is belog to standard process template or project setting file has valid process template id.");
                                StatusMessages[model.Id] = "100";
                                return (model.Id, accountName, templateUsed);
                            }
                        }
                        else
                        {
                            settings.Type = "scrum";
                            processTemplateId = Default.SCRUM;
                        }
                    }
                }
                else
                {
                    AddMessage(model.Id.ErrorId(), "Project Template not found");
                    StatusMessages[model.Id] = "100";
                    return (model.Id, accountName, templateUsed);
                }
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            //create team project


            string jsonProject = model.ReadJsonFile(Path.GetFullPath(".") + @"\Utilities\DemoGenerator\Templates\" + "CreateProject.json");
            jsonProject = jsonProject.Replace("$projectName$", model.ProjectName).Replace("$processTemplateId$", processTemplateId);

            Projects proj = new Projects(_projectCreationVersion);
            string projectID = proj.CreateTeamProject(jsonProject);

            if (projectID == "-1")
            {
                if (!string.IsNullOrEmpty(proj.LastFailureMessage))
                {
                    if (proj.LastFailureMessage.Contains("TF400813"))
                    {
                        AddMessage(model.Id, "OAUTHACCESSDENIED");
                    }
                    else if (proj.LastFailureMessage.Contains("TF50309"))
                    {
                        AddMessage(model.Id.ErrorId(), proj.LastFailureMessage);
                    }
                    else
                    {
                        AddMessage(model.Id.ErrorId(), proj.LastFailureMessage);
                    }
                }
                Thread.Sleep(2000); // Adding Delay to Get Error message
                return (model.Id, accountName, templateUsed);
            }
            else
            {
                AddMessage(model.Id, string.Format("Project {0} created", model.ProjectName));
            }
            // waiting to add first message
            Thread.Sleep(2000);

            //Check for project state 
            Stopwatch watch = new Stopwatch();
            watch.Start();
            string projectStatus = string.Empty;
            Projects objProject = new Projects(_projectCreationVersion);
            while (projectStatus.ToLower() != "wellformed")
            {
                projectStatus = objProject.GetProjectStateByName(model.ProjectName);
                if (watch.Elapsed.Minutes >= 5)
                {
                    return (model.Id, accountName, templateUsed);
                }
            }
            watch.Stop();

            //get project id after successfull in VSTS
            model.Environment.ProjectId = objProject.GetProjectIdByName(model.ProjectName);
            model.Environment.ProjectName = model.ProjectName;

            // Fork Repo
            if (model.GitHubFork && model.GitHubToken != null)
            {
                ForkGitHubRepository(model, gitHubConfig);
            }

            //Add user as project admin
            bool isAdded = AddUserToProject(_graphApiVersion, model);
            if (isAdded)
            {
                AddMessage(model.Id, string.Format("Added user {0} as project admin ", model.Email));
            }

            //Install required extensions
            if (!model.IsApi && model.IsExtensionNeeded && model.IsAgreeTerms)
            {
                bool isInstalled = InstallExtensions(model, model.AccountName, model.AccessToken);
                if (isInstalled) { AddMessage(model.Id, "Required extensions are installed"); }
            }

            //current user Details
            string teamName = model.ProjectName + " team";
            TeamMemberResponse.TeamMembers teamMembers = GetTeamMembers(model.ProjectName, teamName, _projectCreationVersion, model.Id);

            var teamMember = teamMembers.Value != null ? teamMembers.Value.FirstOrDefault() : new TeamMemberResponse.Value();

            if (teamMember != null)
            {
                model.Environment.UserUniqueName = model.Environment.UserUniqueName ?? teamMember.Identity.UniqueName;
                model.Environment.UserUniqueId = model.Environment.UserUniqueId ?? teamMember.Identity.Id;
            }
            //model.Environment.UserUniqueId = model.Email;
            //model.Environment.UserUniquename = model.Email;
            //update board columns and rows
            // Checking for template version
            string projectTemplate = File.ReadAllText(ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "ProjectTemplate.json"));

            if (!string.IsNullOrEmpty(projectTemplate))
            {
                JObject jObject = JsonConvert.DeserializeObject<JObject>(projectTemplate);
                templateVersion = jObject["TemplateVersion"] == null ? string.Empty : jObject["TemplateVersion"].ToString();
            }

            TeamsCreator teamsCreator = new TeamsCreator(xtProjSettings);

            if (templateVersion != "2.0")
            {
                teamsCreator.CreateV1Template(_projectCreationVersion, _boardVersion, settings, model, template, templateUsed);
            }
            else
            {
                // for newer version of templates
                teamsCreator.CreateV2Template(_projectCreationVersion, _boardVersion, settings, model, template, templateUsed);
            }

            foreach (string result in teamsCreator.Results)
            {
                AddMessage(model.Id.ErrorId(), result);
            }

            foreach (string error in teamsCreator.Errors)
            {
                AddMessage(model.Id.ErrorId(), error);
            }

            //Create Deployment Group
            //CreateDeploymentGroup(templatesFolder, model, _deploymentGroup);

            //create service endpoint
            List<string> listEndPointsJsonPath = new List<string>();
            string serviceEndPointsPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"ServiceEndpoints");
            if (Directory.Exists(serviceEndPointsPath))
            {
                Directory.GetFiles(serviceEndPointsPath).ToList().ForEach(i => listEndPointsJsonPath.Add(i));
            }
            CreateServiceEndPoint(model, listEndPointsJsonPath, _endPointVersion);
            //create agent queues on demand
            Queue queue = new Queue(_agentQueueVersion);
            model.Environment.AgentQueues = queue.GetQueues();
            if (settings.Queues != null && settings.Queues.Count > 0)
            {
                foreach (string aq in settings.Queues)
                {
                    if (model.Environment.AgentQueues.ContainsKey(aq))
                    {
                        continue;
                    }

                    var id = queue.CreateQueue(aq);
                    if (id > 0)
                    {
                        model.Environment.AgentQueues[aq] = id;
                    }
                }
            }

            //import source code from GitHub
            List<string> listImportSourceCodeJsonPaths = new List<string>();
            string importSourceCodePath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\ImportSourceCode");
            //templatesFolder + templateUsed + @"\ImportSourceCode";
            if (Directory.Exists(importSourceCodePath))
            {
                Directory.GetFiles(importSourceCodePath).ToList().ForEach(i => listImportSourceCodeJsonPaths.Add(i));
                if (listImportSourceCodeJsonPaths.Contains(importSourceCodePath + "\\GitRepository.json"))
                {
                    listImportSourceCodeJsonPaths.Remove(importSourceCodePath + "\\GitRepository.json");
                }
            }
            foreach (string importSourceCode in listImportSourceCodeJsonPaths)
            {
                ImportSourceCode(model, importSourceCode, _repoVersion, model.Id, _getSourceCodeVersion);
            }
            if (isDefaultRepoTodetele)
            {
                Repository objRepository = new Repository(_repoVersion);
                string repositoryToDelete = objRepository.GetRepositoryToDelete(model.ProjectName);
                bool isDeleted = objRepository.DeleteRepository(repositoryToDelete);
            }

            //Create Pull request
            Thread.Sleep(10000); //Adding delay to wait for the repository to create and import from the source

            //Create WIKI
            WikiCreator wikiCreator = new WikiCreator(xtProjSettings);
            wikiCreator.CreateProjetWiki(Path.GetFullPath(".") + @"\Utilities\DemoGenerator\Templates\", model, _wikiVersion);
            wikiCreator.CreateCodeWiki(model, _wikiVersion);

            if (wikiCreator.Errors.Count > 0)
            {
                foreach (string error in wikiCreator.Errors)
                {
                    AddMessage(model.Id.ErrorId(), error);
                }
            }

            List<string> listPullRequestJsonPaths = new List<string>();
            string pullRequestFolder = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\PullRequests");
            //templatesFolder + templateUsed + @"\PullRequests";
            if (Directory.Exists(pullRequestFolder))
            {
                Directory.GetFiles(pullRequestFolder).ToList().ForEach(i => listPullRequestJsonPaths.Add(i));
            }

            PullRequestCreator pullRequestCreator = new PullRequestCreator(xtProjSettings);

            foreach (string pullReq in listPullRequestJsonPaths)
            {
                pullRequestCreator.CreatePullRequest(model, pullReq, _workItemsVersion);
            }

            if (pullRequestCreator.Errors.Count > 0)
            {
                foreach (string error in pullRequestCreator.Errors)
                {
                    AddMessage(model.Id.ErrorId(), error);
                }
            }

            //Configure account users
            if (model.UserMethod == "Select")
            {
                model.SelectedUsers = model.SelectedUsers.TrimEnd(',');
                model.AccountUsersForWi = model.SelectedUsers.Split(',').ToList();
            }
            else if (model.UserMethod == "Random")
            {
                //GetAccount Members
                Accounts objAccount = new Accounts(_projectCreationVersion);
                //accountMembers = objAccount.GetAccountMembers(accountName, AccessToken);
                foreach (var member in accountMembers.Value)
                {
                    model.AccountUsersForWi.Add(member.Member.MailAddress);
                }
            }
            Dictionary<string, string> workItems = new Dictionary<string, string>();

            if (templateVersion != "2.0")
            {

                //import work items
                string featuresFilePath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.FeaturefromTemplate == null ? string.Empty : template.FeaturefromTemplate);
                // Path.Combine(templatesFolder + templateUsed, template.FeaturefromTemplate == null ? string.Empty : template.FeaturefromTemplate);
                string productBackLogPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.PBIfromTemplate == null ? string.Empty : template.PBIfromTemplate);
                // Path.Combine(templatesFolder + templateUsed, template.PBIfromTemplate == null ? string.Empty : template.PBIfromTemplate);
                string taskPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.TaskfromTemplate == null ? string.Empty : template.TaskfromTemplate);
                // Path.Combine(templatesFolder + templateUsed, template.TaskfromTemplate == null ? string.Empty : template.TaskfromTemplate);
                string testCasePath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.TestCasefromTemplate == null ? string.Empty : template.TestCasefromTemplate);
                // Path.Combine(templatesFolder + templateUsed, template.TestCasefromTemplate == null ? string.Empty : template.TestCasefromTemplate);
                string bugPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.BugfromTemplate ?? string.Empty);
                // Path.Combine(templatesFolder + templateUsed, template.BugfromTemplate == null ? string.Empty : template.BugfromTemplate);
                string epicPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.EpicfromTemplate == null ? string.Empty : template.EpicfromTemplate);
                // Path.Combine(templatesFolder + templateUsed, template.EpicfromTemplate == null ? string.Empty : template.EpicfromTemplate);
                string userStoriesPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.UserStoriesFromTemplate == null ? string.Empty : template.UserStoriesFromTemplate);
                // Path.Combine(templatesFolder + templateUsed, template.UserStoriesFromTemplate == null ? string.Empty : template.UserStoriesFromTemplate);
                string testPlansPath = string.Empty;
                string testSuitesPath = string.Empty;
                if (templateUsed.ToLower() == "myshuttle2")
                {
                    testPlansPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.TestPlanfromTemplate);
                    // Path.Combine(templatesFolder + templateUsed, template.TestPlanfromTemplate);
                    testSuitesPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.TestSuitefromTemplate);
                    // Path.Combine(templatesFolder + templateUsed, template.TestSuitefromTemplate);
                }

                if (templateUsed.ToLower() == "myshuttle")
                {
                    testPlansPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.TestPlanfromTemplate);
                    // Path.Combine(templatesFolder + templateUsed, template.TestPlanfromTemplate);
                    testSuitesPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.TestSuitefromTemplate);
                    // Path.Combine(templatesFolder + templateUsed, template.TestSuitefromTemplate);
                }

                if (File.Exists(featuresFilePath))
                {
                    workItems.Add("Feature", model.ReadJsonFile(featuresFilePath));
                }

                if (File.Exists(productBackLogPath))
                {
                    workItems.Add("Product Backlog Item", model.ReadJsonFile(productBackLogPath));
                }

                if (File.Exists(taskPath))
                {
                    workItems.Add("Task", model.ReadJsonFile(taskPath));
                }

                if (File.Exists(testCasePath))
                {
                    workItems.Add("Test Case", model.ReadJsonFile(testCasePath));
                }

                if (File.Exists(bugPath))
                {
                    workItems.Add("Bug", model.ReadJsonFile(bugPath));
                }

                if (File.Exists(userStoriesPath))
                {
                    workItems.Add("User Story", model.ReadJsonFile(userStoriesPath));
                }

                if (File.Exists(epicPath))
                {
                    workItems.Add("Epic", model.ReadJsonFile(epicPath));
                }

                if (File.Exists(testPlansPath))
                {
                    workItems.Add("Test Plan", model.ReadJsonFile(testPlansPath));
                }

                if (File.Exists(testSuitesPath))
                {
                    workItems.Add("Test Suite", model.ReadJsonFile(testSuitesPath));
                }
            }
            //// Modified Work Item import logic
            else
            {
                string _WitPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\WorkItems");
                //Path.Combine(templatesFolder + templateUsed + "\\WorkItems");
                if (Directory.Exists(_WitPath))
                {
                    string[] workItemFilePaths = Directory.GetFiles(_WitPath);
                    if (workItemFilePaths.Length > 0)
                    {
                        foreach (var workItem in workItemFilePaths)
                        {
                            string[] workItemPatSplit = workItem.Split('\\');
                            if (workItemPatSplit.Length > 0)
                            {
                                string workItemName = workItemPatSplit[workItemPatSplit.Length - 1];
                                if (!string.IsNullOrEmpty(workItemName))
                                {
                                    string[] nameExtension = workItemName.Split('.');
                                    string name = nameExtension[0];
                                    if (!workItems.ContainsKey(name))
                                    {
                                        workItems.Add(name, model.ReadJsonFile(workItem));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            ImportWorkItems import = new ImportWorkItems(_workItemsVersion, model.Environment.BoardRowFieldName);
            if (File.Exists(projectSettingsFile))
            {
                string attchmentFilesFolder = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\WorkItemAttachments");
                //string.Format(templatesFolder + @"{0}\WorkItemAttachments", templateUsed);
                if (listPullRequestJsonPaths.Count > 0)
                {
                    if (templateUsed == "MyHealthClinic")
                    {
                        wiMapping = import.ImportWorkitems(workItems, model.ProjectName, model.Environment.UserUniqueName, model.ReadJsonFile(projectSettingsFile), attchmentFilesFolder, model.Environment.RepositoryIdList.ContainsKey("MyHealthClinic") ? model.Environment.RepositoryIdList["MyHealthClinic"] : string.Empty, model.Environment.ProjectId, model.Environment.PullRequests, model.UserMethod, model.AccountUsersForWi, templateUsed);
                    }
                    else if (templateUsed == "SmartHotel360")
                    {
                        wiMapping = import.ImportWorkitems(workItems, model.ProjectName, model.Environment.UserUniqueName, model.ReadJsonFile(projectSettingsFile), attchmentFilesFolder, model.Environment.RepositoryIdList.ContainsKey("PublicWeb") ? model.Environment.RepositoryIdList["PublicWeb"] : string.Empty, model.Environment.ProjectId, model.Environment.PullRequests, model.UserMethod, model.AccountUsersForWi, templateUsed);
                    }
                    else
                    {
                        wiMapping = import.ImportWorkitems(workItems, model.ProjectName, model.Environment.UserUniqueName, model.ReadJsonFile(projectSettingsFile), attchmentFilesFolder, model.Environment.RepositoryIdList.ContainsKey(templateUsed) ? model.Environment.RepositoryIdList[templateUsed] : string.Empty, model.Environment.ProjectId, model.Environment.PullRequests, model.UserMethod, model.AccountUsersForWi, templateUsed);
                    }
                }
                else
                {
                    wiMapping = import.ImportWorkitems(workItems, model.ProjectName, model.Environment.UserUniqueName, model.ReadJsonFile(projectSettingsFile), attchmentFilesFolder, string.Empty, model.Environment.ProjectId, model.Environment.PullRequests, model.UserMethod, model.AccountUsersForWi, templateUsed);
                }
                AddMessage(model.Id, "Work Items created");
            }
            //Creat TestPlans and TestSuites
            List<string> listTestPlansJsonPaths = new List<string>();
            string testPlansFolder = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\TestPlans");
            //templatesFolder + templateUsed + @"\TestPlans";
            if (Directory.Exists(testPlansFolder))
            {
                Directory.GetFiles(testPlansFolder).ToList().ForEach(i => listTestPlansJsonPaths.Add(i));
            }
            foreach (string testPlan in listTestPlansJsonPaths)
            {
                CreateTestManagement(wiMapping, model, testPlan, _testPlanVersion);
            }
            if (listTestPlansJsonPaths.Count > 0)
            {
                //AddMessage(model.Id, "TestPlans, TestSuites and TestCases created");
            }
            // create varibale groups

            CreateVaribaleGroups(model, _variableGroupApiVersion);

            //create build Definition

            BuildDefinitionCreator buildDefinitionCreator = new BuildDefinitionCreator(xtProjSettings);

            if (buildDefinitionCreator.CreateBuildDefinition(model, _buildVersion, model.Id, templateUsed))
            {
                AddMessage(model.Id, "Build definition created");
            }
            else
            {
                if (buildDefinitionCreator.Errors.Count > 0)
                {
                    foreach (string error in buildDefinitionCreator.Errors)
                    {
                        AddMessage(model.Id.ErrorId(), error);
                    }
                }
            }

            //Queue a Build
            string buildJson = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "QueueBuild.json");
            //string.Format(templatesFolder + @"{0}\QueueBuild.json", templateUsed);
            if (File.Exists(buildJson))
            {
                buildDefinitionCreator.QueueABuild(model, buildJson, _buildVersion);
            }

            //create release Definition
            string releaseDefinitionsPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\ReleaseDefinitions");
            //templatesFolder + templateUsed + @"\ReleaseDefinitions";
            model.ReleaseDefinitions = new List<ReleaseDef>();
            if (Directory.Exists(releaseDefinitionsPath))
            {
                Directory.GetFiles(releaseDefinitionsPath, "*.json", SearchOption.AllDirectories).ToList().ForEach(i => model.ReleaseDefinitions.Add(new Models.ReleaseDef() { FilePath = i }));
            }

            ReleaseDefinitionCreator releaseDefinitionCreator = new ReleaseDefinitionCreator(xtProjSettings);

            if (releaseDefinitionCreator.CreateReleaseDefinition(model, _releaseVersion, model.Id, teamMembers))
            {
                AddMessage(model.Id, "Release definition created");
            }
            else
            {
                if (releaseDefinitionCreator.Errors.Count > 0)
                {
                    foreach (string error in releaseDefinitionCreator.Errors)
                    {
                        AddMessage(model.Id.ErrorId(), error);
                    }
                }
            }

            //Create query and widgets
            List<string> listDashboardQueriesPath = new List<string>();
            string dashboardQueriesPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\Dashboard\Queries");
            //templatesFolder + templateUsed + @"\Dashboard\Queries";
            string dashboardPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"\Dashboard");
            //templatesFolder + templateUsed + @"\Dashboard";

            if (Directory.Exists(dashboardQueriesPath))
            {
                Directory.GetFiles(dashboardQueriesPath).ToList().ForEach(i => listDashboardQueriesPath.Add(i));
            }
            if (Directory.Exists(dashboardPath))
            {
                QueryAndWidgetsCreator queryAndWidgetsCreator = new QueryAndWidgetsCreator(xtProjSettings);

                queryAndWidgetsCreator.CreateQueryAndWidgets(model, listDashboardQueriesPath, _queriesVersion, _dashboardVersion, _releaseVersion, _projectCreationVersion, _boardVersion);

                if (queryAndWidgetsCreator.Errors.Count == 0)
                {
                    AddMessage(model.Id, "Queries, Widgets and Charts created");
                }
                else
                {
                    foreach (string error in queryAndWidgetsCreator.Errors)
                    {
                        AddMessage(model.Id.ErrorId(), error);
                    }
                }
            }

            StatusMessages[model.Id] = "100";
            return (model.Id, accountName, templateUsed);
        }

        private void ForkGitHubRepository(Project model, AppConfiguration _gitHubConfig)
        {
            List<string> listRepoFiles = new List<string>();
            string repoFilePath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, @"\ImportSourceCode\GitRepository.json");
            if (File.Exists(repoFilePath))
            {
                string readRepoFile = model.ReadJsonFile(repoFilePath);
                if (!string.IsNullOrEmpty(readRepoFile))
                {
                    ForkRepos.Fork forkRepos = new ForkRepos.Fork();
                    forkRepos = JsonConvert.DeserializeObject<ForkRepos.Fork>(readRepoFile);
                    if (forkRepos.Repositories != null && forkRepos.Repositories.Count > 0)
                    {
                        foreach (var repo in forkRepos.Repositories)
                        {
                            GitHubImportRepo user = new GitHubImportRepo(_gitHubConfig);
                            GitHubUserDetail userDetail = new GitHubUserDetail();
                            GitHubRepoResponse.RepoCreated GitHubRepo = new GitHubRepoResponse.RepoCreated();
                            //HttpResponseMessage listForks = user.ListForks(repo.fullName);
                            HttpResponseMessage forkResponse = user.ForkRepo(repo.FullName);
                            if (forkResponse.IsSuccessStatusCode)
                            {
                                string forkedRepo = forkResponse.Content.ReadAsStringAsync().Result;
                                dynamic fr = JsonConvert.DeserializeObject<dynamic>(forkedRepo);
                                model.GitRepoName = fr.name;
                                model.GitRepoURL = fr.html_url;
                                if (!model.Environment.GitHubRepos.ContainsKey(model.GitRepoName))
                                {
                                    model.Environment.GitHubRepos.Add(model.GitRepoName, model.GitRepoURL);
                                }
                                AddMessage(model.Id, string.Format("Forked {0} repository to {1} user", model.GitRepoName, _gitHubConfig.UserName));
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Get Team members
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="teamName"></param>
        /// <param name="_configuration"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private TeamMemberResponse.TeamMembers GetTeamMembers(string projectName, string teamName, AppConfiguration _configuration, string id)
        {
            try
            {
                TeamMemberResponse.TeamMembers viewModel = new TeamMemberResponse.TeamMembers();
                Teams objTeam = new Teams(_configuration);
                viewModel = objTeam.GetTeamMembers(projectName, teamName);

                if (!(string.IsNullOrEmpty(objTeam.LastFailureMessage)))
                {
                    AddMessage(id.ErrorId(), "Error while getting team members: " + objTeam.LastFailureMessage + Environment.NewLine);
                }
                return viewModel;
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(id.ErrorId(), "Error while getting team members: " + ex.Message);
            }

            return new TeamMemberResponse.TeamMembers();
        }

        /// <summary>
        /// Create Work Items
        /// </summary>
        /// <param name="model"></param>
        /// <param name="workItemJSON"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="id"></param>
        private void CreateWorkItems(Project model, string workItemJSON, AppConfiguration _defaultConfiguration, string id)
        {
            try
            {
                string jsonWorkItems = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, workItemJSON);
                //string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, workItemJSON);
                if (File.Exists(jsonWorkItems))
                {
                    WorkItem objWorkItem = new WorkItem(_defaultConfiguration);
                    jsonWorkItems = model.ReadJsonFile(jsonWorkItems);
                    JContainer workItemsParsed = JsonConvert.DeserializeObject<JContainer>(jsonWorkItems);

                    AddMessage(id, "Creating " + workItemsParsed.Count + " work items...");

                    jsonWorkItems = jsonWorkItems.Replace("$version$", _defaultConfiguration.VersionNumber);
                    bool workItemResult = objWorkItem.CreateWorkItemUsingByPassRules(model.ProjectName, jsonWorkItems);

                    if (!(string.IsNullOrEmpty(objWorkItem.LastFailureMessage)))
                    {
                        AddMessage(id.ErrorId(), "Error while creating workitems: " + objWorkItem.LastFailureMessage + Environment.NewLine);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(id.ErrorId(), "Error while creating workitems: " + ex.Message);

            }
        }



        /// <summary>
        /// Updates work items with parent child links
        /// </summary>
        /// <param name="model"></param>
        /// <param name="workItemUpdateJSON"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="id"></param>
        /// <param name="currentUser"></param>
        /// <param name="projectSettingsJSON"></param>
        private void UpdateWorkItems(Project model, string workItemUpdateJSON, AppConfiguration _defaultConfiguration, string id, string currentUser, string projectSettingsJSON)
        {
            try
            {
                string jsonWorkItemsUpdate = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, workItemUpdateJSON);
                //string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, workItemUpdateJSON);
                string jsonProjectSettings = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, projectSettingsJSON);
                //string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, projectSettingsJSON);
                if (File.Exists(jsonWorkItemsUpdate))
                {
                    WorkItem objWorkItem = new WorkItem(_defaultConfiguration);
                    jsonWorkItemsUpdate = model.ReadJsonFile(jsonWorkItemsUpdate);
                    jsonProjectSettings = model.ReadJsonFile(jsonProjectSettings);

                    bool workItemUpdateResult = objWorkItem.UpdateWorkItemUsingByPassRules(jsonWorkItemsUpdate, model.ProjectName, currentUser, jsonProjectSettings);
                    if (!(string.IsNullOrEmpty(objWorkItem.LastFailureMessage)))
                    {
                        AddMessage(id.ErrorId(), "Error while updating work items: " + objWorkItem.LastFailureMessage + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");

                AddMessage(id.ErrorId(), "Error while updating work items: " + ex.Message);

            }
        }





        /// <summary>
        /// Import source code from sourec repo or GitHub
        /// </summary>
        /// <param name="model"></param>
        /// <param name="sourceCodeJSON"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="importSourceConfiguration"></param>
        /// <param name="id"></param>
        private void ImportSourceCode(Project model, string sourceCodeJSON, AppConfiguration _repo, string id, AppConfiguration _retSourceCodeVersion)
        {

            try
            {
                string[] repositoryDetail = new string[2];
                if (model.GitHubFork)
                {

                }
                if (File.Exists(sourceCodeJSON))
                {
                    Repository objRepository = new Repository(_repo);
                    string repositoryName = Path.GetFileName(sourceCodeJSON).Replace(".json", "");
                    if (model.ProjectName.ToLower() == repositoryName.ToLower())
                    {
                        repositoryDetail = objRepository.GetDefaultRepository(model.ProjectName);
                    }
                    else
                    {
                        repositoryDetail = objRepository.CreateRepository(repositoryName, model.Environment.ProjectId);
                    }
                    if (repositoryDetail.Length > 0)
                    {
                        model.Environment.RepositoryIdList[repositoryDetail[1]] = repositoryDetail[0];
                    }

                    string jsonSourceCode = model.ReadJsonFile(sourceCodeJSON);

                    //update endpoint ids
                    foreach (string endpoint in model.Environment.ServiceEndpoints.Keys)
                    {
                        string placeHolder = string.Format("${0}$", endpoint);
                        jsonSourceCode = jsonSourceCode.Replace(placeHolder, model.Environment.ServiceEndpoints[endpoint]);
                    }

                    Repository objRepositorySourceCode = new Repository(_retSourceCodeVersion);
                    bool copySourceCode = objRepositorySourceCode.GetSourceCodeFromGitHub(jsonSourceCode, model.ProjectName, repositoryDetail[0]);

                    if (!(string.IsNullOrEmpty(objRepository.LastFailureMessage)))
                    {
                        AddMessage(id.ErrorId(), "Error while importing source code: " + objRepository.LastFailureMessage + Environment.NewLine);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(id.ErrorId(), "Error while importing source code: " + ex.Message);
            }
        }


        /// <summary>
        /// Creates service end points
        /// </summary>
        /// <param name="model"></param>
        /// <param name="jsonPaths"></param>
        /// <param name="_defaultConfiguration"></param>
        private void CreateServiceEndPoint(Project model, List<string> jsonPaths, AppConfiguration _endpointConfig)
        {
            try
            {
                string serviceEndPointId = string.Empty;
                foreach (string jsonPath in jsonPaths)
                {
                    string fileName = Path.GetFileName(jsonPath);
                    string jsonCreateService = jsonPath;
                    if (File.Exists(jsonCreateService))
                    {
                        string username = System.Configuration.ConfigurationManager.AppSettings["UserID"];
                        string password = System.Configuration.ConfigurationManager.AppSettings["Password"];
                        //string extractPath = HostingEnvironment.MapPath("~/Templates/" + model.SelectedTemplate);
                        string projectFileData = File.ReadAllText(ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "ProjectTemplate.json"));
                        ExtractorProjectSettings settings = JsonConvert.DeserializeObject<ExtractorProjectSettings>(projectFileData);
                        ServiceEndPoint objService = new ServiceEndPoint(_endpointConfig);

                        string gitUserName = System.Configuration.ConfigurationManager.AppSettings["GitUserName"];
                        string gitUserPassword = System.Configuration.ConfigurationManager.AppSettings["GitUserPassword"];

                        jsonCreateService = model.ReadJsonFile(jsonCreateService);

                        if (!string.IsNullOrEmpty(settings.IsPrivate))
                        {
                            jsonCreateService = jsonCreateService.Replace("$ProjectName$", model.ProjectName);
                            jsonCreateService = jsonCreateService.Replace("$username$", model.Email).Replace("$password$", model.AccessToken);
                        }
                        // File contains "GitHub_" means - it contains GitHub URL, user wanted to fork repo to his github
                        if (fileName.Contains("GitHub_") && model.GitHubFork && model.GitHubToken != null)
                        {
                            JObject jsonToCreate = JObject.Parse(jsonCreateService);
                            string type = jsonToCreate["type"].ToString();
                            string url = jsonToCreate["url"].ToString();
                            string repoNameInUrl = Path.GetFileName(url);
                            // Endpoint type is Git(External Git), so we should point Build def to his repo by creating endpoint of Type GitHub(Public)
                            foreach (var repo in model.Environment.GitHubRepos.Keys)
                            {
                                if (repoNameInUrl.Contains(repo))
                                {
                                    if (type.ToLower() == "git")
                                    {
                                        jsonToCreate["type"] = "GitHub"; //Changing endpoint type
                                        jsonToCreate["url"] = model.Environment.GitHubRepos[repo].ToString(); // updating endpoint URL with User forked repo URL
                                    }
                                    // Endpoint type is GitHub(Public), so we should point the build def to his repo by updating the URL
                                    else if (type.ToLower() == "github")
                                    {
                                        jsonToCreate["url"] = model.Environment.GitHubRepos[repo].ToString(); // Updating repo URL to user repo
                                    }
                                    else
                                    {

                                    }
                                }
                            }
                            jsonCreateService = jsonToCreate.ToString();
                            jsonCreateService = jsonCreateService.Replace("$GitUserName$", model.GitHubUserName).Replace("$GitUserPassword$", model.GitHubToken);
                        }
                        // user doesn't want to fork repo
                        else
                        {
                            jsonCreateService = jsonCreateService.Replace("$ProjectName$", model.ProjectName); // Replaces the Place holder with project name if exists
                            jsonCreateService = jsonCreateService.Replace("$username$", username).Replace("$password$", password) // Replaces user name and password with app setting username and password if require[to import soure code to Azure Repos]
                                .Replace("$GitUserName$", gitUserName).Replace("$GitUserPassword$", gitUserPassword); // Replaces GitUser name and passwords with Demo gen username and password [Just to point build def to respective repo]
                        }
                        if (model.SelectedTemplate.ToLower() == "bikesharing360")
                        {
                            string bikeSharing360username = System.Configuration.ConfigurationManager.AppSettings["UserID"];
                            string bikeSharing360password = System.Configuration.ConfigurationManager.AppSettings["BikeSharing360Password"];
                            jsonCreateService = jsonCreateService.Replace("$BikeSharing360username$", bikeSharing360username).Replace("$BikeSharing360password$", bikeSharing360password);
                        }
                        else if (model.SelectedTemplate.ToLower() == "contososhuttle" || model.SelectedTemplate.ToLower() == "contososhuttle2")
                        {
                            string contosousername = System.Configuration.ConfigurationManager.AppSettings["ContosoUserID"];
                            string contosopassword = System.Configuration.ConfigurationManager.AppSettings["ContosoPassword"];
                            jsonCreateService = jsonCreateService.Replace("$ContosoUserID$", contosousername).Replace("$ContosoPassword$", contosopassword);
                        }
                        else if (model.SelectedTemplate.ToLower() == "sonarqube")
                        {
                            if (!string.IsNullOrEmpty(model.SonarQubeDNS))
                            {
                                jsonCreateService = jsonCreateService.Replace("$URL$", model.SonarQubeDNS);
                            }
                        }
                        else if (model.SelectedTemplate.ToLower() == "octopus")
                        {
                            var url = model.Parameters["OctopusURL"];
                            var apiKey = model.Parameters["APIkey"];
                            if (!string.IsNullOrEmpty(url.ToString()) && !string.IsNullOrEmpty(apiKey.ToString()))
                            {
                                jsonCreateService = jsonCreateService.Replace("$URL$", url).Replace("$Apikey$", apiKey);

                            }
                        }
                        var endpoint = objService.CreateServiceEndPoint(jsonCreateService, model.ProjectName);

                        if (!(string.IsNullOrEmpty(objService.LastFailureMessage)))
                        {
                            AddMessage(model.Id.ErrorId(), "Error while creating service endpoint: " + objService.LastFailureMessage + Environment.NewLine);
                        }
                        else
                        {
                            model.Environment.ServiceEndpoints[endpoint.Name] = endpoint.Id;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.Id.ErrorId(), "Error while creating service endpoint: " + ex.Message);
            }
        }

        /// <summary>
        /// Create Test Cases
        /// </summary>
        /// <param name="wiMapping"></param>
        /// <param name="model"></param>
        /// <param name="testPlanJson"></param>
        /// <param name="_defaultConfiguration"></param>
        private void CreateTestManagement(List<WiMapData> wiMapping, Project model, string testPlanJson, AppConfiguration _testPlanVersion)
        {
            try
            {
                if (File.Exists(testPlanJson))
                {
                    List<WiMapData> testCaseMap = new List<WiMapData>();
                    testCaseMap = wiMapping.Where(x => x.WiType == "Test Case").ToList();

                    string fileName = Path.GetFileName(testPlanJson);
                    testPlanJson = model.ReadJsonFile(testPlanJson);

                    testPlanJson = testPlanJson.Replace("$project$", model.ProjectName);
                    TestManagement objTest = new TestManagement(_testPlanVersion);
                    string[] testPlanResponse = new string[2];
                    testPlanResponse = objTest.CreateTestPlan(testPlanJson, model.ProjectName);

                    if (testPlanResponse.Length > 0)
                    {
                        string testSuiteJson = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, @"\TestPlans\TestSuites\" + fileName);
                        //string.Format(templateFolder + @"{0}\TestPlans\TestSuites\{1}", model.SelectedTemplate, fileName);
                        if (File.Exists(testSuiteJson))
                        {
                            testSuiteJson = model.ReadJsonFile(testSuiteJson);
                            testSuiteJson = testSuiteJson.Replace("$planID$", testPlanResponse[0]).Replace("$planName$", testPlanResponse[1]);
                            foreach (var wi in wiMapping)
                            {
                                string placeHolder = string.Format("${0}$", wi.OldId);
                                testSuiteJson = testSuiteJson.Replace(placeHolder, wi.NewId);
                            }
                            TestSuite.TestSuites listTestSuites = JsonConvert.DeserializeObject<TestSuite.TestSuites>(testSuiteJson);
                            if (listTestSuites.Count > 0)
                            {
                                foreach (var TS in listTestSuites.Value)
                                {
                                    string[] testSuiteResponse = new string[2];
                                    string testSuiteJSON = JsonConvert.SerializeObject(TS);
                                    testSuiteResponse = objTest.CreatTestSuite(testSuiteJSON, testPlanResponse[0], model.ProjectName);
                                    if (testSuiteResponse[0] != null && testSuiteResponse[1] != null)
                                    {
                                        string testCasesToAdd = string.Empty;
                                        foreach (string id in TS.TestCases)
                                        {
                                            foreach (var wiMap in testCaseMap)
                                            {
                                                if (wiMap.OldId == id)
                                                {
                                                    testCasesToAdd = testCasesToAdd + wiMap.NewId + ",";
                                                }
                                            }
                                        }
                                        testCasesToAdd = testCasesToAdd.TrimEnd(',');
                                        bool isTestCasesAddded = objTest.AddTestCasesToSuite(testCasesToAdd, testPlanResponse[0], testSuiteResponse[0], model.ProjectName);
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
                AddMessage(model.Id.ErrorId(), "Error while creating test plan and test suites: " + ex.Message);
            }
        }




        public bool InstallExtensions(Project model, string accountName, string PAT)
        {
            try
            {
                //string templatesFolder = HostingEnvironment.MapPath("~") + @"\Templates\";
                string projTemplateFile = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "Extensions.json");
                //string.Format(templatesFolder + @"{0}\Extensions.json", model.SelectedTemplate);
                if (!(File.Exists(projTemplateFile)))
                {
                    return false;
                }
                string templateItems = File.ReadAllText(projTemplateFile);
                var template = JsonConvert.DeserializeObject<RequiredExtensions.Extension>(templateItems);
                string requiresExtensionNames = string.Empty;

                //Check for existing extensions
                if (template.Extensions.Count > 0)
                {
                    Dictionary<string, bool> dict = new Dictionary<string, bool>();
                    foreach (RequiredExtensions.ExtensionWithLink ext in template.Extensions)
                    {
                        if (!dict.ContainsKey(ext.ExtensionName))
                        {
                            dict.Add(ext.ExtensionName, false);
                        }
                    }
                    //var connection = new VssConnection(new Uri(string.Format("https://{0}.visualstudio.com", accountName)), new Microsoft.VisualStudio.Services.OAuth.VssOAuthAccessTokenCredential(PAT));// VssOAuthCredential(PAT));
                    var connection = new VssConnection(new Uri(string.Format("https://{0}.visualstudio.com", accountName)), new VssBasicCredential(string.Empty, PAT));// VssOAuthCredential(PAT));

                    var client = connection.GetClient<ExtensionManagementHttpClient>();
                    var installed = client.GetInstalledExtensionsAsync().Result;
                    var extensions = installed.Where(x => x.Flags == 0).ToList();

                    var trustedFlagExtensions = installed.Where(x => x.Flags == ExtensionFlags.Trusted).ToList();
                    var builtInExtensions = installed.Where(x => x.Flags.ToString() == "BuiltIn, Trusted").ToList();
                    extensions.AddRange(trustedFlagExtensions);
                    extensions.AddRange(builtInExtensions);

                    foreach (var ext in extensions)
                    {
                        foreach (var extension in template.Extensions)
                        {
                            if (extension.ExtensionName.ToLower() == ext.ExtensionDisplayName.ToLower() && extension.ExtensionId.ToLower() == ext.ExtensionName.ToLower())
                            {
                                dict[extension.ExtensionName] = true;
                            }
                        }
                    }
                    var required = dict.Where(x => x.Value == false).ToList();

                    if (required.Count > 0)
                    {
                        Parallel.ForEach(required, async req =>
                        {
                            string publisherName = template.Extensions.Where(x => x.ExtensionName == req.Key).FirstOrDefault().PublisherId;
                            string extensionName = template.Extensions.Where(x => x.ExtensionName == req.Key).FirstOrDefault().ExtensionId;
                            try
                            {
                                InstalledExtension extension = null;
                                extension = await client.InstallExtensionByNameAsync(publisherName, extensionName);
                            }
                            catch (OperationCanceledException cancelException)
                            {
                                AddMessage(model.Id.ErrorId(), "Error while Installing extensions - operation cancelled: " + cancelException.Message + Environment.NewLine);
                            }
                            catch (Exception exc)
                            {
                                AddMessage(model.Id.ErrorId(), "Error while Installing extensions: " + exc.Message);
                            }
                        });
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                AddMessage(model.Id.ErrorId(), "Error while Installing extensions: " + ex.Message);
                return false;
            }
        }
        public void CreateDeploymentGroup(string templateFolder, Project model, AppConfiguration _deploymentGroup)
        {
            string path = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, @"\DeploymentGroups\CreateDeploymentGroup.json");
            //templateFolder + model.SelectedTemplate + "\\DeploymentGroups\\CreateDeploymentGroup.json";
            if (File.Exists(path))
            {
                string json = model.ReadJsonFile(path);
                if (!string.IsNullOrEmpty(json))
                {
                    DeploymentGroup deploymentGroup = new DeploymentGroup(_deploymentGroup);
                    bool isCreated = deploymentGroup.CreateDeploymentGroup(json);
                    if (isCreated) { } else if (!string.IsNullOrEmpty(deploymentGroup.LastFailureMessage)) { AddMessage(model.Id.ErrorId(), "Error while creating deployment group: " + deploymentGroup.LastFailureMessage); }
                }
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public string GetTemplateMessage(string TemplateName)
        {
            try
            {
                string groupDetails = "";
                TemplateSelection.Templates templates = new TemplateSelection.Templates();
                string templatesPath = ""; templatesPath = Path.Combine("~") + @"\Templates\";
                if (File.Exists(templatesPath + "TemplateSetting.json"))
                {
                    groupDetails = File.ReadAllText(templatesPath + @"\TemplateSetting.json");
                    templates = JsonConvert.DeserializeObject<TemplateSelection.Templates>(groupDetails);
                    foreach (var template in templates.GroupwiseTemplates.FirstOrDefault().Template)
                    {
                        if (template.TemplateFolder.ToLower() == TemplateName.ToLower())
                        {
                            return template.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return string.Empty;
        }

        private bool AddUserToProject(AppConfiguration con, Project model)
        {
            try
            {
                HttpServices httpService = new HttpServices(con);
                string PAT = string.Empty;
                string descriptorUrl = string.Format("_apis/graph/descriptors/{0}?api-version={1}", Convert.ToString(model.Environment.ProjectId), con.VersionNumber);
                var groups = httpService.Get(descriptorUrl);
                //dynamic obj = new dynamic();
                if (groups.IsSuccessStatusCode)
                {
                    dynamic obj = JsonConvert.DeserializeObject<dynamic>(groups.Content.ReadAsStringAsync().Result);
                    string getGroupDescriptor = string.Format("_apis/graph/groups?scopeDescriptor={0}&api-version={1}", Convert.ToString(obj.Value), con.VersionNumber);
                    var getAllGroups = httpService.Get(getGroupDescriptor);
                    if (getAllGroups.IsSuccessStatusCode)
                    {
                        GetAllGroups.GroupList allGroups = JsonConvert.DeserializeObject<GetAllGroups.GroupList>(getAllGroups.Content.ReadAsStringAsync().Result);
                        foreach (var group in allGroups.Value)
                        {
                            if (group.DisplayName.ToLower() == "project administrators")
                            {
                                string urpParams = string.Format("_apis/graph/users?groupDescriptors={0}&api-version={1}", Convert.ToString(group.Descriptor), con.VersionNumber);
                                var json = CreatePrincipalReqBody(model.Email);
                                var response = httpService.Post(json, urpParams);
                            }
                            if (group.DisplayName.ToLower() == model.ProjectName.ToLower() + " team")
                            {
                                string urpParams = string.Format("_apis/graph/users?groupDescriptors={0}&api-version={1}", Convert.ToString(group.Descriptor), con.VersionNumber);
                                var json = CreatePrincipalReqBody(model.Email);
                                var response = httpService.Post(json, urpParams);
                            }
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return false;
        }

        public static string CreatePrincipalReqBody(string name)
        {
            return "{\"principalName\": \"" + name + "\"}";
        }
        #endregion

        public bool CheckForInstalledExtensions(string extensionJsonFile, string token, string account)
        {
            bool ExtensionRequired = false;
            try
            {
                string accountName = account;
                string pat = token;
                string listedExtension = File.ReadAllText(extensionJsonFile);
                var template = JsonConvert.DeserializeObject<RequiredExtensions.Extension>(listedExtension);
                string requiresExtensionNames = string.Empty;
                string requiredMicrosoftExt = string.Empty;
                string requiredThirdPartyExt = string.Empty;
                string finalExtensionString = string.Empty;

                //Check for existing extensions
                if (template.Extensions.Count > 0)
                {
                    Dictionary<string, bool> dict = new Dictionary<string, bool>();
                    foreach (RequiredExtensions.ExtensionWithLink ext in template.Extensions)
                    {
                        dict.Add(ext.ExtensionName, false);
                    }
                    //pat = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", pat)));//configuration.PersonalAccessToken;

                    //var connection = new VssConnection(new Uri(string.Format("https://{0}.visualstudio.com", accountName)), new Microsoft.VisualStudio.Services.OAuth.VssOAuthAccessTokenCredential(pat));// VssOAuthCredential(PAT));
                    var connection = new VssConnection(new Uri(string.Format("https://{0}.visualstudio.com", accountName)), new VssBasicCredential(string.Empty, pat));// VssOAuthCredential(PAT));

                    var client = connection.GetClient<ExtensionManagementHttpClient>();
                    var installed = client.GetInstalledExtensionsAsync().Result;
                    var extensions = installed.Where(x => x.Flags == 0).ToList();

                    var trustedFlagExtensions = installed.Where(x => x.Flags == ExtensionFlags.Trusted).ToList();
                    var builtInExtensions = installed.Where(x => x.Flags.ToString() == "BuiltIn, Trusted").ToList();

                    extensions.AddRange(trustedFlagExtensions);
                    extensions.AddRange(builtInExtensions);

                    foreach (var ext in extensions)
                    {
                        foreach (var extension in template.Extensions)
                        {
                            if (extension.ExtensionName.ToLower() == ext.ExtensionDisplayName.ToLower())
                            {
                                dict[extension.ExtensionName] = true;
                            }
                        }
                    }
                    var required = dict.Where(x => x.Value == false).ToList();
                    if (required.Count > 0)
                    {
                        ExtensionRequired = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                //return Json(new { message = "Error", status = "false" }, JsonRequestBehavior.AllowGet);
                ExtensionRequired = false;
            }
            return ExtensionRequired;
        }

        private void CreateVaribaleGroups(Project model, AppConfiguration _variableGroups)
        {
            VariableGroups variableGroups = new VariableGroups(_variableGroups);
            model.Environment.VariableGroups = new Dictionary<int, string>();
            string filePath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, @"\VariableGroups\VariableGroup.json");
            if (File.Exists(filePath))
            {
                string jsonString = model.ReadJsonFile(filePath);
                GetVariableGroups.Groups groups = JsonConvert.DeserializeObject<GetVariableGroups.Groups>(jsonString);
                if (groups.Count > 0)
                {
                    foreach (var group in groups.Value)
                    {
                        GetVariableGroups.VariableGroupsCreateResponse response = variableGroups.PostVariableGroups(JsonConvert.SerializeObject(group));
                        if (!string.IsNullOrEmpty(response.Name))
                        {
                            model.Environment.VariableGroups.Add(response.Id, response.Name);
                        }
                    }
                }
            }
        }
    }
}