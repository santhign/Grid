﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;


namespace Core.Models
{
    [DataContract]
    public class BSSAssetRequest
    {
        [DataMember(Name = "request_id")] //
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


        [DataMember(Name = "dataset")]
        public SetParam dataset { get; set; }

        public int BSSCallLogID { get; set; }

    }

    [DataContract]
    public class SetParam
    {
        [DataMember(Name = "param")]
        public List<RequestParam> param { get; set; }
    }


    [DataContract]
    public class RequestParam
    {
        [DataMember(Name = "id")]
        public string id { get; set; }

        [DataMember(Name = "value")]
        public string value { get; set; }
    }

    [DataContract]
    public class RequestObject
    {
        [DataMember(Name = "Request")]
        public BSSAssetRequest Request { get; set; }


    }
}
