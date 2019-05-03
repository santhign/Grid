using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace OrderService.Models
{
    public class TransactionResponseModel
    {
        public string ApiResult { get; set; }

        public string GatewayCode { get; set; }

        public string OrderAmount { get; set; }

        public string OrderCurrency { get; set; }

        public string OrderId { get; set; }

        public string OrderDescription { get; set; }

        public string TransactionID { get; set; }

        public string CardNumber { get; set; }

        public string CardFundMethod { get; set; }

        public string CardBrand { get; set; }

        public string CardType { get; set; }

        public string CardIssuer { get; set; }

        public string CardHolderName { get; set; }

        public int ExpiryYear { get; set; }

        public int ExpiryMonth { get; set; }

        public string Token { get; set; }

        public string PaymentStatus { get; set; }

        public int PaymentMethodSubscription { get; set; }

        public string CustomerIP { get; set; }

        /// <summary>
        /// Parses JSON response from Hosted/Browser Checkout transaction into TransactionResponse object
        /// </summary>
        /// <param name="response">response from API</param>
        /// <returns>TransactionResponseModel</returns>
        public static TransactionResponseModel toTransactionResponseModel(string response)
        {
            TransactionResponseModel model = new TransactionResponseModel();

            JObject jObject = JObject.Parse(response);
            var transactionList = jObject["transaction"];
            model.GatewayCode = transactionList[0]["response"]["gatewayCode"].ToObject<String>();
            model.ApiResult = transactionList[0]["result"].ToObject<String>();
            model.OrderAmount = transactionList[0]["order"]["amount"].ToObject<String>();
            model.OrderCurrency = transactionList[0]["order"]["currency"].ToObject<String>();
            model.OrderId = transactionList[0]["order"]["id"].ToObject<String>();
            model.OrderDescription = transactionList[0]["order"]["description"] != null ? transactionList[0]["order"]["description"].ToObject<String>() : null;
            model.TransactionID = transactionList[0]["authorizationResponse"]["transactionIdentifier"].ToObject<String>();
            model.CardNumber = transactionList[0]["sourceOfFunds"]["provided"]["card"]["number"].ToObject<String>();
            model.CardFundMethod = transactionList[0]["sourceOfFunds"]["provided"]["card"]["fundingMethod"].ToObject<String>();
            model.CardBrand = transactionList[0]["sourceOfFunds"]["provided"]["card"]["brand"].ToObject<String>();
            model.CardIssuer = transactionList[0]["sourceOfFunds"]["provided"]["card"]["scheme"].ToObject<String>();
            model.CardHolderName = transactionList[0]["sourceOfFunds"]["provided"]["card"]["nameOnCard"] != null ? transactionList[0]["sourceOfFunds"]["provided"]["card"]["nameOnCard"].ToObject<String>() : null;
            model.ExpiryYear = transactionList[0]["sourceOfFunds"]["provided"]["card"]["expiry"]["year"].ToObject<int>();
            model.ExpiryMonth = transactionList[0]["sourceOfFunds"]["provided"]["card"]["expiry"]["month"].ToObject<int>();
            model.Token = transactionList[0]["3DSecure"] != null ? (transactionList[0]["3DSecure"]["authenticationToken"] != null ? transactionList[0]["3DSecure"]["authenticationToken"].ToObject<String>() : null) : null;
            model.PaymentStatus = transactionList[0]["order"]["status"] != null ? transactionList[0]["order"]["status"].ToObject<String>() : null;
            model.CustomerIP = transactionList[0]["device"] != null ? (transactionList[0]["device"]["ipAddress"] != null ? transactionList[0]["device"]["ipAddress"].ToObject<String>() : null) : null;
            return model;
        }


    }
    public class TransactionRetrieveResponseOperation
    {
        public string RequestId { get; set; }
        public string Cause { get; set; }
        public string Message { get; set; }

        public TransactionResponseModel TrasactionResponse { get; set; }

    }

    public class Order_RescheduleDeliveryRequest
    {
        public int OrderID { get; set; }
        public string ShippingContactNumber { get; set; }
        public string ShippingFloor { get; set; }
        public string ShippingUnit { get; set; }
        public string ShippingBuildingName { get; set; }
        public string ShippingBuildingNumber { get; set; }
        public string ShippingStreetName { get; set; }

        public string ShippingPostCode { get; set; }
        public string AlternateRecipientName { get; set; }
        public string AlternateRecipientEmail { get; set; }
        public string AlternateRecipientContact { get; set; }
        public string AlternateRecioientIDNumber { get; set; }
        public string AlternateRecioientIDType { get; set; }
        public string PortalSlotID { get; set; }
        public DateTime? ScheduledDate { get; set; }

    }

    public class Order_RescheduleDeliveryResponse
    {
        public int OrderID { get; set; }
        public double? PayableAmount { get; set; }
    }
}
