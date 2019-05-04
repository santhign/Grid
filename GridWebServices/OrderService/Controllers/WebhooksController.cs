using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Core.Enums;
using Core.Helpers;
using Core.Extensions;
using Core.Models;
using Microsoft.Extensions.Options;
using OrderService.Models;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Extensions.Configuration;
using InfrastructureService;
using OrderService.Helpers;
using OrderService.DataAccess;
using InfrastructureService.MessageQueue;
using InfrastructureService.Handlers;
using OrderService.Enums;


namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhooksController : ControllerBase
    {
        private WebhookNotificationModel _notificationModel;

        IConfiguration _iconfiguration;       
        private readonly IMessageQueueDataAccess _messageQueueDataAccess;
        public WebhooksController(IConfiguration configuration, IMessageQueueDataAccess messageQueueDataAccess)
        {
            _iconfiguration = configuration;

            _messageQueueDataAccess = messageQueueDataAccess;
        }
      
        [HttpPost("process-webhook")]
        public  IActionResult ProcessWebhook([FromBody] WebhookNotificationModel notification, [FromHeader(Name = "X-Notification-Secret")] string notificationSecret)
        {
            try
            {
                if (notification.OrderStatus == MPGSAPIResponse.CAPTURED.ToString())
                {
                    _notificationModel = notification;

                    notification.Timestamp = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);

                    PaymentHelper.InitWebhooksNotificationsFolder();

                    DatabaseResponse configResponse = ConfigHelper.GetValue(ConfiType.MPGS.ToString(), _iconfiguration);

                    PaymentHelper gatewayHelper = new PaymentHelper();

                    GridMPGSConfig gatewayConfig = gatewayHelper.GetGridMPGSConfig((List<Dictionary<string, string>>)configResponse.Results);

                    GatewayApiConfig _gatewayApiConfig = new GatewayApiConfig(gatewayConfig);

                    LogInfo.Information("Webhooks controller ProcessWebhook action");

                    Debug.WriteLine($"-------------GatewayApiConfig.WebhooksNotificationSecret {_gatewayApiConfig.WebhooksNotificationSecret} --- {notificationSecret}");

                    if (_gatewayApiConfig.WebhooksNotificationSecret == null || notificationSecret == null || notificationSecret != _gatewayApiConfig.WebhooksNotificationSecret)
                    {
                        LogInfo.Error("Webhooks notification secret doesn't match, so not processing the incoming request!");

                        return Ok();
                    }

                    LogInfo.Information("Webhooks notification secret matches");

                    var parent = Task.Factory.StartNew(() =>
                    {
                        Action MPGSOrderFinalProcessing = FinalPaymentProcessing;

                        Task.Factory.StartNew(MPGSOrderFinalProcessing, TaskCreationOptions.DenyChildAttach);

                    });
                }
               

                return Ok();

            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok();
            }
        }
        [HttpGet("list-webhook-notifications")]
        public ActionResult ListWebhooks()
        {
            LogInfo.Information("Webhooks controller ListWebhooks action");

            IList<WebhookNotificationModel> notifications = new List<WebhookNotificationModel>();

            string[] files = Directory.GetFiles(GatewayApiConfig.WEBHOOKS_NOTIFICATION_FOLDER); // remove this folder after testing

            if (files != null && files.Length > 0)
            {
                foreach (string fileName in files)
                {
                    LogInfo.Information($"Reading webhook notification file {fileName}");

                    string json = System.IO.File.ReadAllText(fileName);

                    LogInfo.Information($"File ReadAllText = {json}");

                    WebhookNotificationModel notification = JsonConvert.DeserializeObject<WebhookNotificationModel>(json);

                    LogInfo.Information($"Webhook notification created from file, WebhookNotificationModel {notification}");

                    notifications.Add(notification);
                }
            }
            else
            {
                LogInfo.Information("No webhook notifications files found!");
            }

            return Ok(notifications);
        }

        [HttpPost("process-webhook-mq")]
        public async Task<IActionResult> ProcessWebhookMq([FromBody] WebhookNotificationModel notification, [FromHeader(Name = "X-Notification-Secret")] string notificationSecret)
        {
            try
            {
                var watch = new System.Diagnostics.Stopwatch();

                watch.Start();

                PaymentHelper.InitWebhooksNotificationsFolder();

                DatabaseResponse configResponse = ConfigHelper.GetValue(ConfiType.MPGS.ToString(), _iconfiguration);

                PaymentHelper gatewayHelper = new PaymentHelper();

                GridMPGSConfig gatewayConfig = gatewayHelper.GetGridMPGSConfig((List<Dictionary<string, string>>)configResponse.Results);

                GatewayApiConfig _gatewayApiConfig = new GatewayApiConfig(gatewayConfig);

                LogInfo.Information("Webhooks controller ProcessWebhook action");

                Debug.WriteLine($"-------------GatewayApiConfig.WebhooksNotificationSecret {_gatewayApiConfig.WebhooksNotificationSecret} --- {notificationSecret}");

                if (_gatewayApiConfig.WebhooksNotificationSecret == null || notificationSecret == null || notificationSecret != _gatewayApiConfig.WebhooksNotificationSecret)
                {
                    LogInfo.Error("Webhooks notification secret doesn't match, so not processing the incoming request!");

                    return Ok();
                }

                LogInfo.Information("Webhooks notification secret matches");

                string json = JsonConvert.SerializeObject(notification);

                LogInfo.Information($"Webhooks notification model {json}");

                WebhookDataAccess _webhookAccess = new WebhookDataAccess(_iconfiguration);

                DatabaseResponse databaseResponse = _webhookAccess.UpdateMPGSWebhookNotification(notification);

                // epoch
                notification.Timestamp = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);

                System.IO.File.WriteAllText($@"{GatewayApiConfig.WEBHOOKS_NOTIFICATION_FOLDER}/WebHookNotifications_{notification.Timestamp}.json", json);

                // need to change config settings here
                Publisher mpgsWhNotificationPublisher = new Publisher(_iconfiguration, "WebhookNotification-Payment");

                 await  mpgsWhNotificationPublisher.PublishAsync(notification);

                watch.Stop();

                // we can trace this and decide wether to implement a queue here or not
                LogInfo.Information($"MPGS Webhook Execution Time: {watch.ElapsedMilliseconds} ms");

                /*               
                The gateway will consider the delivery of the Webhook notification as successful
                
                if  system responds with a successful acknowledgement message containing 
                
                HTTP 200 Status Code within 30 seconds.  */

                return Ok();
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok();
            }


        }

        [HttpGet("aws-mq")]
        public async Task<IActionResult> GetAWSMQ()
        {
            try
            {
                //change the config settings here
               Subscriber mpgsWhNotificationPublisher = new Subscriber(_iconfiguration, "WebhookNotification-Payment", "OrderPayment");

               Action<object> MPGSOrderFinalProcessing = SQSHandler.FinalProcessing;

               await   mpgsWhNotificationPublisher.ListenAsync(MPGSOrderFinalProcessing);

                return Ok();
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok();
            }

        }

        [NonAction]
        public void ProcessPayment(Action paymentProcessor)
        {
            try
            {
                paymentProcessor();
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
            }

        }

        [NonAction]
        public void FinalPaymentProcessing()
        {
            try
            {
                ProcessPayment(_notificationModel);
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
            }
        }

        [NonAction]
        public async void ProcessPayment(WebhookNotificationModel notification)
        {
            try
            {   
                string json = JsonConvert.SerializeObject(notification);

                LogInfo.Information($"Webhooks notification model {json}");

                WebhookDataAccess _webhookAccess = new WebhookDataAccess(_iconfiguration);

                DatabaseResponse databaseResponse = _webhookAccess.UpdateMPGSWebhookNotification(notification);

                // epoch


                System.IO.File.WriteAllText($@"{GatewayApiConfig.WEBHOOKS_NOTIFICATION_FOLDER}/WebHookNotifications_{notification.Timestamp}.json", json);


                CheckOutResponseUpdate updateRequest = new CheckOutResponseUpdate { MPGSOrderID = notification.OrderId, Result = notification.OrderStatus };

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                //update checkout details

                DatabaseResponse updateCheckoutDetailsResponse = await _orderAccess.UpdateCheckOutResponse(updateRequest);

                // retrieve transaction details from MPGS

                DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.MPGS.ToString());

                PaymentHelper gatewayHelper = new PaymentHelper();

                GridMPGSConfig gatewayConfig = gatewayHelper.GetGridMPGSConfig((List<Dictionary<string, string>>)configResponse.Results);

                TransactionRetrieveResponseOperation transactionResponse = new TransactionRetrieveResponseOperation();

                transactionResponse = gatewayHelper.RetrieveCheckOutTransaction(gatewayConfig, updateRequest);

                DatabaseResponse paymentProcessingRespose = new DatabaseResponse();

                paymentProcessingRespose = await _orderAccess.UpdateCheckOutReceipt(transactionResponse.TrasactionResponse);
                               
                if (paymentProcessingRespose.ResponseCode == (int)DbReturnValue.TransactionSuccess)
                {
                    LogInfo.Information(EnumExtensions.GetDescription(DbReturnValue.TransactionSuccess));

                    QMHelper qMHelper = new QMHelper(_iconfiguration, _messageQueueDataAccess);

                    if (await qMHelper.ProcessSuccessTransaction(updateRequest) == 1)
                    {
                        LogInfo.Information(EnumExtensions.GetDescription(CommonErrors.ProcessingQue));
                    }

                    else
                    {
                        // 0
                        LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.SourceTypeNotFound) + " " + EnumExtensions.GetDescription(CommonErrors.ProcessingQueFailed));
                      
                    }
                }
           
                else
                {
                    LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.TransactionFailed));
                }
            }
            catch(Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
            }
        }

    }
}