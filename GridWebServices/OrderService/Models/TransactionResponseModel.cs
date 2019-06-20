using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using OrderService.Enums;

namespace OrderService.Models.Transaction
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
            RootObject transactionReceipt = new RootObject();

            TransactionResponseModel model = new TransactionResponseModel();

            transactionReceipt = JsonConvert.DeserializeObject<RootObject>(response);

            foreach (Transaction trans in transactionReceipt.transaction)
            {
                if(trans.result=="SUCCESS" && trans.transaction.type== "CAPTURE" && trans.order.status== MPGSAPIResponse.CAPTURED.ToString() && trans.response.gatewayCode== "APPROVED")
                {
                    model.GatewayCode = trans.response.gatewayCode;
                    model.ApiResult = trans.result;
                    model.OrderAmount = trans.order.amount.ToString();
                    model.OrderCurrency = trans.order.currency;
                    model.OrderId = trans.order.id;
                    model.OrderDescription = trans.order.description;
                    model.TransactionID = trans.authorizationResponse.transactionIdentifier != null ? trans.authorizationResponse.transactionIdentifier : null;
                    model.CardNumber = trans.sourceOfFunds.provided.card.number!= null ? trans.sourceOfFunds.provided.card.number : null;
                    model.CardFundMethod = trans.sourceOfFunds.provided.card.fundingMethod != null ? trans.sourceOfFunds.provided.card.fundingMethod : null;
                    model.CardBrand = trans.sourceOfFunds.provided.card.brand != null ? trans.sourceOfFunds.provided.card.brand : null;
                    model.CardIssuer = trans.sourceOfFunds.provided.card.scheme != null ? trans.sourceOfFunds.provided.card.scheme : null;
                    model.CardHolderName = trans.sourceOfFunds.provided.card.nameOnCard != null ? trans.sourceOfFunds.provided.card.nameOnCard : null;
                    model.ExpiryYear = int.Parse( trans.sourceOfFunds.provided.card.expiry.year);
                    model.ExpiryMonth = int.Parse( trans.sourceOfFunds.provided.card.expiry.month);
                    model.Token = trans.sourceOfFunds.token;
                    model.PaymentStatus = trans.order.status;
                    model.CustomerIP = trans.device != null ? trans.device.ipAddress != null ?trans.device.ipAddress : null : null;
                }
            }

            //JObject jObject = JObject.Parse(response);
            //var transactionList = jObject["transaction"];
            //model.GatewayCode = transactionList[0]["response"]["gatewayCode"].ToObject<String>();
            //model.ApiResult = transactionList[0]["result"].ToObject<String>();
            //model.OrderAmount = transactionList[0]["order"]["amount"].ToObject<String>();
            //model.OrderCurrency = transactionList[0]["order"]["currency"].ToObject<String>();
            //model.OrderId = transactionList[0]["order"]["id"].ToObject<String>();
            //model.OrderDescription = transactionList[0]["order"]["description"]!=null? transactionList[0]["order"]["description"].ToObject<String>():null;
            //model.TransactionID = transactionList[0]["authorizationResponse"]["transactionIdentifier"]!=null? transactionList[0]["authorizationResponse"]["transactionIdentifier"].ToObject<String>():null;
            //model.CardNumber = transactionList[0]["sourceOfFunds"]["provided"]["card"]["number"]!=null? transactionList[0]["sourceOfFunds"]["provided"]["card"]["number"].ToObject<String>():null;
            //model.CardFundMethod = transactionList[0]["sourceOfFunds"]["provided"]["card"]["fundingMethod"]!=null? transactionList[0]["sourceOfFunds"]["provided"]["card"]["fundingMethod"].ToObject<String>():null;
            //model.CardBrand = transactionList[0]["sourceOfFunds"]["provided"]["card"]["brand"]!=null? transactionList[0]["sourceOfFunds"]["provided"]["card"]["brand"].ToObject<String>():null;
            //model.CardIssuer = transactionList[0]["sourceOfFunds"]["provided"]["card"]["scheme"]!=null?transactionList[0]["sourceOfFunds"]["provided"]["card"]["scheme"].ToObject<String>():null;
            //model.CardHolderName = transactionList[0]["sourceOfFunds"]["provided"]["card"]["nameOnCard"]!=null? transactionList[0]["sourceOfFunds"]["provided"]["card"]["nameOnCard"].ToObject<String>():null;
            //model.ExpiryYear = transactionList[0]["sourceOfFunds"]["provided"]["card"]["expiry"]["year"].ToObject<int>();
            //model.ExpiryMonth = transactionList[0]["sourceOfFunds"]["provided"]["card"]["expiry"]["month"].ToObject<int>();
            //model.Token = transactionList[0]["3DSecure"]!=null? (transactionList[0]["3DSecure"]["authenticationToken"]!=null? transactionList[0]["3DSecure"]["authenticationToken"].ToObject<String>():null):null;
            //model.PaymentStatus = transactionList[0]["order"]["status"]!=null? transactionList[0]["order"]["status"].ToObject<String>():null;
            //model.CustomerIP = transactionList[0]["device"] != null ? (transactionList[0]["device"]["ipAddress"] != null ? transactionList[0]["device"]["ipAddress"].ToObject<String>() : null):null ;

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
    public class Address
    {
        public string city { get; set; }
        public string country { get; set; }
        public string postcodeZip { get; set; }
        public string stateProvince { get; set; }
        public string street { get; set; }
    }

    public class Billing
    {
        public Address address { get; set; }
    }

    public class Chargeback
    {
        public int amount { get; set; }
        public string currency { get; set; }
    }

    public class PaymentCustomer
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
    }

    public class Device
    {
        public string browser { get; set; }
        public string ipAddress { get; set; }
    }

    public class Expiry
    {
        public string month { get; set; }
        public string year { get; set; }
    }

    public class Card
    {
        public string brand { get; set; }
        public Expiry expiry { get; set; }
        public string fundingMethod { get; set; }
        public string issuer { get; set; }
        public string nameOnCard { get; set; }
        public string number { get; set; }
        public string scheme { get; set; }
    }

    public class Provided
    {
        public Card card { get; set; }
    }

    public class SourceOfFunds
    {
        public Provided provided { get; set; }
        public string token { get; set; }
        public string type { get; set; }
    }

    public class __invalid_type__3DSecure
    {
        public string acsEci { get; set; }
        public string authenticationToken { get; set; }
        public string paResStatus { get; set; }
        public string veResEnrolled { get; set; }
        public string xid { get; set; }
    }

    public class AuthorizationResponse
    {
        public string avsCode { get; set; }
        public string cardSecurityCodeError { get; set; }
        public string commercialCardIndicator { get; set; }
        public string date { get; set; }
        public string financialNetworkCode { get; set; }
        public string processingCode { get; set; }
        public string responseCode { get; set; }
        public string stan { get; set; }
        public string time { get; set; }
        public string transactionIdentifier { get; set; }
    }

    public class Address2
    {
        public string city { get; set; }
        public string country { get; set; }
        public string postcodeZip { get; set; }
        public string stateProvince { get; set; }
        public string street { get; set; }
    }

    public class Billing2
    {
        public Address2 address { get; set; }
    }

    public class Customer2
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
    }

    public class Device2
    {
        public string browser { get; set; }
        public string ipAddress { get; set; }
    }

    public class Chargeback2
    {
        public int amount { get; set; }
        public string currency { get; set; }
    }

    public class Address3
    {
        public string city { get; set; }
        public string company { get; set; }
        public string country { get; set; }
        public string postcodeZip { get; set; }
        public string stateProvince { get; set; }
        public string street { get; set; }
        public string street2 { get; set; }
    }

    public class StatementDescriptor
    {
        public Address3 address { get; set; }
        public string name { get; set; }
    }

    public class PaymentOrder
    {
        public double amount { get; set; }
        public string certainty { get; set; }
        public Chargeback2 chargeback { get; set; }
        public DateTime creationTime { get; set; }
        public string currency { get; set; }
        public string description { get; set; }
        public string fundingStatus { get; set; }
        public string id { get; set; }
        public string merchantCategoryCode { get; set; }
        public string reference { get; set; }
        public StatementDescriptor statementDescriptor { get; set; }
        public string status { get; set; }
        public double totalAuthorizedAmount { get; set; }
        public double totalCapturedAmount { get; set; }
        public double totalRefundedAmount { get; set; }
    }

    public class CardSecurityCode
    {
        public string acquirerCode { get; set; }
        public string gatewayCode { get; set; }
    }

    public class Avs
    {
        public string acquirerCode { get; set; }
        public string gatewayCode { get; set; }
    }

    public class CardholderVerification
    {
        public Avs avs { get; set; }
    }

    public class Response
    {
        public string acquirerCode { get; set; }
        public string acquirerMessage { get; set; }
        public CardSecurityCode cardSecurityCode { get; set; }
        public CardholderVerification cardholderVerification { get; set; }
        public string gatewayCode { get; set; }
    }

    public class Expiry2
    {
        public string month { get; set; }
        public string year { get; set; }
    }

    public class Card2
    {
        public string brand { get; set; }
        public Expiry2 expiry { get; set; }
        public string fundingMethod { get; set; }
        public string issuer { get; set; }
        public string nameOnCard { get; set; }
        public string number { get; set; }
        public string scheme { get; set; }
    }

    public class Provided2
    {
        public Card2 card { get; set; }
    }

    public class SourceOfFunds2
    {
        public Provided2 provided { get; set; }
        public string token { get; set; }
        public string type { get; set; }
    }

    public class Acquirer
    {
        public int batch { get; set; }
        public string date { get; set; }
        public string id { get; set; }
        public string merchantId { get; set; }
        public string transactionId { get; set; }
        public string settlementDate { get; set; }
        public string timeZone { get; set; }
    }

    public class Funding
    {
        public string status { get; set; }
    }

    public class Transaction2
    {
        public Acquirer acquirer { get; set; }
        public double amount { get; set; }
        public string authorizationCode { get; set; }
        public string currency { get; set; }
        public string frequency { get; set; }
        public Funding funding { get; set; }
        public string id { get; set; }
        public string receipt { get; set; }
        public string source { get; set; }
        public string terminal { get; set; }
        public string type { get; set; }
    }

    public class Transaction
    {
        public __invalid_type__3DSecure __invalid_name__3DSecure { get; set; }
        public string __invalid_name__3DSecureId { get; set; }
        public AuthorizationResponse authorizationResponse { get; set; }
        public Billing2 billing { get; set; }
        public Customer2 customer { get; set; }
        public Device2 device { get; set; }
        public string gatewayEntryPoint { get; set; }
        public string merchant { get; set; }
        public PaymentOrder order { get; set; }
        public Response response { get; set; }
        public string result { get; set; }
        public SourceOfFunds2 sourceOfFunds { get; set; }
        public DateTime timeOfRecord { get; set; }
        public Transaction2 transaction { get; set; }
        public string version { get; set; }
    }

    public class RootObject
    {
        public double amount { get; set; }
        public Billing billing { get; set; }
        public string certainty { get; set; }
        public Chargeback chargeback { get; set; }
        public DateTime creationTime { get; set; }
        public string currency { get; set; }
        public Customer customer { get; set; }
        public string description { get; set; }
        public Device device { get; set; }
        public string fundingStatus { get; set; }
        public string id { get; set; }
        public string merchant { get; set; }
        public string merchantCategoryCode { get; set; }
        public string reference { get; set; }
        public string result { get; set; }
        public SourceOfFunds sourceOfFunds { get; set; }
        public string status { get; set; }
        public double totalAuthorizedAmount { get; set; }
        public double totalCapturedAmount { get; set; }
        public double totalRefundedAmount { get; set; }
        public List<Transaction> transaction { get; set; }
    }
}
