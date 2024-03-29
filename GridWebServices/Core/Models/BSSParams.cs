﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
   public class BSSParams
    {
        public BSSParams()
        {
            ProductType = "product_type";

            AssetStatus = "asset_status";

            ProductType = "product_type";

            AssetId = "asset_id";

            ProductId = "product_id";

            Offset = "offset";

            Limit = "limit";

            StartRange = "start_range";

            EndRange = "end_range";

            PosId = "pos_id";

            CategoryId = "category_id";

            EntityId = "entity_id";

            UnBlockAsset = "unblock_asset";

            ConnectionType = "connectionType";

            AccountType = "accountType ";

            ServiceId = "service_id";

            GroupId = "group_Id";

            GroupPlan = "groupPlan";

            FromDate = "from_date";

            ToDate = "to_date";

            AccountId = "account_id";

            Status = "status";

        }

        public string ProductType { get; set; }
        public string AssetStatus { get; set; }
        public string AssetId { get; set; }
        public string ProductId { get; set; }
        public string Offset { get; set; }
        public string Limit { get; set; }
        public string StartRange { get; set; }
        public string EndRange { get; set; }
        public string PosId { get; set; }
        public string CategoryId { get; set; }
        public string EntityId { get; set; }
        public string UnBlockAsset { get; set; }
        public string ConnectionType { get; set; }
        public string AccountType { get; set; }
        public string ServiceId { get; set; }
        public string GroupId { get; set; }
        public string GroupPlan { get; set; }
        public string FromDate { get; set; }
        public string ToDate  { get; set; }
        public string AccountId { get; set; }
        public string Status { get; set; }
    }
    
}
