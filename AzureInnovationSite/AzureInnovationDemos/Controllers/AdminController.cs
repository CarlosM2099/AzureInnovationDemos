using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureInnovationDemos.Helpers;
using AzureInnovationDemos.Utilities;
using AzureInnovationDemosDAL;
using AzureInnovationDemosDAL.Models;
using AzureInnovationDemosDAL.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AzureInnovationDemos.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AzureDemosDBManager demosDBManager;
        private readonly AzAppsDemosAPIOptions demosAPIOptions;
        private readonly MDConverter mdConverter;
        IHubContext<LogHub> logHubContext;
        public AdminController(AzureDemosDBContext context, AzAppsDemosAPIOptions apiOptions, GlobalSettings globalSettings, IHubContext<LogHub> hubContext)
        {
            demosDBManager = new AzureDemosDBManager(context);
            demosAPIOptions = apiOptions;
            mdConverter = new MDConverter(context, demosAPIOptions, globalSettings, hubContext);
            logHubContext = hubContext;
        }

        [HttpPost("~/api/admin/syncmdcontent")]
        public async Task SyncMDContent()
        {
            await mdConverter.ConvertMDGuides();
        }
    }
}