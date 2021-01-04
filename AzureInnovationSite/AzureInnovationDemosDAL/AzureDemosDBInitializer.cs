using AzureInnovationDemosDAL.Models;
using AzureInnovationDemosDAL.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL
{
    public class AzureDemosDBInitializer
    {
        public static void Initialize(AzureDemosDBContext context)
        {
            context.Database.EnsureCreated();

            context.DemoTypes.SeedEnumValues<DemoType, DemoTypeEnum>(@enum => @enum, t => t, context);
            context.DemoAssetTypes.SeedEnumValues<DemoAssetType, DemoAssetTypeEnum>(@enum => @enum, t => t, context);
            context.DemoAzureResourceTypes.SeedEnumValues<DemoAzureResourceType, DemoAzureResourceTypeEnum>(@enum => @enum, t => t, context);

            context.SaveChanges();
 

            var adoDemoType = context
               .DemoTypes
               .FirstOrDefault(r => r.Id == (int)DemoTypeEnum.ADODemo);

            var autoglassDemoType = context
               .DemoTypes
               .FirstOrDefault(r => r.Id == (int)DemoTypeEnum.AutoGlass);

            var modernAppDemoType = context
              .DemoTypes
              .FirstOrDefault(r => r.Id == (int)DemoTypeEnum.AppModernization);

            var kubDemoType = context
              .DemoTypes
              .FirstOrDefault(r => r.Id == (int)DemoTypeEnum.Kubernetes);

            var demos = new List<Demo> { new Demo()
            {
                Id = 1,
                Name = "Tailwind Traders Demo",
                Categories = "Azure DevOps and GitHub",
                Description = "Experience modern developer productivity with continuous integration using Azure DevOps and GitHub.",
                Abstract = "This demo can be performed as a click-through or a live demo.",
                Technologies = "Azure DevOps, GitHub, Azure Boards",
                IsSharedEnvironment = false,
                IsVisible = true,
                Additional = "To perform this demo live, you will need access to a shared demo environment where some pre-demo setup and configuration steps have already been configured for you. You will have restricted access to the environment, but will be able to perform all activities listed in the live code demo guide. Your personal demo user credentials in this shared environment will be active for a limited period. Please note your credentials and relevant resource links below.",
                Type = adoDemoType
            }, new Demo()
            {
                Id = 2,
                Name = "AutoGlass PowerApps Demo",
                Categories = "PowerApps and Azure Functions",
                Description = "Autoglass is the U.K.’s leading glass repair-and-replacement company, serving more than one million motorists each year. Many of its staff visit businesses to inspect and repair fleets of vehicles. This demo shows how you can use a combination of canvas-based and model-driven PowerApps solutions to reinvent and digitize a process, such as the one Autoglass uses for inspecting fleets.",
                Abstract = "This demo can be performed as a click-through or a live demo.",
                Technologies = "PowerApps, Azure Functions",
                IsSharedEnvironment = false,
                IsVisible = true,
                Additional = "To perform this demo live, you will need access to a shared demo environment using a shared user credential. The credentials will grant you limited access to the demo environment (which includes a Microsoft 365 tenant and an Azure subscription) but will be able to perform all steps and activities described in the live code demo guide.",
                Type = autoglassDemoType
            }, new Demo()
            {
                Id = 3,
                Name = "App Modernization Demo",
                Categories = "Azure App Services and Azure Logic Apps",
                Description = "A developer benefits the following key takeaways: With Visual Studio and Azure App Service, you can easily migrate your on-premise ASP.NET application to Azure.",
                Abstract = "This demo can be performed as a click-through or a live demo.",
                Technologies = "Azure Apps, Azure Logic Apps, Microsoft Translation Services",
                IsSharedEnvironment = false,
                IsVisible = true,
                Additional = "To perform this demo live, you will need access to a shared demo environment using a shared user credential. The credentials will grant you limited access to the demo environment (which includes a Microsoft 365 tenant and an Azure subscription) but will be able to perform all steps and activities described in the live code demo guide.",
                Type = modernAppDemoType
            }, new Demo()
            {
                Id = 4,
                Name = "Kubernetes demo",
                Categories = "Kubernetes",
                Description = "Coming Soon: See how a real-world customer used Microsoft 365 and Azure to automate business processes and save time and money!",
                Abstract = "This demo can be performed as a click-through or a live demo.",
                Technologies = "Azure Apps",
                IsSharedEnvironment = false,
                IsVisible = true,
                Additional = "To perform this demo live, you will need access to a shared demo environment using a shared user credential. The credentials will grant you limited access to the demo environment (which includes a Microsoft 365 tenant and an Azure subscription) but will be able to perform all steps and activities described in the live code demo guide.",
                Type = kubDemoType,
                IsDisabled = true
            } };


            using (var transaction = context.Database.BeginTransaction())
            {                 
                context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT [dbo].[Demos] ON");

               
                context.Demos.AddOrUpdate(ref demos, d => d);
                context.SaveChanges();

                context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT [dbo].[Demos] OFF");
                transaction.Commit();
            }                        
        }
    }
}
