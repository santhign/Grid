using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerService.Models
{
    public class Rewards
    {
        public int? totalPointsAwarded { get; set; }
        public int? totalPointsRedeemed { get; set; }
        public int? totalPointsExpired { get; set; }
        public int? totalPointsAdjusted { get; set; }
        public int? totalPointsAvailable { get; set; }
        public double? totalAmountRedeemed { get; set; }
        public int? totalReferralCount { get; set; }
        public DateTime? lastUpdatedOn { get; set; }
    }
    public class RewardDetails
    {
        public int? rowID { get; set; }
        public int? rewardPeriodID { get; set; }
        public int? rewardEventTypeID { get; set; }
        public int? rewardEventSubTypeID { get; set; }
        public string rewardEventTypeName { get; set; }
        public DateTime? rewardEventDate { get; set; }
        public string rewardEventDetail { get; set; }
        public int? totalPoints { get; set; }
        public string totalPointsDisplay { get; set; }
        public int? runningBalance { get; set; }
    }
}
