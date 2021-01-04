using AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureDevOpsAPI.QueriesAndWidgets;
using AzureDevOpsAPI.Release;
using AzureDevOpsAPI.Viewmodel.Extractor;
using AzureDevOpsAPI.Viewmodel.ProjectAndTeams;
using AzureDevOpsAPI.Viewmodel.QueriesAndWidgets;
using AzureDevOpsAPI.Viewmodel.Sprint;

namespace AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Creators
{
    public class QueryAndWidgetsCreator : BaseCreator
    {
        public QueryAndWidgetsCreator(AzureDevOpsAPI.Viewmodel.Extractor.ExtractorProjectSettings setting) : base(setting)
        {

        }

        /// <summary>
        /// Dashboard set up operations
        /// </summary>
        /// <param name="model"></param>
        /// <param name="listQueries"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="_configuration2"></param>
        /// <param name="_configuration3"></param>
        /// <param name="releaseConfig"></param>
        public void CreateQueryAndWidgets(Project model, List<string> listQueries, AzureDevOpsAPI.AppConfiguration _queriesVersion, AzureDevOpsAPI.AppConfiguration _dashboardVersion, AzureDevOpsAPI.AppConfiguration _releaseConfig, AzureDevOpsAPI.AppConfiguration _projectConfig, AzureDevOpsAPI.AppConfiguration _boardConfig)
        {
            try
            {
                Queries objWidget = new Queries(_dashboardVersion);
                Queries objQuery = new Queries(_queriesVersion);
                List<QueryResponse> queryResults = new List<QueryResponse>();

                //GetDashBoardDetails
                string dashBoardId = objWidget.GetDashBoardId(model.ProjectName);
                Thread.Sleep(2000); // Adding delay to get the existing dashboard ID 

                if (!string.IsNullOrEmpty(objQuery.LastFailureMessage))
                {
                    Errors.Add("Error while getting dashboardId: " + objWidget.LastFailureMessage + Environment.NewLine);
                }

                foreach (string query in listQueries)
                {
                    Queries _newobjQuery = new Queries(_queriesVersion);

                    //create query
                    string json = model.ReadJsonFile(query);
                    json = json.Replace("$projectId$", model.Environment.ProjectName);
                    QueryResponse response = _newobjQuery.CreateQuery(model.ProjectName, json);
                    queryResults.Add(response);

                    if (!string.IsNullOrEmpty(_newobjQuery.LastFailureMessage))
                    {
                        Errors.Add("Error while creating query: " + _newobjQuery.LastFailureMessage + Environment.NewLine);
                    }
                }
                //Create DashBoards
                string dashBoardTemplate = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, @"\Dashboard\Dashboard.json");
                //string.Format(templatesFolder + @"{0}\Dashboard\Dashboard.json", model.SelectedTemplate);
                if (File.Exists(dashBoardTemplate))
                {
                    dynamic dashBoard = new System.Dynamic.ExpandoObject();
                    dashBoard.name = "Working";
                    dashBoard.position = 4;

                    string jsonDashBoard = Newtonsoft.Json.JsonConvert.SerializeObject(dashBoard);
                    string dashBoardIdToDelete = objWidget.CreateNewDashBoard(model.ProjectName, jsonDashBoard);

                    bool isDashboardDeleted = objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardId);

                    if (model.SelectedTemplate.ToLower() == "bikesharing360")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);

