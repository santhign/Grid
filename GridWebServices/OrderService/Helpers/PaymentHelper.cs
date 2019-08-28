using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderService.Models;
using OrderService.Enums;
using System.IO;
using InfrastructureService;
using Core.Helpers;
using Core.Enums;
using Core.Extensions;
using Newtonsoft.Json;
using Core.Models;
using Microsoft.Extensions.Configuration;
using OrderService.DataAccess;
using OrderService.Models.Transaction;


namespace OrderService.Helpers
{
    public class PaymentHelper
    {
        public static string GenerateOrderId()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        public Checkout GetCheckoutDetails()
        {
            Checkout checkout = new Checkout();

            checkout.OrderId = GenerateOrderId();

            checkout.CheckoutSession = new CheckoutSessionModel();

            return checkout;
        }

        public static void InitWebhooksNotificationsFolder()
        {
            try
            {
                if (!Directory.Exists(GatewayApiConfig.WEBHOOKS_NOTIFICATION_FOLDER))
                {
                    Directory.CreateDirectory(GatewayApiConfig.WEBHOOKS_NOTIFICATION_FOLDER);
                }
            }
            catch (IOException ex)
            {
                 LogInfo.Fatal(ex,  $": {EnumExtensions.GetDescription(MPGSAPIResponse.WebhookNotificationFolderError) + " : " +  GatewayApiConfig.WEBHOOKS_NOTIFICATION_FOLDER}");
            }
        }

        public static void InitSNSNotificationsFolder()
        {
            try
            {
                if (!Directory.Exists(GatewayApiConfig.WEBHOOKS_NOTIFICATION_FOLDER))
                {
                    Directory.CreateDirectory(GatewayApiConfig.WEBHOOKS_NOTIFICATION_FOLDER);
                }
            }
            catch (IOException ex)
            {
                LogInfo.Fatal(ex, $": {EnumExtensions.GetDescription(MPGSAPIResponse.WebhookNotificationFolderError) + " : " + GatewayApiConfig.WEBHOOKS_NOTIFICATION_FOLDER}");
            }
        }

        public void CleanUpWebhooksNotificationsFolder() 
        {
            try
            {
                if (Directory.Exists(GatewayApiConfig.WEBHOOKS_NOTIFICATION_FOLDER))
                {
                    Directory.Delete(GatewayApiConfig.WEBHOOKS_NOTIFICATION_FOLDER, true);
                }
            }
            catch (IOException ex)
            {
                LogInfo.Fatal(ex, $" {EnumExtensions.GetDescription(MPGSAPIResponse.WebhookNotificationFolderDeleteError) + " : "+GatewayApiConfig.WEBHOOKS_NOTIFICATION_FOLDER}");
            }
        }

        
        public Checkout CreateCheckoutSession(GridMPGSConfig mpgsConfig, customerBilling customerBillingDetails, string MpgsOrderID, string transactionID, string receiptNumber, string orderNumber)
        {
            try
            {
                LogInfo.Information(EnumExtensions.GetDescription(MPGSAPIOperation.CREATE_CHECKOUT_SESSION));

                GatewayApiConfig config = new GatewayApiConfig(mpgsConfig);

                GatewayApiRequest gatewayApiRequest = new GatewayApiRequest(config, customerBillingDetails, receiptNumber, orderNumber);

                gatewayApiRequest.ApiOperation = MPGSAPIOperation.CREATE_CHECKOUT_SESSION.ToString();

                gatewayApiRequest.OrderId = MpgsOrderID;

                gatewayApiRequest.TransactionId = transactionID;

                gatewayApiRequest.OrderCurrency = config.Currency;

                gatewayApiRequest.buildSessionRequestUrl();

                gatewayApiRequest.buildPayload();

                gatewayApiRequest.ApiMethod = GatewayApiClient.POST;

                GatewayApiClient gatewayApiClient = new GatewayApiClient(config);
              
                String response = gatewayApiClient.SendTransaction(gatewayApiRequest);

                LogInfo.Information(EnumExtensions.GetDescription(MPGSAPIResponse.HostedCheckoutResponse) + response);

                CheckoutSessionModel checkoutSessionModel = CheckoutSessionModel.toCheckoutSessionModel(response);

                Checkout checkOut = new Checkout();

                checkOut.CheckoutJsUrl = $@"{config.GatewayUrl}/checkout/version/{config.Version}/checkout.js";

                checkOut.MerchantId = config.MerchantId;

                checkOut.OrderId = gatewayApiRequest.OrderId;

                checkOut.TransactionID = gatewayApiRequest.TransactionId;

                checkOut.CheckoutSession = checkoutSessionModel;

                checkOut.Currency = config.Currency;

                return checkOut;
                
            }
            catch(Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw ex;
            }
                  
        }

