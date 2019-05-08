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
                }

                else if (((OrderSource)sourceTyeResponse.Results).SourceType == CheckOutType.Orders.ToString())
                {
                    DatabaseResponse buddyCheckResponse = new DatabaseResponse();

                    buddyCheckResponse = await _orderAccess.OrderBuddyCheck(((OrderSource)sourceTyeResponse.Results).SourceID);

                    if (buddyCheckResponse.ResponseCode == (int)DbReturnValue.RecordExists && ((List<BuddyCheckList>)buddyCheckResponse.Results).Count > 0)
                    {
                        // need buddy processing so delay messaging    

                        buddyActionList = (List<BuddyCheckList>)buddyCheckResponse.Results;

                        //var parent = Task.Factory.StartNew(() =>
                        //{
                        //    Action buddyProcessing = FinalBuddyProcessing;

                        //    Task.Factory.StartNew(buddyProcessing, TaskCreationOptions.DenyChildAttach);

                        //});


                        //  Action buddyProcessing = FinalBuddyProcessing;

                            int processed=  await   FinalBuddyProcessing();

                        

                    }

                    else
                    {
                        //send queue message

                        ProcessOrderQueueMessage(((OrderSource)sourceTyeResponse.Results).SourceID);
                    }                   

                }

                else if (((OrderSource)sourceTyeResponse.Results).SourceType == CheckOutType.AccountInvoices.ToString())
                {                   
                        //send queue message

                  ProcessOrderQueueMessage(((OrderSource)sourceTyeResponse.Results).SourceID);                 

                }


                return 1;
            }

            else
            {
                // unable to get sourcetype form db

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
                                MessageBody = JsonConvert.SerializeObject(orderDetails),
                                Status = 0
                            };
                            await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                        }

                    }
                    catch (Exception ex)
                    {
                        LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

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
                            Status = 0
                        };
                        await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
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
                    throw ex;
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

                ResponseObject res = await bsshelper.GetAssetInventory(config, serviceCode, (BSSAssetRequest)requestIdRes.Results);

                string AssetToSubscribe = bsshelper.GetAssetId(res);

                if (res != null)
                {
                    BSSNumbers numbers = new BSSNumbers();

                    numbers.FreeNumbers = bsshelper.GetFreeNumbers(res);

                    //insert these number into database
                    string json = bsshelper.GetJsonString(numbers.FreeNumbers); // json insert

                    DatabaseResponse updateBssCallFeeNumbers = await _orderAccess.UpdateBSSCallNumbers(json, ((BSSAssetRequest)requestIdRes.Results).userid, ((BSSAssetRequest)requestIdRes.Results).BSSCallLogID);
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
                            
                        }

                        else
                        {
                            // buddy process failed
                            buddy.IsProcessed = false;
                        }

                       
                    }
                    else
                    {
                        // buddy block failed
                        buddy.IsProcessed = false;

                        LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.UpdateAssetBlockingFailed));
                        
                    }

                }
                else
                {
                    // no assets returned                                   
                    buddy.IsProcessed = false;
                    LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.GetAssetFailed));

                }
                return 1;

            }
            catch (Exception ex)
            {
                buddy.IsProcessed = false;
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return 0;
            }
           
        }
    }
}
