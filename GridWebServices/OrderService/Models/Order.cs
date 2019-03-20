using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public class CreateSubscriber
    {
        [Required(ErrorMessage = "@OrderID is required")]
        public int @OrderID { get; set; }

        [Required(ErrorMessage = "BundleID is required")]
        public int @BundleID { get; set; }

        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "MobileNumber is required")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "IsPrimary is required")]
        public int @IsPrimary { get; set; }
        public string PromotionCode { get; set; }
    }

    public class ServiceFees
    {       
        public int ServiceCode { get; set; }
        public double ServiceFee { get; set; }

    }

    public class AuthTokenResponse
    {
        public int CustomerID { get; set; }
        public DateTime CreatedOn { get; set; }
    }


}
