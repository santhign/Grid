﻿using System;
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
        public int IsBuddyLine { get; set; }
        public int GroupNumber { get; set; }
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
        public string UserCode { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class CreateOrderRequest
    {
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

        /// <summary>
        /// Gets or sets the user code.
        /// </summary>
        /// <value>
        /// The user code.
        /// </value>
        public string UserCode { get; set; }

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
        [MaxLength(8, ErrorMessage = "Maximum 8 digits allowed")]
        [MinLength(8, ErrorMessage = "Minimum 8 digits Required")]
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
        [RegularExpression(@"^([0-9]{8})$", ErrorMessage = "Invalid Mobile Number")]
        [MaxLength(8, ErrorMessage = "Maximum 8 digits allowed")]
        [MinLength(8, ErrorMessage = "Minimum 8 digits Required")]
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
        public string DisplayName { get; set; }

    }


    /// <summary>
    /// 
    /// </summary>
    public class UpdateSubscriberPortingNumberRequest
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
        [RegularExpression(@"^([0-9]{8})$", ErrorMessage = "Invalid Mobile Number")]
        [MaxLength(8, ErrorMessage = "Maximum 8 digits allowed")]
        [MinLength(8, ErrorMessage = "Minimum 8 digits Required")]
        [Required(ErrorMessage = "OldMobileNumber is required")]
        public string OldMobileNumber { get; set; }

        /// <summary>
        /// Creates new mobilenumber.
        /// </summary>
        /// <value>
        /// The new mobile number.
        /// </value>
       [RegularExpression(@"^([0-9]{8})$", ErrorMessage = "Invalid Mobile Number")]
        [MaxLength(8, ErrorMessage = "Maximum 8 digits allowed")]
        [MinLength(8, ErrorMessage = "Minimum 8 digits Required")]
        [Required(ErrorMessage = "New MobileNumber is required")]
        public string NewMobileNumber { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
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
        /// 
        [RegularExpression(@"^([0-9]{8})$", ErrorMessage = "Invalid Mobile Number")]
        [MaxLength(8, ErrorMessage = "Maximum 8 digits allowed")]
        [MinLength(8, ErrorMessage = "Minimum 8 digits Required")]
        [Required(ErrorMessage = "OldMobileNumber is required")]
        public string OldMobileNumber { get; set; }

        /// <summary>
        /// Creates new mobilenumber.
        /// </summary>
        /// <value>
        /// The new mobile number.
        /// </value>
        [RegularExpression(@"^([0-9]{8})$", ErrorMessage = "Invalid Mobile Number")]
        [MaxLength(8, ErrorMessage = "Maximum 8 digits allowed")]
        [MinLength(8, ErrorMessage = "Minimum 8 digits Required")]
        [Required(ErrorMessage = "New MobileNumber is required")]
        public string NewMobileNumber { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
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
    public class AdditionalSubscriberRequest
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
    }

    /// <summary>
    /// 
    /// </summary>
    public class UpdateOrderPersonalDetailsRequest
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
        /// Gets or sets the name in nric.
        /// </summary>
        /// <value>
        /// The name in nric.
        /// </value>
        [Required(ErrorMessage = "NameInNRIC is required")]
        public string NameInNRIC { get; set; }
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        //[Required(ErrorMessage = "Display Name is required")]
        public string DisplayName { get; set; }

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
        [DisplayFormat()]
        public DateTime DOB { get; set; }

        /// <summary>
        /// Gets or sets the contact number.
        /// </summary>
        /// <value>
        /// The contact number.
        /// </value>
       
        public string ContactNumber { get; set; }
       
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
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }
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

    public class IDResponse
    {
        public string IDNumber { get; set; }
        public string IDFrontImageUrl { get; set; }
        public string IDBackImageUrl { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UpdateOrderBillingDetailsRequest
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
        public string Unit { get; set; }

        /// <summary>
        /// Gets or sets the floor.
        /// </summary>
        /// <value>
        /// The floor.
        /// </value>
        public string Floor { get; set; }

        /// <summary>
        /// Gets or sets the name of the building.
        /// </summary>
        /// <value>
        /// The name of the building.
        /// </value>
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
        [RegularExpression(@"^([0-9]{8})$", ErrorMessage = "Invalid Mobile Number")]
        [MaxLength(8, ErrorMessage = "Maximum 8 digits allowed")]
        [MinLength(8, ErrorMessage = "Minimum 8 digits Required")]
        [Required(ErrorMessage = "ContactNumber is required")]
        public string ContactNumber { get; set; }       

    }

    /// <summary>
    /// 
    /// </summary>
    public class UpdateOrderShippingDetailsRequest 
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
        public string Unit { get; set; }

        /// <summary>
        /// Gets or sets the floor.
        /// </summary>
        /// <value>
        /// The floor.
        /// </value>
        public string Floor { get; set; }

        /// <summary>
        /// Gets or sets the name of the building.
        /// </summary>
        /// <value>
        /// The name of the building.
        /// </value>
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
        [RegularExpression(@"^([0-9]{8})$", ErrorMessage = "Invalid Mobile Number")]
        [MaxLength(8, ErrorMessage = "Maximum 8 digits allowed")]
        [MinLength(8, ErrorMessage = "Minimum 8 digits Required")]
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
        /// 
        [RegularExpression(@"^([0-9]{8})$", ErrorMessage = "Invalid Mobile Number")]
        [MaxLength(8, ErrorMessage = "Maximum 8 digits allowed")]
        [MinLength(8, ErrorMessage = "Minimum 8 digits Required")]
        [Required(ErrorMessage = "ContactNumber is required")]
        public string ContactNumber { get; set; }

        /// <summary>
        /// Gets or sets the email adddress.
        /// </summary>
        /// <value>
        /// The email adddress.
        /// </value>
       
        
        public string EmailAdddress { get; set; }
      
    }

    /// <summary>
    /// 
    /// </summary>
    public class ValidateOrderReferralCodeRequest
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
     
        [MaxLength(8, ErrorMessage = "Maximum 8 digits allowed")]
        [MinLength(8, ErrorMessage = "Minimum 8 digits Required")]
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
        public int EmailMessage { get; set; }
        public int SMSMessage { get; set; }
        public int VoiceMessage { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RemoveAdditionalLineRequest
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
        /// Gets or sets the mobile number.
        /// </summary>
        /// <value>
        /// The mobile number.
        /// </value>
        [RegularExpression(@"^([0-9]{8})$", ErrorMessage = "Invalid Mobile Number")]
        [MaxLength(8, ErrorMessage = "Maximum 8 digits allowed")]
        [MinLength(8, ErrorMessage = "Minimum 8 digits Required")]
        [Required(ErrorMessage = "Mobile Number is required")]
        public string MobileNumber { get; set; }
        
    }

    /// <summary>
    /// 
    /// </summary>
    public class AssignNewNumberRequest
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
        /// Gets or sets the old number.
        /// </summary>
        /// <value>
        /// The old number.
        /// </value>
        [RegularExpression(@"^([0-9]{8})$", ErrorMessage = "Invalid Mobile Number")]
        [MaxLength(8, ErrorMessage = "Maximum 8 digits allowed")]
        [MinLength(8, ErrorMessage = "Minimum 8 digits Required")]
        [Required(ErrorMessage = "OldNumber Number is required")]
        public string OldNumber { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class AssignNewNumber
    {
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

    public class BSSRequestMore
    {
        public int Type { get; set; }

    }

    public class OrderNRICDetails
    {     
      
        public int CustomerID { get; set; }       
        public int DocumentID { get; set; }               
        public string DocumentURL { get; set; }

        public string DocumentBackURL { get; set; }

        public string IdentityCardNumber { get; set; }

        public string IdentityCardType { get; set; }

        public string Nationality { get; set; }

    }

    public class TokenizeRequest
    {
        public int OrderId { get; set; }
        public int OrderType { get; set; }

    }
    public class OrderSource
    {
        public string SourceType { get; set; }

        public int SourceID { get; set; }

    }

    public class OrderCount
    {     
        public int SuccessfulOrders { get; set; }

    }

    public class UpdateSubscriberBasicDetails
    {
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "BundleID is required")]
        public int BundleID { get; set; }

        [Required(ErrorMessage = "DisplayName is required")]
        public string DisplayName { get; set; }

        [RegularExpression(@"^([0-9]{8})$", ErrorMessage = "Invalid Mobile Number")]
        [MaxLength(8, ErrorMessage = "Maximum 8 digits allowed")]
        [MinLength(8, ErrorMessage = "Minimum 8 digits Required")]       
        [Required(ErrorMessage = "MobileNumber is required")]
        public string MobileNumber { get; set; }

    }

    public class CreateBuddySubscriber
    {       
        public int OrderID { get; set; }
        public string UserId { get; set; }        
        public string MobileNumber { get; set; }      
        public string MainLineMobileNumber { get; set; }
    }

    public class BuddyToRemove
    {
        public int BuddyRemovalID { get; set; }    
        public string MobileNumber { get; set; }
        public int? IsPorted { get; set; }
        public int? IsRemoved { get; set; }      
    }

    public class AdditionalBuddy
    {
        public int OrderAdditionalBuddyID { get; set; }
        public string MobileNumber { get; set; }
        public int? IsProcessed { get; set; }
        public int? IsPorted { get; set; }

        

    }

    public class VasAddRemoveRequest
    {
        public int OrderID { get; set; }
        public int BundleID { get; set; }
        public string MobileNumber { get; set; }
        public int? IsRemove { get; set; }
    }

    public class VasToProcess
    {
        public int OrderSubscriberVASBundleID { get; set; }
        public int BundleID { get; set; }
        public string MobileNumber { get; set; }        
    }
    
}
