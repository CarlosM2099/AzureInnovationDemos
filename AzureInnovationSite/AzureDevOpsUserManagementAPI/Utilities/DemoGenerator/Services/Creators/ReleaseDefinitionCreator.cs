using AzureDevOpsAPI;
using AzureDevOpsAPI.Release;
using AzureDevOpsAPI.Viewmodel.Extractor;
using AzureDevOpsAPI.Viewmodel.ProjectAndTeams;
using AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Models;
using System;
using System.IO;
using System.Linq;

namespace AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Creators
{
    public class ReleaseDefinitionCreator : BaseCreator
    {
        public ReleaseDefinitionCreator(ExtractorProjectSettings setting) : base(setting)
        {

        }

        /// <summary>
        /// Create Release Definitions
        /// </summary>
        /// <param name="model"></param>
        /// <param name="_releaseConfiguration"></param>
        /// <param name="_config3_0"></param>
        /// <param name="id"></param>
        /// <param name="teamMembers"></param>
        /// <returns></returns>
        public bool CreateReleaseDefinition(Project model, AppConfiguration _releaseConfiguration, string id, TeamMemberResponse.TeamMembers teamMembers)
        {
            try
            {
                var teamMember = teamMembers.Value.FirstOrDefault();
                foreach (ReleaseDef relDef in model.ReleaseDefinitions)
                {
                    if (File.Exists(relDef.FilePath))
                    {
                        ReleaseDefinition objRelease = new ReleaseDefinition(_releaseConfiguration);
                        string jsonReleaseDefinition = model.ReadJsonFile(relDef.FilePath);
                        jsonReleaseDefinition = jsonReleaseDefinition.Replace("$ProjectName$", model.Environment.ProjectName)
                                             .Replace("$ProjectId$", model.Environment.ProjectId)
                                             .Replace("$OwnerUniqueName$", teamMember.Identity.UniqueName)
                                             .Replace("$OwnerId$", teamMember.Identity.Id)
                                  .Replace("$OwnerDisplayName$", teamMember.Identity.DisplayName);

                        if (model.Environment.VariableGroups.Count > 0)
                        {
                            foreach (var vGroupsId in model.Environment.VariableGroups)
                            {
                                string placeHolder = string.Format("${0}$", vGroupsId.Value);
                                jsonReleaseDefinition = jsonReleaseDefinition.Replace(placeHolder, vGroupsId.Key.ToString());
                            }
                        }
                        //Adding randon UUID to website name
                        string uuid = Guid.NewGuid().ToString();
                        uuid = uuid.Substring(0, 8);
                        jsonReleaseDefinition = jsonReleaseDefinition.Replace("$UUID$", uuid).Replace("$RandomNumber$", uuid).Replace("$AccountName$", model.AccountName);

                        //update agent queue ids
                        foreach (string queue in model.Environment.AgentQueues.Keys)
                        {
                            string placeHolder = string.Format("${0}$", queue);
                            jsonReleaseDefinition = jsonReleaseDefinition.Replace(placeHolder, model.Environment.AgentQueues[queue].ToString());
                        }

                        //update endpoint ids
                        foreach (string endpoint in model.Environment.ServiceEndpoints.Keys)
                        {
                            string placeHolder = string.Format("${0}$", endpoint);
                            jsonReleaseDefinition = jsonReleaseDefinition.Replace(placeHolder, model.Environment.ServiceEndpoints[endpoint]);
                        }

                        foreach (BuildDef objBuildDef in model.BuildDefinitions)
                        {
                            //update build ids
                            string placeHolder = string.Format("${0}-id$", objBuildDef.Name);
                            jsonReleaseDefinition = jsonReleaseDefinition.Replace(placeHolder, objBuildDef.Id);
                        }
                        var releaseDef = objRelease.CreateReleaseDefinition(jsonReleaseDefinition, model.ProjectName);
                        if (!(string.IsNullOrEmpty(objRelease.LastFailureMessage)))
                        {
                            if (objRelease.LastFailureMessage.TrimEnd() == "Tasks with versions 'ARM Outputs:3.*' are not valid for deploy job 'Function' in stage Azure-Dev.")
                            {
                                jsonReleaseDefinition = jsonReleaseDefinition.Replace("3.*", "4.*");
                                releaseDef = objRelease.CreateReleaseDefinition(jsonReleaseDefinition, model.ProjectName);
                                if (relDef.Id != "error")
                                {
                                    relDef.Id = releaseDef.releaseDefId;
                                    relDef.Name = releaseDef.releaseDefName;
                                }
                                if (!string.IsNullOrEmpty(relDef.Name))
                                {
                                    objRelease.LastFailureMessage = string.Empty;
                                }
                            }
                        }
                        relDef.Id = releaseDef.releaseDefId;
                        relDef.Name = releaseDef.releaseDefName;

                        if (!(string.IsNullOrEmpty(objRelease.LastFailureMessage)))
                        {
                            Errors.Add("Error while creating release definition: " + objRelease.LastFailureMessage + Environment.NewLine);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                Errors.Add("Error while creating release definition: " + ex.Message);
            }

            return false;
        }
    }
}
