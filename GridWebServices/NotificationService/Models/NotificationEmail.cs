using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.Models
{

    public class NotificationEmail
    {
        public string Subject { get; set; }
        public string Content { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string ReplyEmail { get; set; }
        public string ReplyName { get; set; }
        public string BccAddress { get; set; }
        public string emailTags { get; set; }
        public List<EmailDetails> EmailDetails { get; set; }
    }
    public class EmailDetails
    {
        public string Userid { get; set; }
        public string FName { get; set; }
        public string EMAIL { get; set; }
        public string Param1 { get; set; }
        public string Param2 { get; set; }
        public string Param3 { get; set; }
        public string Param4 { get; set; }
        public string Param5 { get; set; }
        public string Param6 { get; set; }
        public string Param7 { get; set; }
        public string Param8 { get; set; }
        public string Param9 { get; set; }
        public string Param10 { get; set; }
    }
}
