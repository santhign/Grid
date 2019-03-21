using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminService.Models
{
    public class AdminUsers
    {
        public int AdminUserID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }         
        public string Role { get; set; }
         
    }
    public class AdminUserLoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
