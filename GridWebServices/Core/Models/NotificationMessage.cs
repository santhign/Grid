﻿using System;
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

        [DataMember(Name = "Message")]
        public MessageObject Message { get; set; }        

    }

    public class MessageObject
    {     

        [DataMember(Name = "messagetemplate")]
        public string messagetemplate { get; set; }
        public string bccAddress { get; set; }
        public List<NotificationParams> parameters { get; set; }

        public override string ToString()
        {
            return "{messagetemplate: " + messagetemplate + ", bccAddress: " + bccAddress + ", parameters list: " + string.Join(", ", parameters) + "}";
        }
    }


    public class NotificationParams
    {
        [DataMember(Name = "emailaddress")]
        public string emailaddress { get; set; }

        [DataMember(Name = "mobilenumber")]
        public string mobilenumber { get; set; }
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

        [DataMember(Name = "param11")]
        public string param11 { get; set; }

        [DataMember(Name = "param12")]
        public string param12 { get; set; }

        [DataMember(Name = "param13")]
        public string param13 { get; set; }

        [DataMember(Name = "param14")]
        public string param14 { get; set; }

        [DataMember(Name = "param15")]
        public string param15 { get; set; }

        [DataMember(Name = "param16")]
        public string param16 { get; set; }

        [DataMember(Name = "param17")]
        public string param17 { get; set; }

        [DataMember(Name = "param18")]
        public string param18 { get; set; }

        [DataMember(Name = "param19")]
        public string param19 { get; set; }

        [DataMember(Name = "param20")]
        public string param20 { get; set; }

        [DataMember(Name = "param21")]
        public string param21 { get; set; }

        [DataMember(Name = "param22")]
        public string param22 { get; set; }

        [DataMember(Name = "param23")]
        public string param23 { get; set; }

        [DataMember(Name = "param24")]
        public string param24 { get; set; }

        [DataMember(Name = "param25")]
        public string param25 { get; set; }

        [DataMember(Name = "param26")]
        public string param26 { get; set; }

        [DataMember(Name = "param27")]
        public string param27 { get; set; }

        [DataMember(Name = "param28")]
        public string param28 { get; set; }

        [DataMember(Name = "param29")]
        public string param29 { get; set; }

        [DataMember(Name = "param30")]
        public string param30 { get; set; }
    }

    public class NotificationLog
    {       
        public int CustomerID { get; set; }
        public string Email { get; set; }

        public string EmailSubject { get; set; }

        public string EmailBody { get; set; }

        public DateTime ScheduledOn { get; set; }

        public int EmailTemplateID { get; set; }

        public DateTime SendOn { get; set; }

        public int Status { get; set; }

    }

    public class SMSNotificationLog
    {
      
        public string Email { get; set; }

        public string SMSText { get; set; }

        public string Mobile { get; set; }

        public DateTime ScheduledOn { get; set; }

        public int SMSTemplateID { get; set; }

        public DateTime SendOn { get; set; }

        public int Status { get; set; }

    }

    public class NotificationLogForDevPurpose : NotificationLog
    {
        public string EventType { get; set; }

        public string Message { get; set; }
    }

    public class NotificationConfig
    {
        public string SNSTopic { get; set; }
        public string SQS { get; set; }

    }
}
   

