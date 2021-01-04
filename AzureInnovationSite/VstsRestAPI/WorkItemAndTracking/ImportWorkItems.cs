using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using VstsRestAPI.Viewmodel.WorkItem;

namespace VstsRestAPI.WorkItemAndTracking
{
    public class ImportWorkItems : ApiServiceBase
    {
        public string BoardRowFieldName;
        private List<WiMapData> _wiData = new List<WiMapData>();
        private List<string> _listAssignToUsers = new List<string>();
        private string[] _relTypes = { "Microsoft.VSTS.Common.TestedBy-Reverse", "System.LinkTypes.Hierarchy-Forward", "System.LinkTypes.Related", "System.LinkTypes.Dependency-Reverse", "System.LinkTypes.Dependency-Forward" };
        private string _attachmentFolder = string.Empty;
        private string _repositoryId = string.Empty;
        private string _projectId = string.Empty;
        private Dictionary<string, string> _pullRequests = new Dictionary<string, string>();
        private readonly ILog logger = LogManager.GetLogger("ErrorLog", "ErrorLog");
        public ImportWorkItems(IConfiguration configuration, string rowFieldName) : base(configuration)
        {
            BoardRowFieldName = rowFieldName;
        }

        /// <summary>
        /// Import Work items form the files
        /// </summary>
        /// <param name="dicWiTypes"></param>
        /// <param name="projectName"></param>
        /// <param name="uniqueUser"></param>
        /// <param name="projectSettingsJson"></param>
        /// <param name="attachmentFolderPath"></param>
        /// <param name="repositoryId"></param>
        /// <param name="projectId"></param>
        /// <param name="dictPullRequests"></param>
        /// <param name="userMethod"></param>
        /// <param name="accountUsers"></param>
        /// <returns></returns>

        public List<WiMapData> ImportWorkitems(Dictionary<string, string> dicWiTypes, string projectName, string uniqueUser, string projectSettingsJson, string attachmentFolderPath, string repositoryId, string projectId, Dictionary<string, string> dictPullRequests, string userMethod, List<string> accountUsers, string selectedTemplate)
        {
            try
            {
                _attachmentFolder = attachmentFolderPath;
                _repositoryId = repositoryId;
                _projectId = projectId;
                _pullRequests = dictPullRequests;
                JArray userList = new JArray();
                JToken userAssignment = null;
                if (userMethod == "Select")
                {
                    foreach (string user in accountUsers)
                    {
                        _listAssignToUsers.Add(user);
                    }
                }
                else
                {
                    var jitems = JObject.Parse(projectSettingsJson);
                    userList = jitems["users"].Value<JArray>();
                    userAssignment = jitems["userAssignment"];
                    if (userList.Count > 0)
                    {
                        _listAssignToUsers.Add(uniqueUser);
                    }
                    else
                    {
                        _listAssignToUsers.Add(uniqueUser);
                    }
                    foreach (var data in userList.Values())
                    {
                        _listAssignToUsers.Add(data.ToString());
                    }
                }

                foreach (string wiType in dicWiTypes.Keys)
                {
                    PrepareAndUpdateTarget(wiType, dicWiTypes[wiType], projectName, selectedTemplate, userAssignment == null ? "" : userAssignment.ToString());
                }

                foreach (string wiType in dicWiTypes.Keys)
                {
                    UpdateWorkItemLinks(dicWiTypes[wiType]);
                }

                return _wiData;
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                return _wiData;
            }

        }

