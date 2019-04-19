using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerService.Models
{
    public class Authenticate
    {
    }
    public class AccessToken
    {
        public int CustomerID { get; set; }
        public int AdminUserID { get; set; }
        public string Token { get; set; }

    }
}
