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

        public string Name { get; set; }

        public string Email { get; set; }

    }

    public class EmailValidationResponse
    {
        public bool IsValid { get; set; }
        public string Status { get; set; }
    }
}