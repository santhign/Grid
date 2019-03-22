using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Models
{
    [DataContract]
    public class BSSUpdateResponse
    {
        [DataMember(Name = "request_id")]
        public string request_id { get; set; }

        [DataMember(Name = "request_timestamp")]
        public string request_timestamp { get; set; }

        [DataMember(Name = "response_timestamp")]
        public string response_timestamp { get; set; }

        [DataMember(Name = "source_node")]
        public string source_node { get; set; }

        [DataMember(Name = "result_code")]
        public string result_code { get; set; }

        [DataMember(Name = "action")]
        public string action { get; set; }

        [DataMember(Name = "result_desc")]
        public string result_desc { get; set; }


        [DataMember(Name = "dataSet")]
        public SetParam dataSet { get; set; }
    }

    [DataContract]
    public class BSSUpdateResponseObject
    {
        [DataMember(Name = "Response")]
        public BSSUpdateResponse Response { get; set; }
    }

}