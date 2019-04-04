using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.Models
{
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
    public class NotificationEmail
    {
        public string Subject { get; set; }
        public string Content { get; set; }
        public string BccAddress { get; set; }
        public List<EmailDetails> EmailDetails { get; set; }
    }
}
