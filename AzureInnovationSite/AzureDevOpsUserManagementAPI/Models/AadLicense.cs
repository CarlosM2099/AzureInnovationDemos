using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureDevOpsUserManagementAPI.Models
{
    public class AadSubscriptionSkuList
    {
        public List<AadSubscriptionSku> Value { get; set; }
    }

    public class AadSubscriptionSku
    {
        public string AppliesTo { get; set; }
        public string CapabilityStatus { get; set; }
        public int ConsumedUnits { get; set; }
        public string Id { get; set; }
        public Guid SkuId { get; set; }
        public string SkuPartNumber { get; set; }
    }

    public class AadLicense
    {
        public object[] DisabledPlans { get; set; }
        public Guid SkuId { get; set; }

    }
}