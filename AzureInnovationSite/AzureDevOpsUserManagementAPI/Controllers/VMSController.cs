using AzureDevOpsUserManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
 
namespace AzureDevOpsUserManagementAPI.Controllers
{
    [Authorize]
    [ApiController]
    public class VMSController : ControllerBase
    {         
        [HttpPost]
        [Route("~/api/VMS/addtoVMadmin")]
        public  ADODemoGenResult SetUserVMadmin([FromBody]AadUser user)
        {
            return new ADODemoGenResult()
            {
                Users = new List<ADODemoGenResultUser>() { new ADODemoGenResultUser() { Email = user.UserPrincipalName,
                ProjectUrl = "http://portal.azure.com/" } }
            };
        }
    }
}