        public static GatewayApiRequest CreateTokenizationApiRequest(GatewayApiConfig gatewayApiConfig, string apiOperation = null, string sessionId = null)
        {
            GatewayApiRequest gatewayApiRequest = new GatewayApiRequest(gatewayApiConfig)
            {
                SessionId = sessionId,
                OrderId = PaymentHelper.GenerateOrderId(),
                TransactionId = PaymentHelper.GenerateOrderId(),
                ApiOperation = apiOperation,
                OrderAmount = "",
                OrderCurrency = gatewayApiConfig.Currency
            };

            gatewayApiRequest.buildRequestUrl();

            if (apiOperation == "CAPTURE" || apiOperation == "REFUND")
            {
                gatewayApiRequest.TransactionAmount = "";

                gatewayApiRequest.TransactionCurrency = gatewayApiConfig.Currency;

                gatewayApiRequest.OrderId = null;
            }
            if (apiOperation == "VOID" || apiOperation == "UPDATE_AUTHORIZATION")
            {
                gatewayApiRequest.OrderId = null;
            }
            if (apiOperation == "RETRIEVE_ORDER" || apiOperation == "RETRIEVE_TRANSACTION")
            {
                gatewayApiRequest.ApiMethod = "GET";

                gatewayApiRequest.OrderId = null;

                gatewayApiRequest.TransactionId = null;
            }

            gatewayApiRequest.buildPayload();

            return gatewayApiRequest;
        }

       public Checkout CreateTokenizationCheckoutSession(GridMPGSConfig mpgsConfig, string apiOperation = null, string sessionId = null)
        {
            try
            {
                LogInfo.Information(EnumExtensions.GetDescription(MPGSAPIOperation.CREATE_CHECKOUT_SESSION));

                GatewayApiConfig config = new GatewayApiConfig(mpgsConfig);            

                GatewayApiRequest gatewayApiRequest = new GatewayApiRequest(config)
                {
                    SessionId = sessionId,
                    OrderId = GenerateOrderId(),
                    TransactionId = GenerateOrderId(),
                    ApiOperation = apiOperation,
                    OrderAmount = "",
                    OrderCurrency = config.Currency
                };

                gatewayApiRequest.buildRequestUrl();

                if (apiOperation == "CAPTURE" || apiOperation == "REFUND")
                {
                    gatewayApiRequest.TransactionAmount = "";
                    gatewayApiRequest.TransactionCurrency = config.Currency;
                    gatewayApiRequest.OrderId = null;
                }
                if (apiOperation == "VOID" || apiOperation == "UPDATE_AUTHORIZATION")
                {
                    gatewayApiRequest.OrderId = null;
                }
                if (apiOperation == "RETRIEVE_ORDER" || apiOperation == "RETRIEVE_TRANSACTION")
                {
                    gatewayApiRequest.ApiMethod = "GET";
                    gatewayApiRequest.OrderId = null;
                    gatewayApiRequest.TransactionId = null;
                }
                gatewayApiRequest.buildPayload();                        

                Checkout checkOut = new Checkout();

                checkOut.CheckoutJsUrl = $@"{config.GatewayUrl}/form/version/{config.Version}/merchant/{config.MerchantId}/session.js";              

                checkOut.MerchantId = config.MerchantId;

                checkOut.OrderId = gatewayApiRequest.OrderId;

                checkOut.CheckoutSession = null;

                checkOut.Currency = config.Currency;

                checkOut.TransactionID = gatewayApiRequest.TransactionId;

                return checkOut;

            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw ex;
            }

        }

