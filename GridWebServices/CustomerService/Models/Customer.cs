using System;
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
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// The Name.
        /// </value>
        public string Name { get; set; }
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
        public string IdentityCardType { get; set; }
        public string IdentityCardNumber { get; set; }
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
        /// <summary>
        /// Gets paid order count.
        /// </summary>
        /// <value>
        /// The number of paid orders if greater then 0 then redirect to dashboard.
        /// </value>
        public int OrderCount { get; set; }
    }

    /// <summary>
    /// Customer Profile class
    /// </summary>
    public class CustomerProfile
    {
        public string Password { get; set; }

        [MaxLength(8, ErrorMessage = "Maximum 8 digits allowed")]
        [MinLength(8, ErrorMessage = "Minimum 8 digits Required")]
        [Required(ErrorMessage = "MobileNumber is required")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "Email address required")]
        [EmailAddress(ErrorMessage = "Enter valid email address")]
        public string Email { get; set; }

    }

    /// <summary>Change Phone Request</summary>
    public class ChangePhoneRequest
    {
        /// <summary>Gets or sets the customer identifier.</summary>
        /// <value>The customer identifier.</value>
        public int ? CustomerId { get; set; }
        /// <summary>Gets or sets the mobile number.</summary>
        /// <value>The mobile number.</value>
        public string MobileNumber { get; set; }
        /// <summary>Creates new mobile number.</summary>
        /// <value>The new mobile number.</value>
        public string NewMobileNumber { get; set; }
        /// <summary>Gets or sets the type of the premium.</summary>
        /// <value>The type of the premium.</value>
        public int PremiumType { get; set; }
        /// <summary>Gets or sets the ported number transfer form.</summary>
        /// <value>The ported number transfer form.</value>
        public string PortedNumberTransferForm { get; set; }
        /// <summary>Gets or sets the ported number owned by.</summary>
        /// <value>The ported number owned by.</value>
        public string PortedNumberOwnedBy { get; set; }
        /// <summary>Gets or sets the ported number owner registration identifier.</summary>
        /// <value>The ported number owner registration identifier.</value>
        public string PortedNumberOwnerRegistrationId { get; set; }

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

    public class ResetPassword
    {
        /// <summary>
        /// New Password
        /// </summary>
        /// <value>
        /// password
        /// </value>
        [Required(ErrorMessage = "NewPassword required")]       
        public string NewPassword { get; set; }
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [Required(ErrorMessage = "Token required")]
        public string ResetToken { get; set; }
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
        /// Gets or sets the linked  mobile number.
        /// </summary>
        /// <value>
        /// The linked mobile number.
        /// </value>
        public string LinkedMobileNumber { get; set; }

        /// <summary>
        /// Gets or sets the Linked Display Name.
        /// </summary>
        /// <value>
        /// The Linked Display Name.
        /// </value>
        public string LinkedDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the Account Type.
        /// </summary>
        /// <value>
        /// The Account Type.
        /// </value>
        public string AccountType { get; set; }

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
        public int PremiumType { get; set; }
        /// <summary>
        /// Gets or sets the activated on.
        /// </summary>
        /// <value>
        /// The activated on.
        /// </value>
        public DateTime ? ActivatedOn { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is primary.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is primary; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrimary { get; set; }
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public string State { get; set; }
        public int SuspensionRaised { get; set; }
        public int TerminationRaised { get; set; }
        public int PlanChangeRaised { get; set; }
        public string PlanChangeMessage { get; set; }
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

        public int SubscriptionID { get; set; }
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
        /// Gets or sets the status of the plan.
        /// </summary>
        /// <value>
        /// The status of the subscription.
        /// </value>
        public string PlanStatus { get; set; }
        /// <summary>
        /// Gets or sets the type of the PlanMarketingName.
        /// </summary>
        /// <value>
        /// The type of the PlanMarketingName.
        /// </value>
        public string PlanMarketingName { get; set; }
        /// <summary>
        /// Gets or sets the type of the PortalSummaryDescription.
        /// </summary>
        /// <value>
        /// The type of the PortalSummaryDescription.
        /// </value>
        public string PortalSummaryDescription { get; set; }
        /// <summary>
        /// Gets or sets the type of the PortalDescription.
        /// </summary>
        /// <value>
        /// The type of the PortalDescription.
        /// </value>
        public string PortalDescription { get; set; }
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
        /// <summary>
        /// Gets removable flag.
        /// </summary>
        /// <value>
        /// 1=Yes, 0=N0.
        /// </value>
        public int Removable { get; set; }

        public DateTime? SubscriptionDate { get; set; }

        public double? SubscriptionFee { get; set; }
    }


    /// <summary>
    /// CustomerNewReferralCode class
    /// </summary>
    public class CustomerNewReferralCode
    {
        /// <summary>
        /// Gets or sets the ReferralCode.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string ReferralCode { get; set; }
    }

    public class customerBilling
    {
        public string Name { get; set; }
        public string BillingUnit { get; set; }
        public string BillingFloor { get; set; }
        [Required(ErrorMessage = "street name is required")]
        public string BillingStreetName { get; set; }
        [Required(ErrorMessage = "building number is required")]
        public string BillingBuildingNumber { get; set; }
        public string BillingBuildingName { get; set; }
        [Required(ErrorMessage = "contact number is required")]
        public string BillingContactNumber { get; set; }
        [Required(ErrorMessage = "postcode is required")]
        public string BillingPostCode { get; set; }
    }

    public class customerPaymentMethod
    {
        public string CardHolderName { get; set; }
        public string MaskedCardNumer { get; set; }
        public string CardType { get; set; }
        public int IsDefault { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string CardFundMethod { get; set; }
        public string CardIssuer { get; set; }
    }
    
    public class BasePlans
    {
        public int BundleID { get; set; }
        public string PlanMarketingName { get; set; }
        /// <summary>
        /// Gets or sets the type of the PortalSummaryDescription.
        /// </summary>
        /// <value>
        /// The type of the PortalSummaryDescription.
        /// </value>
        public string PortalSummaryDescription { get; set; }
        /// <summary>
        /// Gets or sets the type of the PortalDescription.
        /// </summary>
        /// <value>
        /// The type of the PortalDescription.
        /// </value>
        public string PortalDescription { get; set; }
        public string MobileNumber { get; set; }
        /// <summary>
        /// Gets or sets the TotalData.
        /// </summary>
        /// <value>
        /// The TotalData.
        /// </value>
        public double TotalData { get; set; }
        public double TotalSMS { get; set; }
        public double TotalVoice { get; set; }
        public double ActualSubscriptionFee { get; set; }
        public double ApplicableSubscriptionFee { get; set; }
    }

    public class DisplayDetails
    {
        [Required(ErrorMessage = "Mobile Number required")]
        public string MobileNumber { get; set; }
        [Required(ErrorMessage = "Display name required")]
        public string DisplayName { get; set; }
    }
    public class customerShipping
    {
        public string ShippingUnit { get; set; }
        public string ShippingFloor { get; set; }
        [Required(ErrorMessage = "street name is required")]
        public string ShippingStreetName { get; set; }
        [Required(ErrorMessage = "building number is required")]
        public string ShippingBuildingNumber { get; set; }
        public string ShippingBuildingName { get; set; }
        [Required(ErrorMessage = "contact number is required")]
        public string ShippingContactNumber { get; set; }
        [Required(ErrorMessage = "postcode is required")]
        public string ShippingPostCode { get; set; }
    }
}
