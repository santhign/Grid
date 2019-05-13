using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using OrderService.Models;
using Newtonsoft.Json;
using OrderService.Helpers;
using Core.Models;

namespace OrderService.Models
{   
    public class GatewayApiRequest
    {
        private GatewayApiConfig gatewayApiConfig;
        public String OrderId { get; set; }
        public String TransactionId { get; set; }

        public String ApiOperation { get; set; }
        public String ApiMethod { get; set; } = "PUT";
        public String RequestUrl { get; set; }
        public String Payload { get; set; }

        public String SessionId { get; set; }
        public String SecureId { get; set; }
        public String SecureIdResponseUrl { get; set; }

        public String SourceType { get; set; }
        public String CardNumber { get; set; }
        public String ExpiryMonth { get; set; }
        public String ExpiryYear { get; set; }
        public String SecurityCode { get; set; }

        public String OrderAmount { get; set; }
        public String OrderCurrency { get; set; }
        public String OrderDescription { get; set; }

        public String TransactionAmount { get; set; }
        public String TransactionCurrency { get; set; }
        public String TargetTransactionId { get; set; }
        public String ReturnUrl { get; set; }

        public String PaymentAuthResponse { get; set; }
        public String SecureId3D { get; set; }        

        public String BrowserPaymentOperation { get; set; }
        public String BrowserPaymentPaymentConfirmation { get; set; }


        public Dictionary<String, String> NVPParameters { get; set; }

        public String ContentType { get; set; } = "application/json; charset=iso-8859-1";


        public String MasterpassOnline { get; set; }
        public String MasterpassOriginUrl { get; set; }

        public String MasterpassOauthToken { get; set; }
        public String MasterpassOauthVerifier { get; set; }
        public String MasterpassCheckoutUrl { get; set; }        

        public String Token { get; set; }

        private String apiBaseUrl { get; set; }

        private String CustomerName { get; set; }

        private String BillingUnit { get; set; }

        private String BillingFloor { get; set; }

        private String BillingStreetName { get; set; }

        private String BillingBuildingNumber { get; set; }

        private String BillingBuildingName { get; set; }

        private String BillingContactNumber { get; set; }

        private String BillingPostCode { get; set; }

        private String Street { get; set; }

        private String Street2 { get; set; }

        private String MerchantName { get; set; }
        private String MerchantAddress1 { get; set; }

        private String MerchantAddress2 { get; set; }

        private String MerchantPostCode { get; set; }

        private String MerchantContactNumber { get; set; }
        private String ReceiptNumber { get; set; }


        public GatewayApiRequest()
        {
        }

        public GatewayApiRequest(GatewayApiConfig gatewayApiConfig)
        {
            GatewayApiConfig = gatewayApiConfig;
        }

        public GatewayApiRequest(GatewayApiConfig gatewayApiConfig, customerBilling billingAddress, string receiptNumber)
        {
            GatewayApiConfig = gatewayApiConfig;

            if(billingAddress!=null)
            {
              if(!string.IsNullOrEmpty(billingAddress.Name))

                    CustomerName = billingAddress.Name;

              if (!string.IsNullOrEmpty(billingAddress.BillingUnit))

                    BillingUnit = billingAddress.BillingUnit;

              if (!string.IsNullOrEmpty(billingAddress.BillingFloor))

                    BillingFloor = billingAddress.BillingFloor;

              if (!string.IsNullOrEmpty(billingAddress.BillingStreetName))

                    BillingStreetName = billingAddress.BillingStreetName;

              if (!string.IsNullOrEmpty(billingAddress.BillingBuildingNumber))

                    BillingBuildingNumber = billingAddress.BillingBuildingNumber;

              if (!string.IsNullOrEmpty(billingAddress.BillingBuildingName))

                    BillingBuildingName = billingAddress.BillingBuildingName;

              if (!string.IsNullOrEmpty(billingAddress.BillingContactNumber))

                    BillingContactNumber = billingAddress.BillingContactNumber;

              if (!string.IsNullOrEmpty(billingAddress.BillingPostCode))

                    BillingPostCode = billingAddress.BillingPostCode;
            }

            if (!string.IsNullOrEmpty(receiptNumber))
            {
                ReceiptNumber = receiptNumber;
            }
        }
        public GatewayApiConfig GatewayApiConfig
        {
            get => gatewayApiConfig;
            set => gatewayApiConfig = value;
        }

