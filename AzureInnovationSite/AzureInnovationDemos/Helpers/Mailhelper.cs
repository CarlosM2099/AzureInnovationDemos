using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace AzureInnovationDemos.Helpers
{
    public class MailHelper
    {
        public static void SendEmail(string subject, string body, string mailAlert)
        {
            using (var client = new SmtpClient("smtp.sendgrid.net")
            {
                EnableSsl = true,
                Port = 587,
                Credentials = new NetworkCredential
                (
                    "azure_1dbfa3d55b1011db55397e5871177ea2@azure.com",
                    "Pass@word125"
                )
            })
            {                 
                client.Send
                (
                    new MailMessage
                    {
                        From = new MailAddress("AzureAppsDemo@3Sharp.com", "Azure Apps Demo Notifications"),
                        To = { new MailAddress(mailAlert, "Azure Apps Demo Notifications") },
                        IsBodyHtml = true,
                        Subject = subject,
                        Body = body,
                    }
                );
            }
        }
    }
}