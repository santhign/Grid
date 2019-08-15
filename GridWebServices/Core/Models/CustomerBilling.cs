using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System;

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

    public class UpdateOrderPersonalIDDetailsRequest
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
        /// Gets or sets the nationality.
        /// </summary>
        /// <value>
        /// The nationality.
        /// </value>
        [Required(ErrorMessage = "Nationality is required")]
        public string Nationality { get; set; }

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


        public IFormFile IDImageFront { get; set; }


        public IFormFile IDImageBack { get; set; }
    }

    public class UpdateOrderPersonalIDDetailsRequest_base64
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
        /// Gets or sets the nationality.
        /// </summary>
        /// <value>
        /// The nationality.
        /// </value>
        [Required(ErrorMessage = "Nationality is required")]
        public string Nationality { get; set; }

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
        public string IDImageFront { get; set; }

        [Required(ErrorMessage = "IDImageBack Image is required")]
        public string IDImageBack { get; set; }
    }


    public class UpdateOrderPersonalIDDetailsPublicRequest
    {
        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "CustomerID is required")]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "IDImageFront Image is required")]
        public IFormFile IDImageFront { get; set; }

        [Required(ErrorMessage = "IDImageBack Image is required")]
        public IFormFile IDImageBack { get; set; }

        [Required(ErrorMessage = "RequestToken is required")]
        public string RequestToken { get; set; }
    }
    public class OrderCustomer
    {
        public int CustomerId { get; set; }
    }

    public class IDImageReUploadDetails
    {
        public int OrderID { get; set; }
        public int VerificationStatus { get; set; }
        public string IDType { get; set; }
        public string IDNumber { get; set; }
        public string Nationality { get; set; }
        public string NameInNRIC { get; set; }
        public DateTime DOB { get; set; }
        public DateTime Expiry { get; set; }
        public string BackImage { get; set; }
        public string FrontImage { get; set; }
        public string Remarks { get; set; }
        public int AdminUserID { get; set; }
    }

}
