﻿using Core.Helpers;
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

                DatabaseResponse serviceCAF = await _buddyAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString(), _connectionString);

                DatabaseResponse requestIdRes = await _buddyAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), buddy.CustomerID, (int)BSSCalls.NewSession, "", _connectionString);

                ResponseObject res = await bsshelper.GetAssetInventory(config, (((List<ServiceFees>)serviceCAF.Results)).FirstOrDefault().ServiceCode, (BSSAssetRequest)requestIdRes.Results);

                string AssetToSubscribe = bsshelper.GetAssetId(res);


                if (res != null && (int.Parse(res.Response.asset_details.total_record_count) > 0))
                {
                    BSSNumbers numbers = new BSSNumbers();

                    numbers.FreeNumbers = bsshelper.GetFreeNumbers(res);

                    //insert these number into database
                    string json = bsshelper.GetJsonString(numbers.FreeNumbers); // json insert

                    DatabaseResponse updateBssCallFeeNumbers = await _buddyAccess.UpdateBSSCallNumbers(json, ((BSSAssetRequest)requestIdRes.Results).userid, ((BSSAssetRequest)requestIdRes.Results).BSSCallLogID, _connectionString);
                    //Block number                                    _connectionString
                    DatabaseResponse requestIdToUpdateRes = await _buddyAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), buddy.CustomerID, (int)BSSCalls.ExistingSession, AssetToSubscribe, _connectionString);

                    BSSUpdateResponseObject bssUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateRes.Results, AssetToSubscribe, false);

                    if (bsshelper.GetResponseCode(bssUpdateResponse) == "0")
                    {
                        // update buddy process

                        BuddyNumberUpdate buddyToProcess = new BuddyNumberUpdate { OrderSubscriberID = buddy.OrderSubscriberID, UserId = ((BSSAssetRequest)requestIdRes.Results).userid, NewMobileNumber = AssetToSubscribe };

                        DatabaseResponse buddyProcessResponse = await _buddyAccess.ProcessBuddyPlan(buddyToProcess, _connectionString);

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


                    }

                }
                else
                {
                    // no assets returned                                   
                    buddy.IsProcessed = false;

                }
                return buddy;

            }
            catch (Exception ex)
            {
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

                string topicName = string.Empty;

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

                            Log.Information(EnumExtensions.GetDescription(CommonErrors.PendingBuddyOrderProcessed));
                             
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
                        Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                        queueRequest.Status = 0;

                        await _buddyAccess.InsertMessageInMessageQueueRequest(queueRequest, _connectionString);

                        return 0;
                    }
                }

                else
                {
                    Log.Information(EnumExtensions.GetDescription(CommonErrors.PendingBuddyMQBodyFailed));
                    return 0;
                }
                
            }
            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return 0;
            }
        }
    }
}

