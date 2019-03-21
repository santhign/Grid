using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

    public class RegisterAdminUser
    {
        [Required(ErrorMessage = "Email address required")]
        [EmailAddress(ErrorMessage = "Enter valid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password required")]
        public string Password { get; set; }

        public int DepartmentID { get; set; }
        public int OfficeID { get; set; }
        public int RoleID { get; set; } 
    }

}
