using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    public class Order
    {

    }

    public class CreateOrder
    {
        [Required(ErrorMessage = "CustomerID is required")]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "BundleID is required")]
        public int BundleID { get; set; }

        [Required(ErrorMessage = "ReferralCode is required")]
        public string ReferralCode { get; set; }
    }
}
