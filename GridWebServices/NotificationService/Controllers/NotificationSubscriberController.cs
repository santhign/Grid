using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Models;
using NotificationService.OutboundHelper;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Core.Models;
using Core.Enums;
using Core.Extensions;
using Core.Helpers;
using Core.DataAccess;
using InfrastructureService;
using InfrastructureService.MessageQueue;
using InfrastructureService.Handlers;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;


namespace NotificationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationSubscriberController : ControllerBase
    {      
        IConfiguration _iconfiguration;
        public NotificationSubscriberController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }

        [HttpPost("process-sns-notifications")]
        public async Task<IActionResult> ProcessSnsNotifications([FromHeader(Name = "x-amz-sns-message-type")] string snsMessageType, [FromHeader(Name = "x-amz-sns-rawdelivery")] bool isRawDelivery,  string id = "")
        {
            try
            {
                if (string.IsNullOrEmpty(snsMessageType))
                {
                    LogInfo.Information(SNSNotification.EmptySNSTypeHeader.ToString());

                    return BadRequest();
                }

                if (isRawDelivery)
                {
                    string msg = "message is raw"; // for testing pupose, 
                }

                SNSSubscription notification = new SNSSubscription();

                string requestBody="";

                using (var reader = new StreamReader(Request.Body))
                {
                    requestBody =  reader.ReadToEndAsync().Result;              
                }

                var requestMessage = Amazon.SimpleNotificationService.Util.Message.ParseMessage(requestBody);

                if (!requestMessage.IsMessageSignatureValid())
                {
                    LogInfo.Information(SNSNotification.InvalidSignature.ToString());

                    return BadRequest();

                }

                //temporary storage for testing

                if (!Directory.Exists($@"SNSNotifications"))
                {
                    Directory.CreateDirectory($@"SNSNotifications");
                }

                System.IO.File.WriteAllText($@"SNSNotifications/notifications_{DateTime.UtcNow.ToString("yyyyMMddHHmmssffff")}.json", requestBody);

                // end temp

                if (string.IsNullOrEmpty(snsMessageType)) return BadRequest(); 

                if (snsMessageType == Amazon.SimpleNotificationService.Util.Message.MESSAGE_TYPE_SUBSCRIPTION_CONFIRMATION && requestMessage.IsSubscriptionType)
                {
                    Subscriber subscriber = new Subscriber(_iconfiguration);

                    var client = subscriber.GetAmazonSimpleNotificationServiceClient();

                    Amazon.SimpleNotificationService.Model.ConfirmSubscriptionResponse confirmSubscriptionResponse = await client.ConfirmSubscriptionAsync(requestMessage.TopicArn, requestMessage.Token);
                       
                    if(confirmSubscriptionResponse.HttpStatusCode == HttpStatusCode.OK)
                    {
                        LogInfo.Information(SNSNotification.SubscriptionConfirmed.ToString());
                    }

                    return Ok();
                }

                else if(snsMessageType == Amazon.SimpleNotificationService.Util.Message.MESSAGE_TYPE_UNSUBSCRIPTION_CONFIRMATION && requestMessage.IsUnsubscriptionType)
                {
                    Subscriber subscriber = new Subscriber(_iconfiguration);

                    var client = subscriber.GetAmazonSimpleNotificationServiceClient();

                    Amazon.SimpleNotificationService.Model.UnsubscribeResponse confirmUnSubscriptionResponse = await client.UnsubscribeAsync(notification.TopicArn);

                    return Ok();
                }

                else if(snsMessageType == Amazon.SimpleNotificationService.Util.Message.MESSAGE_TYPE_NOTIFICATION && requestMessage.IsNotificationType)
                {

                    ConfigDataAccess _configAccess = new ConfigDataAccess(_iconfiguration);

                    MiscHelper helper = new MiscHelper();

                    DatabaseResponse notificationConfigResponse = await _configAccess.GetConfiguration(ConfiType.Notification.ToString());

                    NotificationConfig snsConfig = helper.GetNotificationConfig((List<Dictionary<string, string>>)notificationConfigResponse.Results);

                    Subscriber subscriber = new Subscriber(_iconfiguration,snsConfig.SNSTopic,snsConfig.SQS);
                 
                    var client = subscriber.GetAmazonSimpleNotificationServiceClient();

                    Action<object> notificationSqsProcessor = ProcessNotification;

                    await subscriber.ListenAsync(notificationSqsProcessor);

                    return Ok();
                   
                }
                else
                {
                    return BadRequest();
                }  

            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok();
            }
        }

        [NonAction]
        public async void ProcessNotification(object message)
        {
            Amazon.SQS.Model.Message msg = (Amazon.SQS.Model.Message)message;

            string queMessage = msg.Body;

            SNSSubscription subscription = JsonConvert.DeserializeObject<SNSSubscription>(queMessage);
            
            NotificationMessage NotMessage = JsonConvert.DeserializeObject<NotificationMessage>(subscription.Message);

            // SNSSubscription messageObject= new SNSSubscription {  Message=me};

            //   NotificationMessage notification= JsonConvert.DeserializeObject<NotificationMessage>(messageObject.Message);

            OutboundEmail _email = new OutboundEmail();

            ConfigDataAccess _configAccess = new ConfigDataAccess(_iconfiguration);

            DatabaseResponse forgotPasswordMsgTemplate = await _configAccess.GetEmailNotificationTemplate(NotificationEvent.ForgetPassword.ToString());

            EmailTemplate template = (EmailTemplate)forgotPasswordMsgTemplate.Results;            

            var responses = await _email.SendEmail(NotMessage, _iconfiguration, template);


            foreach (Mandrill.Model.MandrillSendMessageResponse response in responses)
            {
                foreach(NotificationParams param in NotMessage.Message.parameters)
                {
                    if(response.Email==param.emailaddress)
                    {
                        DatabaseResponse notificationLogResponse = await _configAccess.CreateEMailNotificationLog(new NotificationLog { Status = response.Status.ToString()=="Sent"? 1:0, Email = response.Email, EmailTemplateID = template.EmailTemplateID, EmailBody = _email.GetMergedTemplate(param, template), EmailSubject=template.EmailSubject, ScheduledOn= subscription.Timestamp, SendOn=DateTime.UtcNow});
                    }
                }
                
            }

        }

        public static JsonSerializerSettings MicrosoftDateFormatSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
                };
            }
        }
    }
}