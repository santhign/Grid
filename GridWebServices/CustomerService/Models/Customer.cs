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
        /// <summary>
        /// Gets or sets the identity card number.
        /// </summary>
        /// <value>
        /// The identity card number.
        /// </value>
        public string IdentityCardNumber { get; set; }
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
        /// Gets or sets the dob.
        /// </summary>
        /// <value>
        /// The dob.
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
        /// <summary>
        /// Gets paid order count.
        /// </summary>
        /// <value>
        /// The number of paid orders if greater then 0 then redirect to dashboard.
        /// </value>
        public int OrderCount { get; set; }

        /// <summary>
        /// Gets or sets the pending allowed subscribers.
        /// </summary>
        /// <value>
        /// The pending allowed subscribers.
        /// </value>
        public int PendingAllowedSubscribers { get; set; }

        public string BillingAccountNumber { get; set; }
        public int PendingSIMCount { get; set; }

        public int? PendingSIMOrderID { get; set; }
        public string Token { get; set; }
    }

    /// <summary>
    /// Customer Profile class
    /// </summary>
    public class CustomerProfile
    {
        /// <summary>
        /// Gets or sets the old password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        /// 
        public string OldPassword { get; set; }

        /// <summary>
        /// Gets or sets the new password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        /// 
        public string NewPassword { get; set; }

        /// <summary>
        /// Gets or sets the mobile number.
        /// </summary>
        /// <value>
        /// The mobile number.
        /// </value>
        [MaxLength(8, ErrorMessage = "Maximum 8 digits allowed")]
        [MinLength(8, ErrorMessage = "Minimum 8 digits Required")]
        [Required(ErrorMessage = "MobileNumber is required")]
        public string MobileNumber { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [Required(ErrorMessage = "Email address required")]
        [EmailAddress(ErrorMessage = "Enter valid email address")]
        public string Email { get; set; }

    }

    /// <summary>
    /// Change Phone Request
    /// </summary>
    public class ChangePhoneRequest
    {
        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        public int ? CustomerId { get; set; }
        /// <summary>
        /// Gets or sets the mobile number.
        /// </summary>
        /// <value>
        /// The mobile number.
        /// </value>
        public string MobileNumber { get; set; }
        /// <summary>
        /// Creates new mobile number.
        /// </summary>
        /// <value>
        /// The new mobile number.
        /// </value>
        public string NewMobileNumber { get; set; }
        /// <summary>
        /// Gets or sets the type of the premium.
        /// </summary>
        /// <value>
        /// The type of the premium.
        /// </value>
        public int PremiumType { get; set; }
        /// <summary>
        /// Gets or sets the ported number transfer form.
        /// </summary>
        /// <value>
        /// The ported number transfer form.
        /// </value>
        public string PortedNumberTransferForm { get; set; }
        /// <summary>
        /// Gets or sets the ported number owned by.
        /// </summary>
        /// <value>
        /// The ported number owned by.
        /// </value>
        public string PortedNumberOwnedBy { get; set; }
        /// <summary>
        /// Gets or sets the ported number owner registration identifier.
        /// </summary>
        /// <value>
        /// The ported number owner registration identifier.
        /// </value>
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

    /// <summary>
    /// 
    /// </summary>
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
        public DateTime? ActivatedOn { get; set; }
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
        /// <summary>
        /// Gets or sets the suspension raised.
        /// </summary>
        /// <value>
        /// The suspension raised.
        /// </value>
        public int SuspensionRaised { get; set; }

        /// <summary>
        /// Gets or sets the unsuspension raised.
        /// </summary>
        /// <value>
        /// The unsuspension raised.
        /// </value>
        public int UnsuspensionRaised { get; set; }
        /// <summary>
        /// Gets or sets the termination raised.
        /// </summary>
        /// <value>
        /// The termination raised.
        /// </value>
        public int TerminationRaised { get; set; }
        /// <summary>
        /// Gets or sets the plan change raised.
        /// </summary>
        /// <value>
        /// The plan change raised.
        /// </value>
        public int PlanChangeRaised { get; set; }
        /// <summary>
        /// Gets or sets the plan change allowed.
        /// </summary>
        /// <value>
        /// The plan change allowed.
        /// </value>
        public int PlanChangeAllowed { get; set; }
        /// <summary>
        /// Gets or sets the plan change message.
        /// </summary>
        /// <value>
        /// The plan change message.
        /// </value>
        public string PlanChangeMessage { get; set; }
        /// <summary>
        /// Gets or sets the sim replacement raised.
        /// </summary>
        /// <value>
        /// The sim replacement raised.
        /// </value>
        public int SIMReplacementRaised { get; set; }
        /// <summary>
        /// Gets or sets the SMS subscription.
        /// </summary>
        /// <value>
        /// The SMS subscription.
        /// </value>
        public int SMSSubscription { get; set; }
        /// <summary>
        /// Gets or sets the voice subscription.
        /// </summary>
        /// <value>
        /// The voice subscription.
        /// </value>
        public int VoiceSubscription { get; set; }
        public int SuspensionAllowed { get; set; }
        public int UnsuspensionAllowed { get; set; }
        public int TerminationAllowed { get; set; }
        public int SIMReplacementAllowed { get; set; }
        public int ChangeNumberRaised { get; set; }
        public int SubscriberID { get; set; }
        public int IsBuddyLine { get; set; }
        public int GroupNumber { get; set; }

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
        /// Gets or sets the subscription identifier.
        /// </summary>
        /// <value>
        /// The subscription identifier.
        /// </value>
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

        /// <summary>
        /// Gets or sets the subscription date.
        /// </summary>
        /// <value>
        /// The subscription date.
        /// </value>
        public DateTime? SubscriptionDate { get; set; }

        /// <summary>
        /// Gets or sets the subscription fee.
        /// </summary>
        /// <value>
        /// The subscription fee.
        /// </value>
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


        [RegularExpression(@"[a-zA-Z0-9_\s]+", ErrorMessage = "Only letters and digits are allowed")]
        [MaxLength(20, ErrorMessage = "Maximum 20 characters allowed")]
        [MinLength(6, ErrorMessage = "Minimum 6 characters required")]
        [Required(ErrorMessage = "ReferralCode is required")]
        public string ReferralCode { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class customerPaymentMethod
    {
        /// <summary>
        /// Gets or sets the name of the card holder.
        /// </summary>
        /// <value>
        /// The name of the card holder.
        /// </value>
        public string CardHolderName { get; set; }
        /// <summary>
        /// Gets or sets the masked card numer.
        /// </summary>
        /// <value>
        /// The masked card numer.
        /// </value>
        public string MaskedCardNumer { get; set; }
        /// <summary>
        /// Gets or sets the type of the card.
        /// </summary>
        /// <value>
        /// The type of the card.
        /// </value>
        public string CardType { get; set; }
        /// <summary>
        /// Gets or sets the is default.
        /// </summary>
        /// <value>
        /// The is default.
        /// </value>
        public int IsDefault { get; set; }
        /// <summary>
        /// Gets or sets the expiry month.
        /// </summary>
        /// <value>
        /// The expiry month.
        /// </value>
        public int ExpiryMonth { get; set; }
        /// <summary>
        /// Gets or sets the expiry year.
        /// </summary>
        /// <value>
        /// The expiry year.
        /// </value>
        public int ExpiryYear { get; set; }
        /// <summary>
        /// Gets or sets the card fund method.
        /// </summary>
        /// <value>
        /// The card fund method.
        /// </value>
        public string CardFundMethod { get; set; }
        /// <summary>
        /// Gets or sets the card issuer.
        /// </summary>
        /// <value>
        /// The card issuer.
        /// </value>
        public string CardIssuer { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BasePlans
    {
        /// <summary>
        /// Gets or sets the bundle identifier.
        /// </summary>
        /// <value>
        /// The bundle identifier.
        /// </value>
        public int BundleID { get; set; }
        /// <summary>
        /// Gets or sets the name of the plan marketing.
        /// </summary>
        /// <value>
        /// The name of the plan marketing.
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
        /// Gets or sets the mobile number.
        /// </summary>
        /// <value>
        /// The mobile number.
        /// </value>
        public string MobileNumber { get; set; }
        /// <summary>
        /// Gets or sets the TotalData.
        /// </summary>
        /// <value>
        /// The TotalData.
        /// </value>
        public double TotalData { get; set; }
        /// <summary>
        /// Gets or sets the total SMS.
        /// </summary>
        /// <value>
        /// The total SMS.
        /// </value>
        public double TotalSMS { get; set; }
        /// <summary>
        /// Gets or sets the total voice.
        /// </summary>
        /// <value>
        /// The total voice.
        /// </value>
        public double TotalVoice { get; set; }
        /// <summary>
        /// Gets or sets the actual subscription fee.
        /// </summary>
        /// <value>
        /// The actual subscription fee.
        /// </value>
        public double ActualSubscriptionFee { get; set; }
        /// <summary>
        /// Gets or sets the applicable subscription fee.
        /// </summary>
        /// <value>
        /// The applicable subscription fee.
        /// </value>
        public double ApplicableSubscriptionFee { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public DateTime? SubscriptionDate { get; set; }

        public string PricingDescription { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DisplayDetails
    {
        /// <summary>
        /// Gets or sets the mobile number.
        /// </summary>
        /// <value>
        /// The mobile number.
        /// </value>
        [Required(ErrorMessage = "Mobile Number required")]
        public string MobileNumber { get; set; }
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        [Required(ErrorMessage = "Display name required")]
        public string DisplayName { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class customerShipping
    {
        /// <summary>
        /// Gets or sets the shipping unit.
        /// </summary>
        /// <value>
        /// The shipping unit.
        /// </value>
        public string ShippingUnit { get; set; }
        /// <summary>
        /// Gets or sets the shipping floor.
        /// </summary>
        /// <value>
        /// The shipping floor.
        /// </value>
        public string ShippingFloor { get; set; }
        /// <summary>
        /// Gets or sets the name of the shipping street.
        /// </summary>
        /// <value>
        /// The name of the shipping street.
        /// </value>
        [Required(ErrorMessage = "street name is required")]
        public string ShippingStreetName { get; set; }
        /// <summary>
        /// Gets or sets the shipping building number.
        /// </summary>
        /// <value>
        /// The shipping building number.
        /// </value>
        [Required(ErrorMessage = "building number is required")]
        public string ShippingBuildingNumber { get; set; }
        /// <summary>
        /// Gets or sets the name of the shipping building.
        /// </summary>
        /// <value>
        /// The name of the shipping building.
        /// </value>
        public string ShippingBuildingName { get; set; }
        /// <summary>
        /// Gets or sets the shipping contact number.
        /// </summary>
        /// <value>
        /// The shipping contact number.
        /// </value>
        [Required(ErrorMessage = "contact number is required")]
        public string ShippingContactNumber { get; set; }
        /// <summary>
        /// Gets or sets the shipping post code.
        /// </summary>
        /// <value>
        /// The shipping post code.
        /// </value>
        [Required(ErrorMessage = "postcode is required")]
        public string ShippingPostCode { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ChangeRequest
    {
        /// <summary>
        /// Gets or sets the change request identifier.
        /// </summary>
        /// <value>
        /// The change request identifier.
        /// </value>
        public int ChangeRequestID { get; set; }
        /// <summary>
        /// Gets or sets the subscriber identifier.
        /// </summary>
        /// <value>
        /// The subscriber identifier.
        /// </value>
        public int SubscriberID { get; set; }
        /// <summary>
        /// Gets or sets the mobile number.
        /// </summary>
        /// <value>
        /// The mobile number.
        /// </value>
        public string MobileNumber { get; set; }
        /// <summary>
        /// Gets or sets the listing status.
        /// </summary>
        /// <value>
        /// The listing status.
        /// </value>
        public string ListingStatus { get; set; }
        /// <summary>
        /// Gets or sets the order number.
        /// </summary>
        /// <value>
        /// The order number.
        /// </value>
        public string OrderNumber { get; set; }
        /// <summary>
        /// Gets or sets the type of the request.
        /// </summary>
        /// <value>
        /// The type of the request.
        /// </value>
        public string RequestType { get; set; }
        /// <summary>
        /// Gets or sets the order status.
        /// </summary>
        /// <value>
        /// The order status.
        /// </value>
        public string OrderStatus { get; set; }
        /// <summary>
        /// Gets or sets the request on.
        /// </summary>
        /// <value>
        /// The request on.
        /// </value>
        public DateTime RequestOn { get; set; }
        /// <summary>
        /// Gets or sets the billing unit.
        /// </summary>
        /// <value>
        /// The billing unit.
        /// </value>
        public string BillingUnit { get; set; }
        /// <summary>
        /// Gets or sets the billing floor.
        /// </summary>
        /// <value>
        /// The billing floor.
        /// </value>
        public string BillingFloor { get; set; }
        /// <summary>
        /// Gets or sets the billing building number.
        /// </summary>
        /// <value>
        /// The billing building number.
        /// </value>
        public string BillingBuildingNumber { get; set; }
        /// <summary>
        /// Gets or sets the name of the billing building.
        /// </summary>
        /// <value>
        /// The name of the billing building.
        /// </value>
        public string BillingBuildingName { get; set; }
        /// <summary>
        /// Gets or sets the name of the billing street.
        /// </summary>
        /// <value>
        /// The name of the billing street.
        /// </value>
        public string BillingStreetName { get; set; }
        /// <summary>
        /// Gets or sets the billing post code.
        /// </summary>
        /// <value>
        /// The billing post code.
        /// </value>
        public string BillingPostCode { get; set; }
        /// <summary>
        /// Gets or sets the billing contact number.
        /// </summary>
        /// <value>
        /// The billing contact number.
        /// </value>
        public string BillingContactNumber { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email { get; set; }
        /// <summary>
        /// Gets or sets the type of the identifier.
        /// </summary>
        /// <value>
        /// The type of the identifier.
        /// </value>
        public string IDType { get; set; }
        /// <summary>
        /// Gets or sets the identifier number.
        /// </summary>
        /// <value>
        /// The identifier number.
        /// </value>
        public string IDNumber { get; set; }
        /// <summary>
        /// Gets or sets the is same as billing.
        /// </summary>
        /// <value>
        /// The is same as billing.
        /// </value>
        public int? IsSameAsBilling { get; set; }
        /// <summary>
        /// Gets or sets the shipping unit.
        /// </summary>
        /// <value>
        /// The shipping unit.
        /// </value>
        public string ShippingUnit { get; set; }
        /// <summary>
        /// Gets or sets the shipping floor.
        /// </summary>
        /// <value>
        /// The shipping floor.
        /// </value>
        public string ShippingFloor { get; set; }
        /// <summary>
        /// Gets or sets the shipping building number.
        /// </summary>
        /// <value>
        /// The shipping building number.
        /// </value>
        public string ShippingBuildingNumber { get; set; }
        /// <summary>
        /// Gets or sets the name of the shipping building.
        /// </summary>
        /// <value>
        /// The name of the shipping building.
        /// </value>
        public string ShippingBuildingName { get; set; }
        /// <summary>
        /// Gets or sets the name of the shipping street.
        /// </summary>
        /// <value>
        /// The name of the shipping street.
        /// </value>
        public string ShippingStreetName { get; set; }
        /// <summary>
        /// Gets or sets the shipping post code.
        /// </summary>
        /// <value>
        /// The shipping post code.
        /// </value>
        public string ShippingPostCode { get; set; }
        /// <summary>
        /// Gets or sets the shipping contact number.
        /// </summary>
        /// <value>
        /// The shipping contact number.
        /// </value>
        public string ShippingContactNumber { get; set; }
        /// <summary>
        /// Gets or sets the alternate recipient contact.
        /// </summary>
        /// <value>
        /// The alternate recipient contact.
        /// </value>
        public string AlternateRecipientContact { get; set; }
        /// <summary>
        /// Gets or sets the name of the alternate recipient.
        /// </summary>
        /// <value>
        /// The name of the alternate recipient.
        /// </value>
        public string AlternateRecipientName { get; set; }
        /// <summary>
        /// Gets or sets the alternate recipient email.
        /// </summary>
        /// <value>
        /// The alternate recipient email.
        /// </value>
        public string AlternateRecipientEmail { get; set; }
        /// <summary>
        /// Gets or sets the portal slot identifier.
        /// </summary>
        /// <value>
        /// The portal slot identifier.
        /// </value>
        public string PortalSlotID { get; set; }
        /// <summary>
        /// Gets or sets the slot date.
        /// </summary>
        /// <value>
        /// The slot date.
        /// </value>
        public DateTime? SlotDate { get; set; }
        /// <summary>
        /// Gets or sets the slot from time.
        /// </summary>
        /// <value>
        /// The slot from time.
        /// </value>
        public TimeSpan? SlotFromTime { get; set; }
        /// <summary>
        /// Gets or sets the slot to time.
        /// </summary>
        /// <value>
        /// The slot to time.
        /// </value>
        public TimeSpan? SlotToTime { get; set; }
        /// <summary>
        /// Gets or sets the scheduled date.
        /// </summary>
        /// <value>
        /// The scheduled date.
        /// </value>
        public DateTime? ScheduledDate { get; set; }
        /// <summary>
        /// Gets or sets the service fee.
        /// </summary>
        /// <value>
        /// The service fee.
        /// </value>
        public double? ServiceFee { get; set; }
        /// <summary>
        /// Gets or sets the allow rescheduling.
        /// </summary>
        /// <value>
        /// The allow rescheduling.
        /// </value>
        public int AllowRescheduling { get; set; }
    }

    public class CustomerPurchasedVASes : CustomerPlans
    {
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime? StartDate { get; set; }
    }   
}
