using AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsAPI;
using AzureDevOpsAPI.ProjectsAndTeams;
using AzureDevOpsAPI.Viewmodel.Extractor;
using AzureDevOpsAPI.Viewmodel.Importer;
using AzureDevOpsAPI.Viewmodel.ProjectAndTeams;
using AzureDevOpsAPI.Viewmodel.WorkItem;
using AzureDevOpsAPI.WorkItemAndTracking;

namespace AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Creators
{
    public class TeamsCreator : BaseCreator
    {
        private string path = string.Empty;
        public TeamsCreator(ExtractorProjectSettings settings) : base(settings)
        {

        }

        public void CreateV1Template(AppConfiguration _projectCreationVersion, AppConfiguration _boardVersion, ProjectSettings settings, Project model, ProjectTemplate template, string templateUsed)
        {
            //create teams
            CreateTeams(model, template.Teams, _projectCreationVersion, model.Id, template.TeamArea);

            // for older templates
            string projectSetting = File.ReadAllText(ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "ProjectSettings.json"));
            // File.ReadAllText( Path.Combine(templatesFolder + templateUsed, "ProjectSettings.json"));
            JObject projectObj = JsonConvert.DeserializeObject<JObject>(projectSetting);
            string processType = projectObj["type"] == null ? string.Empty : projectObj["type"].ToString();
            string boardType = string.Empty;
            if (processType == "" || processType == "Scrum")
            {
                processType = "Scrum";
                boardType = "Backlog%20items";
            }
            else if (processType == "Basic")
            {
                boardType = "Issue";
            }
            else
            {
                boardType = "Stories";
            }
            BoardColumn objBoard = new BoardColumn(_boardVersion);
            string updateSwimLanesJSON = "";
            if (template.BoardRows != null)
            {
                updateSwimLanesJSON = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.BoardRows);
                // Path.Combine(templatesFolder + templateUsed, template.BoardRows);
                SwimLanes objSwimLanes = new SwimLanes(_boardVersion);
                if (File.Exists(updateSwimLanesJSON))
                {
                    updateSwimLanesJSON = File.ReadAllText(updateSwimLanesJSON);
                    bool isUpdated = objSwimLanes.UpdateSwimLanes(updateSwimLanesJSON, model.ProjectName, boardType, model.ProjectName + " Team");
                }
            }
            if (template.SetEpic != null)
            {
                string team = model.ProjectName + " Team";
                string json = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.SetEpic);
                //string.Format(templatesFolder + @"{0}\{1}", templateUsed, template.SetEpic);
                if (File.Exists(json))
                {
                    json = model.ReadJsonFile(json);
                    EnableEpic(model, json, _boardVersion, model.Id, team);
                }
            }

            if (template.BoardColumns != null)
            {
                string team = model.ProjectName + " Team";
                string json = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.BoardColumns);
                //string.Format(templatesFolder + @"{0}\{1}", templateUsed, template.BoardColumns);
                if (File.Exists(json))
                {
                    json = model.ReadJsonFile(json);
                    bool success = UpdateBoardColumn(model, json, _boardVersion, model.Id, boardType, team);
                    if (success)
                    {
                        //update Card Fields
                        if (template.CardField != null)
                        {
                            string cardFieldJson = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.CardField);
                            //string.Format(templatesFolder + @"{0}\{1}", templateUsed, template.CardField);
                            if (File.Exists(cardFieldJson))
                            {
                                cardFieldJson = model.ReadJsonFile(cardFieldJson);
                                UpdateCardFields(model, cardFieldJson, _boardVersion, model.Id, boardType, model.ProjectName + " Team");
                            }
                        }
                        //Update card styles
                        if (template.CardStyle != null)
                        {
                            string cardStyleJson = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, template.CardStyle);
                            if (File.Exists(cardStyleJson))
                            {
                                cardStyleJson = model.ReadJsonFile(cardStyleJson);
                                UpdateCardStyles(model, cardStyleJson, _boardVersion, model.Id, boardType, model.ProjectName + " Team");
                            }
                        }
                        //Enable Epic Backlog
                        Results.Add("Board-Column, Swimlanes, Styles updated");
                    }
                }
            }

            //update sprint dates
            UpdateSprintItems(model, _boardVersion, settings);
            UpdateIterations(model, _boardVersion, "Iterations.json");
            RenameIterations(model, _boardVersion, settings.RenameIterations);
        }

        public void CreateV2Template(AppConfiguration _projectCreationVersion, AppConfiguration _boardVersion, ProjectSettings settings, Project model, ProjectTemplate template, string templateUsed)
        {
            string teamsJsonPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, "Teams\\Teams.json");
            // Path.Combine(templatesFolder + templateUsed, "Teams\\Teams.json");
            if (File.Exists(teamsJsonPath))
            {
                template.Teams = "Teams\\Teams.json";
                template.TeamArea = "TeamArea.json";
                CreateTeams(model, template.Teams, _projectCreationVersion, model.Id, template.TeamArea);
                string jsonTeams = model.ReadJsonFile(teamsJsonPath);
                JArray jTeams = JsonConvert.DeserializeObject<JArray>(jsonTeams);
                JContainer teamsParsed = JsonConvert.DeserializeObject<JContainer>(jsonTeams);
                foreach (var jteam in jTeams)
                {
                    string _teamName = string.Empty;
                    string isDefault = jteam["isDefault"] != null ? jteam["isDefault"].ToString() : string.Empty;
                    if (isDefault == "true")
                    {
                        _teamName = model.ProjectName + " Team";
                    }
                    else
                    {
                        _teamName = jteam["name"].ToString();
                    }
                    string teamFolderPath = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, templateUsed, @"Teams\" + jteam["name"].ToString());
                    // Path.Combine(templatesFolder + templateUsed, "Teams", jteam["name"].ToString());
                    if (Directory.Exists(teamFolderPath))
                    {
                        BoardColumn objBoard = new BoardColumn(_boardVersion);

                        // updating swimlanes for each teams each board(epic, feature, PBI, Stories) 
                        string updateSwimLanesJSON = "";
                        SwimLanes objSwimLanes = new SwimLanes(_boardVersion);
                        template.BoardRows = "BoardRows.json";
                        updateSwimLanesJSON = Path.Combine(teamFolderPath, template.BoardRows);
                        if (File.Exists(updateSwimLanesJSON))
                        {
                            updateSwimLanesJSON = File.ReadAllText(updateSwimLanesJSON);
                            List<ImportBoardRows.Rows> importRows = JsonConvert.DeserializeObject<List<ImportBoardRows.Rows>>(updateSwimLanesJSON);
                            foreach (var board in importRows)
                            {
                                bool isUpdated = objSwimLanes.UpdateSwimLanes(JsonConvert.SerializeObject(board.Value), model.ProjectName, board.BoardName, _teamName);
                            }
                        }

                        // updating team setting for each team
                        string teamSettingJson = "";
                        template.SetEpic = "TeamSetting.json";
                        teamSettingJson = Path.Combine(teamFolderPath, template.SetEpic);
                        if (File.Exists(teamSettingJson))
                        {
                            teamSettingJson = File.ReadAllText(teamSettingJson);
                            EnableEpic(model, teamSettingJson, _boardVersion, model.Id, _teamName);
                        }

                        // updating board columns for each teams each board
                        string teamBoardColumns = "";
                        template.BoardColumns = "BoardColumns.json";
                        teamBoardColumns = Path.Combine(teamFolderPath, template.BoardColumns);
                        if (File.Exists(teamBoardColumns))
                        {
                            teamBoardColumns = File.ReadAllText(teamBoardColumns);
                            List<ImportBoardColumns.ImportBoardCols> importBoardCols = JsonConvert.DeserializeObject<List<ImportBoardColumns.ImportBoardCols>>(teamBoardColumns);
                            foreach (var board in importBoardCols)
                            {
                                bool success = UpdateBoardColumn(model, JsonConvert.SerializeObject(board.Value, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), _boardVersion, model.Id, board.BoardName, _teamName);
                            }
                        }

                        // updating card fields for each team and each board
                        string teamCardFields = "";
                        template.CardField = "CardFields.json";
                        teamCardFields = Path.Combine(teamFolderPath, template.CardField);
                        if (File.Exists(teamCardFields))
                        {
                            teamCardFields = File.ReadAllText(teamCardFields);
                            List<ImportCardFields.CardFields> cardFields = new List<ImportCardFields.CardFields>();
                            cardFields = JsonConvert.DeserializeObject<List<ImportCardFields.CardFields>>(teamCardFields);
                            foreach (var card in cardFields)
                            {
                                UpdateCardFields(model, JsonConvert.SerializeObject(card, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), _boardVersion, model.Id, card.BoardName, _teamName);
                            }
                        }

                        // updating card styles for each team and each board
                        string teamCardStyle = "";
                        template.CardStyle = "CardStyles.json";
                        teamCardStyle = Path.Combine(teamFolderPath, template.CardStyle);
                        if (File.Exists(teamCardStyle))
                        {
                            teamCardStyle = File.ReadAllText(teamCardStyle);
                            List<CardStyle.Style> cardStyles = new List<CardStyle.Style>();
                            cardStyles = JsonConvert.DeserializeObject<List<CardStyle.Style>>(teamCardStyle);
                            foreach (var cardStyle in cardStyles)
                            {
                                if (cardStyle.Rules.Fill != null)
                                {
                                    UpdateCardStyles(model, JsonConvert.SerializeObject(cardStyle), _boardVersion, model.Id, cardStyle.BoardName, _teamName);
                                }
                            }
                        }
                    }
                    Results.Add("Board-Column, Swimlanes, Styles updated");
                }
                UpdateSprintItems(model, _boardVersion, settings);
                UpdateIterations(model, _boardVersion, "Iterations.json");
                RenameIterations(model, _boardVersion, settings.RenameIterations);
            }
        }

        /// <summary>
        /// Create Teams
        /// </summary>
        /// <param name="model"></param>
        /// <param name="teamsJSON"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="id"></param>
        /// <param name="teamAreaJSON"></param>
        private void CreateTeams(Project model, string teamsJSON, AppConfiguration _projectConfig, string id, string teamAreaJSON)
        {
            try
            {
                string jsonTeams = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, teamsJSON);
                if (File.Exists(jsonTeams))
                {
                    Teams objTeam = new Teams(_projectConfig);
                    jsonTeams = model.ReadJsonFile(jsonTeams);
                    JArray jTeams = JsonConvert.DeserializeObject<JArray>(jsonTeams);
                    JContainer teamsParsed = JsonConvert.DeserializeObject<JContainer>(jsonTeams);

                    //get Backlog Iteration Id
                    string backlogIteration = objTeam.GetTeamSetting(model.ProjectName);
                    //get all Iterations
                    TeamIterationsResponse.Iterations iterations = objTeam.GetAllIterations(model.ProjectName);

                    foreach (var jTeam in jTeams)
                    {
                        string isDefault = jTeam["isDefault"] != null ? jTeam["isDefault"].ToString() : string.Empty;
                        if (isDefault == "false" || isDefault == "")
                        {
                            GetTeamResponse.Team teamResponse = objTeam.CreateNewTeam(jTeam.ToString(), model.ProjectName);
                            if (!(string.IsNullOrEmpty(teamResponse.Id)))
                            {
                                string areaName = objTeam.CreateArea(model.ProjectName, teamResponse.Name);
                                string updateAreaJSON = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, teamAreaJSON);

                                //updateAreaJSON = string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, teamAreaJSON);


                                if (File.Exists(updateAreaJSON))
                                {
                                    updateAreaJSON = model.ReadJsonFile(updateAreaJSON);
                                    updateAreaJSON = updateAreaJSON.Replace("$ProjectName$", model.ProjectName).Replace("$AreaName$", areaName);
                                    bool isUpdated = objTeam.SetAreaForTeams(model.ProjectName, teamResponse.Name, updateAreaJSON);
                                }
                                bool isBackLogIterationUpdated = objTeam.SetBackLogIterationForTeam(backlogIteration, model.ProjectName, teamResponse.Name);
                                if (iterations.Count > 0)
                                {
                                    foreach (var iteration in iterations.Value)
                                    {
                                        bool isIterationUpdated = objTeam.SetIterationsForTeam(iteration.Id, teamResponse.Name, model.ProjectName);
                                    }
                                }
                            }
                        }
                        if (!(string.IsNullOrEmpty(objTeam.LastFailureMessage)))
                        {
                            Errors.Add("Error while creating teams: " + objTeam.LastFailureMessage + Environment.NewLine);
                        }
                        else
                        {
                            Results.Add(string.Format("{0} team(s) created", teamsParsed.Count));
                        }
                        if (model.SelectedTemplate.ToLower() == "smarthotel360")
                        {
                            string updateAreaJSON = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, "UpdateTeamArea.json");

                            //updateAreaJSON = string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, "UpdateTeamArea.json");
                            if (File.Exists(updateAreaJSON))
                            {
                                updateAreaJSON = model.ReadJsonFile(updateAreaJSON);
                                updateAreaJSON = updateAreaJSON.Replace("$ProjectName$", model.ProjectName);
                                bool isUpdated = objTeam.UpdateTeamsAreas(model.ProjectName, updateAreaJSON);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                Errors.Add("Error while creating teams: " + ex.Message);

            }
        }

        /// <summary>
        /// Udpate Card Styles
        /// </summary>
        /// <param name="model"></param>
        /// <param name="json"></param>
        /// <param name="_configuration"></param>
        /// <param name="id"></param>
        private void UpdateCardStyles(Project model, string json, AppConfiguration _configuration, string id, string boardType, string team)
        {
            try
            {
                Cards objCards = new Cards(_configuration);
                objCards.ApplyRules(model.ProjectName, json, boardType, team);

                if (!string.IsNullOrEmpty(objCards.LastFailureMessage))
                {
                    Errors.Add("Error while updating card styles: " + objCards.LastFailureMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                Errors.Add("Error while updating card styles: " + ex.Message);
            }

        }

        /// <summary>
        /// Enable Epic
        /// </summary>
        /// <param name="model"></param>
        /// <param name="json"></param>
        /// <param name="_config3_0"></param>
        /// <param name="id"></param>
        private void EnableEpic(Project model, string json, AppConfiguration _boardVersion, string id, string team)
        {
            try
            {
                Cards objCards = new Cards(_boardVersion);
                Projects project = new Projects(_boardVersion);
                objCards.EnablingEpic(model.ProjectName, json, model.ProjectName, team);

                if (!string.IsNullOrEmpty(objCards.LastFailureMessage))
                {
                    Errors.Add("Error while Setting Epic Settings: " + objCards.LastFailureMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                Errors.Add("Error while Setting Epic Settings: " + ex.Message);
            }

        }

        /// <summary>
        /// Update Board Columns styles
        /// </summary>
        /// <param name="model"></param>
        /// <param name="BoardColumnsJSON"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool UpdateBoardColumn(Project model, string BoardColumnsJSON, AppConfiguration _BoardConfig, string id, string BoardType, string team)
        {
            bool result = false;
            try
            {
                BoardColumn objBoard = new BoardColumn(_BoardConfig);
                bool boardColumnResult = objBoard.UpdateBoard(model.ProjectName, BoardColumnsJSON, BoardType, team);
                if (boardColumnResult)
                {
                    model.Environment.BoardRowFieldName = objBoard.RowFieldName;
                    result = true;
                }
                else if (!(string.IsNullOrEmpty(objBoard.LastFailureMessage)))
                {
                    Errors.Add("Error while updating board column " + objBoard.LastFailureMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                Errors.Add("Error while updating board column " + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Updates Card Fields
        /// </summary>
        /// <param name="model"></param>
        /// <param name="json"></param>
        /// <param name="_configuration"></param>
        /// <param name="id"></param>
        private void UpdateCardFields(Project model, string json, AppConfiguration _configuration, string id, string boardType, string team)
        {
            try
            {
                json = json.Replace("null", "\"\"");
                Cards objCards = new Cards(_configuration);
                objCards.UpdateCardField(model.ProjectName, json, boardType, team);

                if (!string.IsNullOrEmpty(objCards.LastFailureMessage))
                {
                    Errors.Add("Error while updating card fields: " + objCards.LastFailureMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                Errors.Add("Error while updating card fields: " + ex.Message);

            }

        }

        /// <summary>
        /// Udpate Sprints dates
        /// </summary>
        /// <param name="model"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="settings"></param>
        private void UpdateSprintItems(Project model, AppConfiguration _boardConfig, Models.ProjectSettings settings)
        {
            try
            {
                if (settings.Type.ToLower() == "scrum" || settings.Type.ToLower() == "agile" || settings.Type.ToLower() == "basic")
                {
                    ClassificationNodes objClassification = new ClassificationNodes(_boardConfig);
                    bool classificationNodesResult = objClassification.UpdateIterationDates(model.ProjectName, settings.Type);

                    if (!(string.IsNullOrEmpty(objClassification.LastFailureMessage)))
                    {
                        Errors.Add("Error while updating sprint items: " + objClassification.LastFailureMessage + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                Errors.Add("Error while updating sprint items: " + ex.Message);
            }
        }


        /// <summary>
        /// Update Iterations
        /// </summary>
        /// <param name="model"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="iterationsJSON"></param>
        private void UpdateIterations(Project model, AppConfiguration _boardConfig, string iterationsJSON)
        {
            try
            {
                string jsonIterations = ProjectHelper.GetJsonFilePath(model.IsPrivatePath, model.PrivateTemplatePath, model.SelectedTemplate, iterationsJSON);
                //string.Format(templatesFolder + @"{0}\{1}", model.SelectedTemplate, iterationsJSON);
                if (File.Exists(jsonIterations))
                {
                    iterationsJSON = model.ReadJsonFile(jsonIterations);
                    ClassificationNodes objClassification = new ClassificationNodes(_boardConfig);

                    GetNodesResponse.Nodes nodes = objClassification.GetIterations(model.ProjectName);

                    GetNodesResponse.Nodes projectNode = JsonConvert.DeserializeObject<GetNodesResponse.Nodes>(iterationsJSON);

                    if (projectNode.HasChildren)
                    {
                        foreach (var child in projectNode.Children)
                        {
                            CreateIterationNode(model, objClassification, child, nodes);
                        }
                    }

                    if (projectNode.HasChildren)
                    {
                        foreach (var child in projectNode.Children)
                        {
                            path = string.Empty;
                            MoveIterationNode(model, objClassification, child);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                Errors.Add("Error while updating iteration: " + ex.Message);
            }
        }


        /// <summary>
        /// Move Iterations to nodes
        /// </summary>
        /// <param name="model"></param>
        /// <param name="objClassification"></param>
        /// <param name="child"></param>
        private void MoveIterationNode(Project model, ClassificationNodes objClassification, GetNodesResponse.Child child)
        {
            if (child.HasChildren && child.Children != null)
            {
                foreach (var c in child.Children)
                {
                    path += child.Name + "\\";
                    var nd = objClassification.MoveIteration(model.ProjectName, path, c.Id);

                    if (c.HasChildren)
                    {
                        MoveIterationNode(model, objClassification, c);
                    }
                }
            }
        }


        /// <summary>
        /// Rename Iterations
        /// </summary>
        /// <param name="model"></param>
        /// <param name="_defaultConfiguration"></param>
        /// <param name="renameIterations"></param>
        public void RenameIterations(Project model, AppConfiguration _defaultConfiguration, Dictionary<string, string> renameIterations)
        {
            try
            {
                if (renameIterations != null && renameIterations.Count > 0)
                {
                    ClassificationNodes objClassification = new ClassificationNodes(_defaultConfiguration);
                    bool IsRenamed = objClassification.RenameIteration(model.ProjectName, renameIterations);
                }
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                Errors.Add("Error while renaming iterations: " + ex.Message);
            }
        }


        /// <summary>
        /// Create Iterations
        /// </summary>
        /// <param name="model"></param>
        /// <param name="objClassification"></param>
        /// <param name="child"></param>
        /// <param name="currentIterations"></param>
        private void CreateIterationNode(Project model, ClassificationNodes objClassification, GetNodesResponse.Child child, GetNodesResponse.Nodes currentIterations)
        {
            string[] defaultSprints = new string[] { "Sprint 1", "Sprint 2", "Sprint 3", "Sprint 4", "Sprint 5", "Sprint 6", };
            if (defaultSprints.Contains(child.Name))
            {
                var nd = (currentIterations.HasChildren) ? currentIterations.Children.FirstOrDefault(i => i.Name == child.Name) : null;
                if (nd != null)
                {
                    child.Id = nd.Id;
                }
            }
            else
            {
                var node = objClassification.CreateIteration(model.ProjectName, child.Name);
                child.Id = node.Id;
            }

            if (child.HasChildren && child.Children != null)
            {
                foreach (var c in child.Children)
                {
                    CreateIterationNode(model, objClassification, c, currentIterations);
                }
            }
        }
    }
}
