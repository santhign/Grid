using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.Models
{
    public class Sms
    {
        public string PhoneNumber { get; set; }
        public string SMSText { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } 
        public string FromPhoneNumber { get; set; } 
        public string  ToPhoneNumber { get; set; }
        public string  Type { get; set; } 
        public string  PostData { get; set; }
        public byte[] buffer { get; set; }
         
    }
     
}
