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

                    if(!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                    {
                        CreateOrder order = new CreateOrder();

                        order = new CreateOrder { BundleID = request.BundleID, PromotionCode = request.PromotionCode, ReferralCode = request.ReferralCode, CustomerID = aTokenResp.CustomerID };
                       
                        DatabaseResponse createOrderRresponse = await _orderAccess.CreateOrder(order);

                        if (createOrderRresponse.ResponseCode == ((int)DbReturnValue.CreationFailed))
                        {
                            // order creation failed
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

                            if( ((OrderInit)createOrderRresponse.Results).Status==OrderStatus.New.ToString())
                            {
                                // if its new order call GetAssets BSSAPI

                                BSSAPIHelper helper = new BSSAPIHelper();

                                DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                                //GridBSSConfi config = LinqExtensions.GeObjectFromDictionary<GridBSSConfi>((Dictionary<string, string>)configResponse.Results);

                                GridBSSConfi config = helper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                                DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                                DatabaseResponse requestIdRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Customer.ToString(), BSSApis.GetAssets.ToString(), aTokenResp.CustomerID); 

                                ResponseObject res = await helper.GetAssetInventory(config, ((ServiceFees)serviceCAF.Results).ServiceCode, ((BSSAssetRequest)requestIdRes.Results).request_id);

                                if(res!=null && (int.Parse( res.Response.asset_details.total_record_count)>0))
                                {                                   
                                   
                                    //Reserve number --

                                    // needd to call BSS API to update asset
                                   

                                    CreateSubscriber subscriberToCreate = new CreateSubscriber { BundleID=request.BundleID, OrderID= ((OrderInit)createOrderRresponse.Results).OrderID, MobileNumber= helper.GetAssetId(res), PromotionCode=request.PromotionCode , IsPrimary=1}; // verify isPrimary
                                                                                                                                                                                                                                                                                
                                    DatabaseResponse createSubscriberResponse = await _orderAccess.CreateSubscriber(subscriberToCreate);

                                    // Get Order Basic Details

                                    DatabaseResponse orderDetailsResponse = await _orderAccess.GetOrderBasicDetails(((OrderInit)createOrderRresponse.Results).OrderID);

                                    return Ok(new OperationResponse
                                    {
                                        // need to change according to subscription
                                        HasSucceeded = true,
                                        Message = EnumExtensions.GetDescription(DbReturnValue.CreateSuccess),
                                        IsDomainValidationErrors = false,
                                        ReturnedObject = orderDetailsResponse.Results

                                    });
                                }
                                else
                                {
                                    // no assets returned

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

                                DatabaseResponse orderDetailsResponse = await _orderAccess.GetOrderBasicDetails(((OrderInit)createOrderRresponse.Results).OrderID);

                                return Ok(new OperationResponse
                                {
                                    // need to change according to subscription
                                    HasSucceeded = true,
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
