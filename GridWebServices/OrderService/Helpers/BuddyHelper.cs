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

                DatabaseResponse checkBuddyResponse = await _orderAccess.CheckBuddyToRemove(orderID);

                DatabaseResponse checkAdditionalBuddyResponse = await _orderAccess.CheckAdditionalBuddy(orderID);

                if (checkBuddyResponse.ResponseCode == (int)DbReturnValue.RecordExists && checkBuddyResponse.Results != null)
                {
                    BuddyToRemove buddyToRemove = (BuddyToRemove)checkBuddyResponse.Results;

                    if (buddyToRemove.BuddyRemovalID > 0 && buddyToRemove.IsRemoved == 0)
                    {
                        BSSAPIHelper bsshelper = new BSSAPIHelper();

                        DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                        GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                        DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                        DatabaseResponse requestIdToUpdateRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customerID, (int)BSSCalls.ExistingSession, buddyToRemove.MobileNumber);

                        BSSUpdateResponseObject bssUpdateResponse = new BSSUpdateResponseObject();

                        try
                        {
                            bssUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateRes.Results, buddyToRemove.MobileNumber, true);

                            DatabaseResponse updateBuddyRemoval = await _orderAccess.UpdateBuddyRemoval(buddyToRemove.BuddyRemovalID);
                        }

                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BuddyRemovalFailed) + " for Order : " + orderID);

                        }
                    }
                }

                // check additional Buddy

                if (checkAdditionalBuddyResponse.ResponseCode == (int)DbReturnValue.RecordExists && checkAdditionalBuddyResponse.Results != null)
                {
                    AdditionalBuddy additionalBuddy = (AdditionalBuddy)checkAdditionalBuddyResponse.Results;

                    if (additionalBuddy.OrderAdditionalBuddyID > 0 && additionalBuddy.IsProcessed == 0)
                    {
                        BSSAPIHelper bsshelper = new BSSAPIHelper();

                        DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                        GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                        DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                        DatabaseResponse requestIdToGetAdditionalBuddy = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), customerID, (int)BSSCalls.NewSession, "");

                        ResponseObject res = new ResponseObject();

                        BSSNumbers numbers = new BSSNumbers();
                        //get a free number for additional buddy
                        try
                        {
                            res = await bsshelper.GetAssetInventory(config, (((List<ServiceFees>)serviceCAF.Results)).FirstOrDefault().ServiceCode, (BSSAssetRequest)requestIdToGetAdditionalBuddy.Results);
                        }

                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                        }

                        if (res != null && res.Response != null && res.Response.asset_details != null && (int.Parse(res.Response.asset_details.total_record_count) > 0))
                        {
                            numbers.FreeNumbers = bsshelper.GetFreeNumbers(res);

                            //insert these number into database
                            string json = bsshelper.GetJsonString(numbers.FreeNumbers); // json insert

                            DatabaseResponse updateBssCallFeeNumbers = await _orderAccess.UpdateBSSCallNumbers(json, ((BSSAssetRequest)requestIdToGetAdditionalBuddy.Results).userid, ((BSSAssetRequest)requestIdToGetAdditionalBuddy.Results).BSSCallLogID);

                            DatabaseResponse requestIdToUpdateAdditionalBuddyRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customerID, (int)BSSCalls.ExistingSession, numbers.FreeNumbers[0].MobileNumber);

                            BSSUpdateResponseObject bssUpdateAdditionalBuddyResponse = new BSSUpdateResponseObject();
                            // block the number for additional buddy
                            try
                            {
                                bssUpdateAdditionalBuddyResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateAdditionalBuddyRes.Results, numbers.FreeNumbers[0].MobileNumber, false);

                                // create buddy subscriber with blocked number and the existing main line

                                CreateBuddySubscriber additinalBuddySubscriberToCreate = new CreateBuddySubscriber { OrderID = orderID, MobileNumber = numbers.FreeNumbers[0].MobileNumber, MainLineMobileNumber = additionalBuddy.MobileNumber, UserId = ((BSSAssetRequest)requestIdToUpdateAdditionalBuddyRes.Results).userid };

                                DatabaseResponse createAdditionalBuddySubscriberResponse = await _orderAccess.CreateBuddySubscriber(additinalBuddySubscriberToCreate);

                                // uPDAT
                                if (createAdditionalBuddySubscriberResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                                {
                                    DatabaseResponse updateBuddyRemoval = await _orderAccess.UpdateAdditionalBuddyProcessing(additionalBuddy.OrderAdditionalBuddyID);
                                }
                            }

                            catch (Exception ex)
                            {
                                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BuddyRemovalFailed) + " for Order : " +orderID);

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
    }
}