        public string buildSessionRequestUrl()
        {
            return buildSessionRequestUrl(SessionId);
        }

        public string buildTokenUrl()
        {
            RequestUrl = $@"{getApiGatewayBaseURL()}/api/rest/version/{GatewayApiConfig.Version}/merchant/{GatewayApiConfig.MerchantId}/token";
            return RequestUrl;
        }

        public string buildSessionRequestUrl(String sessionId)
        {
            string url = $@"{getApiGatewayBaseURL()}/api/rest/version/{GatewayApiConfig.Version}/merchant/{GatewayApiConfig.MerchantId}/session";
            if (!String.IsNullOrEmpty(sessionId))
            {
                url = $"{url}/{sessionId}";
            }
            RequestUrl = url;
            return RequestUrl;
        }

        public string buildSecureIdRequestUrl()
        {
            string url = $@"{getApiGatewayBaseURL()}/api/rest/version/{GatewayApiConfig.Version}/merchant/{GatewayApiConfig.MerchantId}/3DSecureId";
            if (!String.IsNullOrEmpty(SecureId))
            {
                url = $"{url}/{SecureId}";
            }
            RequestUrl = url;
            return RequestUrl;
        }


        public string buildOrderUrl()
        {
            RequestUrl = $@"{getApiGatewayBaseURL()}/api/rest/version/{GatewayApiConfig.Version}/merchant/{GatewayApiConfig.MerchantId}/order/{OrderId}";
            return RequestUrl;
        }

        public string buildRequestUrl()
        {
            RequestUrl = $@"{getApiGatewayBaseURL()}/api/rest/version/{GatewayApiConfig.Version}/merchant/{GatewayApiConfig.MerchantId}/order/{OrderId}/transaction/{TransactionId}";
            return RequestUrl;
        }

        public string buildDeleteUrl(string tokenid)
        {
            RequestUrl = $@"{getApiGatewayBaseURL()}/api/rest/version/{GatewayApiConfig.Version}/merchant/{GatewayApiConfig.MerchantId}/token/{tokenid}";                
            return RequestUrl;
        }


        public string buildRequestNPVUrl()
        {
            RequestUrl = $@"{getApiGatewayBaseURL()}/api/nvp/version/{GatewayApiConfig.Version}";
            return RequestUrl;
        }

        

        /// <summary>
        /// return the correct API gateway base URL, depends on the authentication method.
        /// </summary>
        /// <returns>The API gateway base URL.</returns>
        public string getApiGatewayBaseURL()
        {
            if (apiBaseUrl == null)
            {
                if (GatewayApiConfig.AuthenticationByCertificate)
                {
                    apiBaseUrl = GatewayApiConfig.GatewayUrlCertificate;
                }
                else
                {
                    apiBaseUrl = GatewayApiConfig.GatewayUrl;
                }
            }
            return apiBaseUrl;
        }


        /// <summary>
        /// Builds the NVP Map
        /// </summary>
        /// <returns>The NVP Map.</returns>
        public Dictionary<String, String> buildNVPMap()
        {
            NVPParameters = new Dictionary<string, string>();

            NVPParameters.Add("apiOperation", "PAY");
            NVPParameters.Add("order.id", OrderId);
            NVPParameters.Add("order.amount", OrderAmount);
            NVPParameters.Add("order.currency", OrderCurrency);
            NVPParameters.Add("transaction.id", TransactionId);
            NVPParameters.Add("session.id", SessionId);
            NVPParameters.Add("sourceOfFunds.type", "CARD");
            NVPParameters.Add("merchant", gatewayApiConfig.MerchantId);

            return NVPParameters;
        }


