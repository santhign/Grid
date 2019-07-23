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

namespace OrderService.Helpers
{
    public class BuddyHelper
    {   
        IConfiguration _iconfiguration;
        public BuddyHelper(IConfiguration configuration)
        {
            _iconfiguration = configuration;           
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
    }
}
