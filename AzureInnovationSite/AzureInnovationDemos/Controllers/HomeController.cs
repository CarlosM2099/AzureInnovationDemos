using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AzureInnovationDemos.Models;
using System.Security.Claims;
using AzureInnovationDemosDAL;
using AzureInnovationDemosDAL.Utilities;
using AzureInnovationDemosDAL.Models;
using AzureInnovationDemos.Extensions;
using AzureInnovationDemos.Helpers;

namespace AzureInnovationDemos.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        ClaimsIdentity user;
        private readonly AzureDemosDBManager demosDBManager;
        private readonly AzAppsDemosAPIOptions demosAPIOptions;


        public HomeController(AzureDemosDBContext context, AzAppsDemosAPIOptions apiOptions)
        {
            demosDBManager = new AzureDemosDBManager(context);
            demosAPIOptions = apiOptions;
        }

        public ActionResult Index()
        {
            user = (HttpContext.User.Identity as ClaimsIdentity);
            ViewBag.UserName = user.GetDisplayName();
            return View();
        }

        [Route("/Demo/{id?}")]
        public async Task<ActionResult> Demo(int id)
        {
            user = (HttpContext.User.Identity as ClaimsIdentity);
            ViewBag.UserName = user.GetDisplayName();

            var demo = await demosDBManager.GetDemo(id);

            if (demo == null || !demo.IsVisible || demo.IsDisabled)
            {
                return View("NotFound");
            }

            return View();
        }

        [Route("/Guide/{id?}")]
        public async Task<ActionResult> Guide(string id)
        {
            if (id == null)
            {
                return View(new DemoGuideContent() { GuideContent = "<h2>No Guide was found</h2>" });
            }

            GuideContentDB guideContentDB = new GuideContentDB(demosAPIOptions.DemosGuideDB);
            var demoAssets = await demosDBManager.GetDemosAssets();

            var guideAsset = demoAssets
                .FirstOrDefault(a => a.Alias != null && a.Alias.Equals(id, StringComparison.InvariantCultureIgnoreCase));

            if (guideAsset == null)
            {
                return View(new DemoGuideContent() { GuideContent = "<h2>No Guide was found</h2>" });
            }

            var guideContent = await guideContentDB.GetGuideContent(guideAsset.Id);

            if (string.IsNullOrEmpty(guideContent))
            {
                return View(new DemoGuideContent() { GuideContent = "<h2>No Guide was found</h2>" });
            }

            var claimsUser = (User.Identity as ClaimsIdentity);
            var user = await demosDBManager.GetUser(claimsUser.GetAccount());

            var userEnvs = await demosDBManager.GetDemoUserEnvironments(guideAsset.DemoId, user.Id);

            if (userEnvs.Count == 0 || !userEnvs.First().EnvironmentProvisioned)
            {
                return View(new DemoGuideContent { DemoAssetId = guideAsset.Id, GuideContent = guideContent, DemoId = guideAsset.DemoId });
            }

            var demoGuideContent = new DemoGuideContent { DemoId = guideAsset.DemoId, DemoAssetId = guideAsset.Id, GuideContent = guideContent };

            demoGuideContent.Environment = userEnvs.FirstOrDefault();
            demoGuideContent.VM = await demosDBManager.GetDemoVM(demoGuideContent.DemoId);
            demoGuideContent.Assets = await demosDBManager.GetDemoAssets(demoGuideContent.DemoId);

            demoGuideContent.Assets = demoGuideContent.Assets
                .Where(a => (DemoAssetTypeEnum)a.Type.Id == DemoAssetTypeEnum.AccessKeyToken || (DemoAssetTypeEnum)a.Type.Id == DemoAssetTypeEnum.Link)
                .ToList();

            var demoUserResources = await demosDBManager.GetDemoUserResources(demoGuideContent.DemoId, user.Id);
            var demoUserAssets = demoUserResources.Select(r => new DemoAsset() { Name = r.Name, Value = r.Value, Type = r.Type, Id = r.Id });

            demoGuideContent.Assets.AddRange(demoUserAssets);

            return View(demoGuideContent);
        }

        [Route("/GuideSync")]
        public async Task<ActionResult> GuideSync()
        {
            user = (HttpContext.User.Identity as ClaimsIdentity);
            var demoUser = await demosDBManager.GetUser(user.GetAccount());

            if (!demoUser.IsAdmin)
            {
                return View("NotFound");
            }

            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
