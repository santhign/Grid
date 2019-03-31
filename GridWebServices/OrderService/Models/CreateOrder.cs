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
    public class OrderInit
    {
        public int OrderID { get; set; }
        public string Status { get; set; }

    }

    public class OrderBasicDetails
    {
        public int OrderID { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderSubscription> OrderSubscriptions { get; set; }
    }

    public class OrderSubscription
    {
        public int BundleID { get; set; }
        public string MobileNumber { get; set; }
        public string DisplayName { get; set; }
    }

    public class CreateOrder
    {
        [Required(ErrorMessage = "CustomerID is required")]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "BundleID is required")]
        public int BundleID { get; set; }

        public string ReferralCode { get; set; }

        public string PromotionCode { get; set; }

    }

    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; }

        [Required(ErrorMessage = "BundleID is required")]
        public int BundleID { get; set; }
        public string ReferralCode { get; set; }

        public string PromotionCode { get; set; }

    }

    public class CreateSubscriber
    {
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "BundleID is required")]
        public int BundleID { get; set; }

        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "MobileNumber is required")]
        public string MobileNumber { get; set; }
        public string PromotionCode { get; set; }
    }



    public class UpdateSubscriberNumber
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; }

        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "OldMobileNumber is required")]
        public string OldMobileNumber { get; set; }
        public NewNumber NewNumber { get; set; }

        [Required(ErrorMessage = "DisplayName is required")]
        public string DisplayName { get; set; }

    }


    public class UpdateSubscriberPortingNumberRequest
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; }
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "OldMobileNumber is required")]
        public string OldMobileNumber { get; set; }

        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "New MobileNumber is required")]
        public string NewMobileNumber { get; set; }

        [Required(ErrorMessage = "DisplayName is required")]
        public string DisplayName { get; set; }

        [Required(ErrorMessage = "IsOwnNumber is required")]
        public int IsOwnNumber { get; set; }

        [Required(ErrorMessage = "Donor Provider is required")]
        public string DonorProvider { get; set; }
        public IFormFile PortedNumberTransferForm { get; set; }
        public string PortedNumberOwnedBy { get; set; }
        public string PortedNumberOwnerRegistrationID { get; set; }

    }

    public class UpdateSubscriberPortingNumber
    {
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "OldMobileNumber is required")]
        public string OldMobileNumber { get; set; }

        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "New MobileNumber is required")]
        public string NewMobileNumber { get; set; }

        [Required(ErrorMessage = "DisplayName is required")]
        public string DisplayName { get; set; }

        [Required(ErrorMessage = "IsOwnNumber is required")]
        public int IsOwnNumber { get; set; }

        [Required(ErrorMessage = "Donor Provider is required")]
        public string DonorProvider { get; set; }
        public string PortedNumberTransferForm { get; set; }
        public string PortedNumberOwnedBy { get; set; }
        public string PortedNumberOwnerRegistrationID { get; set; }

    }
    public class AuthTokenResponse
    {
        public int CustomerID { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class AdditionalSubscriberRequest
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; }

        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "BundleID is required")]
        public int BundleID { get; set; }
    }

    public class UpdateOrderPersonalDetailsRequest
    {
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }

        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }
        
        [Required(ErrorMessage = "IDType is required")]
        public string IDType { get; set; }

        [Required(ErrorMessage = "IDNumber is required")]
        public string IDNumber { get; set; }

        [Required(ErrorMessage = "ID Image is required")]
        public IFormFile ID { get; set; }

        [Required(ErrorMessage = "NameInNRIC is required")]
        public string NameInNRIC { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }
        
        [Required(ErrorMessage = "DOB is required")]
        public DateTime DOB { get; set; }

        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "ContactNumber is required")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Nationality is required")]
        public string Nationality { get; set; }
    }

    public class UpdateOrderPersonalDetails
    {        
        public int OrderID { get; set; }        
        public string IDType { get; set; }      
        public string IDNumber { get; set; }      
        public string IDImageUrl { get; set; }       
        public string NameInNRIC { get; set; }      
        public string Gender { get; set; }        
        public DateTime DOB { get; set; }       
        public string ContactNumber { get; set; }       
        public string Nationality { get; set; }

    }

    public class UpdateOrderBillingDetailsRequest
    {
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }

        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "Postcode is required")]
        public string Postcode { get; set; }

        [Required(ErrorMessage = "BlockNumber is required")]
        public string BlockNumber { get; set; }

        [Required(ErrorMessage = "Unit is required")]
        public string Unit { get; set; }

        [Required(ErrorMessage = "Floor is required")]
        public string Floor { get; set; }

        [Required(ErrorMessage = "BuildingName is required")]
        public string BuildingName { get; set; }

        [Required(ErrorMessage = "StreetName is required")]
        public string StreetName { get; set; }

        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "ContactNumber is required")]
        public string ContactNumber { get; set; }       

    }

    public class UpdateOrderShippingDetailsRequest 
    {
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }

        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "Postcode is required")]
        public string Postcode { get; set; }

        [Required(ErrorMessage = "BlockNumber is required")]
        public string BlockNumber { get; set; }

        [Required(ErrorMessage = "Unit is required")]
        public string Unit { get; set; }

        [Required(ErrorMessage = "Floor is required")]
        public string Floor { get; set; }

        [Required(ErrorMessage = "BuildingName is required")]
        public string BuildingName { get; set; }

        [Required(ErrorMessage = "StreetName is required")]
        public string StreetName { get; set; }

        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "ContactNumber is required")]
        public string ContactNumber { get; set; }
        [Required(ErrorMessage = "IsBillingSame required")]
        public int IsBillingSame { get; set; }

        [Required(ErrorMessage = "PortalSlotID is required")]
        public string PortalSlotID { get; set; }     

    }

    public class UpdateOrderLOADetailsRequest
    {
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }

        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "RecipientName is required")]
        public string RecipientName { get; set; }

        [Required(ErrorMessage = "IDType is required")]
        public string IDType { get; set; }

        [Required(ErrorMessage = "IDNumber is required")]
        public string IDNumber { get; set; }

        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "ContactNumber is required")]
        public string ContactNumber { get; set; }

        [EmailAddress(ErrorMessage ="Enter Valid Email Address")]
        [Required(ErrorMessage = "EmailAdddress is required")]
        public string EmailAdddress { get; set; }
      
    }

    public class ValidateOrderReferralCodeRequest
    {
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }

        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "RecipientName is required")]
        public string ReferralCode { get; set; }

    }

    public class OrderedNumberRequest
    {
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }

        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }      

    }

    public class UpdateOrderSubcriptionDetailsRequest
    {
        [Required(ErrorMessage = "Token required")]
        public string Token { get; set; }

        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }
        
        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]      
        public string ContactNumber { get; set; }
       
        [Required(ErrorMessage = "Terms is required")]
        public int Terms { get; set; }

        [Required(ErrorMessage = "PaymentSubscription is required")]
        public int PaymentSubscription { get; set; }

        [Required(ErrorMessage = "PromotionMessage is required")]
        public int PromotionMessage { get; set; }
    }
}
