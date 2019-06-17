using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

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
        public string Param11 { get; set; }
        public string Param12 { get; set; }
        public string Param13 { get; set; }
        public string Param14 { get; set; }
        public string Param15 { get; set; }
        public string Param16 { get; set; }
        public string Param17 { get; set; }
        public string Param18 { get; set; }
        public string Param19 { get; set; }
        public string Param20 { get; set; }
        public string Param21 { get; set; }
        public string Param22 { get; set; }
        public string Param23 { get; set; }
        public string Param24 { get; set; }
        public string Param25 { get; set; }
        public string Param26 { get; set; }
        public string Param27 { get; set; }
        public string Param28 { get; set; }
        public string Param29 { get; set; }
        public string Param30 { get; set; }
    }
    public class NotificationEmail
    {
        public string Subject { get; set; }
        public string Content { get; set; }
        public string BccAddress { get; set; }
        public List<EmailDetails> EmailDetails { get; set; }  

    }

   
}
