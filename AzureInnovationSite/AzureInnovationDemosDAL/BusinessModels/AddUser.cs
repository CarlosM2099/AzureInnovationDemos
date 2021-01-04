using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL.BusinessModels
{
    public class AadUser
    {
        public bool AccountEnabled { get; set; }
        public Guid Id { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string MailNickname { get; set; }
        public string UserPrincipalName { get; set; }
        public string UsageLocation { get; set; }
    }
}
