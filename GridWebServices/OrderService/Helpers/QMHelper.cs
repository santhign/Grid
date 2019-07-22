using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.DataAccess;
using Core.Models;
using Core.Enums;
using Core.Extensions;
using InfrastructureService;
using Core.Helpers;
using System.IO;
using OrderService.Enums;
using Newtonsoft.Json;
using Core.DataAccess;
using InfrastructureService.MessageQueue;
using System.Text;



namespace OrderService.Helpers
{
    public class QMHelper
    {
        List<BuddyCheckList> buddyActionList = new List<BuddyCheckList>();

        IConfiguration _iconfiguration;

        private readonly IMessageQueueDataAccess _messageQueueDataAccess;

        public QMHelper(IConfiguration configuration, IMessageQueueDataAccess messageQueueDataAccess)
        {
            _iconfiguration = configuration;

            _messageQueueDataAccess = messageQueueDataAccess; 
        }   
        
        public async Task<int> ProcessSuccessTransaction(CheckOutResponseUpdate updateRequest)
        {
            try
            {

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse sourceTyeResponse = new DatabaseResponse();

                sourceTyeResponse = await _orderAccess.GetSourceTypeByMPGSSOrderId(updateRequest.MPGSOrderID);

                if (sourceTyeResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                {
                    if (((OrderSource)sourceTyeResponse.Results).SourceType == CheckOutType.ChangeRequest.ToString())
                    {
                        var details = await _messageQueueDataAccess.GetMessageDetails(updateRequest.MPGSOrderID);

                        if (details != null)
                        {
                            MessageBodyForCR msgBody = new MessageBodyForCR();

                            string topicName = string.Empty, pushResult = string.Empty;

                            try
                            {
                                Dictionary<string, string> attribute = new Dictionary<string, string>();

                                msgBody = await _messageQueueDataAccess.GetMessageBodyByChangeRequest(details.ChangeRequestID);

                                DatabaseResponse changeRequestTypeResponse = await _orderAccess.GetChangeRequestTypeFromID(details.ChangeRequestID);

                                if(((string) changeRequestTypeResponse.Results)== NotificationEvent.ReplaceSIM.ToString())
                                {
                                    if(msgBody.SlotDate!=null)
                                    {
                                        CustomerDetails customer = new CustomerDetails
                                        {

                                            Name = msgBody.Name,
                                            DeliveryEmail = msgBody.Email,
                                            ShippingContactNumber = msgBody.ShippingContactNumber,
                                            OrderNumber = msgBody.OrderNumber,
                                            SlotDate = msgBody.SlotDate ?? DateTime.Now,
                                            SlotFromTime = msgBody.SlotFromTime ?? DateTime.Now.TimeOfDay,
                                            SlotToTime = msgBody.SlotToTime ?? DateTime.Now.TimeOfDay
                                        };

                                        string status = await SendOrderSuccessSMSNotification(customer, NotificationEvent.ReplaceSIM.ToString());
                                    }                                   
                                }                               

                                if (details.RequestTypeID == (int)Core.Enums.RequestType.ReplaceSIM)
                                {
                                    topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration).Results.ToString().Trim();
                                    attribute.Add(EventTypeString.EventType, Core.Enums.RequestType.ReplaceSIM.GetDescription());
                                    pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, msgBody, attribute);
                                }
                                if (pushResult.Trim().ToUpper() == "OK")
                                {
                                    MessageQueueRequest queueRequest = new MessageQueueRequest
                                    {
                                        Source = Source.ChangeRequest,
                                        NumberOfRetries = 1,
                                        SNSTopic = topicName,
                                        CreatedOn = DateTime.Now,
                                        LastTriedOn = DateTime.Now,
                                        PublishedOn = DateTime.Now,
                                        MessageAttribute = Core.Enums.RequestType.ReplaceSIM.GetDescription(),
                                        MessageBody = JsonConvert.SerializeObject(msgBody),
                                        Status = 1
                                    };
                                    await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                                }
                                else
                                {
                                    MessageQueueRequest queueRequest = new MessageQueueRequest
                                    {
                                        Source = Source.ChangeRequest,
                                        NumberOfRetries = 1,
                                        SNSTopic = topicName,
                                        CreatedOn = DateTime.Now,
                                        LastTriedOn = DateTime.Now,
                                        PublishedOn = DateTime.Now,
                                        MessageAttribute = Core.Enums.RequestType.ReplaceSIM.GetDescription(),
                                        MessageBody = JsonConvert.SerializeObject(msgBody),
                                        Status = 0
                                    };
                                    await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                                }

                            }
                            catch (Exception ex)
                            {
                                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                                MessageQueueRequestException queueRequest = new MessageQueueRequestException
                                {
                                    Source = Source.ChangeRequest,
                                    NumberOfRetries = 1,
                                    SNSTopic = string.IsNullOrWhiteSpace(topicName) ? null : topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.ReplaceSIM.GetDescription().ToString(),
                                    MessageBody = msgBody != null ? JsonConvert.SerializeObject(msgBody) : null,
                                    Status = 0,
                                    Remark = "Error Occured in ProcessSuccessTransaction",
                                    Exception = new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical)

                                };

                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequestException(queueRequest);
                            }
                          
                        }

                        return 3;
                    }

                    else if (((OrderSource)sourceTyeResponse.Results).SourceType == CheckOutType.Orders.ToString())
                    {
                        try
                        {
                            LogInfo.Information("Calling SendEmailNotification");
                            string emailStatus = await SendEmailNotification(updateRequest.MPGSOrderID, ((OrderSource)sourceTyeResponse.Results).SourceID);
                            LogInfo.Information("Email Send status for : " + emailStatus);
                        }

                        catch (Exception ex)
                        {
                            LogInfo.Information("Email Send failed");
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                        }

                        ProcessOrderQueueMessage(((OrderSource)sourceTyeResponse.Results).SourceID);

                        return 3; // not buddy plan; MQ send  
                    }

                    else if (((OrderSource)sourceTyeResponse.Results).SourceType == CheckOutType.AccountInvoices.ToString())
                    {
                        //send invoice queue message                     

                        ProcessAccountInvoiceQueueMessage(((OrderSource)sourceTyeResponse.Results).SourceID);

                        return 3;
                    }

                    else
                    {
                        return 5; // incorrect CheckOutType, no chance to reach here, but just to do 
                                  //returnn from all code path, because in all of the above I need to keep CheckOutType check 
                    }
                }

