using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerService.Models
{
    public class Emails
    {
        public string EmailId { get; set; }

    }

    public class ForgetPassword
    {

        public int CustomerId { get; set; }

        public string Token { get; set; }

    }
    public class NotificationEmail
    {
        public string Subject { get; set; }
        public string Content { get; set; }
        public string BccAddress { get; set; }
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
    public class Postcode
    {

        public string APIKey { get; set; }

        public string APISecret { get; set; }

        public string PostcodeNumber { get; set; }

        public string PostData { get; set; }
    }

    public class Sms
    {
        public string PhoneNumber { get; set; }
        public string SMSText { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FromPhoneNumber { get; set; }
        public string ToPhoneNumber { get; set; }
        public string Type { get; set; }
        public string PostData { get; set; }
        public byte[] buffer { get; set; }

    }
}