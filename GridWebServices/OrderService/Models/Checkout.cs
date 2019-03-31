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
      public CheckoutSessionModel CheckoutSession { get; set; } 

    }

    public class CheckOutRequestDBUpdateModel
    {
        public string Source { get; set; }
        public int SourceID { get; set; }
        public string MPGSOrderID { get; set; }
        public string CheckOutSessionID { get; set; }

        public string SuccessIndicator { get; set; }

        public string CheckoutVersion { get; set; }

    }
    public class CheckOutResponseUpdate
    {      
        public string Token { get; set; }
        public string MPGSOrderID { get; set; }
        public string CheckOutSessionID { get; set; }
        public string Result { get; set; }

    }

    public enum MPGSAPIOperation
    {
        [EnumMember(Value = "CREATE_CHECKOUT_SESSION")]
        [Description("Create MPGS checkout session action")]
        CREATE_CHECKOUT_SESSION = 1,

        [EnumMember(Value = "RETRIEVE_ORDER")]
        [Description("MPGS Playment Order retrieve action")]
        RETRIEVE_ORDER = 2,

    }

    public enum MPGSAPIResponse
    {
        [EnumMember(Value = "SUCCESS")]
        [Description("SUCCESS")]
        SUCCESS = 1,

        [EnumMember(Value = "Unsuccessful")]
        [Description("The payment was unsuccessful")]
        Unsuccessful = 2,

        [EnumMember(Value = "Problem Completing Transaction")]
        [Description("There was a problem completing your transaction")]
        ProblemCompletingTransaction = 3,

        [EnumMember(Value = "Checkout receipt error")]
        [Description("Hosted checkout receipt error")]
        HostedCheckoutReceiptError = 4,

        [EnumMember(Value = "Checkout retrieve receipt")]
        [Description("Hosted checkout retrieve order response")]
        HostedCheckoutRetrieveReceipt = 5,

        [EnumMember(Value = "Checkout response")]
        [Description("Hosted checkout response")]
        HostedCheckoutResponse = 6,


        [EnumMember(Value = "Webhook Notification Folder Error")]
        [Description("Error Creating Webhook notification folder")]
        WebhookNotificationFolderError = 7,

        [EnumMember(Value = "Webhook Notification Folder Delete Error")]
        [Description("Error Deleting Webhook notification folder")]
        WebhookNotificationFolderDeleteError = 8,
    }
   

}
