using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
namespace Core.Models
{
    [DataContract]
    public class BSSUpdateRequest
    {
            [DataMember(Name = "request_id")] 
            public string request_id { get; set; }

            [DataMember(Name = "request_timestamp")]
            public string request_timestamp { get; set; }

            [DataMember(Name = "action")]
            public string action { get; set; }

            [DataMember(Name = "userid")]
            public string userid { get; set; }

            [DataMember(Name = "username")]
            public string username { get; set; }

            [DataMember(Name = "source_node")]
            public string source_node { get; set; }

            [DataMember(Name = "order_information")]
            public BSSOrderInformation order_information { get; set; }

    }

    [DataContract]
    public class BSSOrderInformation
    {
        [DataMember(Name = "order_type")]
        public string order_type { get; set; }

        [DataMember(Name = "customer_name")]
        public string customer_name { get; set; }
       
        [DataMember(Name = "dataset")]
        public SetParam dataset { get; set; }

    }

    [DataContract]
    public class UpdateRequestObject
    {
        [DataMember(Name = "Request")]
        public BSSUpdateRequest Request { get; set; }


    }

}



  