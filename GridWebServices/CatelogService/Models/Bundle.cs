using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatelogService.Models
{
    public class Bundle
    {
        public int BundleID { get; set; }
        public string BundleName { get; set; }
        public string PlanMarketingName { get; set; }
        public string PortalDescription { get; set; }
        public string PortalSummaryDescription { get; set; }
        public double TotalData { get; set; }
        public double TotalSMS { get; set; }
        public double TotalVoice { get; set; }
        public double ActualSubscriptionFee { get; set; }
        public double ApplicableSubscriptionFee { get; set; }
        public double ActualServiceFee { get; set; }
        public double ApplicableServiceFee { get; set; }
        public string ServiceName { get; set; }
    }
}
