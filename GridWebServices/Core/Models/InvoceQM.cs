using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
   public class InvoceQM
    {
        public int accountID { get; set; }
        public int customerID { get; set; }
        public int subscriberID { get; set; }
        public string mobilenumber { get; set; }
        public string MaskedCardNumber { get; set; }
        public string Token { get; set; }
        public string CardType { get; set; }
        public string IsDefault { get; set; }
        public string CardHolderName { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string CardFundMethod { get; set; }
        public string CardBrand { get; set; }
        public string CardIssuer { get; set; }
        public string billingUnit { get; set; }
        public string billingFloor { get; set; }
        public string billingBuildingNumber { get; set; }
        public string billingBuildingName { get; set; }
        public string billingStreetName { get; set; }
        public string billingPostCode { get; set; }
        public string billingContactNumber { get; set; }
        public string email { get; set; }
        public string displayname { get; set; }
        public string paymentmode { get; set; }
        public float amountpaid { get; set; }
        public string MPGSOrderID { get; set; }
        public Recordset invoicelist { get; set; }
        public float invoiceamounts { get; set; }

    }
}
