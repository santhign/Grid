using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Models
{
    [DataContract]
    public class BSSAsset
    {
        [DataMember(Name = "seq_id")]
        public string seq_id { get; set; }

        [DataMember(Name = "asset_name")]
        public string asset_name { get; set; }

        [DataMember(Name = "asset_status")]
        public string asset_status { get; set; }

        [DataMember(Name = "poduct_id")]
        public string poduct_id { get; set; }

        [DataMember(Name = "expiry_date")]
        public string expiry_date { get; set; }

        [DataMember(Name = "asset_id")]
        public string asset_id { get; set; }

        [DataMember(Name = "vendor_id")]
        public string vendor_id { get; set; }

        [DataMember(Name = "ware_house")]
        public string ware_house { get; set; }

        [DataMember(Name = "order_id")]
        public string order_id { get; set; }

        [DataMember(Name = "company_id")]
        public string company_id { get; set; }

        [DataMember(Name = "Company_name")]
        public string Company_name { get; set; }

        [DataMember(Name = "price")]
        public string price { get; set; }

        [DataMember(Name = "selling_price")]
        public string selling_price { get; set; }

        [DataMember(Name = "shipping_cost")]
        public string shipping_cost { get; set; }

        [DataMember(Name = "bundle_id")]
        public string bundle_id { get; set; }

        [DataMember(Name = "whole_sale_price")]
        public string whole_sale_price { get; set; }

        [DataMember(Name = "stock_request_id")]
        public string stock_request_id { get; set; }

        [DataMember(Name = "pos_id")]
        public string pos_id { get; set; }

        [DataMember(Name = "pos_name")]
        public string pos_name { get; set; }

        [DataMember(Name = "currency")]
        public string currency { get; set; }

        [DataMember(Name = "category_type")]
        public string category_type { get; set; }

        [DataMember(Name = "stock_type")]
        public string stock_type { get; set; }

        [DataMember(Name = "owner_id")]
        public string owner_id { get; set; }

        [DataMember(Name = "product_image")]
        public string product_image { get; set; }

        [DataMember(Name = "bundle_sale")]
        public string bundle_sale { get; set; }

        [DataMember(Name = "asset_specific_info")]
        public string asset_specific_info { get; set; }

        [DataMember(Name = "asset_status_id")]
        public string asset_status_id { get; set; }
    }

    [DataContract]
    public class AssetDetails
    {
        [DataMember(Name = "total_record_count")]
        public string total_record_count { get; set; }

        [DataMember(Name = "assets")]
        public List<BSSAsset> assets { get; set; } 
    }


    [DataContract]
    public class AssetsResponse
    {
        [DataMember(Name = "request_id")] 
        public string request_id { get; set; }

        [DataMember(Name = "request_timestamp")]
        public string request_timestamp { get; set; }        

        [DataMember(Name = "responseTimestamp")]
        public string responseTimestamp { get; set; }

        [DataMember(Name = "action")]
        public string action { get; set; }

        [DataMember(Name = "userid")]
        public int userid { get; set; }

        [DataMember(Name = "username")]
        public string username { get; set; }

        [DataMember(Name = "source_node")]
        public string source_node { get; set; }

        [DataMember(Name = "resultCode")]
        public string resultCode { get; set; }

        [DataMember(Name = "result_description")]
        public string result_description { get; set; }

        [DataMember(Name = "asset_details")]
        public AssetDetails asset_details { get; set; }
    }
}