        public string RetrieveCheckOutTransaction(GridMPGSConfig mpgsConfig,CheckOutResponseUpdate responseUpdate)
        {
            try
            {
                LogInfo.Information($" {EnumExtensions.GetDescription(MPGSAPIResponse.HostedCheckoutRetrieveReceipt) + " orderId  " +  responseUpdate.MPGSOrderID} result {responseUpdate.Result} sessionId {responseUpdate.CheckOutSessionID}");

                GatewayApiConfig config = new GatewayApiConfig(mpgsConfig);             

                if (responseUpdate.Result == MPGSAPIResponse.SUCCESS.ToString() || responseUpdate.Result == MPGSAPIResponse.CAPTURED.ToString())
                {
                    GatewayApiRequest gatewayApiRequest = new GatewayApiRequest(config)
                    {
                        ApiOperation = MPGSAPIOperation.RETRIEVE_ORDER.ToString(),

                        OrderId = responseUpdate.MPGSOrderID,

                        ApiMethod = GatewayApiClient.GET
                    };

                    gatewayApiRequest.buildOrderUrl();

                    GatewayApiClient gatewayApiClient = new GatewayApiClient(config);

                    string response = gatewayApiClient.SendTransaction(gatewayApiRequest);

                    LogInfo.Information($" {EnumExtensions.GetDescription(MPGSAPIResponse.HostedCheckoutRetrieveReceipt) + " " + response}");

                    //parse response

                    return response;

                }
                else
                {
                    LogInfo.Error($"  {MPGSAPIResponse.Unsuccessful.ToString()+ ".  " + responseUpdate.Result}");

                    return string.Empty;
                }

            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw ex;
            }
        }
        public GridMPGSConfig GetGridMPGSConfig(List<Dictionary<string, string>> configDict)
        {
            GridMPGSConfig config = new GridMPGSConfig();
            config.GatewayUrl = configDict.Single(x => x["key"] == "GatewayBaseUrl")["value"];
            config.Version = configDict.Single(x => x["key"] == "GatewayVersion")["value"];
            config.MerchantId = configDict.Single(x => x["key"] == "GatewayGridMerchantId")["value"];
            config.Password = configDict.Single(x => x["key"] == "GatewayGridPassword")["value"];
            config.Currency = configDict.Single(x => x["key"] == "GatewayGridCurrency")["value"];
            config.GridMerchantName = configDict.Single(x => x["key"] == "GridMerchantName")["value"];
            config.GridMerchantEmail = configDict.Single(x => x["key"] == "GridMerchantEmail")["value"];
            config.GridMerchantLogo = configDict.Single(x => x["key"] == "GridMerchantLogo")["value"];
            config.GridMerchantAddress1 = configDict.Single(x => x["key"] == "GridMerchantAddress1")["value"];
            config.GridMerchantAddress2 = configDict.Single(x => x["key"] == "GridMerchantAddress2")["value"];
            config.GridMerchantPostCode = configDict.Single(x => x["key"] == "GridMerchantPostCode")["value"];
            config.GridMerchantContactNumber = configDict.Single(x => x["key"] == "GridMerchantContactNumber")["value"];
            config.WebhooksNotificationSecret = configDict.Single(x => x["key"] == "GatewayGridWebhookSecret")["value"];
            return config;
        }

