using log4net;
using Newtonsoft.Json;
using System;
using VstsRestAPI.Services;

namespace VstsRestAPI.WorkItemAndTracking
{
    public class IssueWi
    {
        private Configuration _con = new Configuration();
        private readonly ILog logger = LogManager.GetLogger("ErrorLog", "ErrorLog");

        // Create Issue Work Items
        public void CreateIssueWi(string credential, string version, string url, string issueName, string description, string projectId, string tag)
        {
            try
            {
                Object[] patchDocument = new Object[3];
                patchDocument[0] = new { op = "add", path = "/fields/System.Title", value = issueName };
                patchDocument[1] = new { op = "add", path = "/fields/System.Description", value = description };
                patchDocument[2] = new { op = "add", path = "/fields/System.Tags", value = tag };


                _con.UriString = url;
                _con.PersonalAccessToken = credential;
                _con.Project = projectId;
                _con.VersionNumber = version;
                _con.UriParams = "/_apis/wit/workitems/$Issue?api-version=";
                _con.RequestBody = JsonConvert.SerializeObject(patchDocument);
                HttpServices httpServices = new HttpServices(_con);
                var response = httpServices.PatchBasic();
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t"   + "\n" + ex.StackTrace + "\n");
            }
        }

        // Create Report work items
        public void CreateReportWi(string credential, string version, string url, string websiteUrl, string reportName, string accountName, string templateName, string projectId, string region)
        {
            try
            {
                if (string.IsNullOrEmpty(region))
                {
                    region = "";
                }

                Object[] reportPatchDocument = new Object[5];

                reportPatchDocument[0] = new { op = "add", path = "/fields/System.Title", value = reportName };
                reportPatchDocument[1] = new { op = "add", path = "/fields/CustomAgile.SiteName", value = websiteUrl };
                reportPatchDocument[2] = new { op = "add", path = "/fields/CustomAgile.AccountName", value = accountName };
                reportPatchDocument[3] = new { op = "add", path = "/fields/CustomAgile.TemplateName", value = templateName };
                reportPatchDocument[4] = new { op = "add", path = "/fields/CustomAgile.Region", value = region };

                _con.UriString = url;
                _con.PersonalAccessToken = credential;
                _con.Project = projectId;
                _con.VersionNumber = version;
                _con.UriParams = "/_apis/wit/workitems/$Analytics?api-version=";
                _con.RequestBody = JsonConvert.SerializeObject(reportPatchDocument);
                HttpServices httpServices = new HttpServices(_con);
                var response = httpServices.PatchBasic();
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t"   + "\n" + ex.StackTrace + "\n");
            }
        }
    }
}
