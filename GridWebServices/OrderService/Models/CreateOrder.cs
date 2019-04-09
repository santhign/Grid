using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using Core.Models;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    /// <summary>
    /// Order Init class
    /// </summary>
    public class OrderInit
    {
        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        public int OrderID { get; set; }
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string Status { get; set; }

    }

    /// <summary>
    /// Order Basic Details class
    /// </summary>
    public class OrderBasicDetails
    {
        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        public int OrderID { get; set; }
        /// <summary>
        /// Gets or sets the order number.
        /// </summary>
        /// <value>
        /// The order number.
        /// </value>
        public string OrderNumber { get; set; }
        /// <summary>
        /// Gets or sets the order date.
        /// </summary>
        /// <value>
        /// The order date.
        /// </value>
        public DateTime OrderDate { get; set; }
        /// <summary>
        /// Gets or sets the order subscriptions.
        /// </summary>
        /// <value>
        /// The order subscriptions.
        /// </value>
        public List<OrderSubscription> OrderSubscriptions { get; set; }
    }


    /// <summary>Response class for Change Sim</summary>
    public class ChangeSimResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminationOrSuspensionResponse" /> class.
        /// </summary>
        public ChangeSimResponse()
        {
            ChangeRequestChargesList = new List<ChangeRequestCharges>();
        }
        /// <summary>
        /// Gets or sets the change request identifier.
        /// </summary>
        /// <value>
        /// The change request identifier.
        /// </value>
        public int ChangeRequestId { get; set; }
        /// <summary>
        /// Gets or sets the order number.
        /// </summary>
        /// <value>
        /// The order number.
        /// </value>
        public string OrderNumber { get; set; }
        /// <summary>
        /// Gets or sets the request on.
        /// </summary>
        /// <value>
        /// The request on.
        /// </value>
        public DateTime RequestOn { get; set; }
        /// <summary>
        /// Gets or sets the request type description.
        /// </summary>
        /// <value>
        /// The request type description.
        /// </value>
        public string RequestTypeDescription { get; set; }
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
        /// Gets or sets the type of the identity card.
        /// </summary>
        /// <value>
        /// The type of the identity card.
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
        //public string IDType { get; set; }
        //public string IDNumber { get; set; }
        /// <summary>
        /// Gets or sets the is same as billing.
        /// </summary>
        /// <value>
        /// The is same as billing.
        /// </value>
        public string IsSameAsBilling { get; set; }
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
        /// Gets or sets the alternate.
        /// </summary>
        /// <value>
        /// The alternate.
        /// </value>
        public string Alternate { get; set; }
        /// <summary>
        /// Gets or sets the recipient email.
        /// </summary>
        /// <value>
        /// The recipient email.
        /// </value>
        public string RecipientEmail { get; set; }
        /// <summary>
        /// Gets or sets the portal slot identifier.
        /// </summary>
        /// <value>
        /// The portal slot identifier.
        /// </value>
        public int PortalSlotId { get; set; }
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
        public DateTime? SlotFromTime { get; set; }
        /// <summary>
        /// Gets or sets the slot to time.
        /// </summary>
        /// <value>
        /// The slot to time.
        /// </value>
        public DateTime ? SlotToTime { get; set; }
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
        public string ServiceFee { get; set; }

        /// <summary>
        /// The change request charges list
        /// </summary>
        public IList<ChangeRequestCharges> ChangeRequestChargesList;
    }


    /// <summary>
    /// Termination and Suspension Response
    /// </summary>
    public class TerminationOrSuspensionResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminationOrSuspensionResponse" /> class.
        /// </summary>
        public TerminationOrSuspensionResponse()
        {
            ChangeRequestChargesList = new List<ChangeRequestCharges>();
        }
        /// <summary>
        /// Gets or sets the change request identifier.
        /// </summary>
        /// <value>
        /// The change request identifier.
        /// </value>
        public int ChangeRequestId { get; set; }
        /// <summary>
        /// Gets or sets the order number.
        /// </summary>
        /// <value>
        /// The order number.
        /// </value>
        public string OrderNumber { get; set; }
        /// <summary>
        /// Gets or sets the request on.
        /// </summary>
        /// <value>
        /// The request on.
        /// </value>
        public DateTime RequestOn { get; set; }
        /// <summary>
        /// Gets or sets the request type description.
        /// </summary>
        /// <value>
        /// The request type description.
        /// </value>
        public string RequestTypeDescription { get; set; }

        /// <summary>
        /// The change request charges list
        /// </summary>
        public IList<ChangeRequestCharges> ChangeRequestChargesList;
    }

    /// <summary>
    /// 
    /// </summary>
    public class ChangeRequestCharges
    {
        /// <summary>
        /// Gets or sets the name of the portal service.
        /// </summary>
        /// <value>
        /// The name of the portal service.
        /// </value>
        public string PortalServiceName { get; set; }
        /// <summary>
        /// Gets or sets the service fee.
        /// </summary>
        /// <value>
        /// The service fee.
        /// </value>
        public double ServiceFee { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is recurring.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is recurring; otherwise, <c>false</c>.
        /// </value>
        public bool IsRecurring { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is GST included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is GST included; otherwise, <c>false</c>.
        /// </value>
        public bool IsGstIncluded { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class OrderSubscription
    {
        /// <summary>
        /// Gets or sets the bundle identifier.
        /// </summary>
        /// <value>
        /// The bundle identifier.
        /// </value>
        public int BundleID { get; set; }
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
    }

    /// <summary>
    /// 
    /// </summary>
    public class CreateOrder
    {
        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        [Required(ErrorMessage = "CustomerID is required")]
        public int CustomerID { get; set; }

        /// <summary>
        /// Gets or sets the bundle identifier.
        /// </summary>
        /// <value>
        /// The bundle identifier.
        /// </value>
        [Required(ErrorMessage = "BundleID is required")]
        public int BundleID { get; set; }

        /// <summary>
        /// Gets or sets the referral code.
        /// </summary>
        /// <value>
        /// The referral code.
        /// </value>
        public string ReferralCode { get; set; }

        /// <summary>
        /// Gets or sets the promotion code.
        /// </summary>
        /// <value>
        /// The promotion code.
        /// </value>
        public string PromotionCode { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class CreateOrderRequest
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the bundle identifier.
        /// </summary>
        /// <value>
        /// The bundle identifier.
        /// </value>
        [Required(ErrorMessage = "BundleID is required")]
        public int BundleID { get; set; }
        /// <summary>
        /// Gets or sets the referral code.
        /// </summary>
        /// <value>
        /// The referral code.
        /// </value>
        public string ReferralCode { get; set; }

        /// <summary>
        /// Gets or sets the promotion code.
        /// </summary>
        /// <value>
        /// The promotion code.
        /// </value>
        public string PromotionCode { get; set; }

    }



    /// <summary>
    /// 
    /// </summary>
    public class CreateSubscriber
    {
        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        /// <summary>
        /// Gets or sets the bundle identifier.
        /// </summary>
        /// <value>
        /// The bundle identifier.
        /// </value>
        [Required(ErrorMessage = "BundleID is required")]
        public int BundleID { get; set; }

        /// <summary>
        /// Gets or sets the mobile number.
        /// </summary>
        /// <value>
        /// The mobile number.
        /// </value>
        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "MobileNumber is required")]
        public string MobileNumber { get; set; }
        /// <summary>
        /// Gets or sets the promotion code.
        /// </summary>
        /// <value>
        /// The promotion code.
        /// </value>
        public string PromotionCode { get; set; }
    }



    /// <summary>
    /// 
    /// </summary>
    public class UpdateSubscriberNumber
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        /// <summary>
        /// Gets or sets the old mobile number.
        /// </summary>
        /// <value>
        /// The old mobile number.
        /// </value>
        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "OldMobileNumber is required")]
        public string OldMobileNumber { get; set; }
        /// <summary>
        /// Creates new number.
        /// </summary>
        /// <value>
        /// The new number.
        /// </value>
        public NewNumber NewNumber { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        [Required(ErrorMessage = "DisplayName is required")]
        public string DisplayName { get; set; }

    }


    /// <summary>
    /// 
    /// </summary>
    public class UpdateSubscriberPortingNumberRequest
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; }
        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        /// <summary>
        /// Gets or sets the old mobile number.
        /// </summary>
        /// <value>
        /// The old mobile number.
        /// </value>
        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "OldMobileNumber is required")]
        public string OldMobileNumber { get; set; }

        /// <summary>
        /// Creates new mobilenumber.
        /// </summary>
        /// <value>
        /// The new mobile number.
        /// </value>
        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "New MobileNumber is required")]
        public string NewMobileNumber { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        [Required(ErrorMessage = "DisplayName is required")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the is own number.
        /// </summary>
        /// <value>
        /// The is own number.
        /// </value>
        [Required(ErrorMessage = "IsOwnNumber is required")]
        public int IsOwnNumber { get; set; }

        /// <summary>
        /// Gets or sets the donor provider.
        /// </summary>
        /// <value>
        /// The donor provider.
        /// </value>
        [Required(ErrorMessage = "Donor Provider is required")]
        public string DonorProvider { get; set; }
        /// <summary>
        /// Gets or sets the ported number transfer form.
        /// </summary>
        /// <value>
        /// The ported number transfer form.
        /// </value>
        public IFormFile PortedNumberTransferForm { get; set; }
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
        public string PortedNumberOwnerRegistrationID { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class UpdateSubscriberPortingNumber
    {
        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        /// <summary>
        /// Gets or sets the old mobile number.
        /// </summary>
        /// <value>
        /// The old mobile number.
        /// </value>
        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "OldMobileNumber is required")]
        public string OldMobileNumber { get; set; }

        /// <summary>
        /// Creates new mobilenumber.
        /// </summary>
        /// <value>
        /// The new mobile number.
        /// </value>
        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "New MobileNumber is required")]
        public string NewMobileNumber { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        [Required(ErrorMessage = "DisplayName is required")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the is own number.
        /// </summary>
        /// <value>
        /// The is own number.
        /// </value>
        [Required(ErrorMessage = "IsOwnNumber is required")]
        public int IsOwnNumber { get; set; }

        /// <summary>
        /// Gets or sets the donor provider.
        /// </summary>
        /// <value>
        /// The donor provider.
        /// </value>
        [Required(ErrorMessage = "Donor Provider is required")]
        public string DonorProvider { get; set; }
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
        public string PortedNumberOwnerRegistrationID { get; set; }

    }
    /// <summary>
    /// 
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
    /// 
    /// </summary>
    public class AdditionalSubscriberRequest
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        /// <summary>
        /// Gets or sets the bundle identifier.
        /// </summary>
        /// <value>
        /// The bundle identifier.
        /// </value>
        [Required(ErrorMessage = "BundleID is required")]
        public int BundleID { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UpdateOrderPersonalDetailsRequest
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        /// <summary>
        /// Gets or sets the type of the identifier.
        /// </summary>
        /// <value>
        /// The type of the identifier.
        /// </value>
        [Required(ErrorMessage = "IDType is required")]
        public string IDType { get; set; }

        /// <summary>
        /// Gets or sets the identifier number.
        /// </summary>
        /// <value>
        /// The identifier number.
        /// </value>
        [Required(ErrorMessage = "IDNumber is required")]
        public string IDNumber { get; set; }

        [Required(ErrorMessage = "IDImageFront Image is required")]
        public IFormFile IDImageFront { get; set; }

        [Required(ErrorMessage = "IDImageBack Image is required")]
        public IFormFile IDImageBack { get; set; }
        /// <summary>
        /// Gets or sets the name in nric.
        /// </summary>
        /// <value>
        /// The name in nric.
        /// </value>
        [Required(ErrorMessage = "NameInNRIC is required")]
        public string NameInNRIC { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        /// <value>
        /// The gender.
        /// </value>
        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets the dob.
        /// </summary>
        /// <value>
        /// The dob.
        /// </value>
        [Required(ErrorMessage = "DOB is required")]
        public DateTime DOB { get; set; }

        /// <summary>
        /// Gets or sets the contact number.
        /// </summary>
        /// <value>
        /// The contact number.
        /// </value>
        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "ContactNumber is required")]
        public string ContactNumber { get; set; }

        /// <summary>
        /// Gets or sets the nationality.
        /// </summary>
        /// <value>
        /// The nationality.
        /// </value>
        [Required(ErrorMessage = "Nationality is required")]
        public string Nationality { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UpdateOrderPersonalDetails
    {
        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        public int OrderID { get; set; }
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
        /// Gets or sets the identifier image URL.
        /// </summary>
        /// <value>
        /// The identifier image URL.
        /// </value>
        public string IDFrontImageUrl { get; set; }
        public string IDBackImageUrl { get; set; }
        /// <summary>
        /// Gets or sets the name in nric.
        /// </summary>
        /// <value>
        /// The name in nric.
        /// </value>
        public string NameInNRIC { get; set; }
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
        public DateTime DOB { get; set; }
        /// <summary>
        /// Gets or sets the contact number.
        /// </summary>
        /// <value>
        /// The contact number.
        /// </value>
        public string ContactNumber { get; set; }
        /// <summary>
        /// Gets or sets the nationality.
        /// </summary>
        /// <value>
        /// The nationality.
        /// </value>
        public string Nationality { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class UpdateOrderBillingDetailsRequest
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        /// <summary>
        /// Gets or sets the postcode.
        /// </summary>
        /// <value>
        /// The postcode.
        /// </value>
        [Required(ErrorMessage = "Postcode is required")]
        public string Postcode { get; set; }

        /// <summary>
        /// Gets or sets the block number.
        /// </summary>
        /// <value>
        /// The block number.
        /// </value>
        [Required(ErrorMessage = "BlockNumber is required")]
        public string BlockNumber { get; set; }

        /// <summary>
        /// Gets or sets the unit.
        /// </summary>
        /// <value>
        /// The unit.
        /// </value>
        [Required(ErrorMessage = "Unit is required")]
        public string Unit { get; set; }

        /// <summary>
        /// Gets or sets the floor.
        /// </summary>
        /// <value>
        /// The floor.
        /// </value>
        [Required(ErrorMessage = "Floor is required")]
        public string Floor { get; set; }

        /// <summary>
        /// Gets or sets the name of the building.
        /// </summary>
        /// <value>
        /// The name of the building.
        /// </value>
        [Required(ErrorMessage = "BuildingName is required")]
        public string BuildingName { get; set; }

        /// <summary>
        /// Gets or sets the name of the street.
        /// </summary>
        /// <value>
        /// The name of the street.
        /// </value>
        [Required(ErrorMessage = "StreetName is required")]
        public string StreetName { get; set; }

        /// <summary>
        /// Gets or sets the contact number.
        /// </summary>
        /// <value>
        /// The contact number.
        /// </value>
        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "ContactNumber is required")]
        public string ContactNumber { get; set; }       

    }

    /// <summary>
    /// 
    /// </summary>
    public class UpdateOrderShippingDetailsRequest 
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        /// <summary>
        /// Gets or sets the postcode.
        /// </summary>
        /// <value>
        /// The postcode.
        /// </value>
        [Required(ErrorMessage = "Postcode is required")]
        public string Postcode { get; set; }

        /// <summary>
        /// Gets or sets the block number.
        /// </summary>
        /// <value>
        /// The block number.
        /// </value>
        [Required(ErrorMessage = "BlockNumber is required")]
        public string BlockNumber { get; set; }

        /// <summary>
        /// Gets or sets the unit.
        /// </summary>
        /// <value>
        /// The unit.
        /// </value>
        [Required(ErrorMessage = "Unit is required")]
        public string Unit { get; set; }

        /// <summary>
        /// Gets or sets the floor.
        /// </summary>
        /// <value>
        /// The floor.
        /// </value>
        [Required(ErrorMessage = "Floor is required")]
        public string Floor { get; set; }

        /// <summary>
        /// Gets or sets the name of the building.
        /// </summary>
        /// <value>
        /// The name of the building.
        /// </value>
        [Required(ErrorMessage = "BuildingName is required")]
        public string BuildingName { get; set; }

        /// <summary>
        /// Gets or sets the name of the street.
        /// </summary>
        /// <value>
        /// The name of the street.
        /// </value>
        [Required(ErrorMessage = "StreetName is required")]
        public string StreetName { get; set; }

        /// <summary>
        /// Gets or sets the contact number.
        /// </summary>
        /// <value>
        /// The contact number.
        /// </value>
        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "ContactNumber is required")]
        public string ContactNumber { get; set; }
        /// <summary>
        /// Gets or sets the is billing same.
        /// </summary>
        /// <value>
        /// The is billing same.
        /// </value>
        [Required(ErrorMessage = "IsBillingSame required")]
        public int IsBillingSame { get; set; }

        /// <summary>
        /// Gets or sets the portal slot identifier.
        /// </summary>
        /// <value>
        /// The portal slot identifier.
        /// </value>
        [Required(ErrorMessage = "PortalSlotID is required")]
        public string PortalSlotID { get; set; }     

    }

    /// <summary>
    /// 
    /// </summary>
    public class UpdateOrderLOADetailsRequest
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        /// <summary>
        /// Gets or sets the name of the recipient.
        /// </summary>
        /// <value>
        /// The name of the recipient.
        /// </value>
        [Required(ErrorMessage = "RecipientName is required")]
        public string RecipientName { get; set; }

        /// <summary>
        /// Gets or sets the type of the identifier.
        /// </summary>
        /// <value>
        /// The type of the identifier.
        /// </value>
        [Required(ErrorMessage = "IDType is required")]
        public string IDType { get; set; }

        /// <summary>
        /// Gets or sets the identifier number.
        /// </summary>
        /// <value>
        /// The identifier number.
        /// </value>
        [Required(ErrorMessage = "IDNumber is required")]
        public string IDNumber { get; set; }

        /// <summary>
        /// Gets or sets the contact number.
        /// </summary>
        /// <value>
        /// The contact number.
        /// </value>
        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "ContactNumber is required")]
        public string ContactNumber { get; set; }

        /// <summary>
        /// Gets or sets the email adddress.
        /// </summary>
        /// <value>
        /// The email adddress.
        /// </value>
        [EmailAddress(ErrorMessage ="Enter Valid Email Address")]
        [Required(ErrorMessage = "EmailAdddress is required")]
        public string EmailAdddress { get; set; }
      
    }

    /// <summary>
    /// 
    /// </summary>
    public class ValidateOrderReferralCodeRequest
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        /// <summary>
        /// Gets or sets the referral code.
        /// </summary>
        /// <value>
        /// The referral code.
        /// </value>
        [Required(ErrorMessage = "RecipientName is required")]
        public string ReferralCode { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class OrderedNumberRequest
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }      

    }

    /// <summary>
    /// 
    /// </summary>
    public class UpdateOrderSubcriptionDetailsRequest
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        /// <summary>
        /// Gets or sets the contact number.
        /// </summary>
        /// <value>
        /// The contact number.
        /// </value>
        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]      
        public string ContactNumber { get; set; }

        /// <summary>
        /// Gets or sets the terms.
        /// </summary>
        /// <value>
        /// The terms.
        /// </value>
        [Required(ErrorMessage = "Terms is required")]
        public int Terms { get; set; }

        /// <summary>
        /// Gets or sets the payment subscription.
        /// </summary>
        /// <value>
        /// The payment subscription.
        /// </value>
        [Required(ErrorMessage = "PaymentSubscription is required")]
        public int PaymentSubscription { get; set; }

        /// <summary>
        /// Gets or sets the promotion message.
        /// </summary>
        /// <value>
        /// The promotion message.
        /// </value>
        [Required(ErrorMessage = "PromotionMessage is required")]
        public int PromotionMessage { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RemoveAdditionalLineRequest
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        /// <summary>
        /// Gets or sets the mobile number.
        /// </summary>
        /// <value>
        /// The mobile number.
        /// </value>
        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed in Mobile Number")]
        [Required(ErrorMessage = "Mobile Number is required")]
        public string MobileNumber { get; set; }
        
    }

    /// <summary>
    /// 
    /// </summary>
    public class AssignNewNumberRequest
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        /// <summary>
        /// Gets or sets the old number.
        /// </summary>
        /// <value>
        /// The old number.
        /// </value>
        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed in Mobile Number")]
        [Required(ErrorMessage = "Mobile Number is required")]
        public string OldNumber { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class AssignNewNumber
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        public string Token { get; set; }
        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        public int OrderID { get; set; }
        /// <summary>
        /// Gets or sets the old number.
        /// </summary>
        /// <value>
        /// The old number.
        /// </value>
        public string OldNumber { get; set; }
        /// <summary>
        /// Creates new number.
        /// </summary>
        /// <value>
        /// The new number.
        /// </value>
        public string NewNumber { get; set; }

    }
    /// <summary>
    /// 
    /// </summary>
    public class CustomerBSSInvoiceRequest
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime EndDate { get; set; }

    }
    public class BSSRequestAssets
    {
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }

    }
    public class BSSRequestMore
    {
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }

        public int Type { get; set; }

    }
}