        public GridMPGSDirectMIDConfig GetGridMPGSDirectMerchant(List<Dictionary<string, string>> configDict)
        {
            GridMPGSDirectMIDConfig config = new GridMPGSDirectMIDConfig();           
            config.MerchantId = configDict.Single(x => x["key"] == "GatewayGridDirectMerchantId")["value"];
            config.Password = configDict.Single(x => x["key"] == "GatewayGridDirectMerchantPassword")["value"];            
            config.WebhooksNotificationSecret = configDict.Single(x => x["key"] == "GatewayGridDirectWebhookSecret")["value"];
            return config;
        }
        public TokenResponse Tokenize(GridMPGSConfig mpgsConfig, TokenSession tokenSession)
        {
            try
            {   
                GatewayApiConfig config = new GatewayApiConfig(mpgsConfig);             

                GatewayApiRequest gatewayUpdateSessionRequest = new GatewayApiRequest(config);              

                GatewayApiClient gatewayApiClient = new GatewayApiClient(config);

                //generate token
                GatewayApiRequest gatewayGenerateTokenRequest = new GatewayApiRequest(config);

                gatewayGenerateTokenRequest.SessionId = tokenSession.CheckOutSessionID;

                gatewayGenerateTokenRequest.ApiMethod = GatewayApiClient.POST;

                gatewayGenerateTokenRequest.buildPayload();

                gatewayGenerateTokenRequest.buildTokenUrl(); 

                string request =JsonConvert.SerializeObject(gatewayGenerateTokenRequest);

                LogInfo.Information(JsonConvert.SerializeObject(gatewayGenerateTokenRequest));

                String response   = gatewayApiClient.SendTransaction(gatewayGenerateTokenRequest);

                LogInfo.Information(response);

                TokenResponse tokenResponse = TokenResponse.ToTokenResponse(response);               

                LogInfo.Information($"Tokenize response :  {response}");

                return tokenResponse;         

            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw ex;
            }
        }

        public TokenResponse TokenizeTest(GridMPGSConfig mpgsConfig)
        {
            try
            {
                GatewayApiConfig config = new GatewayApiConfig(mpgsConfig);

                GatewayApiRequest gatewayUpdateSessionRequest = new GatewayApiRequest(config);

                GatewayApiClient gatewayApiClient = new GatewayApiClient(config);

                //generate token
                GatewayApiRequest gatewayGenerateTokenRequest = new GatewayApiRequest(config);

                gatewayGenerateTokenRequest.SessionId = "SESSION0002676972204L29024409K5";

                gatewayGenerateTokenRequest.ApiMethod = GatewayApiClient.POST;

                gatewayGenerateTokenRequest.buildPayload();

                gatewayGenerateTokenRequest.buildTokenUrl();

                string request = JsonConvert.SerializeObject(gatewayGenerateTokenRequest);

                LogInfo.Information(JsonConvert.SerializeObject(gatewayGenerateTokenRequest));

                String response = gatewayApiClient.SendTransaction(gatewayGenerateTokenRequest);

                LogInfo.Information(response);

                TokenResponse tokenResponse = TokenResponse.ToTokenResponse(response);

                LogInfo.Information($"Tokenize response :  {response}");

                return tokenResponse;

            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw ex;
            }
        }

        public string PayWithToken(GridMPGSConfig mpgsConfig, string token, string sessionID, string orderID, string transactionID, string amount)
        {
            try
            {
                
                //payment with token

                GatewayApiConfig config = new GatewayApiConfig(mpgsConfig);

                GatewayApiRequest gatewayGeneratePaymentRequest = new GatewayApiRequest(config);

                gatewayGeneratePaymentRequest.ApiOperation = "PAY";

                gatewayGeneratePaymentRequest.ApiMethod = GatewayApiClient.PUT;

                gatewayGeneratePaymentRequest.Token = token;//tokenResponse.Token;

                gatewayGeneratePaymentRequest.SessionId = sessionID; // responseUpdate.MPGSResponse.session.id;

                gatewayGeneratePaymentRequest.OrderId = orderID; //updateTokenSesisonDetails.MPGSOrderID;

                gatewayGeneratePaymentRequest.TransactionId = transactionID;// updateTokenSesisonDetails.TransactionID;

                gatewayGeneratePaymentRequest.OrderAmount = amount;

                gatewayGeneratePaymentRequest.buildPayload();

                gatewayGeneratePaymentRequest.buildRequestUrl();

                //payment response
                GatewayApiClient gatewayApiClient = new GatewayApiClient(config);

                string response= gatewayApiClient.SendTransaction(gatewayGeneratePaymentRequest);

                LogInfo.Information($" {EnumExtensions.GetDescription(MPGSAPIResponse.HostedCheckoutRetrieveReceipt) + " " + response}");

                return TokenResponse.GetResponseResult(response);

            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw ex;
            }
        }
        public string  Authorize(GridMPGSConfig mpgsConfig, Checkout checkOutDetails, PaymentMethod paymentMethod)
        {

            GatewayApiConfig config = new GatewayApiConfig(mpgsConfig);

            GatewayApiRequest gatewayApiRequest = new GatewayApiRequest(config, checkOutDetails.ReceiptNumber,checkOutDetails.OrderNumber)
            {
               SessionId = checkOutDetails.CheckoutSession.Id,
                OrderId = checkOutDetails.OrderId,
                TransactionId = checkOutDetails.TransactionID,
                ApiOperation = MPGSAPIOperation.AUTHORIZE.ToString(),
                OrderAmount = checkOutDetails.Amount.ToString(),
                OrderCurrency = config.Currency,
                ApiMethod = "PUT",
                Token = paymentMethod.Token,
                SourceType = paymentMethod.SourceType,
                OrderDescription = "",              
               
                 
            };           

            gatewayApiRequest.buildRequestUrl();

            gatewayApiRequest.buildPayload();

            string request = JsonConvert.SerializeObject(gatewayApiRequest);

            LogInfo.Information(JsonConvert.SerializeObject(gatewayApiRequest));

            GatewayApiClient gatewayApiClient = new GatewayApiClient(config);

            string response = gatewayApiClient.SendTransaction(gatewayApiRequest);

            LogInfo.Information(response);

            return TokenResponse.GetResponseResult(response);
        }