        /// <summary>
        /// Update the work items in Target with all required field values
        /// </summary>
        /// <param name="workItemType"></param>
        /// <param name="workImport"></param>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public bool PrepareAndUpdateTarget(string workItemType, string workImport, string projectName, string selectedTemplate, string userAssignment)
        {
            try
            {
                workImport = workImport.Replace("$ProjectName$", projectName);
                ImportWorkItemModel.WorkItems fetchedWIs = JsonConvert.DeserializeObject<ImportWorkItemModel.WorkItems>(workImport);

                if (fetchedWIs.Count > 0)
                {
                    if (workItemType.ToLower() == "epic" || workItemType.ToLower() == "feature")
                    {
                        fetchedWIs.Value = fetchedWIs.Value.OrderBy(x => x.Id).ToArray();
                    }
                    foreach (ImportWorkItemModel.Value newWi in fetchedWIs.Value)
                    {
                        newWi.Fields.SystemCreatedDate = DateTime.Now.AddDays(-3);
                        Dictionary<string, object> dicWiFields = new Dictionary<string, object>();
                        string assignToUser = string.Empty;
                        if (_listAssignToUsers.Count > 0)
                        {
                            assignToUser = _listAssignToUsers[new Random().Next(0, _listAssignToUsers.Count)] ?? string.Empty;
                        }

                        //Test cases have different fields compared to other items like bug, Epics, etc.                     
                        if ((workItemType == "Test Case"))
                        {
                            //replacing null values with Empty strngs; creation fails if the fields are null
                            if (newWi.Fields.MicrosoftVststcmParameters == null)
                            {
                                newWi.Fields.MicrosoftVststcmParameters = string.Empty;
                            }

                            if (newWi.Fields.MicrosoftVststcmSteps == null)
                            {
                                newWi.Fields.MicrosoftVststcmSteps = string.Empty;
                            }

                            if (newWi.Fields.MicrosoftVststcmLocalDataSource == null)
                            {
                                newWi.Fields.MicrosoftVststcmLocalDataSource = string.Empty;
                            }

                            dicWiFields.Add("/fields/System.Title", newWi.Fields.SystemTitle);
                            dicWiFields.Add("/fields/System.State", newWi.Fields.SystemState);
                            dicWiFields.Add("/fields/System.Reason", newWi.Fields.SystemReason);
                            dicWiFields.Add("/fields/Microsoft.VSTS.Common.Priority", newWi.Fields.MicrosoftVstsCommonPriority);
                            dicWiFields.Add("/fields/Microsoft.VSTS.TCM.Steps", newWi.Fields.MicrosoftVststcmSteps);
                            dicWiFields.Add("/fields/Microsoft.VSTS.TCM.Parameters", newWi.Fields.MicrosoftVststcmParameters);
                            dicWiFields.Add("/fields/Microsoft.VSTS.TCM.LocalDataSource", newWi.Fields.MicrosoftVststcmLocalDataSource);
                            dicWiFields.Add("/fields/Microsoft.VSTS.TCM.AutomationStatus", newWi.Fields.MicrosoftVststcmAutomationStatus);

                            if (newWi.Fields.MicrosoftVstsCommonAcceptanceCriteria != null)
                            {
                                dicWiFields.Add("/fields/Microsoft.VSTS.Common.AcceptanceCriteria", newWi.Fields.MicrosoftVstsCommonAcceptanceCriteria);
                            }

                            if (newWi.Fields.SystemTags != null)
                            {
                                dicWiFields.Add("/fields/System.Tags", newWi.Fields.SystemTags);
                            }

                            dicWiFields.Add("/fields/Microsoft.VSTS.Scheduling.RemainingWork", newWi.Fields.MicrosoftVstsSchedulingRemainingWork);

                        }
                        else
                        {
                            string iterationPath = projectName;
                            string boardRowField = string.Empty;

                            if (newWi.Fields.SystemIterationPath.Contains("\\"))
                            {
                                iterationPath = string.Format(@"{0}\{1}", projectName, newWi.Fields.SystemIterationPath.Split('\\')[1]);

                            }

                            if (!string.IsNullOrWhiteSpace(BoardRowFieldName))
                            {
                                boardRowField = string.Format("/fields/{0}", BoardRowFieldName);
                            }

                            if (newWi.Fields.SystemDescription == null)
                            {
                                newWi.Fields.SystemDescription = newWi.Fields.SystemTitle;
                            }

                            if (string.IsNullOrEmpty(newWi.Fields.SystemBoardLane))
                            {
                                newWi.Fields.SystemBoardLane = string.Empty;
                            }

                            dicWiFields.Add("/fields/System.Title", newWi.Fields.SystemTitle);
                            if (userAssignment.ToLower() != "any")
                            {
                                if (newWi.Fields.SystemState == "Done")
                                {
                                    dicWiFields.Add("/fields/System.AssignedTo", assignToUser);
                                }
                            }
                            else
                            {
                                dicWiFields.Add("/fields/System.AssignedTo", assignToUser);
                            }
                            //string areaPath = newWI.fields.SystemAreaPath ?? projectName;
                            //dicWIFields.Add("/fields/System.AreaPath", areaPath);
                            dicWiFields.Add("/fields/System.Description", newWi.Fields.SystemDescription);
                            dicWiFields.Add("/fields/System.State", newWi.Fields.SystemState);
                            dicWiFields.Add("/fields/System.Reason", newWi.Fields.SystemReason);
                            dicWiFields.Add("/fields/Microsoft.VSTS.Common.Priority", newWi.Fields.MicrosoftVstsCommonPriority);
                            dicWiFields.Add("/fields/System.IterationPath", iterationPath);
                            dicWiFields.Add("/fields/Microsoft.VSTS.Scheduling.RemainingWork", newWi.Fields.MicrosoftVstsSchedulingRemainingWork);
                            dicWiFields.Add("/fields/Microsoft.VSTS.Scheduling.Effort", newWi.Fields.MicrosoftVstsSchedulingEffort);

                            if (newWi.Fields.MicrosoftVstsCommonAcceptanceCriteria != null)
                            {
                                dicWiFields.Add("/fields/Microsoft.VSTS.Common.AcceptanceCriteria", newWi.Fields.MicrosoftVstsCommonAcceptanceCriteria);
                            }

                            if (newWi.Fields.SystemTags != null)
                            {
                                dicWiFields.Add("/fields/System.Tags", newWi.Fields.SystemTags);
                            }

                            if (newWi.Fields.MicrosoftVststcmParameters != null)
                            {
                                dicWiFields.Add("/fields/Microsoft.VSTS.TCM.Parameters", newWi.Fields.MicrosoftVststcmParameters);
                            }

                            if (newWi.Fields.MicrosoftVststcmSteps != null)
                            {
                                dicWiFields.Add("/fields/Microsoft.VSTS.TCM.Steps", newWi.Fields.MicrosoftVststcmSteps);
                            }

                            if (!string.IsNullOrWhiteSpace(boardRowField))
                            {
                                dicWiFields.Add(boardRowField, newWi.Fields.SystemBoardLane);
                            }
                        }
                        UpdateWorkIteminTarget(workItemType, newWi.Id.ToString(), projectName, dicWiFields);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return false;
        }

        /// <summary>
        /// Update work ietm with all required field values
        /// </summary>
        /// <param name="workItemType"></param>
        /// <param name="old_wi_ID"></param>
        /// <param name="projectName"></param>
        /// <param name="dictionaryWiFields"></param>
        /// <returns></returns>
        public bool UpdateWorkIteminTarget(string workItemType, string oldWiId, string projectName, Dictionary<string, object> dictionaryWiFields)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    List<WorkItemPatch.Field> listFields = new List<WorkItemPatch.Field>();
                    WorkItemPatchResponse.WorkItem viewModel = new WorkItemPatchResponse.WorkItem();
                    // change some values on a few fields
                    foreach (string key in dictionaryWiFields.Keys)
                    {
                        listFields.Add(new WorkItemPatch.Field() { Op = "add", Path = key, Value = dictionaryWiFields[key] });
                    }
                    WorkItemPatch.Field[] fields = listFields.ToArray();
                    using (var client = GetHttpClient())
                    {
                        var postValue = new StringContent(JsonConvert.SerializeObject(fields), Encoding.UTF8, "application/json-patch+json"); // mediaType needs to be application/json-patch+json for a patch call
                                                                                                                                              // set the httpmethod to Patch
                        var method = new HttpMethod("PATCH");

                        // send the request               
                        var request = new HttpRequestMessage(method, projectName + "/_apis/wit/workitems/$" + workItemType + "?bypassRules=true&api-version=" + Configuration.VersionNumber) { Content = postValue };
                        var response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            viewModel = response.Content.ReadAsAsync<WorkItemPatchResponse.WorkItem>().Result;
                            _wiData.Add(new WiMapData() { OldId = oldWiId, NewId = viewModel.Id.ToString(), WiType = workItemType });
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            this.LastFailureMessage = error;
                        }

                        return response.IsSuccessStatusCode;
                    }
                }
                catch (OperationCanceledException opr)
                {
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t OperationCanceledException: " + opr.Message + "\t" + "\n" + opr.StackTrace + "\n");
                    LastFailureMessage = opr.Message + " ," + opr.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return false;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
                catch (Exception ex)
                {
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return false;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return false;
        }
        /// <summary>
        /// Update work items links - parent child- Hyperlinks-artifact links-attachments
        /// </summary>
        /// <param name="workItemTemplateJson"></param>
        /// <returns></returns>
        public bool UpdateWorkItemLinks(string workItemTemplateJson)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    ImportWorkItemModel.WorkItems fetchedPbIs = JsonConvert.DeserializeObject<ImportWorkItemModel.WorkItems>(workItemTemplateJson);
                    string wiToUpdate = "";
                    WiMapData findIDforUpdate;
                    if (fetchedPbIs.Count > 0)
                    {

                        foreach (ImportWorkItemModel.Value newWi in fetchedPbIs.Value)
                        {
                            //continue next iteration if there is no relation
                            if (newWi.Relations == null)
                            {
                                continue;
                            }

                            int relCount = newWi.Relations.Length;
                            string oldWiid = newWi.Id.ToString();

                            findIDforUpdate = _wiData.Find(t => t.OldId == oldWiid);
                            if (findIDforUpdate != null)
                            {
                                wiToUpdate = findIDforUpdate.NewId;
                                foreach (ImportWorkItemModel.Relations rel in newWi.Relations)
                                {
                                    if (_relTypes.Contains(rel.Rel.Trim()))
                                    {
                                        oldWiid = rel.Url.Substring(rel.Url.LastIndexOf("/") + 1);
                                        WiMapData findIDforlink = _wiData.Find(t => t.OldId == oldWiid);

                                        if (findIDforlink != null)
                                        {
                                            string newWiid = findIDforlink.NewId;
                                            Object[] patchWorkItem = new Object[1];
                                            // change some values on a few fields
                                            patchWorkItem[0] = new
                                            {
                                                op = "add",
                                                path = "/relations/-",
                                                value = new
                                                {
                                                    rel = rel.Rel,
                                                    url = Configuration.UriString + "/_apis/wit/workitems/" + newWiid,
                                                    attributes = new
                                                    {
                                                        comment = "Making a new link for the dependency"
                                                    }
                                                }
                                            };
                                            if (UpdateLink("Product Backlog Item", wiToUpdate, patchWorkItem))
                                            {
                                            }
                                        }
                                    }
                                    if (rel.Rel == "Hyperlink")
                                    {
                                        Object[] patchWorkItem = new Object[1];
                                        patchWorkItem[0] = new
                                        {
                                            op = "add",
                                            path = "/relations/-",
                                            value = new
                                            {
                                                rel = "Hyperlink",
                                                url = rel.Url
                                            }
                                        };
                                        bool isHyperLinkCreated = UpdateLink(string.Empty, wiToUpdate, patchWorkItem);
                                    }
                                    if (rel.Rel == "AttachedFile")
                                    {
                                        Object[] patchWorkItem = new Object[1];
                                        string filPath = string.Format(_attachmentFolder + @"\{0}{1}", rel.Attributes["id"], rel.Attributes["name"]);
                                        string fileName = rel.Attributes["name"];
                                        string attchmentURl = UploadAttchment(filPath, fileName);
                                        if (!string.IsNullOrEmpty(attchmentURl))
                                        {
                                            patchWorkItem[0] = new
                                            {
                                                op = "add",
                                                path = "/relations/-",
                                                value = new
                                                {
                                                    rel = "AttachedFile",
                                                    url = attchmentURl
                                                }
                                            };
                                            bool isAttachmemntCreated = UpdateLink(string.Empty, wiToUpdate, patchWorkItem);
                                        }
                                    }
                                    if (rel.Rel == "ArtifactLink")
                                    {
                                        rel.Url = rel.Url.Replace("$projectId$", _projectId).Replace("$RepositoryId$", _repositoryId);
                                        foreach (var pullReqest in _pullRequests)
                                        {
                                            string key = string.Format("${0}$", pullReqest.Key);
                                            rel.Url = rel.Url.Replace(key, pullReqest.Value);
                                        }
                                        Object[] patchWorkItem = new Object[1];
                                        patchWorkItem[0] = new
                                        {
                                            op = "add",
                                            path = "/relations/-",
                                            value = new
                                            {
                                                rel = "ArtifactLink",
                                                url = rel.Url,
                                                attributes = new
                                                {
                                                    name = rel.Attributes["name"]
                                                }
                                            }

                                        };
                                        bool isArtifactLinkCreated = UpdateLink(string.Empty, wiToUpdate, patchWorkItem);
                                    }
                                }
                            }
                        }
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return false;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return false;
        }
        /// <summary>
        /// Udpate Links to work items
        /// </summary>
        /// <param name="workItemType"></param>
        /// <param name="witoUpdate"></param>
        /// <param name="patchWorkItem"></param>
        /// <returns></returns>
        public bool UpdateLink(string workItemType, string witoUpdate, object[] patchWorkItem)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    using (var client = GetHttpClient())
                    {
                        // serialize the fields array into a json string          
                        var patchValue = new StringContent(JsonConvert.SerializeObject(patchWorkItem), Encoding.UTF8, "application/json-patch+json"); // mediaType needs to be application/json-patch+json for a patch call

                        var method = new HttpMethod("PATCH");
                        var request = new HttpRequestMessage(method, Project + "/_apis/wit/workitems/" + witoUpdate + "?bypassRules=true&api-version=" + Configuration.VersionNumber) { Content = patchValue };
                        var response = client.SendAsync(request).Result;                       

                        return response.IsSuccessStatusCode;
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return false;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return false;
        }
        /// <summary>
        /// Upload attachments to VSTS server
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string UploadAttchment(string filePath, string fileName)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        //read file bytes and put into byte array        
                        Byte[] bytes = File.ReadAllBytes(filePath);

                        using (var client = GetHttpClient())
                        {
                            ByteArrayContent content = new ByteArrayContent(bytes);
                            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                            HttpResponseMessage uploadResponse = client.PostAsync("_apis/wit/attachments?fileName=" + fileName + "&api-version=" + Configuration.VersionNumber, content).Result;

                            if (uploadResponse.IsSuccessStatusCode)
                            {
                                //get the result, we need this to get the url of the attachment
                                string attachmentUrl = JObject.Parse(uploadResponse.Content.ReadAsStringAsync().Result)["url"].ToString();
                                return attachmentUrl;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return string.Empty;
                    }

                    Thread.Sleep(retryCount * 1000);

                }
            }
            return string.Empty;
        }
    }

    public class WiMapData
    {
        public string OldId { get; set; }
        public string NewId { get; set; }
        public string WiType { get; set; }
    }
}
