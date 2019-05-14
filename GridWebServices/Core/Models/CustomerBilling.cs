using System.ComponentModel.DataAnnotations;

namespace Core.Models
{
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
}
