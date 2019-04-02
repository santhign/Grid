using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CustomerService.Models
{
    /// <summary>
    /// Customer class
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        public int CustomerID { get; set; }
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email { get; set; }
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }
        /// <summary>
        /// Gets or sets the mobile number.
        /// </summary>
        /// <value>
        /// The mobile number.
        /// </value>
        public string MobileNumber { get; set; }
        /// <summary>
        /// Gets or sets the referral code.
        /// </summary>
        /// <value>
        /// The referral code.
        /// </value>
        public string ReferralCode { get; set; }
        /// <summary>
        /// Gets or sets the nationality.
        /// </summary>
        /// <value>
        /// The nationality.
        /// </value>
        public string Nationality { get; set; }
        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        /// <value>
        /// The gender.
        /// </value>
        public string Gender { get; set; }
        /// <summary>
        /// Gets or sets the SMS subscription.
        /// </summary>
        /// <value>
        /// The SMS subscription.
        /// </value>
        public string SMSSubscription { get; set; }
        /// <summary>
        /// Gets or sets the email subscription.
        /// </summary>
        /// <value>
        /// The email subscription.
        /// </value>
        public string EmailSubscription { get; set; }
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string Status { get; set; }
    }

    /// <summary>
    /// RegisterCustomer class
    /// </summary>
    public class RegisterCustomer
    {
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [Required(ErrorMessage = "Email address required")]       
        [EmailAddress(ErrorMessage = "Enter valid email address")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [Required(ErrorMessage = "Password required")]
        public string Password { get; set; }       
    }

    /// <summary>
    /// LoginDto class
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [Required(ErrorMessage = "Email address required")]
        [EmailAddress(ErrorMessage = "Enter valid email address")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [Required(ErrorMessage = "Password required")]
        public string Password { get; set; }
    }

    /// <summary>
    /// LoggedInPrinciple class
    /// </summary>
    public class LoggedInPrinciple
    {
        /// <summary>
        /// Gets or sets the customer.
        /// </summary>
        /// <value>
        /// The customer.
        /// </value>
        public Customer Customer { get; set; }
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

    /// <summary>
    /// AuthTokenResponse class
    /// </summary>
    public class AuthTokenResponse
    {
        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        public int CustomerID { get; set; }
        /// <summary>
        /// Gets or sets the created on.
        /// </summary>
        /// <value>
        /// The created on.
        /// </value>
        public DateTime CreatedOn { get; set; }
    }

    /// <summary>
    /// ValidateReferralCodeRequest class
    /// </summary>
    public class ValidateReferralCodeRequest
    {
        /// <summary>
        /// Gets or sets the referral code.
        /// </summary>
        /// <value>
        /// The referral code.
        /// </value>
        public string ReferralCode { get; set; }
    }

    /// <summary>
    /// ValidateReferralCodeResponse class
    /// </summary>
    public class ValidateReferralCodeResponse
    {
        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        public int CustomerID { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is referral code valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is referral code valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsReferralCodeValid { get; set; }
    }
    /// <summary>
    /// Subscriber class
    /// </summary>
    public class Subscriber
    {
        /// <summary>
        /// Gets or sets the mobile number.
        /// </summary>
        /// <value>
        /// The mobile number.
        /// </value>
        public string MobileNumber { get; set; }
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }
        /// <summary>
        /// Gets or sets the simid.
        /// </summary>
        /// <value>
        /// The simid.
        /// </value>
        public string SIMID { get; set; }
        /// <summary>
        /// Gets or sets the type of the premium.
        /// </summary>
        /// <value>
        /// The type of the premium.
        /// </value>
        public string PremiumType { get; set; }
        /// <summary>
        /// Gets or sets the activated on.
        /// </summary>
        /// <value>
        /// The activated on.
        /// </value>
        public DateTime ActivatedOn { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is primary.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is primary; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrimary { get; set; }
    }

    /// <summary>
    /// CustomerSearch class
    /// </summary>
    public class CustomerSearch
    {
        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        public int CustomerId { get; set; }
        /// <summary>
        /// Gets or sets the name of the customer.
        /// </summary>
        /// <value>
        /// The name of the customer.
        /// </value>
        public string CustomerName { get; set; }
        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>
        /// The phone number.
        /// </value>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// Gets or sets the plan.
        /// </summary>
        /// <value>
        /// The plan.
        /// </value>
        public string Plan { get; set; }
        /// <summary>
        /// Gets or sets the additional lines.
        /// </summary>
        /// <value>
        /// The additional lines.
        /// </value>
        public int AdditionalLines { get; set; }
        /// <summary>
        /// Gets or sets the joined on.
        /// </summary>
        /// <value>
        /// The joined on.
        /// </value>
        public DateTime JoinedOn { get; set; }
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string Status { get; set; }
    }

    /// <summary>
    /// CustomerPlans class
    /// </summary>
    public class CustomerPlans
    {
        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        public int CustomerID { get; set; }
        /// <summary>
        /// Gets or sets the plan identifier.
        /// </summary>
        /// <value>
        /// The plan identifier.
        /// </value>
        public int PlanId { get; set; }
        /// <summary>
        /// Gets or sets the type of the subscription.
        /// </summary>
        /// <value>
        /// The type of the subscription.
        /// </value>
        public string SubscriptionType { get; set; }

        /// <summary>
        /// Gets or sets the type of the PlanMarketingName.
        /// </summary>
        /// <value>
        /// The type of the PlanMarketingName.
        /// </value>
        public string PlanMarketingName { get; set; }
        /// <summary>
        /// Gets or sets the is recurring.
        /// </summary>
        /// <value>
        /// The is recurring.
        /// </value>
        public int IsRecurring { get; set; }
        /// <summary>
        /// Gets or sets the mobile number.
        /// </summary>
        /// <value>
        /// The mobile number.
        /// </value>
        public string MobileNumber { get; set; }
        /// <summary>
        /// Gets or sets the expiry date.
        /// </summary>
        /// <value>
        /// The expiry date.
        /// </value>
        public DateTime ? ExpiryDate { get; set; }
     

    }

}
