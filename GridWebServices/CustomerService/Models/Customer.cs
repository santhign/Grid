using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

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
        [Required(ErrorMessage = "Email address required")]       
        [EmailAddress(ErrorMessage = "Enter valid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password required")]
        public string Password { get; set; }       
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Email address required")]
        [EmailAddress(ErrorMessage = "Enter valid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password required")]
        public string Password { get; set; }
    }

    public class LoggedInPrinciple
    {
        public Customer Customer { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Token { get; set; }
    }

    public class AuthTokenResponse
    {
        public int CustomerID { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