        public string AuthorizeTest(GridMPGSConfig mpgsConfig)
        {

            GatewayApiConfig config = new GatewayApiConfig(mpgsConfig);

            GatewayApiRequest gatewayApiRequest = new GatewayApiRequest(config)
            {
               // SessionId = "" ,//checkOutDetails.CheckoutSession.Id,
            OrderId = GenerateOrderId(), //checkOutDetails.OrderId,
            TransactionId = GenerateOrderId(), //checkOutDetails.TransactionID,
                ApiOperation = MPGSAPIOperation.AUTHORIZE.ToString(),
                OrderAmount = "20",
                OrderCurrency = config.Currency,
                ApiMethod = "PUT",
                Token = "4440008087700014",
                SourceType = "CARD",
                OrderDescription = "test pay",


            };

            gatewayApiRequest.buildRequestUrl();

            gatewayApiRequest.buildPayload();

            string request = JsonConvert.SerializeObject(gatewayApiRequest);

            LogInfo.Information(JsonConvert.SerializeObject(gatewayApiRequest));

            GatewayApiClient gatewayApiClient = new GatewayApiClient(config);

            string response = gatewayApiClient.SendTransaction(gatewayApiRequest);

            LogInfo.Information(response);

            return TokenResponse.GetResponseResult(response);
        }

        public string  Capture(GridMPGSConfig mpgsConfig, TokenSession tokenSession)
        {
            GatewayApiConfig config = new GatewayApiConfig(mpgsConfig);           

            GatewayApiRequest gatewayApiRequest = new GatewayApiRequest(config)
            {               
                OrderId = tokenSession.MPGSOrderID,               
                ApiOperation = MPGSAPIOperation.CAPTURE.ToString(),
                TransactionAmount = tokenSession.Amount.ToString(),
                TransactionId=PaymentHelper.GenerateOrderId(),               
                TransactionCurrency = config.Currency,
                Token= tokenSession.Token,
                SourceType= tokenSession.SourceOfFundType

            };
            gatewayApiRequest.buildRequestUrl();         

            gatewayApiRequest.buildPayload();

            string request = JsonConvert.SerializeObject(gatewayApiRequest);

            LogInfo.Information(JsonConvert.SerializeObject(gatewayApiRequest));

            GatewayApiClient gatewayApiClient = new GatewayApiClient(config);

            string response = gatewayApiClient.SendTransaction(gatewayApiRequest);

            LogInfo.Information(response);

            return TokenResponse.GetResponseResult(response); 
        }

