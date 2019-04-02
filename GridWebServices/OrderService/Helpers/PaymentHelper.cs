using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderService.Models;
using System.IO;
using InfrastructureService;
using Core.Helpers;
using Core.Enums;
using Core.Extensions;
using Newtonsoft.Json;

namespace OrderService.Helpers
{
    public class PaymentHelper
    {
        public static string GenerateOrderId()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 10);
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
        public Checkout CreateCheckoutSession(GridMPGSConfig mpgsConfig)
        {
            try
            {
                LogInfo.Information(EnumExtensions.GetDescription(MPGSAPIOperation.CREATE_CHECKOUT_SESSION));

                GatewayApiConfig config = new GatewayApiConfig(mpgsConfig);

                GatewayApiRequest gatewayApiRequest = new GatewayApiRequest(config);

                gatewayApiRequest.ApiOperation = MPGSAPIOperation.CREATE_CHECKOUT_SESSION.ToString();

                gatewayApiRequest.OrderId = PaymentHelper.GenerateOrderId();

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

        public TransactionRetrieveResponseOperation RetrieveCheckOutTransaction(GridMPGSConfig mpgsConfig,CheckOutResponseUpdate responseUpdate)
        {
            try
            {
                LogInfo.Information($" {EnumExtensions.GetDescription(MPGSAPIResponse.HostedCheckoutRetrieveReceipt) + " orderId  " +  responseUpdate.MPGSOrderID} result {responseUpdate.Result} sessionId {responseUpdate.CheckOutSessionID}");

                GatewayApiConfig config = new GatewayApiConfig(mpgsConfig);             

                if (responseUpdate.Result == MPGSAPIResponse.SUCCESS.ToString())
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
                    TransactionResponseModel transactionResponseModel = null;                  

                    try
                    {
                        transactionResponseModel = TransactionResponseModel.toTransactionResponseModel(response);

                    }
                    catch (Exception ex)
                    {
                        LogInfo.Error($" : { EnumExtensions.GetDescription(MPGSAPIResponse.HostedCheckoutReceiptError) + " " + JsonConvert.SerializeObject(ex)}");

                        throw ex;
                    }
                    

                    return new TransactionRetrieveResponseOperation { TrasactionResponse= transactionResponseModel }; 

                }
                else
                {
                    LogInfo.Error($"  {MPGSAPIResponse.Unsuccessful.ToString()+ ".  " + responseUpdate.Result}");

                    return new TransactionRetrieveResponseOperation
                    {                       
                        Cause = EnumExtensions.GetDescription(MPGSAPIResponse.Unsuccessful),

                        Message = EnumExtensions.GetDescription(MPGSAPIResponse.ProblemCompletingTransaction),
                    };
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
            config.WebhooksNotificationSecret = configDict.Single(x => x["key"] == "GatewayGridWebhookSecret")["value"];
            return config;
        }
    }
}
