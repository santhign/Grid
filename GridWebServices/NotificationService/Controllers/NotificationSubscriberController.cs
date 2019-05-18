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

                //LogInfo.Information("1 - ProcessSnsNotification started with {"+ snsMessageType + "} object");

                if (string.IsNullOrEmpty(snsMessageType))
                {
                    LogInfo.Information(SNSNotification.EmptySNSTypeHeader.ToString());

                    return BadRequest();
                }

                //if (isRawDelivery)
                //{
                //    string msg = "message is raw"; // for testing pupose, 
                //}

                SNSSubscription notification = new SNSSubscription();

                string requestBody="";

                using (var reader = new StreamReader(Request.Body))
                {
                    requestBody =  reader.ReadToEndAsync().Result;              
                }
                //LogInfo.Information("2 - Now fetching Body with {" + requestBody + "} object");
                var requestMessage = Amazon.SimpleNotificationService.Util.Message.ParseMessage(requestBody);
                //LogInfo.Information("3 - Parsing messages with {" + requestMessage + "} object");
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

                if (string.IsNullOrEmpty(snsMessageType)) {
                    //LogInfo.Information("4 - snsMessageType is  {" + snsMessageType + "} object");
                    return BadRequest();
                }

                if (snsMessageType == Amazon.SimpleNotificationService.Util.Message.MESSAGE_TYPE_SUBSCRIPTION_CONFIRMATION && requestMessage.IsSubscriptionType)
                {
                    
                    Subscriber subscriber = new Subscriber(_iconfiguration);

                    var client = subscriber.GetAmazonSimpleNotificationServiceClient();
                    //LogInfo.Information("5 - before snsMessageType is  {" + client + "} object");
                    Amazon.SimpleNotificationService.Model.ConfirmSubscriptionResponse confirmSubscriptionResponse = await client.ConfirmSubscriptionAsync(requestMessage.TopicArn, requestMessage.Token);
                       
                    if(confirmSubscriptionResponse.HttpStatusCode == HttpStatusCode.OK)
                    {
                        LogInfo.Information(SNSNotification.SubscriptionConfirmed.ToString());
                    }
                    //LogInfo.Information("5.1 - before snsMessageType is  {" + client + "} object");
                    return Ok();
                }

                else if(snsMessageType == Amazon.SimpleNotificationService.Util.Message.MESSAGE_TYPE_UNSUBSCRIPTION_CONFIRMATION && requestMessage.IsUnsubscriptionType)
                {
                    Subscriber subscriber = new Subscriber(_iconfiguration);

                    var client = subscriber.GetAmazonSimpleNotificationServiceClient();

                    Amazon.SimpleNotificationService.Model.UnsubscribeResponse confirmUnSubscriptionResponse = await client.UnsubscribeAsync(notification.TopicArn);
                    //LogInfo.Information("6 - before snsMessageType is  {" + snsMessageType + "} object");
                    return Ok();
                }

                else if(snsMessageType == Amazon.SimpleNotificationService.Util.Message.MESSAGE_TYPE_NOTIFICATION && requestMessage.IsNotificationType)
                {
                    //LogInfo.Information("7 - before snsMessageType is  {" + snsMessageType + "} object");
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
            try
            {

                Amazon.SQS.Model.Message msg = (Amazon.SQS.Model.Message)message;

                string queMessage = msg.Body;
                //LogInfo.Information("8 - bsubscription is  {" + msg.MessageId + "} object");
                SNSSubscription subscription = JsonConvert.DeserializeObject<SNSSubscription>(queMessage);

                //LogInfo.Information("8 - bsubscription is  {" + subscription.MessageId + "} object");
                NotificationMessage NotMessage = JsonConvert.DeserializeObject<NotificationMessage>(subscription.Message);
                //LogInfo.Information("9 - NotMessage is  {" + NotMessage.Message +" "+ NotMessage.MessageType + "} object");
                // SNSSubscription messageObject= new SNSSubscription {  Message=me};

                //   NotificationMessage notification= JsonConvert.DeserializeObject<NotificationMessage>(messageObject.Message);
                if (NotMessage.MessageType == NotificationMsgType.Email.GetDescription())
                {

                    OutboundEmail _email = new OutboundEmail();

                    ConfigDataAccess _configAccess = new ConfigDataAccess(_iconfiguration);

                    DatabaseResponse emailTemplate = await _configAccess.GetEmailNotificationTemplate(NotMessage.Message.messagetemplate.ToString());

                    EmailTemplate template = (EmailTemplate)emailTemplate.Results;

                    var responses = await _email.SendEmail(NotMessage, _iconfiguration, template);


                    foreach (Mandrill.Model.MandrillSendMessageResponse response in responses)
                    {
                        foreach (NotificationParams param in NotMessage.Message.parameters)
                        {
                            if (response.Email == param.emailaddress)
                            {
                                DatabaseResponse notificationLogResponse = await _configAccess.CreateEMailNotificationLog(new NotificationLog {
                                    Status = response.Status.ToString() == "Sent" ? 1 : 0, Email = response.Email,
                                    EmailTemplateID = template.EmailTemplateID, EmailBody = _email.GetMergedTemplate(param, template),
                                    EmailSubject = template.EmailSubject, ScheduledOn = subscription.Timestamp, SendOn = DateTime.UtcNow });
                            }
                        }

                    }

                }
                else if (NotMessage.MessageType == NotificationMsgType.SMS.GetDescription())
                {
                    OutboundSMS _SMS = new OutboundSMS();
                    Sms smsData = new Sms();
                    ConfigDataAccess _configAccess = new ConfigDataAccess(_iconfiguration);

                    DatabaseResponse emailTemplate = await _configAccess.GetSMSNotificationTemplate(NotMessage.Message.messagetemplate.ToString());
                    SMSTemplates template = (SMSTemplates)emailTemplate.Results;                    
                    smsData.PhoneNumber = NotMessage.Message.parameters.Select(x => x.mobilenumber).FirstOrDefault();

                    smsData.SMSText = template.SMSTemplate.Replace("[NAME]", NotMessage.MessageName)
                        .Replace("[PARAM1]", NotMessage.Message.parameters.Select(x => x.param1).FirstOrDefault())
                        .Replace("[PARAM2]", NotMessage.Message.parameters.Select(x => x.param2).FirstOrDefault())
                        .Replace("[PARAM3]", NotMessage.Message.parameters.Select(x => x.param3).FirstOrDefault())
                        .Replace("[PARAM4]", NotMessage.Message.parameters.Select(x => x.param4).FirstOrDefault())
                        .Replace("[PARAM5]", NotMessage.Message.parameters.Select(x => x.param5).FirstOrDefault())
                        .Replace("[PARAM6]", NotMessage.Message.parameters.Select(x => x.param6).FirstOrDefault())
                        .Replace("[PARAM7]", NotMessage.Message.parameters.Select(x => x.param7).FirstOrDefault())
                        .Replace("[PARAM8]", NotMessage.Message.parameters.Select(x => x.param8).FirstOrDefault())
                        .Replace("[PARAM9]", NotMessage.Message.parameters.Select(x => x.param9).FirstOrDefault())
                        .Replace("[PARAM10]", NotMessage.Message.parameters.Select(x => x.param10).FirstOrDefault());
                    //LogInfo.Information("10 - SendSMS is  { "+ smsData+ "}");
                    string response = await _SMS.SendSMSNotification(smsData, _iconfiguration);
                   
                    await _configAccess.CreateSMSNotificationLog(new SMSNotificationLog()
                    {
                        Email = NotMessage.Message.parameters.Select(x => x.emailaddress).FirstOrDefault(),
                        Mobile = smsData.PhoneNumber,
                        SMSTemplateID = template.SMSTemplateID,
                        SMSText = smsData.SMSText,
                        Status = response != "failure" ? 1 : 0,
                        ScheduledOn = subscription.Timestamp,                        
                        SendOn = DateTime.UtcNow

                    });
                    //LogInfo.Information("10 - SendSMSLog is  { " + NotMessage.Message.parameters.Select(x => x.emailaddress).FirstOrDefault() + " " + smsData.PhoneNumber + " "+ response + "}");
                }
                
            }

            catch (Exception ex)
            {

                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
            }

            LogInfo.Information("Last Step for sending SMS");

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