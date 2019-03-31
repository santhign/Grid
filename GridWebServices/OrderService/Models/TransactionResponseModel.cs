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
        /// <param name="response">response from API
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
            model.OrderDescription = transactionList[0]["order"]["description"].ToObject<String>();
            model.TransactionID = transactionList[0]["authorizationResponse"]["transactionIdentifier"].ToObject<String>();
            model.CardNumber = transactionList[0]["sourceOfFunds"]["provided"]["card"]["number"].ToObject<String>();
            model.CardFundMethod = transactionList[0]["sourceOfFunds"]["provided"]["card"]["fundingMethod"].ToObject<String>();
            model.CardBrand = transactionList[0]["sourceOfFunds"]["provided"]["card"]["brand"].ToObject<String>();
            model.CardIssuer = transactionList[0]["sourceOfFunds"]["provided"]["card"]["scheme"].ToObject<String>();
            model.CardHolderName = transactionList[0]["sourceOfFunds"]["provided"]["card"]["nameOnCard"].ToObject<String>();
            model.ExpiryYear = transactionList[0]["sourceOfFunds"]["provided"]["card"]["expiry"]["year"].ToObject<int>();
            model.ExpiryMonth = transactionList[0]["sourceOfFunds"]["provided"]["card"]["expiry"]["month"].ToObject<int>();
            model.Token = transactionList[0]["3DSecure"]["authenticationToken"].ToObject<String>();
            model.PaymentStatus = transactionList[0]["order"]["status"].ToObject<String>();
            model.CustomerIP = transactionList[0]["device"]["ipAddress"].ToObject<String>();
            return model;
        }


        public static TransactionResponseModel fromMasterpassResponseToTransactionResponseModel(string response)
        {
            TransactionResponseModel model = new TransactionResponseModel();

            JObject jObject = JObject.Parse(response);
            model.GatewayCode = jObject["response"]["gatewayCode"].ToObject<String>();
            model.ApiResult = jObject["result"].ToObject<String>();
            model.OrderAmount = jObject["order"]["amount"].ToObject<String>();
            model.OrderCurrency = jObject["order"]["currency"].ToObject<String>();
            model.OrderId = jObject["order"]["id"].ToObject<String>();

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

}
