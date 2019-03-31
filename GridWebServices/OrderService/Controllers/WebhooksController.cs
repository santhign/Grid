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
using Core.Helpers;
using OrderService.DataAccess;

namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhooksController : ControllerBase
    {
      
        IConfiguration _iconfiguration;
        public WebhooksController( IConfiguration configuration)
        {            
            _iconfiguration = configuration;
        }

        [HttpPost("process-webhook")]
        public IActionResult ProcessWebhook([FromBody] WebhookNotificationModel notification, [FromHeader(Name = "X-Notification-Secret")] string notificationSecret)
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

                watch.Stop();

                // we can trace this and decide wether to implement a queue here or not
                LogInfo.Information($"MPGS Webhook Execution Time: {watch.ElapsedMilliseconds} ms");
                                  
                /*               
                The gateway will consider the delivery of the Webhook notification as successful
                
                if  system responds with a successful acknowledgement message containing 
                
                HTTP 200 Status Code within 30 seconds.  */

                return Ok();
            }
            catch(Exception ex)
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
    }
}