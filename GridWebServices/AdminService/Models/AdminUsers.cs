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
        [RegularExpression(@"(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,}", ErrorMessage = "Please enter a valid password")]
        public string Password { get; set; }         
        public string Role { get; set; }
        public int Status { get; set; }
        public List<string> Permissions { get; set; }

    }

    public class Permission
    {
        public string RolePermission { get; set; }
    }
    public class Roles
    {
        public int RoleID { get; set; }
        public string Role { get; set; }
    }
    public class AdminUserLoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterAdminUser
    {
        public string FullName { get; set; }
        [Required(ErrorMessage = "Email address required")]
        [EmailAddress(ErrorMessage = "Enter valid email address")]
        public string Email { get; set; }

        [RegularExpression(@"(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,}", ErrorMessage = "Please enter a valid password")]
        [Required(ErrorMessage = "Password required")]
        public string Password { get; set; }
        [Required(ErrorMessage = "RoleID required")]
        public int RoleID { get; set; } 
    }

    public class AdminProfile
    {
        public string  Name { get; set; }

        [RegularExpression(@"(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,}", ErrorMessage = "Please enter a valid password")]
        public string  NewPassword { get; set; }
    }

    public class AdminUserProfile
    {
        public int AdminUserID { get; set; }
        public string Name { get; set; }
        [Required(ErrorMessage = "Email address required")]
        [EmailAddress(ErrorMessage = "Enter valid email address")]
        public string Email { get; set; }
        public int RoleID { get; set; }
    }
    public class AdminUserPassword
    {
        public int AdminUserID { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,}", ErrorMessage = "Please enter a valid password")]
        public string Password { get; set; }
    }

    public class LoggedInPrinciple
    {
        /// <summary>
        /// Gets or sets the customer.
        /// </summary>
        /// <value>
        /// The customer.
        /// </value>
        public AdminUsers AdminUser { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is authenticated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is authenticated; otherwise, <c>false</c>.
        /// </value>
        public bool IsAuthenticated { get; set; }
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        public string Token { get; set; }

      
    }
}