                else
                {
                    // unable to get sourcetype form db

                    return 4;

                }
            }
            catch(Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                return 0;
            }
        }

        public async void ProcessOrderQueueMessage(int orderID)
        {
            try
            {
                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse orderMqResponse = new DatabaseResponse();

                orderMqResponse = await _messageQueueDataAccess.GetOrderMessageQueueBody(orderID);

                OrderQM orderDetails = new OrderQM();

                string topicName = string.Empty;

                string pushResult = string.Empty;

                if (orderMqResponse != null && orderMqResponse.Results != null)
                {
                    orderDetails = (OrderQM)orderMqResponse.Results;

                    DatabaseResponse OrderCountResponse = await _orderAccess.GetCustomerOrderCount(orderDetails.customerID);

                    try
                    {
                        Dictionary<string, string> attribute = new Dictionary<string, string>();

                        topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration).Results.ToString().Trim();

                        if (string.IsNullOrWhiteSpace(topicName))
                        {
                            throw new NullReferenceException("topicName is null for Order (" + orderID + ") for RemoveVAS Request Service API");
                        }

                        attribute.Add(EventTypeString.EventType, ((OrderCount)OrderCountResponse.Results).SuccessfulOrders == 1 ? Core.Enums.RequestType.NewCustomer.GetDescription() : Core.Enums.RequestType.NewService.GetDescription());

                        pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, orderDetails, attribute);

                        if (pushResult.Trim().ToUpper() == "OK")
                        {
                            MessageQueueRequest queueRequest = new MessageQueueRequest
                            {
                                Source = CheckOutType.Orders.ToString(),
                                NumberOfRetries = 1,
                                SNSTopic = topicName,
                                CreatedOn = DateTime.Now,
                                LastTriedOn = DateTime.Now,
                                PublishedOn = DateTime.Now,
                                MessageAttribute = ((OrderCount)OrderCountResponse.Results).SuccessfulOrders == 1 ? Core.Enums.RequestType.NewCustomer.GetDescription() : Core.Enums.RequestType.NewService.GetDescription(),
                                MessageBody = JsonConvert.SerializeObject(orderDetails),
                                Status = 1
                            };
                            await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                        }
                        else
                        {
                            MessageQueueRequest queueRequest = new MessageQueueRequest
                            {
                                Source = CheckOutType.Orders.ToString(),
                                NumberOfRetries = 1,
                                SNSTopic = topicName,
                                CreatedOn = DateTime.Now,
                                LastTriedOn = DateTime.Now,
                                PublishedOn = DateTime.Now,
                                MessageAttribute = ((OrderCount)OrderCountResponse.Results).SuccessfulOrders == 1 ? Core.Enums.RequestType.NewCustomer.GetDescription() : Core.Enums.RequestType.NewService.GetDescription(),
                                MessageBody = orderDetails != null ? JsonConvert.SerializeObject(orderDetails) : null,
                                Status = 0
                            };
                            await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                        }

                    }
                    catch (Exception ex)
                    {
                        LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                        MessageQueueRequestException queueRequest = new MessageQueueRequestException
                        {
                            Source = CheckOutType.Orders.ToString(),
                            NumberOfRetries = 1,
                            SNSTopic = topicName,
                            CreatedOn = DateTime.Now,
                            LastTriedOn = DateTime.Now,
                            PublishedOn = DateTime.Now,
                            MessageAttribute = ((OrderCount)OrderCountResponse.Results).SuccessfulOrders == 1 ? Core.Enums.RequestType.NewCustomer.GetDescription() : Core.Enums.RequestType.NewService.GetDescription(),
                            MessageBody = JsonConvert.SerializeObject(orderDetails),
                            Status = 0
                        };
                        await _messageQueueDataAccess.InsertMessageInMessageQueueRequestException(queueRequest);
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async void ProcessAccountInvoiceQueueMessage(int InvoiceID)
        {
            try
            {
                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse accountTypeResponse = await _orderAccess.GetInvoiceRemarksFromInvoiceID(InvoiceID);

                if (((string)accountTypeResponse.Results) == "RecheduleDeliveryInformation")
                {
                    DatabaseResponse orderMqResponse = new DatabaseResponse();
                    orderMqResponse = await _messageQueueDataAccess.GetRescheduleMessageQueueBody(InvoiceID);

                    QMHelper qMHelper = new QMHelper(_iconfiguration, _messageQueueDataAccess);
                    var result = await qMHelper.SendMQ(orderMqResponse);
                    var invoiceDetails = (RescheduleDeliveryMessage)orderMqResponse.Results;

                    if (invoiceDetails != null && invoiceDetails.slotDate != null)
                    {
                        CustomerDetails customer = new CustomerDetails
                        {
                            
                            Name = invoiceDetails.name,
                            DeliveryEmail = invoiceDetails.email,
                            ShippingContactNumber = invoiceDetails.shippingContactNumber,
                            OrderNumber = invoiceDetails.orderNumber,
                            SlotDate = invoiceDetails.slotDate ?? DateTime.Now,
                            SlotFromTime = invoiceDetails.slotFromTime ?? DateTime.Now.TimeOfDay,
                            SlotToTime = invoiceDetails.slotToTime ?? DateTime.Now.TimeOfDay
                        };

                        string status = await SendOrderSuccessSMSNotification(customer, NotificationEvent.RescheduleDelivery.ToString());
                    }
                }

                else
                {

                    DatabaseResponse invoiceMqResponse = new DatabaseResponse();

                    invoiceMqResponse = await _messageQueueDataAccess.GetAccountInvoiceMessageQueueBody(InvoiceID);

                    InvoceQM invoiceDetails = new InvoceQM();

                    string topicName = string.Empty;

                    string pushResult = string.Empty;

                    if (invoiceMqResponse != null && invoiceMqResponse.Results != null)
                    {
                        invoiceDetails = (InvoceQM)invoiceMqResponse.Results;

                        // invoiceDetails.invoicelist= await GetInvoiceList(invoiceDetails.customerID);

                        invoiceDetails.paymentmode = invoiceDetails.CardFundMethod == EnumExtensions.GetDescription(PaymentMode.CC) ? PaymentMode.CC.ToString() : PaymentMode.DC.ToString();

                        MessageQueueRequest queueRequest = new MessageQueueRequest
                        {
                            Source = CheckOutType.AccountInvoices.ToString(),
                            NumberOfRetries = 1,
                            SNSTopic = topicName,
                            CreatedOn = DateTime.Now,
                            LastTriedOn = DateTime.Now,
                            PublishedOn = DateTime.Now,
                            MessageAttribute = EnumExtensions.GetDescription(RequestType.PayBill),
                            MessageBody = JsonConvert.SerializeObject(invoiceDetails),
                            Status = 0
                        };

                        try
                        {
                            Dictionary<string, string> attribute = new Dictionary<string, string>();

                            topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration).Results.ToString().Trim();

                            attribute.Add(EventTypeString.EventType, EnumExtensions.GetDescription(RequestType.PayBill));

                            pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, invoiceDetails, attribute);

                            queueRequest.PublishedOn = DateTime.Now;

                            if (pushResult.Trim().ToUpper() == "OK")
                            {
                                queueRequest.Status = 1;

                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                            else
                            {
                                queueRequest.Status = 0;

                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }

                        }
                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                            queueRequest.Status = 0;

                            await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
            }
        }
        public void ProcessBuddy(Action buddyProcessor)
        {
            try
            {
                buddyProcessor();
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                throw ex;
            }

        }

        public async Task<int> FinalBuddyProcessing()
        {
            try
            {
                BSSAPIHelper bsshelper = new BSSAPIHelper();

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                try
                {
                    foreach(BuddyCheckList b in buddyActionList)
                    {
                      int process= await  ProcessBuddy(b, config, (((List<ServiceFees>)serviceCAF.Results)).FirstOrDefault().ServiceCode);
                    }                   

                    List<BuddyCheckList> unProcessedBuddies = buddyActionList.Where(b => b.IsProcessed == false).ToList();

                    if(unProcessedBuddies!=null && unProcessedBuddies.Count>0)
                    {
                        foreach (BuddyCheckList upBuddy in unProcessedBuddies)
                        {
                           DatabaseResponse upBuddyCreateResponse= await   _orderAccess.CreatePendingBuddyList(upBuddy);
                        }

                        return 0;
                    }
                    else
                    {
                       
                        ProcessOrderQueueMessage(buddyActionList[0].OrderID);

                        return 1;
                    }

                }

                catch(Exception ex)
                {
                    LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                    return 0;
                }               

            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                return 0;
            }
            
        }
               
        public async Task<int> ProcessBuddy(BuddyCheckList buddy, GridBSSConfi config, int serviceCode)
        {
            try
            {
               
                BSSAPIHelper bsshelper = new BSSAPIHelper();

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse requestIdRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), buddy.CustomerID, (int)BSSCalls.NewSession, "");

                ResponseObject res = new ResponseObject();

                try
                {
                    res = await bsshelper.GetAssetInventory(config, serviceCode, (BSSAssetRequest)requestIdRes.Results);
                }

                catch (Exception ex)
                {
                    LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                    buddy.IsProcessed = false;

                    return 0;
                }

                string AssetToSubscribe = string.Empty;

                if (res != null)
                {
                    AssetToSubscribe = bsshelper.GetAssetId(res);

                    BSSNumbers numbers = new BSSNumbers();

                    numbers.FreeNumbers = bsshelper.GetFreeNumbers(res);

                    //insert these number into database
                    string json = bsshelper.GetJsonString(numbers.FreeNumbers); // json insert

                    DatabaseResponse updateBssCallFeeNumbers = await _orderAccess.UpdateBSSCallNumbers(json, ((BSSAssetRequest)requestIdRes.Results).userid, ((BSSAssetRequest)requestIdRes.Results).BSSCallLogID);
                }

                else
                {
                    return 0;
                }

                if (res != null && (int.Parse(res.Response.asset_details.total_record_count) > 0))
                {
                    //Block number                                    

                    DatabaseResponse requestIdToUpdateRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), buddy.CustomerID, (int)BSSCalls.ExistingSession, AssetToSubscribe);

                    BSSUpdateResponseObject bssUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateRes.Results, AssetToSubscribe, false);

                    if (bsshelper.GetResponseCode(bssUpdateResponse) == "0")
                    {
                        // update buddy process

                        BuddyNumberUpdate buddyToProcess = new BuddyNumberUpdate { OrderSubscriberID = buddy.OrderSubscriberID, UserId = ((BSSAssetRequest)requestIdRes.Results).userid, NewMobileNumber = AssetToSubscribe };

                        DatabaseResponse buddyProcessResponse = await _orderAccess.ProcessBuddyPlan(buddyToProcess);

                        if (buddyProcessResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                        {
                            // update process status

                            buddy.IsProcessed = true;

                            return 1;
                            
                        }

                        else
                        {
                            // buddy process failed
                            buddy.IsProcessed = false;

                            return 0;
                        }

                       
                    }
                    else
                    {
                        // buddy block failed
                        buddy.IsProcessed = false;

                        LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.UpdateAssetBlockingFailed));

                        return 0;
                    }

                }
                else
                {
                    // no assets returned                                   
                    buddy.IsProcessed = false;
                    LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.GetAssetFailed));

                    return 0;
                }
               

            }
            catch (Exception ex)
            {
                buddy.IsProcessed = false;

                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return 0;
            }
           
        }

        public async Task<List<Recordset>> GetInvoiceList(int customerID)
        {
            BSSAPIHelper bsshelper = new BSSAPIHelper();

            OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

            DatabaseResponse systemConfigResponse = await _orderAccess.GetConfiguration(ConfiType.System.ToString());

            DatabaseResponse bssConfigResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

            GridBSSConfi bssConfig = bsshelper.GetGridConfig((List<Dictionary<string, string>>)bssConfigResponse.Results);

            GridSystemConfig systemConfig = bsshelper.GetGridSystemConfig((List<Dictionary<string, string>>)systemConfigResponse.Results);

            DatabaseResponse accountResponse = await _orderAccess.GetCustomerBSSAccountNumber(customerID);

            if (accountResponse.ResponseCode == (int)DbReturnValue.RecordExists)
            {
                if (!string.IsNullOrEmpty(((BSSAccount)accountResponse.Results).AccountNumber))
                {
                    // Get default daterange in month from config by key - BSSInvoiceDefaultDateRangeInMonths
                    DatabaseResponse dateRangeResponse = ConfigHelper.GetValueByKey(ConfigKeys.BSSInvoiceDefaultDateRangeInMonths.ToString(), _iconfiguration);

                    int rangeInMonths = int.Parse(((string)dateRangeResponse.Results));

                    DatabaseResponse requestIdRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Customer.ToString(), BSSApis.GetInvoiceDetails.ToString(), customerID, 0, "");

                    BSSInvoiceResponseObject invoiceResponse = await bsshelper.GetBSSCustomerInvoice(bssConfig, ((BSSAssetRequest)requestIdRes.Results).request_id, ((BSSAccount)accountResponse.Results).AccountNumber, rangeInMonths);

                    if (invoiceResponse.Response.result_code == "0")
                    {
                        // Get download link prefix from config
                        DatabaseResponse downloadLinkResponse = ConfigHelper.GetValueByKey(ConfigKeys.BSSInvoiceDownloadLink.ToString(), _iconfiguration);

                        string downloadLinkPrefix = (string)downloadLinkResponse.Results;

                        foreach (Recordset recordset in invoiceResponse.Response.invoice_details.recordset)
                        {
                            recordset.download_url = downloadLinkPrefix + recordset.bill_id;
                        }
                        return invoiceResponse.Response.invoice_details.recordset;
                    }

                    else
                    {                        
                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.NoInvoiceFound) + " : " + customerID);
                        return new List<Recordset>();
                    }
                }
                else
                {                    
                   
                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.MandatoryRecordEmpty) + " for Customer - " + customerID);
                    return new List<Recordset>();
                }
            }

            else
            {                
                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToGetBillingAccount));
                return new List<Recordset>();
            }
        }

        public async Task<string> SendEmailNotification(string MPGSOrderID, int orderID)
        {
            string status = string.Empty;

            LogInfo.Information("Email orderID : " + orderID);

            LogInfo.Information("Email MPGSOrderID : " + MPGSOrderID);

            try
            {
                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                CommonDataAccess _commonAccess = new CommonDataAccess(_iconfiguration);

                ConfigDataAccess _configAccess = new ConfigDataAccess(_iconfiguration);

                DatabaseResponse templateResponse = await _configAccess.GetEmailNotificationTemplate(NotificationEvent.OrderSuccess.ToString());

                // customerID,
                DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(orderID);

                LogInfo.Information("Email Customer : " + (int)((OrderCustomer)customerResponse.Results).CustomerId);
                // Get Customer Data from CustomerID for email and Name
                CustomerDetails customer= await _commonAccess.GetCustomerDetailByOrder(((OrderCustomer)customerResponse.Results).CustomerId, orderID);

                LogInfo.Information("Email Customer data : "  + JsonConvert.SerializeObject(customer));

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
                        orderedNumbersSb.Append("<tr><td width='25%'>Mobile Number :<td width='20%'>");
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

                    if (!string.IsNullOrEmpty(customer.ShippingBuildingNumber))
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
                    if (deliveryAddressSb.ToString() != "")
                    {
                        deliveryAddressSb.Append("<br />");
                    }

                    StringBuilder shippingAddr2 = new StringBuilder();

                    if (!string.IsNullOrEmpty(customer.ShippingFloor))
                    {
                        shippingAddr2.Append("#" + customer.ShippingFloor);
                    }

                    if (!string.IsNullOrEmpty(customer.ShippingUnit))
                    {
                        if (shippingAddr2.ToString() != "")
                        {
                            shippingAddr2.Append(" - ");
                        }
                        shippingAddr2.Append(customer.ShippingUnit);
                    }

                    if (!string.IsNullOrEmpty(customer.ShippingBuildingName))
                    {
                        if (shippingAddr2.ToString() != "")
                        {
                            shippingAddr2.Append(", ");
                        }

                        shippingAddr2.Append(customer.ShippingBuildingName);
                    }
                    if (shippingAddr2.ToString() != "")
                    {
                        deliveryAddressSb.Append(shippingAddr2.ToString());

                        deliveryAddressSb.Append("<br />");
                    }

                    if (!string.IsNullOrEmpty(customer.ShippingPostCode))
                    {
                        deliveryAddressSb.Append("Singapore - " + customer.ShippingPostCode);
                    }

                    string deliveryDate = customer.SlotDate.ToString("dd MMM yyyy") + " " + new DateTime(customer.SlotFromTime.Ticks).ToString("hh:mm tt") + " to " + new DateTime(customer.SlotToTime.Ticks).ToString("hh:mm tt");

                    var notificationMessage = MessageHelper.GetMessage(customer.ToEmailList, customer.Name,

                                                        NotificationEvent.OrderSuccess.ToString(),

                                                     ((EmailTemplate)templateResponse.Results).TemplateName, _iconfiguration, customer.DeliveryEmail, customer.OrderNumber, orderedNumbersSb.ToString(), deliveryAddressSb.ToString(), customer.AlternateRecipientName == null ? customer.Name : customer.AlternateRecipientName, customer.AlternateRecipientContact == null ? customer.ShippingContactNumber : customer.AlternateRecipientContact, string.IsNullOrEmpty( customer.AlternateRecipientEmail) ? customer.DeliveryEmail : customer.AlternateRecipientEmail, deliveryDate, customer.ReferralCode);

                    DatabaseResponse notificationResponse = await _configAccess.GetConfiguration(ConfiType.Notification.ToString());

                    MiscHelper parser = new MiscHelper();

                    var notificationConfig = parser.GetNotificationConfig((List<Dictionary<string, string>>)notificationResponse.Results);

                    LogInfo.Information("Email Message to send  "+    JsonConvert.SerializeObject(notificationResponse));

                    Publisher orderSuccessNotificationPublisher = new Publisher(_iconfiguration, notificationConfig.SNSTopic);

                    try
                    {

                        status = await orderSuccessNotificationPublisher.PublishAsync(notificationMessage);
                    }
                    catch(Exception ex)
                    {
                        LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + "publishing :" + status);
                        throw ex;
                    }

                    LogInfo.Information("Email send status : " + status + " " + JsonConvert.SerializeObject(notificationMessage));

                    status = await SendOrderSuccessSMSNotification(customer, NotificationEvent.OrderSuccess.ToString());

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
                        LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + "Email send:" + MPGSOrderID);
                        throw ex;
                    }
                }

                
            }
            catch(Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + "MPGS OrderID:" + MPGSOrderID);
                throw ex;
            }

            return status;
        }

        public async Task<string> SendOrderSuccessSMSNotification(CustomerDetails customer, string MessageName )
        {
            string status = string.Empty;

            try
            {
              
                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                ConfigDataAccess _configAccess = new ConfigDataAccess(_iconfiguration);

                DatabaseResponse smsTemplateResponse = await _configAccess.GetSMSNotificationTemplate(MessageName);               

                var notificationMessage = MessageHelper.GetSMSMessage(MessageName, ((SMSTemplates)smsTemplateResponse.Results).TemplateName,customer.Name,customer.DeliveryEmail,customer.ShippingContactNumber, customer.OrderNumber, customer.SlotDate.ToString("dd MMM yyyy"), new DateTime(customer.SlotFromTime.Ticks).ToString("hh:mm tt") + " to " + new DateTime(customer.SlotToTime.Ticks).ToString("hh:mm tt"));

                DatabaseResponse notificationResponse = await _configAccess.GetConfiguration(ConfiType.Notification.ToString());

                MiscHelper parser = new MiscHelper();             

                var notificationConfig = parser.GetNotificationConfig((List<Dictionary<string, string>>)notificationResponse.Results);

                Publisher orderSuccessSMSNotificationPublisher = new Publisher(_iconfiguration, notificationConfig.SNSTopic);

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

        public async Task<int> SendMQ(DatabaseResponse orderMqResponse)
        {
            string topicName = string.Empty;
            string pushResult = string.Empty;
            if (orderMqResponse != null && orderMqResponse.Results != null)
            {

                object orderDetails = orderMqResponse.Results;

                try
                {
                    Dictionary<string, string> attribute = new Dictionary<string, string>();

                    topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration).Results.ToString().Trim();


                    attribute.Add(EventTypeString.EventType, RequestType.RescheduleDelivery.GetDescription());

                    pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, orderDetails, attribute);

                    if (pushResult.Trim().ToUpper() == "OK")
                    {
                        MessageQueueRequest queueRequest = new MessageQueueRequest
                        {
                            Source = CheckOutType.Orders.ToString(),
                            NumberOfRetries = 1,
                            SNSTopic = topicName,
                            CreatedOn = DateTime.Now,
                            LastTriedOn = DateTime.Now,
                            PublishedOn = DateTime.Now,
                            MessageAttribute = RequestType.RescheduleDelivery.GetDescription(),
                            MessageBody = JsonConvert.SerializeObject(orderDetails),
                            Status = 1
                        };
                        await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                    }
                    else
                    {
                        MessageQueueRequest queueRequest = new MessageQueueRequest
                        {
                            Source = CheckOutType.Orders.ToString(),
                            NumberOfRetries = 1,
                            SNSTopic = topicName,
                            CreatedOn = DateTime.Now,
                            LastTriedOn = DateTime.Now,
                            PublishedOn = DateTime.Now,
                            MessageAttribute = RequestType.RescheduleDelivery.GetDescription(),
                            MessageBody = JsonConvert.SerializeObject(orderDetails),
                            Status = 0
                        };
                        await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                    }

                }
                catch (Exception ex)
                {
                    LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                    MessageQueueRequestException queueRequest = new MessageQueueRequestException
                    {
                        Source = CheckOutType.Orders.ToString(),
                        NumberOfRetries = 1,
                        SNSTopic = string.IsNullOrWhiteSpace(topicName) ? null : topicName,
                        CreatedOn = DateTime.Now,
                        LastTriedOn = DateTime.Now,
                        PublishedOn = DateTime.Now,
                        MessageAttribute = Core.Enums.RequestType.RescheduleDelivery.GetDescription().ToString(),
                        MessageBody = orderDetails != null ? JsonConvert.SerializeObject(orderDetails) : null,
                        Status = 0,
                        Remark = "Error Occured in Reschedule Delivery",
                        Exception = new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical)


                    };

                    await _messageQueueDataAccess.InsertMessageInMessageQueueRequestException(queueRequest);
                    return 0;
                }


            }
            return 1;
        }
    }
}
