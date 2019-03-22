using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.DataAccess;
using Core.Models;
using Core.Enums;
using Core.Extensions;
using InfrastructureService;
using Core.Helpers;

namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        IConfiguration _iconfiguration;

        public OrdersController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }
        /// <summary>
        /// This will Create an order for the logged in customer with the bundle selected.
        /// </summary>
        /// <param name="request">CreateOrderRequest</param>
         ///Body: 
        ///{
        ///	"token" : "kfnsldfnksdnfefiu3r9882",
        ///	"BundleID" : "1",
        ///	"ReferralCode" : "dkfsdsd" --optional
        ///	"PromotionCode" : "Launch2019" --optional
        ///}
        /// <returns>OperationResponse</returns>
        // POST: api/Orders
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateOrderRequest request)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    };
                }               

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(request.Token);

                if(tokenAuthResponse.ResponseCode== (int) DbReturnValue.AuthSuccess)
                {

                    AuthTokenResponse aTokenResp = (AuthTokenResponse) tokenAuthResponse.Results;

                    if(!(aTokenResp.CreatedOn <DateTime.UtcNow.AddDays(-7)))
                    {
                        CreateOrder order = new CreateOrder();

                        order = new CreateOrder { BundleID = request.BundleID, PromotionCode = request.PromotionCode, ReferralCode = request.ReferralCode, CustomerID = aTokenResp.CustomerID };
                       
                        DatabaseResponse createOrderRresponse = await _orderAccess.CreateOrder(order);

                        if (createOrderRresponse.ResponseCode == ((int)DbReturnValue.CreationFailed))
                        {
                            // order creation failed

                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.CreateOrderFailed));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.CreationFailed),
                                IsDomainValidationErrors = true
                            });
                        }
                        else
                        {
                            // order creation Success

                            if( ((OrderInit)createOrderRresponse.Results).Status==OrderStatus.NewOrder.ToString())
                            {
                                // if its new order call GetAssets BSSAPI

                                BSSAPIHelper helper = new BSSAPIHelper();

                                DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                                //GridBSSConfi config = LinqExtensions.GeObjectFromDictionary<GridBSSConfi>((Dictionary<string, string>)configResponse.Results);

                                GridBSSConfi config = helper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                                DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                                DatabaseResponse requestIdRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Customer.ToString(), BSSApis.GetAssets.ToString(), aTokenResp.CustomerID); 

                                ResponseObject res = await helper.GetAssetInventory(config, ((ServiceFees)serviceCAF.Results).ServiceCode, ((BSSAssetRequest)requestIdRes.Results).request_id);

                                string AssetToSubscribe = helper.GetAssetId(res);

                                if (res!=null && (int.Parse( res.Response.asset_details.total_record_count)>0))
                                {

                                    //Block number                                    

                                    DatabaseResponse requestIdToUpdateRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Customer.ToString(), BSSApis.UpdateAssetStatus.ToString(), aTokenResp.CustomerID);

                                    BSSUpdateResponseObject bssUpdateResponse = await helper.UpdateAssetBlockNumber(config, ((BSSAssetRequest)requestIdToUpdateRes.Results).request_id, AssetToSubscribe,false);

                                    if(helper.GetResponseCode(bssUpdateResponse)=="0")
                                    {
                                        // create subscription
                                        CreateSubscriber subscriberToCreate = new CreateSubscriber { BundleID = request.BundleID, OrderID = ((OrderInit)createOrderRresponse.Results).OrderID, MobileNumber = AssetToSubscribe, PromotionCode = request.PromotionCode }; // verify isPrimary

                                        DatabaseResponse createSubscriberResponse = await _orderAccess.CreateSubscriber(subscriberToCreate);

                                        // Get Order Basic Details

                                        DatabaseResponse orderDetailsResponse = await _orderAccess.GetOrderBasicDetails(((OrderInit)createOrderRresponse.Results).OrderID);

                                        //Start Remove while going live

                                        DatabaseResponse requestIdToUpdateUnblock = await _orderAccess.GetBssApiRequestId(GridMicroservices.Customer.ToString(), BSSApis.UpdateAssetStatus.ToString(), aTokenResp.CustomerID);

                                        BSSUpdateResponseObject bssUnblockUpdateResponse = await helper.UpdateAssetBlockNumber(config, ((BSSAssetRequest)requestIdToUpdateRes.Results).request_id, AssetToSubscribe,true);

                                        // end remove

                                        return Ok(new OperationResponse
                                        {                                            
                                            HasSucceeded = true,
                                            Message = EnumExtensions.GetDescription(DbReturnValue.CreateSuccess),
                                            IsDomainValidationErrors = false,
                                            ReturnedObject = orderDetailsResponse.Results

                                        });
                                    }
                                    else
                                    {
                                        //blocking failed
                                        
                                        LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.UpdateAssetBlockingFailed));
                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = false,
                                            Message = EnumExtensions.GetDescription(DbReturnValue.BlockingFailed),
                                            IsDomainValidationErrors = false
                                        });
                                    }  
                                  
                                }
                                else
                                {
                                    // no assets returned

                                    LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.GetAssetFailed));

                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = false,
                                        Message = EnumExtensions.GetDescription(DbReturnValue.GetAssetsFailed),
                                        IsDomainValidationErrors = false
                                    });

                                }   
                               
                            }

                            else
                            {
                                // old order-- return order details

                                LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.CreateOrderFailed));

                                DatabaseResponse orderDetailsResponse = await _orderAccess.GetOrderBasicDetails(((OrderInit)createOrderRresponse.Results).OrderID);

                                return Ok(new OperationResponse
                                {                                   
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.RecordExists),
                                    IsDomainValidationErrors = false,
                                    ReturnedObject = orderDetailsResponse.Results

                                });
                            }
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));                        

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = EnumExtensions.GetDescription(DbReturnValue.TokenExpired),
                            IsDomainValidationErrors = true
                        });

                    }
                    
                }

                else
                {
                    // token auth failure
                    LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = false
                    });
                }


                
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });

            }
        }
      
    }
}
