using System;
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
        public string MerchantName { get; set; }
        public string MerchantAddressLine1 { get; set; }
        public string MerchantAddressLine2 { get; set; }
        public string MerchantEmail { get; set; }
        public string MerchantLogo { get; set; }
        public CheckoutSessionModel CheckoutSession { get; set; }
        public string TransactionID { get; set; }
        public string ReceiptNumber { get; set; }
        public string OrderNumber { get; set; }      

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
        public string CardType { get; set; }

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

    

    public class CustomerDetails
    {
        public string Name{ get; set; }  
        public string DeliveryEmail { get; set; }
        public string ToEmailList { get; set; }
        public string OrderNumber { get; set; }
        public string ReferralCode { get; set; }

        public string ShippingUnit { get; set; }

        public string ShippingFloor { get; set; }

        public string ShippingBuildingNumber { get; set; }

        public string ShippingBuildingName { get; set; }

        public string ShippingStreetName { get; set; }

        public string ShippingPostCode { get; set; }

        public string ShippingContactNumber { get; set; }

        public string AlternateRecipientContact { get; set; }

        public string AlternateRecipientName { get; set; }

        public string AlternateRecipientEmail { get; set; }
        public DateTime SlotDate { get; set; }
        public TimeSpan SlotFromTime { get; set; }
        public TimeSpan SlotToTime { get; set; }
        public List<OrderNumber> OrderedNumbers { get; set; }

    }
    public class OrderNumber
    {
        public string MobileNumber { get; set; }
        public int IsBuddyLine { get; set; }
        public string PlanMarketingName { get; set; }
        public string PricingDescription { get; set; }
        public double ApplicableSubscriptionFee { get; set; }    
        
    }
    
}
