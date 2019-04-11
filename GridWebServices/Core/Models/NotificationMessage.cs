using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Models
{
    [DataContract]
    public class NotificationMessage
    {
        [DataMember(Name = "MessageType")]
        public string MessageType { get; set; }

        [DataMember(Name = "MessageName")]
        public string MessageName { get; set; }

        [DataMember(Name = "MessageTemplate")]
        public string MessageTemplate { get; set; }

        [DataMember(Name = "Message")]
        public MessageObject Message { get; set; }

    }

    public class MessageObject
    {
        [DataMember(Name = "emailaddress")]
        public string emailaddress { get; set; }

        [DataMember(Name = "mobilenumber")]
        public string mobilenumber { get; set; }

        [DataMember(Name = "messagetemplate")]
        public string messagetemplate { get; set; }

        public List<NotificationParams> parameters { get; set; }
    }


    public class NotificationParams
    {
        [DataMember(Name = "name")]
        public string name { get; set; }

        [DataMember(Name = "param1")]
        public string param1 { get; set; }

        [DataMember(Name = "param2")]
        public string param2 { get; set; }

        [DataMember(Name = "param3")]
        public string param3 { get; set; }

        [DataMember(Name = "param4")]
        public string param4 { get; set; }

        [DataMember(Name = "param5")]
        public string param5 { get; set; }

        [DataMember(Name = "param6")]
        public string param6 { get; set; }

        [DataMember(Name = "param7")]
        public string param7 { get; set; }

        [DataMember(Name = "param8")]
        public string param8 { get; set; }

        [DataMember(Name = "param9")]
        public string param9 { get; set; }

        [DataMember(Name = "param10")]
        public string param10 { get; set; }
    }

}
   

