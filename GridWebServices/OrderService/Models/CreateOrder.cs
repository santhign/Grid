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

}
