using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatelogService.Models
{
    public class VAS
    {
        public int VASID { get; set; }
        public string BSSPlanCode { get; set; }
        public string PlanMarketingName { get; set; }
        public string PortalDescription { get; set; }
        public string PortalSummaryDescription { get; set; }
        public double Data { get; set; }
        public double SMS { get; set; }
        public double Voice { get; set; }
        public double SubscriptionFee { get; set; }
        public string IsRecurring { get; set; }
    }
}
