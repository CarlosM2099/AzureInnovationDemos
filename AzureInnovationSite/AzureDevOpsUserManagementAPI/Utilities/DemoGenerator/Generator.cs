using AzureDevOpsUserManagementAPI.Helpers;
using AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services;
using AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Interface;
using AzureDevOpsUserManagementAPI.Utilities.DemoGenerator.Services.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AzureDevOpsAPI.Viewmodel.ProjectAndTeams;
using AzureDevOpsAPI.WorkItemAndTracking;

namespace AzureDevOpsUserManagementAPI.Utilities.DemoGenerator
{
    public class Generator
    {
        private int userCount = 0;
        private IProjectService projectService;
        private ITemplateService templateService;
        public delegate (string, string, string) ProcessEnvironment(Project model);
        private readonly DemoGenSettings demoGenSettings;
        private (string projectId, string accountName, string templateUsed) projectResult;

        public Generator(DemoGenSettings demoGenSettings)
        {
            templateService = new TemplateService(demoGenSettings);
            projectService = new ProjectService(demoGenSettings);
            this.demoGenSettings = demoGenSettings;
        }

        public async Task<ProjectResponse> Create(MultiProjects model)
        {
            ProjectResponse returnObj = new ProjectResponse();
            returnObj.TemplatePath = model.TemplatePath;
            returnObj.TemplateName = model.TemplateName;
            string PrivateTemplatePath = string.Empty;
            string extractedTemplate = string.Empty;
            List<RequestedProject> returnProjects = new List<RequestedProject>();

            try
            {
                string ReadErrorMessages = File.ReadAllText(string.Format(Path.GetFullPath(".") + @"\Utilities\DemoGenerator\JSON\" + @"{0}", "ErrorMessages.json"));
                var Messages = JsonConvert.DeserializeObject<Messages>(ReadErrorMessages);
                var errormessages = Messages.ErrorMessages;
                List<string> ListOfExistedProjects = new List<string>();

                //check for Organization Name
                if (string.IsNullOrEmpty(model.OrganizationName))
                {
                    throw new ApplicationException(errormessages.AccountMessages.InvalidAccountName); //"Provide a valid Account name"
                }

                //Check for AccessToken
                if (string.IsNullOrEmpty(model.AccessToken))
                {
                    throw new ApplicationException(errormessages.AccountMessages.InvalidAccessToken); //"Token of type Basic must be provided"
                }

                else
                {
                    HttpResponseMessage response = projectService.GetprojectList(model.OrganizationName, model.AccessToken);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new ApplicationException(errormessages.AccountMessages.CheckaccountDetails);
                    }

                    else
                    {
                        var projectResult = response.Content.ReadAsAsync<ProjectsResponse.ProjectResult>().Result;
                        foreach (var project in projectResult.Value)
                        {
                            ListOfExistedProjects.Add(project.Name); // insert list of existing projects in selected organiszation to dummy list
                        }
                    }
                }

                if (model.Users.Count > 0)
                {
                    List<string> ListOfRequestedProjectNames = new List<string>();
                    foreach (var project in model.Users)
                    {
                        //check for Email and Validate project name
                        if (!string.IsNullOrEmpty(project.Email) && !string.IsNullOrEmpty(project.ProjectName))
                        {
                            string pattern = @"^(?!_)(?![.])[a-zA-Z0-9!^\-`)(]*[a-zA-Z0-9_!^\.)( ]*[^.\/\\~@#$*%+=[\]{\}'"",:;?<>|](?:[a-zA-Z!)(][a-zA-Z0-9!^\-` )(]+)?$";
                            bool isProjectNameValid = Regex.IsMatch(project.ProjectName, pattern);
                            List<string> restrictedNames = new List<string>() { "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM10", "PRN", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LTP", "LTP8", "LTP9", "NUL", "CON", "AUX", "SERVER", "SignalR", "DefaultCollection", "Web", "App_code", "App_Browesers", "App_Data", "App_GlobalResources", "App_LocalResources", "App_Themes", "App_WebResources", "bin", "web.config" };

                            if (!isProjectNameValid)
                            {
                                project.Status = errormessages.ProjectMessages.InvalidProjectName; //"Invalid Project name";
                                throw new ApplicationException(project.Status);
                            }

                            else if (restrictedNames.ConvertAll(d => d.ToLower()).Contains(project.ProjectName.Trim().ToLower()))
                            {

                                project.Status = errormessages.ProjectMessages.ProjectNameWithReservedKeyword;//"Project name must not be a system-reserved name such as PRN, COM1, COM2, COM3, COM4, COM5, COM6, COM7, COM8, COM9, COM10, LPT1, LPT2, LPT3, LPT4, LPT5, LPT6, LPT7, LPT8, LPT9, NUL, CON, AUX, SERVER, SignalR, DefaultCollection, or Web";

                                throw new ApplicationException(project.Status);

                            }
                            ListOfRequestedProjectNames.Add(project.ProjectName.ToLower());
                        }
                        else
                        {
                            project.Status = errormessages.ProjectMessages.ProjectNameOrEmailID;//"EmailId or ProjectName is not found";
                            throw new ApplicationException(project.Status);
                        }
                    }

                    //check for duplicatte project names from request body
                    bool anyDuplicateProjects = ListOfRequestedProjectNames.GroupBy(n => n).Any(c => c.Count() > 1);

                    if (anyDuplicateProjects)
                    {
                        throw new ApplicationException(errormessages.ProjectMessages.DuplicateProject); //"ProjectName must be unique"
                    }
                    else
                    {
                        string templateName = string.Empty;
                        bool isPrivate = false;

                        if (string.IsNullOrEmpty(model.TemplateName) && string.IsNullOrEmpty(model.TemplatePath))
                        {
                            throw new ApplicationException(errormessages.TemplateMessages.TemplateNameOrTemplatePath); //"Please provide templateName or templatePath(GitHub)"
                        }
                        else
                        {
                            //check for Private template path provided in request body
                            if (!string.IsNullOrEmpty(model.TemplatePath))
                            {
                                string fileName = Path.GetFileName(model.TemplatePath);
                                string extension = Path.GetExtension(model.TemplatePath);

                                if (extension.ToLower() == ".zip")
                                {
                                    extractedTemplate = fileName.ToLower().Replace(".zip", "").Trim() + "-" + Guid.NewGuid().ToString().Substring(0, 6) + extension.ToLower();
                                    templateName = extractedTemplate;
                                    model.TemplateName = extractedTemplate.ToLower().Replace(".zip", "").Trim();

                                    //Get template  by extarcted the template from TemplatePath and returning boolean value for Valid template
                                    PrivateTemplatePath = templateService.GetTemplateFromPath(model.TemplatePath, extractedTemplate, model.GitHubToken, model.UserId, model.Password);
                                    if (string.IsNullOrEmpty(PrivateTemplatePath))
                                    {
                                        throw new ApplicationException($"{errormessages.TemplateMessages.FailedTemplate} : {model.TemplatePath}");//"Failed to load the template from given template path. Check the repository URL and the file name.  If the repository is private then make sure that you have provided a GitHub token(PAT) in the request body"
                                    }

                                    else
                                    {
                                        string privateErrorMessage = templateService.checkSelectedTemplateIsPrivate(PrivateTemplatePath);
                                        if (privateErrorMessage != "SUCCESS")
                                        {
                                            var templatepath = Path.GetFullPath(".") + @"\Utilities\DemoGenerator\Templates\" + model.TemplateName;
                                            if (Directory.Exists(templatepath))
                                                Directory.Delete(templatepath, true);

                                            throw new ApplicationException(privateErrorMessage);//"TemplatePath should have .zip extension file name at the end of the url"
                                        }
                                        else
                                        {
                                            isPrivate = true;
                                        }
                                    }
                                }

                                else
                                {
                                    throw new ApplicationException(errormessages.TemplateMessages.PrivateTemplateFileExtension);//"TemplatePath should have .zip extension file name at the end of the url"
                                }
                            }

                            else
                            {
                                string response = templateService.GetTemplate(model.TemplateName);
                                if (response == "Template Not Found!")
                                {
                                    throw new ApplicationException(errormessages.TemplateMessages.TemplateNotFound);
                                }

                                templateName = model.TemplateName;
                            }
                        }

                        //check for Extension file from selected template(public or private template)
                        string extensionJsonFile = ProjectHelper.GetJsonFilePath(isPrivate, PrivateTemplatePath, templateName, "Extensions.json");//string.Format(templatesFolder + @"{ 0}\Extensions.json", selectedTemplate);

                        if (File.Exists(extensionJsonFile))
                        {
                            //check for Extension installed or not from selected template in selected organization
                            if (projectService.CheckForInstalledExtensions(extensionJsonFile, model.AccessToken, model.OrganizationName))
                            {
                                if (model.InstallExtensions)
                                {
                                    Project pmodel = new Project();
                                    pmodel.SelectedTemplate = model.TemplateName;
                                    pmodel.AccessToken = model.AccessToken;
                                    pmodel.AccountName = model.OrganizationName;

                                    bool isextensionInstalled = projectService.InstallExtensions(pmodel, model.OrganizationName, model.AccessToken);
                                }

                                else
                                {
                                    throw new ApplicationException(errormessages.ProjectMessages.ExtensionNotInstalled); //"Extension is not installed for the selected Template, Please provide IsExtensionRequired: true in the request body"
                                }
                            }
                        }

                        // continue to create project with async delegate method
                        foreach (var project in model.Users)
                        {
                            var result = ListOfExistedProjects.ConvertAll(d => d.ToLower()).Contains(project.ProjectName.ToLower());

                            if (result == true)
                            {
                                project.Status = project.ProjectName + " is already exist";
                            }
                            else
                            {
                                userCount++;
                                project.TrackId = Guid.NewGuid().ToString().Split('-')[0];
                                project.Status = "Project creation is initiated..";

                                Project pmodel = new Project();
                                pmodel.SelectedTemplate = model.TemplateName;
                                pmodel.AccessToken = model.AccessToken;
                                pmodel.AccountName = model.OrganizationName;
                                pmodel.ProjectName = project.ProjectName;
                                pmodel.Email = project.Email;
                                pmodel.Id = project.TrackId;
                                pmodel.IsApi = true;

                                if (model.TemplatePath != "")
                                {
                                    pmodel.PrivateTemplatePath = PrivateTemplatePath;
                                    pmodel.PrivateTemplateName = model.TemplateName;
                                    pmodel.IsPrivatePath = true;
                                }

                                ProcessEnvironment processDelegate = new ProcessEnvironment(projectService.CreateProjectEnvironment);

                                var processTask = Task.Run(() =>
                                {
                                    projectResult = processDelegate.Invoke(pmodel);
                                });
                                var endSetupTask = processTask.ContinueWith(EndEnvironmentSetupProcess);

                                await processTask;
                                await endSetupTask;
                            }

                            returnProjects.Add(project);
                        }

                        if (!string.IsNullOrEmpty(model.TemplatePath) && userCount == 0 && string.IsNullOrEmpty(extractedTemplate))
                        {
                            templateService.DeletePrivateTemplate(extractedTemplate);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                ProjectService.Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t BulkProject \t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                throw new ApplicationException(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t BulkProject \t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }

            returnObj.Users = returnProjects;

            return returnObj;
        }



        private void EndEnvironmentSetupProcess(IAsyncResult result)
        {
            string templateUsed = string.Empty;
            string ID = string.Empty;
            string accName = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(projectResult.projectId))
                {
                    ID = projectResult.projectId;
                    accName = projectResult.accountName;
                    templateUsed = projectResult.templateUsed;
                    projectService.RemoveKey(ID);

                    if (ProjectService.StatusMessages.Keys.Count(x => x == ID + "_Errors") == 1)
                    {
                        string errorMessages = ProjectService.statusMessages[ID + "_Errors"];

                        if (errorMessages != "")
                        {
                            //also, log message to file system
                            string logPath = Path.GetFullPath(".") + @"\Utilities\DemoGenerator\Log";
                            string accountName = projectResult.accountName;
                            string fileName = string.Format("{0}_{1}.txt", templateUsed, DateTime.Now.ToString("ddMMMyyyy_HHmmss"));

                            if (!Directory.Exists(logPath))
                            {
                                Directory.CreateDirectory(logPath);
                            }

                            System.IO.File.AppendAllText(Path.Combine(logPath, fileName), errorMessages);

                            //Create ISSUE work item with error details in VSTSProjectgenarator account
                            string patBase64 = System.Configuration.ConfigurationManager.AppSettings["PATBase64"];
                            string url = System.Configuration.ConfigurationManager.AppSettings["URL"];
                            string projectId = System.Configuration.ConfigurationManager.AppSettings["PROJECTID"];
                            string issueName = string.Format("{0}_{1}", templateUsed, DateTime.Now.ToString("ddMMMyyyy_HHmmss"));
                            IssueWi objIssue = new IssueWi();

                            errorMessages = errorMessages + Environment.NewLine + "TemplateUsed: " + templateUsed;
                            errorMessages = errorMessages + Environment.NewLine + "ProjectCreated : " + ProjectService.projectName;

                            ProjectService.Logger.Error(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t  Error: " + errorMessages);

                            string logWIT = System.Configuration.ConfigurationManager.AppSettings["LogWIT"];

                            if (logWIT == "true")
                            {
                                objIssue.CreateIssueWi(patBase64, "4.1", url, issueName, errorMessages, projectId, "Demo Generator");
                            }
                        }
                    }
                }
                userCount--;
            }
            catch (Exception ex)
            {
                ProjectService.Logger.Info(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            finally
            {
                if (userCount == 0 && !string.IsNullOrEmpty(templateUsed))
                {
                    templateService.DeletePrivateTemplate(templateUsed);
                }
            }
        }
    }
}