        /// <summary>
        /// Builds the JSON payload.
        /// </summary>
        /// <returns>The JSON payload.</returns>
        public string buildPayload()
        {

            NameValueCollection nvc = new NameValueCollection();
            if (!String.IsNullOrEmpty(ApiOperation))
            {
                nvc.Add("apiOperation", ApiOperation);
            }

            if (!String.IsNullOrEmpty(SecureId3D))
            {
                nvc.Add("3DSecureId", SecureId3D);
            }

            if (!String.IsNullOrEmpty(SessionId))
            {
                nvc.Add("session.id", SessionId);
            }
            if (!String.IsNullOrEmpty(SourceType))
            {
                nvc.Add("sourceOfFunds.type", SourceType);
            }

            if (!String.IsNullOrEmpty(Token))
            {
                nvc.Add("sourceOfFunds.token", Token);
            }


            //payer name
            if (!String.IsNullOrEmpty(CustomerName))
            {
                nvc.Add("order.requestorName", CustomerName);              
                
            }

            //payer's contact number
            if (!String.IsNullOrEmpty(BillingContactNumber))
            {
                nvc.Add("customer.mobilePhone", BillingContactNumber);    

            }
           
            // billing address
            //street = BillingBuildingNumber BillingStreetName
            if (!String.IsNullOrEmpty(BillingBuildingNumber) && !String.IsNullOrEmpty(BillingStreetName))
            {
                Street = BillingBuildingNumber + " " + BillingStreetName;
            }

            else if (String.IsNullOrEmpty(BillingBuildingNumber) && !String.IsNullOrEmpty(BillingStreetName))
            {
                Street = BillingStreetName;
            }

            else if (!String.IsNullOrEmpty(BillingBuildingNumber) && String.IsNullOrEmpty(BillingStreetName))
            {
                Street = BillingBuildingNumber;
            }

            else 
            {
                Street = "";
            }

            if(!String.IsNullOrEmpty(Street))
            {
                nvc.Add("billing.address.street", Street);

            }

            //street2 = BillingFloor-BillingUnit BillingBuildingName
            if (!String.IsNullOrEmpty(BillingFloor) && !String.IsNullOrEmpty(BillingUnit) && !String.IsNullOrEmpty(BillingBuildingName))
            {
                Street2 = BillingFloor + "-" + BillingUnit + " " + BillingBuildingName;
            }

            else if (String.IsNullOrEmpty(BillingFloor) && !String.IsNullOrEmpty(BillingUnit) && !String.IsNullOrEmpty(BillingBuildingName))
            {
                Street2 =  BillingUnit + " " + BillingBuildingName;
            }

           else if (String.IsNullOrEmpty(BillingFloor) && String.IsNullOrEmpty(BillingUnit) && !String.IsNullOrEmpty(BillingBuildingName))
            {
                Street2 =  BillingBuildingName;
            }

           else if (String.IsNullOrEmpty(BillingFloor) && String.IsNullOrEmpty(BillingUnit) && String.IsNullOrEmpty(BillingBuildingName))
            {
                Street2 = "";
            }

            else if (!String.IsNullOrEmpty(BillingFloor) && !String.IsNullOrEmpty(BillingUnit) && String.IsNullOrEmpty(BillingBuildingName))
            {
                Street2 = BillingFloor + "-" + BillingUnit;
            }

            else if (!String.IsNullOrEmpty(BillingFloor) && String.IsNullOrEmpty(BillingUnit) && String.IsNullOrEmpty(BillingBuildingName))
            {
                Street2 = BillingFloor ;
            }

            else if (!String.IsNullOrEmpty(BillingFloor) && String.IsNullOrEmpty(BillingUnit) && !String.IsNullOrEmpty(BillingBuildingName))
            {
                Street2 = BillingFloor + " " + BillingBuildingName;
            }

            else if (String.IsNullOrEmpty(BillingFloor) && !String.IsNullOrEmpty(BillingUnit) && String.IsNullOrEmpty(BillingBuildingName))
            {
                Street2 = BillingUnit;
            }


            if (!String.IsNullOrEmpty(Street2))
            {
                nvc.Add("billing.address.street2", Street2);
            } 

            nvc.Add("billing.address.city", "Singapore");
            nvc.Add("billing.address.stateProvince", "Singapore");
            nvc.Add("billing.address.country", "SGP");

            if (!String.IsNullOrEmpty(BillingPostCode))
            {
                nvc.Add("billing.address.postcodeZip", BillingPostCode);
            }

            //statement Descriptor
            //nvc.Add("order.statementDescriptor.name", gatewayApiConfig.GridMerchantName);
            //nvc.Add("order.statementDescriptor.address.company", gatewayApiConfig.GridMerchantName);
            //nvc.Add("order.statementDescriptor.address.street", gatewayApiConfig.GridMerchantAddress1);
            //nvc.Add("order.statementDescriptor.address.street2", gatewayApiConfig.GridMerchantAddress2);
           
            //nvc.Add("order.statementDescriptor.address.city", "Singapore");
            //nvc.Add("order.statementDescriptor.address.stateProvince", "Singapore");
            //nvc.Add("order.statementDescriptor.address.country", "SGP");
            //nvc.Add("order.statementDescriptor.address.postcodeZip", gatewayApiConfig.GridMerchantPostCode);

            





            if (!String.IsNullOrEmpty(CardNumber))
            {
                nvc.Add("sourceOfFunds.provided.card.number", CardNumber);
                nvc.Add("sourceOfFunds.provided.card.expiry.month", ExpiryMonth);
                nvc.Add("sourceOfFunds.provided.card.expiry.year", ExpiryYear);
                nvc.Add("sourceOfFunds.provided.card.securityCode", SecurityCode);
            }

            if ("CREATE_CHECKOUT_SESSION" == ApiOperation || String.IsNullOrEmpty(ApiOperation) || "UPDATE_SESSION_FROM_WALLET" == ApiOperation)
            {
                // Need to add order ID in the request body for CREATE_CHECKOUT_SESSION or UPDATE SESSION. 
                // Its presence in the body will cause an error for the other operations.
                if (!String.IsNullOrEmpty(OrderId))
                {
                    nvc.Add("order.id", OrderId);
                }


                //masterpass
                if (String.IsNullOrEmpty(ApiOperation) || "CREATE_CHECKOUT_SESSION" != ApiOperation)
                {

                    if (!String.IsNullOrEmpty(MasterpassOnline))
                    {
                        nvc.Add("order.walletProvider", MasterpassOnline);
                    }
                    if (!String.IsNullOrEmpty(MasterpassOriginUrl))
                    {
                        nvc.Add("wallet.masterpass.originUrl", MasterpassOriginUrl);
                    }
                    if (!String.IsNullOrEmpty(MasterpassOauthToken))
                    {
                        nvc.Add("wallet.masterpass.oauthToken", MasterpassOauthToken);
                    }
                    if (!String.IsNullOrEmpty(MasterpassOauthVerifier))
                    {
                        nvc.Add("wallet.masterpass.oauthVerifier", MasterpassOauthVerifier);
                    }
                    if (!String.IsNullOrEmpty(MasterpassCheckoutUrl))
                    {
                        nvc.Add("wallet.masterpass.checkoutUrl", MasterpassCheckoutUrl);
                    }
                }


                if (!String.IsNullOrEmpty(ReturnUrl) && "CREATE_CHECKOUT_SESSION" == ApiOperation)
                {
                    nvc.Add("interaction.returnUrl", ReturnUrl);
                }

            }

            if (!String.IsNullOrEmpty(OrderAmount))
            {
                nvc.Add("order.amount", OrderAmount);
            }
            if (!String.IsNullOrEmpty(OrderCurrency))
            {
                nvc.Add("order.currency", OrderCurrency);
            }
            if (!String.IsNullOrEmpty(TransactionAmount))
            {
                nvc.Add("transaction.amount", TransactionAmount);
            }
            if (!String.IsNullOrEmpty(TransactionCurrency))
            {
                nvc.Add("transaction.currency", TransactionCurrency);
            }
            if (!String.IsNullOrEmpty(TargetTransactionId))
            {
                nvc.Add("transaction.targetTransactionId", TargetTransactionId);
            }
            if (!String.IsNullOrEmpty(SecureIdResponseUrl))
            {
                nvc.Add("3DSecure.authenticationRedirect.responseUrl", SecureIdResponseUrl);
                nvc.Add("3DSecure.authenticationRedirect.pageGenerationMode", "CUSTOMIZED");
            }
            if (!String.IsNullOrEmpty(PaymentAuthResponse))
            {
                nvc.Add("3DSecure.paRes", PaymentAuthResponse);
            }

            //browser payment

            if (!String.IsNullOrEmpty(BrowserPaymentOperation))
            {
                nvc.Add("browserPayment.operation", BrowserPaymentOperation);
            }


            //paypal
            if (!String.IsNullOrEmpty(BrowserPaymentPaymentConfirmation) && "PAYPAL" == SourceType)
            {
                nvc.Add("browserPayment.paypal.paymentConfirmation", BrowserPaymentPaymentConfirmation);
            }


            if ("INITIATE_BROWSER_PAYMENT" == ApiOperation)
            {
                nvc.Add("browserPayment.returnUrl", ReturnUrl);
            }


            //build json
            Payload = JsonHelper.BuildJsonFromNVC(nvc);

            return Payload;
        }

    }
}
