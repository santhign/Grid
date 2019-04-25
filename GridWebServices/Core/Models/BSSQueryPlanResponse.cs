using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{   
    public class Bucket
    {
        public string bucketId { get; set; }
        public string remaining { get; set; }
        public string unit { get; set; }
        public string usage { get; set; }
        public string size { get; set; }
    }

    public class ReccuringCharge
    {
        public string chargeId { get; set; }
        public string chargeType { get; set; }
        public string chargeName { get; set; }
        public string chargeCode { get; set; }
        public string chargeCodeId { get; set; }
        public string chargeCategory { get; set; }
        public string chargeAmount { get; set; }
        public string chargeMode { get; set; }
        public string chargeTax { get; set; }
        public string isRenewal { get; set; }
        public string factor { get; set; }
        public string suspensionChargeEnable { get; set; }
        public string chargeAmountType { get; set; }
    }

    public class Charges
    {
        public List<ReccuringCharge> reccuringCharges { get; set; }
    }

    public class BSSBundle
    {
        public string bundleId { get; set; }
        public string subscriptionId { get; set; }
        public string bundleStatus { get; set; }
        public string startDate { get; set; }
        public string expiryDate { get; set; }
        public string productName { get; set; }
        public string productDescription { get; set; }
        public string productMarketName { get; set; }
        public string price { get; set; }
        public string bundleName { get; set; }
        public string bundleDesc { get; set; }
        public string offerCategory { get; set; }
        public string offerServiceId { get; set; }
        public string addOnType { get; set; }
        public List<Bucket> buckets { get; set; }
        public Charges charges { get; set; }
        public string __invalid_name__subscriptionId { get; set; }
    } 

    public class BSSQueryPlanResponse
    {
        public string request_id { get; set; }
        public string request_timestamp { get; set; }
        public string response_timestamp { get; set; }
        public string action { get; set; }
        public string userid { get; set; }
        public string username { get; set; }
        public string source_node { get; set; }
        public string result_code { get; set; }
        public string result_desc { get; set; }
        public List<BSSBundle> bundles { get; set; }
        public QueryPlanDataset dataSet { get; set; }
    }

    public class BSSQueryPlanResponseObject
    {
        public BSSQueryPlanResponse Response { get; set; }
    }




}
