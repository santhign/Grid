using Core.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using System.IO;
using Core;
using System.Data.SqlClient;
using Core.Models;
using Core.Extensions;
using Core.Enums;
using System.Linq;
using Newtonsoft.Json;
using Core.DataAccess;
using Serilog;
using InfrastructureService;
using InfrastructureService.MessageQueue;

namespace BuddyProcessingApp
{
    /// <summary>
    /// Publish Message To Queue class
    /// </summary>
    public class BuddyHelper
    {

        private readonly string _connectionString;

        public BuddyHelper(string connectionString)
        {
            _connectionString = connectionString;
        }        
        public async Task<BuddyCheckList> ProcessBuddy(BuddyCheckList buddy)
        {
            try
            {

                BSSAPIHelper bsshelper = new BSSAPIHelper();

                BuddyDataAccess _buddyAccess = new BuddyDataAccess();

                DatabaseResponse configResponse = await _buddyAccess.GetConfiguration(ConfiType.BSS.ToString(), _connectionString);

                GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                LogInfo.Information(JsonConvert.SerializeObject(config));

                DatabaseResponse serviceCAF = await _buddyAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString(), _connectionString);

                DatabaseResponse requestIdRes = await _buddyAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), buddy.CustomerID, (int)BSSCalls.NewSession, "", _connectionString);

                ResponseObject res = new ResponseObject();

                try
                {
                    res = await bsshelper.GetAssetInventory(config, (((List<ServiceFees>)serviceCAF.Results)).FirstOrDefault().ServiceCode, (BSSAssetRequest)requestIdRes.Results);

                    LogInfo.Information(JsonConvert.SerializeObject(res));
                }                

