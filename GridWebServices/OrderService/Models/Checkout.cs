﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Core.Models;


namespace OrderService.Models
{
    public class CheckoutSessionModel
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string SuccessIndicator { get; set; }

        public static CheckoutSessionModel toCheckoutSessionModel(string response)
        {
           JObject jObject = JObject.Parse(response);
            CheckoutSessionModel model = jObject["session"].ToObject<CheckoutSessionModel>();
            model.SuccessIndicator = jObject["successIndicator"] != null ? jObject["successIndicator"].ToString() : "";
            return model;

        }
    }
    public class Checkout
    {
      public string CheckoutJsUrl { get; set; }
      public string OrderId { get; set; }
      public double Amount { get; set; }
      public string Currency { get; set; }
      public string MerchantId { get; set; }
      public CheckoutSessionModel CheckoutSession { get; set; }
      public string TransactionID { get; set; }
        

    }

    public class TokenSession
    {
        public string CheckOutSessionID { get; set; }
        public string MPGSOrderID { get; set; }
        public double Amount { get; set; }
        public string Token { get; set; }
        public string SourceOfFundType { get; set; }

    }

    public class PaymentMethod
    {
        public int PaymentMethodID { get; set; }       
        public string Token { get; set; }
        public string SourceType { get; set; }
        public string CardHolderName { get; set; }
        
    }
    public class CheckOutRequestDBUpdateModel
    {
        public string Source { get; set; }
        public int SourceID { get; set; }
        public string MPGSOrderID { get; set; }
        public string CheckOutSessionID { get; set; }

        public string SuccessIndicator { get; set; }

        public string CheckoutVersion { get; set; }

        public string TransactionID { get; set; }        

    }
    public class CheckOutResponseUpdate
    {
        public string MPGSOrderID { get; set; }
        public string CheckOutSessionID { get; set; }
        public string Result { get; set; }

    }

    public class BuddyCheckList
    {
        public int CustomerID { get; set; }
        public int OrderID { get; set; }
        public int OrderSubscriberID { get; set; }
        public string MobileNumber { get; set; }
        public int HasBuddyPromotion { get; set; }
        public bool IsProcessed { get; set; }

    }
    public class BuddyNumberUpdate
    {
        public int OrderSubscriberID { get; set; }
        public string UserId { get; set; }
        public string NewMobileNumber { get; set; }

    }
}
