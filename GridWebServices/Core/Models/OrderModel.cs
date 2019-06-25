using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Models
{

    public class OrderList
    {
        public int OrderID { get; set; }
        public string OrderNumber { get; set; }      
        public int ? OrderStatusNumber { get; set; }
        public string OrderStatus { get; set; }
        public string IDVerificationStatus { get; set; }
        public int ? IDVerificationStatusNumber { get; set; }
        public int ? RejectionCount { get; set; }
        public string Name { get; set; }
        public DateTime ? OrderDate { get; set; }
        public DateTime ? DeliveryDate { get; set; }
        public TimeSpan? DeliveryFromTime { get; set; }
        public TimeSpan? DeliveryToTime { get; set; }
        public string IdentityCardNumber { get; set; }
        public string IdentityCardType { get; set; }
    }

    public class OrderDetails : OrderList
    {        
        public DateTime? ExpiryDate { get; set; }
        public DateTime? DOB { get; set; }
        public string Nationality { get; set; }
        public string DocumentURL { get; set; }
        public string DocumentBackURL { get; set; }
        public string FrontImage { get; set; }
        public string BackImage { get; set; }
        public List<IDVerificaionHistory> VerificaionHistories { get; set; }
        
    }

    public class IDVerificaionHistory
    {
        public int VerificationLogID { get; set; }
        public int  OrderID { get; set; }
        public int IDVerificationStatusNumber { get; set; }
        public string IDVerificationStatus { get; set; }
        public string ChangeLog { get; set; }

        public string Remarks { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class NRICDetailsRequest
    {
        public int OrderID { get; set; }
        public string IDVerificationStatus { get; set; }
        public string IdentityCardNumber { get; set; }
        public string IdentityCardType { get; set; }
        public string Nationality { get; set; }
        public string NameInNRIC { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime? Expiry { get; set; }
        public string BackImage { get; set; }
        public string FrontImage { get; set; }
        public string Remarks { get; set; }       
        
    }

    public class NRICDetails
    {
        [Required(ErrorMessage = "OrderID is required")]
        public int OrderID { get; set; }
        [Required(ErrorMessage = "Status is required")]
        public string IDVerificationStatus { get; set; }
        public string IdentityCardNumber { get; set; }
        public string IdentityCardType { get; set; }
        public string Nationality { get; set; }
        public string NameInNRIC { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime? Expiry { get; set; }
        public IFormFile BackImage { get; set; }
        public IFormFile FrontImage { get; set; }
        public string Remarks { get; set; }        
       

    }

    public class EmailResponse
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public int VerificationStatus { get; set; }

    }

    public class VerificationRequestResponse
    {
        public int VerificationRequestID { get; set; }
        public int OrderID { get; set; }
        public string RequestToken { get; set; }
        public DateTime ? CreatedOn { get; set; }
        public int IsUsed { get; set; }

    }

    public class VerificationResponse
    {
        public int? CustomerID { get; set; }
        public int? OrderID { get; set; }
        public string OrderNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

    }
}
