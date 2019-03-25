using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;


namespace Core.Models
{
    [DataContract]
   
    public class GridBSSConfi
    {
       
        [DataMember(Name = "BSSAPIUrl")] 
        public string BSSAPIUrl { get; set; }

        [DataMember(Name = "GridDefaultAssetLimit")]
        public int GridDefaultAssetLimit { get; set; }

        [DataMember(Name = "GridEntityId")]
        public int GridEntityId { get; set; }

        [DataMember(Name = "GridProductId")]
        public int GridProductId { get; set; }

        [DataMember(Name = "GridId")]
        public int GridId { get; set; }

        [DataMember(Name = "GridSourceNode")]
        public string GridSourceNode { get; set; }

        [DataMember(Name = "GridUserName")]
        public string GridUserName { get; set; }

        [DataMember(Name = "GridDefaultOffset")]
        public int GridDefaultOffset { get; set; }

        [DataMember(Name = "GridDefaultLimit")]
        public int GridDefaultLimit { get; set; }        

    }

    [DataContract]
    public class GridSystemConfig
    {
        [DataMember(Name = "DeliveryMarginInDays")]
        public int DeliveryMarginInDays { get; set; }

        [DataMember(Name = "FreeNumberListCount")]
        public int FreeNumberListCount { get; set; }

        [DataMember(Name = "PremiumNumberListCount")]
        public int PremiumNumberListCount { get; set; }

    }
}
