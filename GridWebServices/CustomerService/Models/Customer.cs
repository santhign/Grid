using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerService.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string MobileNumber { get; set; }
        public string ReferralCode { get; set; }
        public string Nationality { get; set; }
        public string Gender { get; set; }
        public string SMSSubscription { get; set; }
        public string EmailSubscription { get; set; }
        public string Status { get; set; }
    }

    public class RegisterCustomer
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
