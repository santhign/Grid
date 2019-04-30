using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminService.Models
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
        /// Gets or sets the DOB.
        /// </summary>
        /// <value>
        /// The DOB.
        /// </value>
        public DateTime? DOB { get; set; }
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
        /// <summary>
        /// Gets or sets the joined on.
        /// </summary>
        /// <value>
        /// The joined on.
        /// </value>
        public DateTime JoinedOn { get; set; }
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
        /// Gets or sets the email of the customer.
        /// </summary>
        /// <value>
        /// The email of the customer.
        /// </value>
        public string Email { get; set; }
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

    public class Subscriber
    {
        public int SubscriberID { get; set; }
        public string MobileNumber { get; set; }
        public string DisplayName { get; set; }

    }
}