                catch (Exception ex)
                {
                    LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));
                    buddy.IsProcessed = false;
                    return buddy;
                }

                string AssetToSubscribe = string.Empty;

                if (res != null && (int.Parse(res.Response.asset_details.total_record_count) > 0))
                {
                     AssetToSubscribe = bsshelper.GetAssetId(res);

                    BSSNumbers numbers = new BSSNumbers();

                    numbers.FreeNumbers = bsshelper.GetFreeNumbers(res);

                    //insert these number into database
                    string json = bsshelper.GetJsonString(numbers.FreeNumbers); // json insert

                    DatabaseResponse updateBssCallFeeNumbers = await _buddyAccess.UpdateBSSCallNumbers(json, ((BSSAssetRequest)requestIdRes.Results).userid, ((BSSAssetRequest)requestIdRes.Results).BSSCallLogID, _connectionString);
                    //Block number                                    _connectionString
                    DatabaseResponse requestIdToUpdateRes = await _buddyAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), buddy.CustomerID, (int)BSSCalls.ExistingSession, AssetToSubscribe, _connectionString);

                    BSSUpdateResponseObject bssUpdateResponse = new BSSUpdateResponseObject();

                    try
                    {
                        bssUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateRes.Results, AssetToSubscribe, false);
                    }

                    catch (Exception ex)
                    {
                        LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));
                        buddy.IsProcessed = false;
                        return buddy;
                    }


                    if (bssUpdateResponse!=null &&  bsshelper.GetResponseCode(bssUpdateResponse) == "0")
                    {
                        // update buddy process

                        BuddyNumberUpdate buddyToProcess = new BuddyNumberUpdate { OrderSubscriberID = buddy.OrderSubscriberID, UserId = ((BSSAssetRequest)requestIdRes.Results).userid, NewMobileNumber = AssetToSubscribe };

                        LogInfo.Information(JsonConvert.SerializeObject(buddyToProcess));

                        DatabaseResponse buddyProcessResponse = await _buddyAccess.ProcessBuddyPlan(buddyToProcess, _connectionString);

                        LogInfo.Information(JsonConvert.SerializeObject(buddyProcessResponse));

                        if (buddyProcessResponse.ResponseCode == (int)DbReturnValue.CreateSuccess || buddyProcessResponse.ResponseCode == (int)DbReturnValue.BuddyAlreadyExists)
                        {
                            // update process status

                            buddy.IsProcessed = true;

                            return buddy;

                        }

                        else
                        {
                            // buddy process failed
                            buddy.IsProcessed = false;

                            return buddy;
                        }


                    }
                    else
                    {
                        // buddy block failed
                        buddy.IsProcessed = false;

                        return buddy;

                    }

                }
                else
                {
                    // no assets returned                                   
                    buddy.IsProcessed = false;
                    return buddy;
                }
               

            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                buddy.IsProcessed = false;

                return buddy;
            }
        }

        public async Task<int> ProcessOrderQueueMessage(int orderID)
        {
            try
            {
                BuddyDataAccess _buddyAccess = new BuddyDataAccess();

                QMDataAccess _qMDataAccess = new QMDataAccess();

                DatabaseResponse orderMqResponse = new DatabaseResponse();

                orderMqResponse = await _qMDataAccess.GetOrderMessageQueueBody(orderID, _connectionString);

                OrderQM orderDetails = new OrderQM();

                string topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _connectionString).Results.ToString().Trim();

                string pushResult = string.Empty;

                if (orderMqResponse != null && orderMqResponse.Results != null)
                {
                    orderDetails = (OrderQM)orderMqResponse.Results;

                    DatabaseResponse OrderCountResponse = await _qMDataAccess.GetCustomerOrderCount(orderDetails.customerID, _connectionString);

                    MessageQueueRequest queueRequest = new MessageQueueRequest
                    {
                        Source = CheckOutType.Orders.ToString(),
                        NumberOfRetries = 1,
                        SNSTopic = topicName,
                        CreatedOn = DateTime.Now,
                        LastTriedOn = DateTime.Now,
                        PublishedOn = DateTime.Now,
                        MessageAttribute = ((int)OrderCountResponse.Results) == 1 ? Core.Enums.RequestType.NewCustomer.GetDescription() : Core.Enums.RequestType.NewService.GetDescription(),
                        MessageBody = JsonConvert.SerializeObject(orderDetails),
                        Status = 0
                    };

                    try
                    {
                        Dictionary<string, string> attribute = new Dictionary<string, string>();

                       DatabaseResponse configValueResponse=ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _connectionString);

                        topicName = (string)configValueResponse.Results;

                        attribute.Add(EventTypeString.EventType, ((int)OrderCountResponse.Results) == 1 ? Core.Enums.RequestType.NewCustomer.GetDescription() : Core.Enums.RequestType.NewService.GetDescription());
                          
                        var publisher = new InfrastructureService.MessageQueue.Publisher(_connectionString, topicName);

                        pushResult =  await publisher.PublishAsync(orderMqResponse, attribute);                       

                        if (pushResult.Trim().ToUpper() == "OK")
                        {
                            queueRequest.Status = 1;

                            queueRequest.PublishedOn = DateTime.Now;

                            LogInfo.Information(EnumExtensions.GetDescription(CommonErrors.PendingBuddyOrderProcessed));
                             
                            await _buddyAccess.InsertMessageInMessageQueueRequest(queueRequest, _connectionString);

                            return 1;
                        }
                        else
                        {
                            // publising failed
                            queueRequest.Status = 0;

                            await _buddyAccess.InsertMessageInMessageQueueRequest(queueRequest, _connectionString);

                            return 0;
                        }

                    }
                    catch (Exception ex)
                    {
                        LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                        queueRequest.Status = 0;

                        await _buddyAccess.InsertMessageInMessageQueueRequest(queueRequest, _connectionString);

                        return 0;
                    }
                }

                else
                {
                    LogInfo.Information(EnumExtensions.GetDescription(CommonErrors.PendingBuddyMQBodyFailed));
                    return 0;
                }
                
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return 0;
            }
        }


        public async Task<string> SendEmailNotification(int CustomerID, int OrderID, IConfiguration _configuration)
        {
            string status = string.Empty;

            try
            {
                ConfigDataAccess _configAccess = new ConfigDataAccess(_configuration);

                BuddyDataAccess _buddyAccess = new BuddyDataAccess();

                CommonDataAccess _commonAccess = new CommonDataAccess(_configuration);

                DatabaseResponse templateResponse = await _configAccess.GetEmailNotificationTemplate(NotificationEvent.OrderSuccess.ToString());
               
                LogInfo.Information("Email Customer : " + CustomerID);

                // Get Customer Data from CustomerID for email and Name
                CustomerDetails customer = await _commonAccess.GetCustomerDetailByOrder(CustomerID, OrderID);

                LogInfo.Information("Email Customer data : " + JsonConvert.SerializeObject(customer));

                if (customer != null && !string.IsNullOrEmpty(customer.DeliveryEmail))
                {

                    StringBuilder orderedNumbersSb = new StringBuilder();

                    StringBuilder deliveryAddressSb = new StringBuilder();

                    orderedNumbersSb.Append("<table width='100%'>");

                    int counter = 0;

                    foreach (OrderNumber number in customer.OrderedNumbers)
                    {
                        if (counter > 0)
                        {
                            orderedNumbersSb.Append("<tr><td width='100%' colspan='3'> </td></tr>");
                        }
                        orderedNumbersSb.Append("<tr><td width='25%'>MobileNumber :<td width='20%'>");
                        orderedNumbersSb.Append(number.MobileNumber);
                        orderedNumbersSb.Append("</td><td width ='55%'></td></tr>");
                        orderedNumbersSb.Append("<tr><td width='25%'>Plan :<td width='20%'>");
                        orderedNumbersSb.Append(number.PlanMarketingName);
                        orderedNumbersSb.Append("</td><td width ='55%'>");
                        orderedNumbersSb.Append(number.PricingDescription);
                        orderedNumbersSb.Append("</td></tr> ");
                        counter++;
                    }

                    orderedNumbersSb.Append("</table>");

                    if (!string.IsNullOrEmpty(customer.ShippingBuildingName))
                    {
                        deliveryAddressSb.Append(customer.ShippingBuildingNumber);
                    }

                    if (!string.IsNullOrEmpty(customer.ShippingStreetName))
                    {
                        if (deliveryAddressSb.ToString() != "")
                        {
                            deliveryAddressSb.Append(" ");
                        }

                        deliveryAddressSb.Append(customer.ShippingStreetName);
                    }

                    deliveryAddressSb.Append("<br />");

                    StringBuilder shippingAddr2 = new StringBuilder();

                    if (!string.IsNullOrEmpty(customer.ShippingFloor))
                    {
                        shippingAddr2.Append(customer.ShippingFloor);
                    }

                    if (!string.IsNullOrEmpty(customer.ShippingUnit))
                    {
                        if (shippingAddr2.ToString() != "")
                        {
                            shippingAddr2.Append(" ");
                        }
                        shippingAddr2.Append(customer.ShippingUnit);
                    }

                    if (!string.IsNullOrEmpty(customer.ShippingBuildingName))
                    {
                        if (shippingAddr2.ToString() != "")
                        {
                            shippingAddr2.Append(" ");
                        }

                        shippingAddr2.Append(customer.ShippingBuildingName);
                    }

                    deliveryAddressSb.Append(shippingAddr2.ToString());

                    deliveryAddressSb.Append("<br />");

                    if (!string.IsNullOrEmpty(customer.ShippingPostCode))
                    {
                        deliveryAddressSb.Append(customer.ShippingPostCode);
                    }

                    string deliveryDate = customer.SlotDate.ToString("dd MMM yyyy") + " " + new DateTime(customer.SlotFromTime.Ticks).ToString("hh:mm tt") 
                        + " to " + new DateTime(customer.SlotToTime.Ticks).ToString("hh:mm tt");

                    var notificationMessage = MessageHelper.GetMessage(customer.ToEmailList, customer.Name,

                                                        NotificationEvent.OrderSuccess.ToString(),

                                                     ((EmailTemplate)templateResponse.Results).TemplateName, _configuration, customer.DeliveryEmail, 
                                                     customer.OrderNumber, orderedNumbersSb.ToString(), deliveryAddressSb.ToString(),
                                                     customer.AlternateRecipientName == null ? customer.Name : customer.AlternateRecipientName, 
                                                     customer.AlternateRecipientContact == null ? customer.ShippingContactNumber : customer.AlternateRecipientContact, 
                                                     string.IsNullOrEmpty(customer.AlternateRecipientEmail) ? customer.DeliveryEmail : customer.AlternateRecipientEmail, 
                                                     deliveryDate, customer.ReferralCode);

                    DatabaseResponse notificationResponse = await _configAccess.GetConfiguration(ConfiType.Notification.ToString());

                    MiscHelper parser = new MiscHelper();

                    var notificationConfig = parser.GetNotificationConfig((List<Dictionary<string, string>>)notificationResponse.Results);

                    LogInfo.Information("Email Message to send  " + JsonConvert.SerializeObject(notificationResponse));

                    Publisher orderSuccessNotificationPublisher = new Publisher(_configuration, notificationConfig.SNSTopic);

                    try
                    {

                        status = await orderSuccessNotificationPublisher.PublishAsync(notificationMessage);
                    }
                    catch (Exception ex)
                    {
                        LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + "publishing :" + status);
                        throw ex;
                    }

                    LogInfo.Information("Email send status : " + status + " " + JsonConvert.SerializeObject(notificationMessage));

                    status = await SendOrderSuccessSMSNotification(customer, _configuration);

                    try
                    {
                        DatabaseResponse notificationLogResponse = await _configAccess.CreateEMailNotificationLogForDevPurpose(
                                    new NotificationLogForDevPurpose
                                    {
                                        EventType = NotificationEvent.OrderSuccess.ToString(),
                                        Message = JsonConvert.SerializeObject(notificationMessage)

                                    });
                    }
                    catch (Exception ex)
                    {
                        LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + "Email send:" + OrderID);
                        throw ex;
                    }
                }


            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + "OrderID:" + OrderID);
                throw ex;
            }

            return status;
        }

        public async Task<string> SendOrderSuccessSMSNotification(CustomerDetails customer, IConfiguration _configuration)
        {
            string status = string.Empty;
            try
            {             

                ConfigDataAccess _configAccess = new ConfigDataAccess(_configuration);

                DatabaseResponse smsTemplateResponse = await _configAccess.GetSMSNotificationTemplate(NotificationEvent.OrderSuccess.ToString());

                var notificationMessage = MessageHelper.GetSMSMessage(NotificationEvent.OrderSuccess.ToString(), ((SMSTemplates)smsTemplateResponse.Results).TemplateName, customer.Name, customer.DeliveryEmail, customer.ShippingContactNumber, customer.OrderNumber, customer.SlotDate.ToString("dd MMM yyyy"), new DateTime(customer.SlotFromTime.Ticks).ToString("hh:mm tt") + " to " + new DateTime(customer.SlotToTime.Ticks).ToString("hh:mm tt"));

                DatabaseResponse notificationResponse = await _configAccess.GetConfiguration(ConfiType.Notification.ToString());

                MiscHelper parser = new MiscHelper();

                var notificationConfig = parser.GetNotificationConfig((List<Dictionary<string, string>>)notificationResponse.Results);

                Publisher orderSuccessSMSNotificationPublisher = new Publisher(_configuration, notificationConfig.SNSTopic);

                status = await orderSuccessSMSNotificationPublisher.PublishAsync(notificationMessage);

                LogInfo.Information("SMS send status : " + status + " " + JsonConvert.SerializeObject(notificationMessage));

                return status;
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + "SMS failure for OrderNumber:" + customer.OrderNumber);

                throw ex;
            }
        }
    }
}