                            string xamarin_DroidBuild = model.BuildDefinitions.Where(x => x.Name == "Xamarin.Droid").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "Xamarin.Droid").FirstOrDefault().Id : string.Empty;
                            string xamarin_IOSBuild = model.BuildDefinitions.Where(x => x.Name == "Xamarin.iOS").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "Xamarin.iOS").FirstOrDefault().Id : string.Empty;
                            string ridesApiBuild = model.BuildDefinitions.Where(x => x.Name == "RidesApi").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "RidesApi").FirstOrDefault().Id : string.Empty;

                            ReleaseDefinition objrelease = new ReleaseDefinition(_releaseConfig);
                            int[] androidEnvironmentIds = objrelease.GetEnvironmentIdsByName(model.ProjectName, "Xamarin.Android", "Test in HockeyApp", "Publish to store");
                            string androidbuildDefId = model.BuildDefinitions.Where(x => x.Name == "Xamarin.Droid").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "Xamarin.Droid").FirstOrDefault().Id : string.Empty;
                            string androidreleaseDefId = model.ReleaseDefinitions.Where(x => x.Name == "Xamarin.Android").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "Xamarin.Android").FirstOrDefault().Id : string.Empty;

                            int[] iosEnvironmentIds = objrelease.GetEnvironmentIdsByName(model.ProjectName, "Xamarin.iOS", "Test in HockeyApp", "Publish to store");
                            string iosBuildDefId = model.BuildDefinitions.Where(x => x.Name == "Xamarin.iOS").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "Xamarin.iOS").FirstOrDefault().Id : string.Empty;
                            string iosReleaseDefId = model.ReleaseDefinitions.Where(x => x.Name == "Xamarin.iOS").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "Xamarin.iOS").FirstOrDefault().Id : string.Empty;

                            string ridesApireleaseDefId = model.ReleaseDefinitions.Where(x => x.Name == "RidesApi").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "RidesApi").FirstOrDefault().Id : string.Empty;
                            QueryResponse openUserStories = objQuery.GetQueryByPathAndName(model.ProjectName, "Open User Stories", "Shared%20Queries/Current%20Iteration");

                            dashBoardTemplate = dashBoardTemplate.Replace("$RidesAPIReleaseId$", ridesApireleaseDefId)
                            .Replace("$RidesAPIBuildId$", ridesApiBuild)
                            .Replace("$repositoryId$", model.Environment.RepositoryIdList.Where(x => x.Key.ToLower() == "bikesharing360").FirstOrDefault().Value)
                            .Replace("$IOSBuildId$", iosBuildDefId).Replace("$IOSReleaseId$", iosReleaseDefId).Replace("$IOSEnv1$", iosEnvironmentIds[0].ToString()).Replace("$IOSEnv2$", iosEnvironmentIds[1].ToString())
                            .Replace("$Xamarin.iOS$", xamarin_IOSBuild)
                            .Replace("$Xamarin.Droid$", xamarin_DroidBuild)
                            .Replace("$AndroidBuildId$", androidbuildDefId).Replace("$AndroidreleaseDefId$", androidreleaseDefId).Replace("$AndroidEnv1$", androidEnvironmentIds[0].ToString()).Replace("$AndroidEnv2$", androidEnvironmentIds[1].ToString())
                            .Replace("$OpenUserStoriesId$", openUserStories.Id)
                            .Replace("$projectId$", model.Environment.ProjectId);

                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }
                    if (model.SelectedTemplate == "MyHealthClinic" || model.SelectedTemplate == "PartsUnlimited" || model.SelectedTemplate == "PartsUnlimited-agile")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);

                            QueryResponse feedBack = objQuery.GetQueryByPathAndName(model.ProjectName, "Feedback_WI", "Shared%20Queries");
                            QueryResponse unfinishedWork = objQuery.GetQueryByPathAndName(model.ProjectName, "Unfinished Work_WI", "Shared%20Queries");


                            dashBoardTemplate = dashBoardTemplate.Replace("$Feedback$", feedBack.Id).
                                         Replace("$AllItems$", queryResults.Where(x => x.Name == "All Items_WI").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "All Items_WI").FirstOrDefault().Id : string.Empty).
                                         Replace("$UserStories$", queryResults.Where(x => x.Name == "User Stories").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "User Stories").FirstOrDefault().Id : string.Empty).
                                         Replace("$TestCase$", queryResults.Where(x => x.Name == "Test Case-Readiness").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Test Case-Readiness").FirstOrDefault().Id : string.Empty).
                                         Replace("$teamID$", "").
                                         Replace("$teamName$", model.ProjectName + " Team").
                                         Replace("$projectID$", model.Environment.ProjectId).
                                         Replace("$Unfinished Work$", unfinishedWork.Id).
                                         Replace("$projectId$", model.Environment.ProjectId).
                                         Replace("$projectName$", model.ProjectName);


                            if (model.SelectedTemplate == "MyHealthClinic")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$ReleaseDefId$", model.ReleaseDefinitions.Where(x => x.Name == "MyHealthClinicE2E").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "MyHealthClinicE2E").FirstOrDefault().Id : string.Empty).
                                             Replace("$ActiveBugs$", queryResults.Where(x => x.Name == "Active Bugs_WI").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Active Bugs_WI").FirstOrDefault().Id : string.Empty).
                                             Replace("$MyHealthClinicE2E$", model.BuildDefinitions.Where(x => x.Name == "MyHealthClinicE2E").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "MyHealthClinicE2E").FirstOrDefault().Id : string.Empty).
                                                 Replace("$RepositoryId$", model.Environment.RepositoryIdList.Any(i => i.Key.ToLower().Contains("myhealthclinic")) ? model.Environment.RepositoryIdList.Where(x => x.Key.ToLower() == "myhealthclinic").FirstOrDefault().Value : string.Empty);
                            }
                            if (model.SelectedTemplate == "PartsUnlimited" || model.SelectedTemplate == "PartsUnlimited-agile")
                            {
                                QueryResponse workInProgress = objQuery.GetQueryByPathAndName(model.ProjectName, "Work in Progress_WI", "Shared%20Queries");

                                dashBoardTemplate = dashBoardTemplate.Replace("$ReleaseDefId$", model.ReleaseDefinitions.Where(x => x.Name == "PartsUnlimitedE2E").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "PartsUnlimitedE2E").FirstOrDefault().Id : string.Empty).
                                          Replace("$ActiveBugs$", queryResults.Where(x => x.Name == "Critical Bugs").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Critical Bugs").FirstOrDefault().Id : string.Empty).
                                          Replace("$PartsUnlimitedE2E$", model.BuildDefinitions.Where(x => x.Name == "PartsUnlimitedE2E").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "PartsUnlimitedE2E").FirstOrDefault().Id : string.Empty)
                                          .Replace("$WorkinProgress$", workInProgress.Id)
                                .Replace("$RepositoryId$", model.Environment.RepositoryIdList.Any(i => i.Key.ToLower().Contains("partsunlimited")) ? model.Environment.RepositoryIdList.Where(x => x.Key.ToLower() == "partsunlimited").FirstOrDefault().Value : string.Empty);

                            }
                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);

                        }
                    }
                    if (model.SelectedTemplate.ToLower() == "bikesharing 360")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);
                            QueryResponse unfinishedWork = objQuery.GetQueryByPathAndName(model.ProjectName, "Unfinished Work_WI", "Shared%20Queries");
                            string allItems = queryResults.Where(x => x.Name == "All Items_WI").FirstOrDefault().Id;
                            string repositoryId = model.Environment.RepositoryIdList.Where(x => x.Key.ToLower() == "bikesharing360").FirstOrDefault().Key;
                            string bikeSharing360_PublicWeb = model.BuildDefinitions.Where(x => x.Name == "BikeSharing360-PublicWeb").FirstOrDefault().Id;

                            dashBoardTemplate = dashBoardTemplate.Replace("$BikeSharing360-PublicWeb$", bikeSharing360_PublicWeb)
                                         .Replace("$All Items$", allItems)
                                         .Replace("$repositoryId$", repositoryId)
                                         .Replace("$Unfinished Work$", unfinishedWork.Id)
                                         .Replace("$projectId$", model.Environment.ProjectId);

                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }
                    if (model.SelectedTemplate == "MyShuttleDocker")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);
                            var buildDefId = model.BuildDefinitions.FirstOrDefault();
                            dashBoardTemplate = dashBoardTemplate.Replace("$BuildDefId$", buildDefId.Id)
                                  .Replace("$projectId$", model.Environment.ProjectId)
                                  .Replace("$PBI$", queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id != null ? queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id : string.Empty)
                                  .Replace("$Bugs$", queryResults.Where(x => x.Name == "Bugs").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Bugs").FirstOrDefault().Id : string.Empty)
                                  .Replace("$AllWorkItems$", queryResults.Where(x => x.Name == "All Work Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "All Work Items").FirstOrDefault().Id : string.Empty)
                                  .Replace("$Test Plan$", queryResults.Where(x => x.Name == "Test Plans").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Test Plans").FirstOrDefault().Id : string.Empty)
                                  .Replace("$Test Cases$", queryResults.Where(x => x.Name == "Test Cases").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Test Cases").FirstOrDefault().Id : string.Empty)
                                  .Replace("$Feature$", queryResults.Where(x => x.Name == "Feature").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Feature").FirstOrDefault().Id : string.Empty)
                                  .Replace("$Tasks$", queryResults.Where(x => x.Name == "Tasks").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Tasks").FirstOrDefault().Id : string.Empty)
                                         .Replace("$RepoMyShuttleDocker$", model.Environment.RepositoryIdList.Where(x => x.Key == "MyShuttleDocker").FirstOrDefault().ToString() != "" ? model.Environment.RepositoryIdList.Where(x => x.Key == "MyShuttleDocker").FirstOrDefault().Value : string.Empty);


                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }
                    if (model.SelectedTemplate == "MyShuttle")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);
                            dashBoardTemplate = dashBoardTemplate
                            .Replace("$PBI$", queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id : string.Empty)
                            .Replace("$Bugs$", queryResults.Where(x => x.Name == "Bugs").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Bugs").FirstOrDefault().Id : string.Empty)
                            .Replace("$AllWorkItems$", queryResults.Where(x => x.Name == "All Work Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "All Work Items").FirstOrDefault().Id : string.Empty)
                            .Replace("$TestPlan$", queryResults.Where(x => x.Name == "Test Plans").FirstOrDefault().Id != null ? queryResults.Where(x => x.Name == "Test Plans").FirstOrDefault().Id : string.Empty)
                            .Replace("$Test Cases$", queryResults.Where(x => x.Name == "Test Cases").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Test Cases").FirstOrDefault().Id : string.Empty)
                            .Replace("$Features$", queryResults.Where(x => x.Name == "Feature").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Feature").FirstOrDefault().Id : string.Empty)
                            .Replace("$Tasks$", queryResults.Where(x => x.Name == "Tasks").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Tasks").FirstOrDefault().Id : string.Empty)
                            .Replace("$TestSuite$", queryResults.Where(x => x.Name == "Test Suites").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Test Suites").FirstOrDefault().Id : string.Empty);


                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }
                    if (model.SelectedTemplate.ToLower() == "myshuttle2")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);

                            dashBoardTemplate = dashBoardTemplate.Replace("$TestCases$", queryResults.Where(x => x.Name == "Test Cases").FirstOrDefault().Id != null ? queryResults.Where(x => x.Name == "Test Cases").FirstOrDefault().Id : string.Empty)
                                         .Replace("$AllWorkItems$", queryResults.Where(x => x.Name == "All Work Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "All Work Items").FirstOrDefault().Id : string.Empty)
                                         .Replace("$PBI$", queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id : string.Empty)
                                         .Replace("$RepoMyShuttleCalc$", model.Environment.RepositoryIdList["MyShuttleCalc"] != null ? model.Environment.RepositoryIdList["MyShuttleCalc"] : string.Empty)
                                         .Replace("$TestPlan$", queryResults.Where(x => x.Name == "Test Plans").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Test Plans").FirstOrDefault().Id : string.Empty)
                                         .Replace("$Tasks$", queryResults.Where(x => x.Name == "Tasks").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Tasks").FirstOrDefault().Id : string.Empty)
                                         .Replace("$Bugs$", queryResults.Where(x => x.Name == "Bugs").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Bugs").FirstOrDefault().Id : string.Empty)
                                         .Replace("$Features$", queryResults.Where(x => x.Name == "Feature").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Feature").FirstOrDefault().Id : string.Empty)
                                         .Replace("$RepoMyShuttle2$", model.Environment.RepositoryIdList.Where(x => x.Key.ToLower() == "myshuttle2").FirstOrDefault().ToString() != "" ? model.Environment.RepositoryIdList.Where(x => x.Key.ToLower() == "myshuttle2").FirstOrDefault().Value : string.Empty);


                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }
                    if (model.SelectedTemplate.ToLower() == "docker" || model.SelectedTemplate.ToLower() == "php" || model.SelectedTemplate.ToLower() == "sonarqube" || model.SelectedTemplate.ToLower() == "github" || model.SelectedTemplate.ToLower() == "whitesource bolt" || model.SelectedTemplate == "DeploymentGroups" || model.SelectedTemplate == "Octopus")
                    {
                        if (isDashboardDeleted)
                        {
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);
                            dashBoardTemplate = dashBoardTemplate.Replace("$Task$", queryResults.Where(x => x.Name == "Tasks").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Tasks").FirstOrDefault().Id : string.Empty)
                                         .Replace("$AllWorkItems$", queryResults.Where(x => x.Name == "All Work Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "All Work Items").FirstOrDefault().Id : string.Empty)
                                         .Replace("$Feature$", queryResults.Where(x => x.Name == "Feature").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Feature").FirstOrDefault().Id : string.Empty)
                                         .Replace("$Projectid$", model.Environment.ProjectId)
                                         .Replace("$Epic$", queryResults.Where(x => x.Name == "Epics").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Epics").FirstOrDefault().Id : string.Empty);

                            if (model.SelectedTemplate.ToLower() == "docker")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$BuildDocker$", model.BuildDefinitions.Where(x => x.Name == "MHCDocker.build").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "MHCDocker.build").FirstOrDefault().Id : string.Empty)
                                .Replace("$ReleaseDocker$", model.ReleaseDefinitions.Where(x => x.Name == "MHCDocker.release").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "MHCDocker.release").FirstOrDefault().Id : string.Empty)
                                  .Replace("$PBI$", queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id : string.Empty);
                            }
                            else if (model.SelectedTemplate.ToLower() == "php")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$buildPHP$", model.BuildDefinitions.Where(x => x.Name == "PHP").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "PHP").FirstOrDefault().Id : string.Empty)
                        .Replace("$releasePHP$", model.ReleaseDefinitions.Where(x => x.Name == "PHP").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "PHP").FirstOrDefault().Id : string.Empty)
                                 .Replace("$PBI$", queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id : string.Empty);
                            }
                            else if (model.SelectedTemplate.ToLower() == "sonarqube")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$BuildSonarQube$", model.BuildDefinitions.Where(x => x.Name == "SonarQube").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "SonarQube").FirstOrDefault().Id : string.Empty)
                                .Replace("$PBI$", queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id : string.Empty);

                            }
                            else if (model.SelectedTemplate.ToLower() == "github")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$PBI$", queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id : string.Empty)
                                             .Replace("$buildGitHub$", model.BuildDefinitions.Where(x => x.Name == "GitHub").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "GitHub").FirstOrDefault().Id : string.Empty)
                                             .Replace("$Hosted$", model.Environment.AgentQueues["Hosted"].ToString())
                                             .Replace("$releaseGitHub$", model.ReleaseDefinitions.Where(x => x.Name == "GitHub").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "GitHub").FirstOrDefault().Id : string.Empty);

                            }
                            else if (model.SelectedTemplate.ToLower() == "whitesource bolt")
                            {
                                dashBoardTemplate = dashBoardTemplate.Replace("$PBI$", queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id : string.Empty)
                                          .Replace("$buildWhiteSource$", model.BuildDefinitions.Where(x => x.Name == "WhiteSourceBolt").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "WhiteSourceBolt").FirstOrDefault().Id : string.Empty);
                            }

                            else if (model.SelectedTemplate == "DeploymentGroups")
                            {
                                QueryResponse WorkInProgress = objQuery.GetQueryByPathAndName(model.ProjectName, "Work in Progress_WI", "Shared%20Queries");
                                dashBoardTemplate = dashBoardTemplate.Replace("$WorkinProgress$", WorkInProgress.Id);
                            }

                            else if (model.SelectedTemplate == "Octopus")
                            {
                                var BuildDefId = model.BuildDefinitions.FirstOrDefault();
                                if (BuildDefId != null)
                                {
                                    dashBoardTemplate = dashBoardTemplate.Replace("$BuildDefId$", BuildDefId.Id)
                                            .Replace("$PBI$", queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault() != null ? queryResults.Where(x => x.Name == "Product Backlog Items").FirstOrDefault().Id : string.Empty);
                                }
                            }


                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }

                    if (model.SelectedTemplate.ToLower() == "smarthotel360")
                    {
                        if (isDashboardDeleted)
                        {
                            string startdate = DateTime.Now.ToString("yyyy-MM-dd");
                            AzureDevOpsAPI.ProjectsAndTeams.Teams objTeam = new AzureDevOpsAPI.ProjectsAndTeams.Teams(_projectConfig);
                            TeamResponse defaultTeam = objTeam.GetTeamByName(model.ProjectName, model.ProjectName + " team");
                            AzureDevOpsAPI.WorkItemAndTracking.ClassificationNodes objnodes = new AzureDevOpsAPI.WorkItemAndTracking.ClassificationNodes(_boardConfig);
                            SprintResponse.Sprints sprints = objnodes.GetSprints(model.ProjectName);
                            QueryResponse allItems = objQuery.GetQueryByPathAndName(model.ProjectName, "All Items_WI", "Shared%20Queries");
                            QueryResponse backlogBoardWI = objQuery.GetQueryByPathAndName(model.ProjectName, "BacklogBoard WI", "Shared%20Queries");
                            QueryResponse boardWI = objQuery.GetQueryByPathAndName(model.ProjectName, "Board WI", "Shared%20Queries");
                            QueryResponse bugsWithoutReproSteps = objQuery.GetQueryByPathAndName(model.ProjectName, "Bugs without Repro Steps", "Shared%20Queries");
                            QueryResponse feedback = objQuery.GetQueryByPathAndName(model.ProjectName, "Feedback_WI", "Shared%20Queries");
                            QueryResponse mobileTeamWork = objQuery.GetQueryByPathAndName(model.ProjectName, "MobileTeam_Work", "Shared%20Queries");
                            QueryResponse webTeamWork = objQuery.GetQueryByPathAndName(model.ProjectName, "WebTeam_Work", "Shared%20Queries");
                            QueryResponse stateofTestCase = objQuery.GetQueryByPathAndName(model.ProjectName, "State of TestCases", "Shared%20Queries");
                            QueryResponse bugs = objQuery.GetQueryByPathAndName(model.ProjectName, "Open Bugs_WI", "Shared%20Queries");

                            QueryResponse unfinishedWork = objQuery.GetQueryByPathAndName(model.ProjectName, "Unfinished Work_WI", "Shared%20Queries");
                            QueryResponse workInProgress = objQuery.GetQueryByPathAndName(model.ProjectName, "Work in Progress_WI", "Shared%20Queries");
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);
                            dashBoardTemplate = dashBoardTemplate.Replace("$WorkinProgress$", workInProgress.Id)
                                .Replace("$projectId$", model.Environment.ProjectId != null ? model.Environment.ProjectId : string.Empty)
                                .Replace("$PublicWebBuild$", model.BuildDefinitions.Where(x => x.Name == "SmartHotel_Petchecker-Web").FirstOrDefault() != null ? model.BuildDefinitions.Where(x => x.Name == "SmartHotel_Petchecker-Web").FirstOrDefault().Id : string.Empty)
                                .Replace("$DefaultTeamId$", defaultTeam.Id != null ? defaultTeam.Id : string.Empty).Replace("$AllItems$", allItems.Id != null ? allItems.Id : string.Empty)
                                .Replace("$BacklogBoardWI$", backlogBoardWI.Id != null ? backlogBoardWI.Id : string.Empty)
                                .Replace("$StateofTestCases$", stateofTestCase.Id != null ? stateofTestCase.Id : string.Empty)
                                .Replace("$Feedback$", feedback.Id != null ? feedback.Id : string.Empty)
                                .Replace("$RepoPublicWeb$", model.Environment.RepositoryIdList.ContainsKey("PublicWeb") ? model.Environment.RepositoryIdList["PublicWeb"] : string.Empty)
                                .Replace("$MobileTeamWork$", mobileTeamWork.Id != null ? mobileTeamWork.Id : string.Empty).Replace("$WebTeamWork$", webTeamWork.Id != null ? webTeamWork.Id : string.Empty)
                                .Replace("$Bugs$", bugs.Id != null ? bugs.Id : string.Empty)
                                .Replace("$sprint2$", sprints.Value.Where(x => x.Name == "Sprint 2").FirstOrDefault() != null ? sprints.Value.Where(x => x.Name == "Sprint 2").FirstOrDefault().Id : string.Empty)
                                .Replace("$sprint3$", sprints.Value.Where(x => x.Name == "Sprint 3").FirstOrDefault() != null ? sprints.Value.Where(x => x.Name == "Sprint 3").FirstOrDefault().Id : string.Empty)
                                .Replace("$startDate$", startdate)
                                .Replace("$BugswithoutRepro$", bugsWithoutReproSteps.Id != null ? bugsWithoutReproSteps.Id : string.Empty).Replace("$UnfinishedWork$", unfinishedWork.Id != null ? unfinishedWork.Id : string.Empty)
                                .Replace("$RepoSmartHotel360$", model.Environment.RepositoryIdList.ContainsKey("SmartHotel360") ? model.Environment.RepositoryIdList["SmartHotel360"] : string.Empty)
                                .Replace("$PublicWebSiteCD$", model.ReleaseDefinitions.Where(x => x.Name == "PublicWebSiteCD").FirstOrDefault() != null ? model.ReleaseDefinitions.Where(x => x.Name == "PublicWebSiteCD").FirstOrDefault().Id : string.Empty);

                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);

                        }
                    }
                    if (model.SelectedTemplate.ToLower() == "contososhuttle" || model.SelectedTemplate.ToLower() == "contososhuttle2")
                    {
                        if (isDashboardDeleted)
                        {
                            QueryResponse workInProgress = objQuery.GetQueryByPathAndName(model.ProjectName, "Work in Progress_WI", "Shared%20Queries");
                            dashBoardTemplate = model.ReadJsonFile(dashBoardTemplate);
                            dashBoardTemplate = dashBoardTemplate.Replace("$WorkinProgress$", workInProgress.Id);

                            string isDashBoardCreated = objWidget.CreateNewDashBoard(model.ProjectName, dashBoardTemplate);
                            objWidget.DeleteDefaultDashboard(model.ProjectName, dashBoardIdToDelete);
                        }
                    }
                }
            }
            catch (OperationCanceledException oce)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + oce.Message + "\t" + oce.InnerException.Message + "\n" + oce.StackTrace + "\n");
                Errors.Add("Error while creating Queries and Widgets: Operation cancelled exception " + oce.Message + "\r\n");
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                Errors.Add("Error while creating Queries and Widgets: " + ex.Message);
            }
        }
    }
}