        public string CaptureTest(GridMPGSConfig mpgsConfig)
        {
            GatewayApiConfig config = new GatewayApiConfig(mpgsConfig);

            GatewayApiRequest gatewayApiRequest = new GatewayApiRequest(config)
            {
                OrderId = "77749b36d8", // authorized order id
                ApiOperation = MPGSAPIOperation.CAPTURE.ToString(),
                TransactionAmount = "20",
                TransactionId = PaymentHelper.GenerateOrderId(),
                TransactionCurrency = config.Currency,
                Token = "4440008087700014",
                SourceType = "CARD"

            };
            gatewayApiRequest.buildRequestUrl();

            gatewayApiRequest.buildPayload();

            string request = JsonConvert.SerializeObject(gatewayApiRequest);

            LogInfo.Information(JsonConvert.SerializeObject(gatewayApiRequest));

            GatewayApiClient gatewayApiClient = new GatewayApiClient(config);

            string response = gatewayApiClient.SendTransaction(gatewayApiRequest);

            LogInfo.Information(response);

            return TokenResponse.GetResponseResult(response);
        }

        public string RemoveToken(GridMPGSConfig mpgsConfig, string token)
        {
            GatewayApiConfig config = new GatewayApiConfig(mpgsConfig);

            GatewayApiRequest gatewayApiRequest = new GatewayApiRequest(config)
            {
                ApiMethod = "DELETE"
            };
            gatewayApiRequest.buildDeleteUrl(token);

            gatewayApiRequest.buildPayload();

            string request = JsonConvert.SerializeObject(gatewayApiRequest);

            LogInfo.Information(JsonConvert.SerializeObject(gatewayApiRequest));

            GatewayApiClient gatewayApiClient = new GatewayApiClient(config);

            string response = gatewayApiClient.executeHTTPMethod(gatewayApiRequest);

            LogInfo.Information(response);

            return TokenResponse.GetResponseResult(response); 
        }

        public string VoidTransaction(GridMPGSConfig mpgsConfig)
        {
            GatewayApiConfig config = new GatewayApiConfig(mpgsConfig);

            GatewayApiRequest gatewayApiRequest = new GatewayApiRequest(config)
            {
                ApiMethod = "VOID",
                TargetTransactionId= "ea2d89bb24",
                TransactionId=GenerateOrderId(),
                Token= "440003320900022"

            };
            gatewayApiRequest.buildRequestUrl();

            gatewayApiRequest.buildPayload();

            string request = JsonConvert.SerializeObject(gatewayApiRequest);

            LogInfo.Information(JsonConvert.SerializeObject(gatewayApiRequest));

            GatewayApiClient gatewayApiClient = new GatewayApiClient(config);

            string response = gatewayApiClient.executeHTTPMethod(gatewayApiRequest);

            LogInfo.Information(response);

            return TokenResponse.GetResponseResult(response);
        }

        public TransactionRetrieveResponseOperation GetCapturedTransaction(string receiptResponse)
        {
            TransactionResponseModel transactionResponseModel = null;

            try
            {
                transactionResponseModel = TransactionResponseModel.toTransactionResponseModel(receiptResponse);

            }
            catch (Exception ex)
            {
                LogInfo.Error($" : { EnumExtensions.GetDescription(MPGSAPIResponse.HostedCheckoutReceiptError) + " " + JsonConvert.SerializeObject(ex)}");

                throw ex;
            }

            return new TransactionRetrieveResponseOperation { TrasactionResponse = transactionResponseModel };

        }

        public TransactionRetrieveResponseOperation GetPaymentTransaction(string receiptResponse)
        {
            TransactionResponseModel transactionResponseModel = null;

            try
            {
                transactionResponseModel = TransactionResponseModel.toPaywithTokenTransactionResponseModel(receiptResponse);

            }
            catch (Exception ex)
            {
                LogInfo.Error($" : { EnumExtensions.GetDescription(MPGSAPIResponse.HostedCheckoutReceiptError) + " " + JsonConvert.SerializeObject(ex)}");

                throw ex;
            }

            return new TransactionRetrieveResponseOperation { TrasactionResponse = transactionResponseModel };

        }

        public GridMPGSConfig GetGridMPGSCombinedConfig(GridMPGSConfig config, GridMPGSDirectMIDConfig directMID )
        {
            config.MerchantId = directMID.MerchantId;
            config.Password = directMID.Password;
            config.WebhooksNotificationSecret = directMID.WebhooksNotificationSecret;            
            return config;
        }

    }
}
