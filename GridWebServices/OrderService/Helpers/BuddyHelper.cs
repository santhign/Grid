using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OrderService.Models;
using OrderService.DataAccess;
using Core.Models;
using Core.Enums;
using Core.Extensions;
using InfrastructureService;
using Core.Helpers;
using Newtonsoft.Json;


namespace OrderService.Helpers
{
    public class BuddyHelper
    {   
        IConfiguration _iconfiguration;

        private readonly IMessageQueueDataAccess _messageQueueDataAccess;       

        public BuddyHelper(IConfiguration configuration, IMessageQueueDataAccess messageQueueDataAccess)
        {
            _iconfiguration = configuration;

            _messageQueueDataAccess = messageQueueDataAccess;
            
        }

        public async Task<int> AddRemoveBuddyHandler(int orderID, int customerID)
        {
            try
            {
                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);             

                // call remove buddy handler here
                int isRemoved = await RemoveBuddyHandler(orderID, customerID);

                DatabaseResponse checkAdditionalBuddyResponse = await _orderAccess.CheckAdditionalBuddy(orderID);

                // check additional Buddy
                if (checkAdditionalBuddyResponse.ResponseCode == (int)DbReturnValue.RecordExists && checkAdditionalBuddyResponse.Results != null)
                {
                    List<AdditionalBuddy> additionalBuddies = (List<AdditionalBuddy>)checkAdditionalBuddyResponse.Results;

                    foreach (AdditionalBuddy buddy in additionalBuddies)
                    {
                        if (buddy.OrderAdditionalBuddyID > 0 && buddy.IsProcessed == 0)
                        {
                            NumberHelper _numberhelper = new NumberHelper();
                            NumberDetails _details = await _numberhelper.GetNumberFromBSS(customerID);

                            if (_details != null && _details.Number != null)
                            {
                                // create buddy subscriber with blocked number and the existing main line

                                CreateBuddySubscriber additinalBuddySubscriberToCreate = new CreateBuddySubscriber { OrderID = orderID, MobileNumber = _details.Number, MainLineMobileNumber = buddy.MobileNumber, UserId = _details.UserSessionID };
                                DatabaseResponse createAdditionalBuddySubscriberResponse = await _orderAccess.CreateBuddySubscriber(additinalBuddySubscriberToCreate);

                                // update
                                if (createAdditionalBuddySubscriberResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                                {
                                    DatabaseResponse updateBuddyProcessedResponse = await _orderAccess.UpdateAdditionalBuddyProcessing(buddy.OrderAdditionalBuddyID);
                                }
                            }
                            else
                            {         // rollback on no assets retunred
                                int buddyRollback = await HandleRollbackOnAdditionalBuddyProcessingFailure(customerID, orderID, additionalBuddies);
                                return 2;
                            }
                        }
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return 0;
            }
        }

        public async Task<int> RemoveBuddyHandler(int orderID, int customerID)
        {
            try
            {
                NumberHelper _numberhelper = new NumberHelper();
                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);
                DatabaseResponse checkBuddyResponse = await _orderAccess.CheckBuddyToRemove(orderID);
                if (checkBuddyResponse.ResponseCode == (int)DbReturnValue.RecordExists && checkBuddyResponse.Results != null)
                {
                    BuddyToRemove buddyToRemove = (BuddyToRemove)checkBuddyResponse.Results;

                    if (buddyToRemove.BuddyRemovalID > 0 && buddyToRemove.IsRemoved == 0 && buddyToRemove.IsPorted!=1)
                    {                        
                        try
                        {
                            bool removalFlag = await _numberhelper.UnblockNumber(customerID, buddyToRemove.MobileNumber);
                            DatabaseResponse updateBuddyRemoval = await _orderAccess.UpdateBuddyRemoval(buddyToRemove.BuddyRemovalID);
                        }
                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BuddyRemovalFailed) + " for Order : " + orderID);
                        }
                    }
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                return 0;
            }
        }

        public async Task<BuyVASStatus> ProcessVas(int customerID, string mobileNumber, int bundleID, int quantity)
        {
            BuyVASStatus processVasStatus = new BuyVASStatus();

            try
            {
                OrderDataAccess _orderDataAccess = new OrderDataAccess(_iconfiguration);

                var statusResponse = await _orderDataAccess.BuyVasService(customerID, mobileNumber, bundleID, quantity);

                processVasStatus.BuyVASResponse = (BuyVASResponse)statusResponse.Results;

                processVasStatus.ResponseCode = statusResponse.ResponseCode;

                if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                {
                    //Ninad K : Message Publish code
                    MessageBodyForCR msgBody = new MessageBodyForCR();
                    Dictionary<string, string> attribute = new Dictionary<string, string>();
                    string topicName = string.Empty, subject = string.Empty;
                    try
                    {
                        topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration)
                        .Results.ToString().Trim();

                        if (string.IsNullOrWhiteSpace(topicName))
                        {
                            throw new NullReferenceException("topicName is null for ChangeRequest (" + processVasStatus.BuyVASResponse.ChangeRequestID + ") for BuyVAS Request Service API");
                        }
                        msgBody = await _messageQueueDataAccess.GetMessageBodyByChangeRequest(processVasStatus.BuyVASResponse.ChangeRequestID);

                        if (msgBody == null || msgBody.ChangeRequestID == 0)
                        {
                            throw new NullReferenceException("message body is null for ChangeRequest (" + processVasStatus.BuyVASResponse.ChangeRequestID + ") for BuyVAS Service API");
                        }

                        attribute.Add(EventTypeString.EventType, Core.Enums.RequestType.AddVAS.GetDescription());
                        var pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, msgBody, attribute);
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
                                MessageAttribute = Core.Enums.RequestType.AddVAS.GetDescription().ToString(),
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
                                MessageAttribute = Core.Enums.RequestType.AddVAS.GetDescription().ToString(),
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
                            MessageAttribute = Core.Enums.RequestType.AddVAS.GetDescription().ToString(),
                            MessageBody = msgBody != null ? JsonConvert.SerializeObject(msgBody) : null,
                            Status = 0,
                            Remark = "Error Occured in BuyVASService",
                            Exception = new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical)


                        };

                        await _messageQueueDataAccess.InsertMessageInMessageQueueRequestException(queueRequest);
                    }

                    processVasStatus.Result = 1;

                    return processVasStatus;
                }
                else
                {
                    LogInfo.Warning(DbReturnValue.NoRecords.GetDescription());

                    processVasStatus.Result = 0;

                    return processVasStatus;
                }
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                processVasStatus.Result = 0;

                return processVasStatus;
            }
        }

        public async Task<int> HandleRollbackOnAdditionalBuddyProcessingFailure(int customerID, int orderID, List<AdditionalBuddy> additionalBuddies)
        {  
            try
            {
                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                //remove additional buddy for the order from additional buddy table
                DatabaseResponse removeAdditionalBuddyRes = await _orderAccess.RemoveAdditionalBuddyOnRollBackOrder(orderID);

                return 1;
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                return 0;
            }
        }
    }
}
