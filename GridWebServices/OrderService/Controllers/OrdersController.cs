using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.DataAccess;
using OrderService.Helpers;
using Core.Models;
using Core.Enums;
using Core.Extensions;
using InfrastructureService;
using Core.Helpers;
using System.IO;
using System.Net;

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
        /// This will return Order details for specific ID passed 
        /// </summary>
        /// <param name="id">OrderID</param>
        /// <returns>OperationsResponse</returns>
        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.DomainValidationError,
                        IsDomainValidationErrors = true
                    });
                }

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse orderDetailsResponse = await _orderAccess.GetOrderDetails(id);

                if (orderDetailsResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = true,
                        Message = EnumExtensions.GetDescription(DbReturnValue.RecordExists),
                        Result = orderDetailsResponse.Results

                    });
                }

                else
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.NotExists)
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
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(request.Token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {

                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
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

                            if (((OrderInit)createOrderRresponse.Results).Status == OrderStatus.NewOrder.ToString())
                            {
                                // if its new order call GetAssets BSSAPI

                                BSSAPIHelper helper = new BSSAPIHelper();

                                DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());
                                

                                GridBSSConfi config = helper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                                DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                                DatabaseResponse requestIdRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), aTokenResp.CustomerID);

                                ResponseObject res = await helper.GetAssetInventory(config, (((List<ServiceFees>)serviceCAF.Results)).FirstOrDefault().ServiceCode, ((BSSAssetRequest)requestIdRes.Results).request_id);

                                string AssetToSubscribe = helper.GetAssetId(res);

                                if (res != null && (int.Parse(res.Response.asset_details.total_record_count) > 0))
                                {

                                    //Block number                                    

                                    DatabaseResponse requestIdToUpdateRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), aTokenResp.CustomerID);

                                    BSSUpdateResponseObject bssUpdateResponse = await helper.UpdateAssetBlockNumber(config, ((BSSAssetRequest)requestIdToUpdateRes.Results).request_id, AssetToSubscribe, false);

                                    if (helper.GetResponseCode(bssUpdateResponse) == "0")
                                    {
                                        // create subscription
                                        CreateSubscriber subscriberToCreate = new CreateSubscriber { BundleID = request.BundleID, OrderID = ((OrderInit)createOrderRresponse.Results).OrderID, MobileNumber = AssetToSubscribe, PromotionCode = request.PromotionCode }; // verify isPrimary

                                        DatabaseResponse createSubscriberResponse = await _orderAccess.CreateSubscriber(subscriberToCreate);

                                        // Get Order Basic Details

                                        DatabaseResponse orderDetailsResponse = await _orderAccess.GetOrderBasicDetails(((OrderInit)createOrderRresponse.Results).OrderID);

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

        /// <summary>
        /// Removes the vas service.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("RemoveVasService")]
        public async Task<IActionResult> RemoveVasService(string token, string mobileNumber, int planId)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode((int) HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage))
                    });
                }

                var orderAccess = new OrderDataAccess(_iconfiguration);
                var tokenAuthResponse = await orderAccess.AuthenticateCustomerToken(token);
                if (tokenAuthResponse.ResponseCode == (int) DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;
                    var statusResponse =
                        await orderAccess.RemoveVasService(aTokenResp.CustomerID, mobileNumber, planId);

                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = statusResponse
                        });
                    }
                    else
                    {
                        LogInfo.Error(DbReturnValue.NoRecords.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.UpdationFailed.GetDescription(),
                            IsDomainValidationErrors = false
                        });
                    }
                }
                else
                {
                    //Token expired
                    LogInfo.Error(CommonErrors.ExpiredToken.GetDescription());
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = DbReturnValue.TokenExpired.GetDescription(),
                        IsDomainValidationErrors = true
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

        /// <summary>
        /// Buys the vas service.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <param name="bundleId">The bundle identifier.</param>
        /// <param name="quantity">The quantity.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("BuyVasService")]
        public async Task<IActionResult> BuyVasService(string token, string mobileNumber, int bundleId, int quantity)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage))
                    });
                }

                var orderAccess = new OrderDataAccess(_iconfiguration);
                var tokenAuthResponse = await orderAccess.AuthenticateCustomerToken(token);
                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;
                    var statusResponse =
                        await orderAccess.BuyVasService(aTokenResp.CustomerID, mobileNumber, bundleId, quantity);

                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = statusResponse
                        });
                    }
                    else
                    {
                        LogInfo.Error(DbReturnValue.NoRecords.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.UpdationFailed.GetDescription(),
                            IsDomainValidationErrors = false
                        });
                    }
                }
                else
                {
                    //Token expired
                    LogInfo.Error(CommonErrors.ExpiredToken.GetDescription());
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = DbReturnValue.TokenExpired.GetDescription(),
                        IsDomainValidationErrors = true
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

        /// <summary>
        /// This will Update subscribers existing number with new number selected.
        /// </summary>
        /// <param name="request">
        /// body{
        /// "Token" :"Auth token",
        /// "OrderID" :1,
        /// "OldMobileNumber" :"97854562",
        /// "NewNumber" :{
        ///             "MobileNumber":"97854572",
        ///             "ServiceCode" :39
        ///             },
        /// "DisplayName" : "shortname"       
        /// }
        /// </param>
        /// <returns>OperationResponse</returns>       
        [HttpPost]
        [Route("UpdateSubscriberNumber")]
        public async Task<IActionResult> UpdateSubscriberNumber([FromBody] UpdateSubscriberNumber request)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(request.Token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                    {
                        BSSAPIHelper helper = new BSSAPIHelper();

                        OrderCustomer customer = new OrderCustomer();

                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(request.OrderID);

                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                        {
                            customer = (OrderCustomer)customerResponse.Results;


                            DatabaseResponse requestIdToUpdateUnblock = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customer.CustomerId);

                            DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                            GridBSSConfi config = helper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                            // Unblock
                            BSSUpdateResponseObject bssUnblockUpdateResponse = await helper.UpdateAssetBlockNumber(config, ((BSSAssetRequest)requestIdToUpdateUnblock.Results).request_id, request.OldMobileNumber, true);


                            if (helper.GetResponseCode(bssUnblockUpdateResponse) == "0")
                            {
                                //Block

                                DatabaseResponse requestIdToUpdateBlock = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customer.CustomerId);

                                BSSUpdateResponseObject bssUpdateResponse = await helper.UpdateAssetBlockNumber(config, ((BSSAssetRequest)requestIdToUpdateBlock.Results).request_id, request.NewNumber.MobileNumber, false);

                                if (helper.GetResponseCode(bssUnblockUpdateResponse) == "0")
                                {
                                    //update subscription
                                    DatabaseResponse updateSubscriberResponse = await _orderAccess.UpdateSubscriberNumber(request);

                                    if (updateSubscriberResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
                                    {
                                        // Get Order Basic Details

                                        DatabaseResponse orderDetailsResponse = await _orderAccess.GetOrderBasicDetails(request.OrderID);

                                        if (orderDetailsResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                                        {
                                            return Ok(new OperationResponse
                                            {
                                                HasSucceeded = true,
                                                Message = EnumExtensions.GetDescription(DbReturnValue.UpdateSuccess),
                                                IsDomainValidationErrors = false,
                                                ReturnedObject = orderDetailsResponse.Results
                                            });
                                        }

                                        else
                                        {
                                            //subscription updated, but details not returned
                                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToLocateUpdatedSubscription));
                                            return Ok(new OperationResponse
                                            {
                                                HasSucceeded = true,
                                                Message = EnumExtensions.GetDescription(DbReturnValue.NotExists),
                                                IsDomainValidationErrors = false,
                                            });
                                        }

                                    }
                                    else
                                    {
                                        LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.UpdateSubscriptionFailed));
                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = false,
                                            Message = EnumExtensions.GetDescription(DbReturnValue.UpdationFailed),
                                            IsDomainValidationErrors = false
                                        });
                                    }

                                }

                                else
                                {
                                    // blocking failed

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
                                // unblocking failed
                                LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.UpdateAssetUnBlockingFailed));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.UnBlockingFailed),
                                    IsDomainValidationErrors = false
                                });
                            }

                        }

                        else
                        {
                            // failed to locate customer
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.UnBlockingFailed),
                                IsDomainValidationErrors = false
                            });
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


        /// <summary>
        /// Subscribers the termination request.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SubscriberTerminationRequest/{token}/{mobileNumber}")]
        public async Task<IActionResult> SubscriberTerminationRequest(string token, string mobileNumber)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                var orderAccess = new OrderDataAccess(_iconfiguration);

                var tokenAuthResponse = await orderAccess.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    var statusResponse = await orderAccess.TerminationRequest(aTokenResp.CustomerID, mobileNumber);

                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = statusResponse
                        });
                    }
                    else
                    {
                        LogInfo.Error(DbReturnValue.NoRecords.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.UpdationFailed.GetDescription(),
                            IsDomainValidationErrors = false
                        });
                    }

                }
                else
                {
                    // token auth failure
                    LogInfo.Error(DbReturnValue.TokenAuthFailed.GetDescription());

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = DbReturnValue.TokenAuthFailed.GetDescription(),
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


        /// <summary>
        /// Subscribers the sim replacement request.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SubscriberSimReplacementRequest/{token}/{mobileNumber}")]
        public async Task<IActionResult> SubscriberSimReplacementRequest(string token, string mobileNumber)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                var orderAccess = new OrderDataAccess(_iconfiguration);

                var tokenAuthResponse = await orderAccess.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    var statusResponse = await orderAccess.SimReplacementRequest(aTokenResp.CustomerID, mobileNumber);

                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = statusResponse
                        });
                    }
                    else
                    {
                        LogInfo.Error(DbReturnValue.NoRecords.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.UpdationFailed.GetDescription(),
                            IsDomainValidationErrors = false
                        });
                    }

                }
                else
                {
                    // token auth failure
                    LogInfo.Error(DbReturnValue.TokenAuthFailed.GetDescription());

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = DbReturnValue.TokenAuthFailed.GetDescription(),
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

        /// <summary>
        /// Subscribers the suspension request.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SubscriberSuspensionRequest/{token}/{mobileNumber}")]
        public async Task<IActionResult> SubscriberSuspensionRequest(string token, string mobileNumber)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                var orderAccess = new OrderDataAccess(_iconfiguration);

                var tokenAuthResponse = await orderAccess.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    var statusResponse = await orderAccess.SuspensionRequest(aTokenResp.CustomerID, mobileNumber);

                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = statusResponse
                        });
                    }
                    else
                    {
                        LogInfo.Error(DbReturnValue.NoRecords.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.UpdationFailed.GetDescription(),
                            IsDomainValidationErrors = false
                        });
                    }

                }
                else
                {
                    // token auth failure
                    LogInfo.Error(DbReturnValue.TokenAuthFailed.GetDescription());

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = DbReturnValue.TokenAuthFailed.GetDescription(),
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
        

        /// <summary>
        /// This will Update subscribers existing number with new number selected.
        /// </summary>
        /// <param name="request">
        /// Form{
        /// "Token" :"Auth token",
        /// "OrderID" :1,
        /// "OldMobileNumber" :"97854562",
        /// "NewMobileNumber":"97854572",
        /// "DisplayName" : "shortname",
        /// "IsOwnNumber" : 1,
        /// "DonorProvider":"",
        /// "PortedNumberTransferForm":"", //optional
        /// "PortedNumberOwnedBy":"", //optional
        /// "PortedNumberOwnerRegistrationID":"", //optional
        /// 
        /// }
        /// </param>
        /// <returns>OperationResponse</returns> 
        [Route("PortingNumber")]
        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> PortingNumber([FromForm] UpdateSubscriberPortingNumberRequest request)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(request.Token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                    {
                        IFormFile file = request.PortedNumberTransferForm;

                        BSSAPIHelper helper = new BSSAPIHelper();

                        MiscHelper configHelper = new MiscHelper();

                        UpdateSubscriberPortingNumber portingRequest = new UpdateSubscriberPortingNumber
                        {
                            OrderID = request.OrderID,
                            OldMobileNumber = request.OldMobileNumber,
                            NewMobileNumber = request.NewMobileNumber,
                            DisplayName = request.DisplayName,
                            DonorProvider = request.DonorProvider,
                            IsOwnNumber = request.IsOwnNumber,
                            PortedNumberOwnedBy = request.PortedNumberOwnedBy,
                            PortedNumberTransferForm = "",
                            PortedNumberOwnerRegistrationID = request.PortedNumberOwnerRegistrationID
                        };

                        //process file if uploaded - non null

                        if (file != null)
                        {
                            DatabaseResponse awsConfigResponse = await _orderAccess.GetConfiguration(ConfiType.AWS.ToString());

                            if (awsConfigResponse != null && awsConfigResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                            {
                                GridAWSS3Config awsConfig = configHelper.GetGridAwsConfig((List<Dictionary<string, string>>)awsConfigResponse.Results);

                                AmazonS3 s3Helper = new AmazonS3(awsConfig);

                                string fileName = "Grid_PNTF_" + portingRequest.OrderID + "_" + DateTime.UtcNow.ToString("yyyymmddhhmmss") + Path.GetExtension(file.FileName);

                                UploadResponse s3UploadResponse = await s3Helper.UploadFile(file, fileName);

                                if (s3UploadResponse.HasSucceed)
                                {
                                    portingRequest.PortedNumberTransferForm = s3UploadResponse.FileName;
                                }
                                else
                                {
                                    LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.S3UploadFailed));
                                }
                            }
                            else
                            {
                                // unable to get aws config
                                LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetConfiguration));

                            }
                        }

                        OrderCustomer customer = new OrderCustomer();

                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(request.OrderID);

                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                        {
                            customer = (OrderCustomer)customerResponse.Results;

                            DatabaseResponse requestIdToUpdateUnblock = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customer.CustomerId);

                            DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                            GridBSSConfi config = helper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                            // Unblock
                            BSSUpdateResponseObject bssUnblockUpdateResponse = await helper.UpdateAssetBlockNumber(config, ((BSSAssetRequest)requestIdToUpdateUnblock.Results).request_id, request.OldMobileNumber, true);

                            if (helper.GetResponseCode(bssUnblockUpdateResponse) == "0")
                            {
                                //update subscription porting
                                DatabaseResponse updateSubscriberResponse = await _orderAccess.UpdateSubscriberPortingNumber(portingRequest);

                                if (updateSubscriberResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
                                {
                                    // Get Order Basic Details

                                    DatabaseResponse orderDetailsResponse = await _orderAccess.GetOrderBasicDetails(request.OrderID);

                                    if (orderDetailsResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                                    {
                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = true,
                                            Message = EnumExtensions.GetDescription(DbReturnValue.UpdateSuccess),
                                            IsDomainValidationErrors = false,
                                            ReturnedObject = orderDetailsResponse.Results
                                        });
                                    }

                                    else
                                    {
                                        //subscription porting updated, but details not returned
                                        LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToLocateUpdatedSubscription));
                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = true,
                                            Message = EnumExtensions.GetDescription(DbReturnValue.NotExists),
                                            IsDomainValidationErrors = false,
                                        });
                                    }

                                }
                                else
                                {
                                    LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.UpdateSubscriptionFailed));
                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = false,
                                        Message = EnumExtensions.GetDescription(DbReturnValue.UpdationFailed),
                                        IsDomainValidationErrors = false
                                    });
                                }
                            }

                            else
                            {
                                // unblocking failed
                                LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.UpdateAssetUnBlockingFailed));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.UnBlockingFailed),
                                    IsDomainValidationErrors = false
                                });
                            }

                        }

                        else
                        {
                            // failed to locate customer
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.UnBlockingFailed),
                                IsDomainValidationErrors = false
                            });
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

        /// <summary>
        /// This will create an additional subscriber for the orderID input with the selected Bundle
        /// </summary>
        /// <param name="request">
        /// body
        /// {
        ///  "Token":"auth token",
        /// "OrderID" :1,
        /// "BundleID" :"1"        
        /// }
        /// </param>
        /// <returns>OperationResponse</returns>
        [HttpPost]
        [Route("CreateSubscriber")]
        public async Task<IActionResult> CreateSubscriber([FromBody] AdditionalSubscriberRequest request)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(request.Token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {

                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                    {
                        // call GetAssets BSSAPI

                        BSSAPIHelper helper = new BSSAPIHelper();

                        DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                        GridBSSConfi config = helper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                        DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                        DatabaseResponse requestIdRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), aTokenResp.CustomerID);

                        ResponseObject res = await helper.GetAssetInventory(config, (((List<ServiceFees>)serviceCAF.Results)).FirstOrDefault().ServiceCode, ((BSSAssetRequest)requestIdRes.Results).request_id);

                        string AssetToSubscribe = helper.GetAssetId(res);

                        if (res != null && (int.Parse(res.Response.asset_details.total_record_count) > 0))
                        {
                            //Block number                                    

                            DatabaseResponse requestIdToUpdateRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), aTokenResp.CustomerID);

                            BSSUpdateResponseObject bssUpdateResponse = await helper.UpdateAssetBlockNumber(config, ((BSSAssetRequest)requestIdToUpdateRes.Results).request_id, AssetToSubscribe, false);

                            if (helper.GetResponseCode(bssUpdateResponse) == "0")
                            {
                                // create subscription
                                CreateSubscriber subscriberToCreate = new CreateSubscriber { BundleID = request.BundleID, OrderID = request.OrderID, MobileNumber = AssetToSubscribe, PromotionCode = "" };

                                DatabaseResponse createSubscriberResponse = await _orderAccess.CreateSubscriber(subscriberToCreate);

                                if (createSubscriberResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                                {
                                    // Get Order Basic Details

                                    DatabaseResponse orderDetailsResponse = await _orderAccess.GetOrderBasicDetails(request.OrderID);

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
                                    // Create subscriber failed
                                    LogInfo.Error(CommonErrors.CreateSubscriptionFailed.GetDescription());

                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = true,
                                        Message = DbReturnValue.CreationFailed.GetDescription(),
                                        IsDomainValidationErrors = false

                                    });
                                }


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

        /// <summary>
        /// This will returns a set of available delivery slots
        /// </summary>
        /// <param name="token"></param>
        /// <returns>OperationResponse</returns>

        [HttpGet]
        [Route("GetAvailableSlots")]
        public async Task<IActionResult> GetAvailableSlots(string token)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                    {

                        DatabaseResponse deliveryDetailsResponse = await _orderAccess.GetAvailableSlots();

                        if (deliveryDetailsResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(DbReturnValue.RecordExists),
                                Result = deliveryDetailsResponse.Results

                            });
                        }

                        else
                        {
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.DeliverySlotNotExists));
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NotExists)
                            });

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

        /// <summary>
        /// This will update personal details of the customer for the order
        /// </summary>
        /// <param name="request">
        /// Form{
        /// "Token":"Auth token"
        /// "OrderID" :1,
        /// "IDType" :"PAN",
        /// "IDNumber":"P23FD",
        /// "ID" : FileInput,
        /// "NameInNRIC" : "Name as in NRIC",
        /// "Gender":"Male",
        /// "DOB":"15/12/2000", //dd/MM/yyyy
        /// "ContactNumber":"95421232", 
        /// "Nationality":"singaporean",
        /// 
        /// }
        /// </param>
        /// <returns>OperationResponse</returns>
        [Route("updateorderpersonaldetails")]
        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> UpdateOrderPersonalDetails([FromForm] UpdateOrderPersonalDetailsRequest request)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(request.Token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                    {
                        IFormFile file = request.ID;

                        BSSAPIHelper helper = new BSSAPIHelper();

                        MiscHelper configHelper = new MiscHelper();

                        UpdateOrderPersonalDetails personalDetails = new UpdateOrderPersonalDetails
                        {
                            OrderID = request.OrderID,
                            ContactNumber = request.ContactNumber,
                            DOB = request.DOB,
                            Gender = request.Gender,
                            IDNumber = request.IDNumber,
                            IDType = request.IDType,
                            NameInNRIC = request.NameInNRIC,
                            Nationality = request.Nationality
                        };

                        //process file if uploaded - non null

                        if (file != null)
                        {
                            DatabaseResponse awsConfigResponse = await _orderAccess.GetConfiguration(ConfiType.AWS.ToString());

                            if (awsConfigResponse != null && awsConfigResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                            {
                                GridAWSS3Config awsConfig = configHelper.GetGridAwsConfig((List<Dictionary<string, string>>)awsConfigResponse.Results);

                                AmazonS3 s3Helper = new AmazonS3(awsConfig);

                                string fileName = "Grid_IDNUMBER_" + DateTime.UtcNow.ToString("yyyymmddhhmmss") + Path.GetExtension(file.FileName); //Grid_IDNUMBER_yyyymmddhhmmss.extension

                                UploadResponse s3UploadResponse = await s3Helper.UploadFile(file, fileName);

                                if (s3UploadResponse.HasSucceed)
                                {
                                    personalDetails.IDImageUrl = s3UploadResponse.FileName;
                                }
                                else
                                {
                                    LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.S3UploadFailed));
                                }
                            }
                            else
                            {
                                // unable to get aws config
                                LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetConfiguration));

                            }
                        }    //file                     

                        //update personal details
                        DatabaseResponse updatePersoanDetailsResponse = await _orderAccess.UpdateOrderPersonalDetails(personalDetails);

                        if (updatePersoanDetailsResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(DbReturnValue.UpdateSuccess),
                                IsDomainValidationErrors = false
                            });
                        }
                        else
                        {
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedUpdatePersonalDetails));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.UpdationFailed),
                                IsDomainValidationErrors = false
                            });
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

        /// <summary>
        /// This will update billing details of the customer for the order
        /// </summary>
        /// <param name="request">
        /// body{
        /// "Token":"Auth token"
        /// "OrderID" :1,
        /// "Postcode" :"4563",
        /// "BlockNumber":"P23FD",
        /// "Unit" : "",
        /// "Floor" : "Name as in NRIC",
        /// "BuildingName":"Male",
        /// "StreetName":"", 
        /// "ContactNumber":"95421232", 
        /// }
        /// </param>
        /// <returns>OperationResponse</returns>
        [Route("updateorderbillingdetails")]
        [HttpPost]
        public async Task<IActionResult> UpdateOrderBillingDetails([FromBody] UpdateOrderBillingDetailsRequest request)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(request.Token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                    {
                        //update billing details
                        DatabaseResponse updatePersoanDetailsResponse = await _orderAccess.UpdateOrderBillingDetails(request);

                        if (updatePersoanDetailsResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(DbReturnValue.UpdateSuccess),
                                IsDomainValidationErrors = false
                            });
                        }
                        else
                        {
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedUpdateBillingDetails));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.UpdationFailed),
                                IsDomainValidationErrors = false
                            });
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

        /// <summary>
        /// This will update Shipping details of the customer for the order
        /// </summary>
        /// <param name="request">
        /// body{
        /// "Token":"Auth token"
        /// "OrderID" :1,
        /// "Postcode" :"4563",
        /// "BlockNumber":"P23FD",
        /// "Unit" : "",
        /// "Floor" : "Name as in NRIC",
        /// "BuildingName":"Male",
        /// "StreetName":"", 
        /// "ContactNumber":"95421232", 
        /// "IsBillingSame":1, 
        /// "PortalSlotID":"12dfsf", 
        /// }
        /// </param>
        /// <returns>OperationResponse</returns>
        [Route("updateordershippingdetails")]
        [HttpPost]
        public async Task<IActionResult> UpdateOrderShippingDetails([FromBody] UpdateOrderShippingDetailsRequest request)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(request.Token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                    {
                        //update shipping details
                        DatabaseResponse updatePersoanDetailsResponse = await _orderAccess.UpdateOrderShippingDetails(request);

                        if (updatePersoanDetailsResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(DbReturnValue.UpdateSuccess),
                                IsDomainValidationErrors = false
                            });
                        }
                        else
                        {
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedUpdateShippingDetails));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.UpdationFailed),
                                IsDomainValidationErrors = false
                            });
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

        /// <summary>
        /// This will update LOA details of the customer for the order
        /// </summary>
        /// <param name="request">
        /// body{
        /// "Token":"Auth token"
        /// "OrderID" :1,
        /// "RecipientName" :"4563",
        /// "IDType":"P23FD",
        /// "IDNumber" : "",      
        /// "ContactNumber":"95421232", 
        /// "EmailAdddress":"",        
        /// }
        /// </param>
        /// <returns>OperationResponse</returns>
        [Route("updateorderloadetails")]
        [HttpPost]
        public async Task<IActionResult> UpdateOrderLOADetails([FromBody] UpdateOrderLOADetailsRequest request)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(request.Token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                    {
                        //update shipping details
                        DatabaseResponse updatePersoanDetailsResponse = await _orderAccess.UpdateOrderLOADetails(request);

                        if (updatePersoanDetailsResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(DbReturnValue.UpdateSuccess),
                                IsDomainValidationErrors = false
                            });
                        }
                        else
                        {
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedUpdateLOADetails));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.UpdationFailed),
                                IsDomainValidationErrors = false
                            });
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

        /// <summary>
        /// This will validate referral code for the order
        /// </summary>
        /// <param name="request">
        /// body{
        /// "Token":"Auth token"
        /// "OrderID" :1,
        /// "ReferralCode" :"A4EDFE23",       
        /// }
        /// </param>
        /// <returns>OperationResponse</returns>
        [Route("validateorderreferralcode")]
        [HttpPost]
        public async Task<IActionResult> ValidateOrderReferralCode([FromBody] ValidateOrderReferralCodeRequest request)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(request.Token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                    {
                        //validate referral code
                        DatabaseResponse validateResponse = await _orderAccess.ValidateOrderReferralCode(request);

                        if (validateResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(CommonErrors.ReferralCodeExists),
                                IsDomainValidationErrors = false
                            });
                        }
                        else
                        {
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.ReferralCodeNotExists));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.ReferralCodeNotExists),
                                IsDomainValidationErrors = false
                            });
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
        /// <summary>
        /// This will return all subscriptions/numbers for the given OrderID
        /// </summary>
        /// <param name="request">
        /// body{
        /// "Token":"Auth token"
        /// "OrderID" :1             
        /// }
        /// </param>
        /// <returns>OperationResponse</returns>

        [Route("getorderednumbers")]
        [HttpPost]
        public async Task<IActionResult> GetOrderedNumbers([FromBody] OrderedNumberRequest request)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(request.Token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                    {
                        //get ordered numbers
                        DatabaseResponse numberResponse = await _orderAccess.GetOrderedNumbers(request);

                        if (numberResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(DbReturnValue.RecordExists),
                                IsDomainValidationErrors = false,
                                ReturnedObject = numberResponse.Results
                            });
                        }
                        else
                        {
                            LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.NoRecords));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NoRecords),
                                IsDomainValidationErrors = false
                            });
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

        /// <summary>
        /// This will update Order subscription details
        /// </summary>
        /// <param name="request">
        /// body{
        /// "Token":"Auth token"
        /// "OrderID" :1,
        /// "ContactNumber" :"95421232", // optional
        /// "Terms":"1",
        /// "PaymentSubscription" : "1",      
        /// "PromotionMessage":"1",             
        /// }
        /// </param>
        /// <returns>OperationResponse</returns>
        [Route("updateordersubscriptiondetails")]
        [HttpPost]
        public async Task<IActionResult> UpdateOrderSubscriptionDetails([FromBody] UpdateOrderSubcriptionDetailsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(request.Token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                    {
                        //update shipping details
                        DatabaseResponse updatePersoanDetailsResponse = await _orderAccess.UpdateOrderSubcriptionDetails(request);

                        if (updatePersoanDetailsResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(DbReturnValue.UpdateSuccess),
                                IsDomainValidationErrors = false
                            });
                        }
                        else
                        {
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToUpdatedSubscriptionDetails));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.UpdationFailed),
                                IsDomainValidationErrors = false
                            });
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
        /// <summary>
        /// This will return OrderID, OrderNumber, OrderDate in pending order Details
        /// </summary>
        /// <param name="token"></param>
        /// <returns>OperationResponse</returns>
        [HttpGet("GetPendingOrderDetails/{token}")]
        public async Task<IActionResult> GetPendingOrderDetails([FromRoute]string token)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage))
                    });
                }

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                    {
                        //get ordered numbers
                        DatabaseResponse pendingOrderDetailsResponse = await _orderAccess.GetPendingOrderDetails(aTokenResp.CustomerID);

                        if (pendingOrderDetailsResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(DbReturnValue.RecordExists),
                                IsDomainValidationErrors = false,
                                ReturnedObject = pendingOrderDetailsResponse.Results
                            });
                        }
                        else
                        {
                            LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.NoRecords));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NoRecords),
                                IsDomainValidationErrors = false
                            });
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
         
        /// <summary>
        /// This will create a checkout session and returns the details to call MPGS 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="orderId"></param>
        /// <returns>OperationsResponse</returns>
        [HttpGet("GetCheckOutDetails/{token}/{orderId}")]
        public async Task<IActionResult> GetCheckOutDetails([FromRoute] string token, int orderId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage))
                    });
                }

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                    {
                        // Call MPGS to create a checkout session and retuen details

                        PaymentHelper gatewayHelper = new PaymentHelper();

                        Checkout checkoutDetails = new Checkout();

                        DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.MPGS.ToString());

                        GridMPGSConfig gatewayConfig = gatewayHelper.GetGridMPGSConfig((List<Dictionary<string, string>>)configResponse.Results);

                        checkoutDetails = gatewayHelper.CreateCheckoutSession(gatewayConfig);

                        CheckOutRequestDBUpdateModel checkoutUpdateModel = new CheckOutRequestDBUpdateModel
                        {
                            Source = "Orders",

                            SourceID =orderId,

                            CheckOutSessionID =checkoutDetails.CheckoutSession.Id,

                            CheckoutVersion =checkoutDetails.CheckoutSession.Version,

                            SuccessIndicator =checkoutDetails.CheckoutSession.SuccessIndicator,

                            MPGSOrderID =checkoutDetails.OrderId
                        };

                        //Update checkout details and return amount

                        DatabaseResponse checkOutAmountResponse = await _orderAccess.GetCheckoutRequestDetails(checkoutUpdateModel);

                        if (checkOutAmountResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                        {
                            checkoutDetails.Amount = ((Checkout)checkOutAmountResponse.Results).Amount;

                            return Ok(new OperationResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(CommonErrors.CheckoutSessionCreated),
                                IsDomainValidationErrors = false,
                                ReturnedObject = checkoutDetails
                            });
                        }
                        else
                        {
                            LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.NoRecords));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NoRecords),
                                IsDomainValidationErrors = false
                            });
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

        /// <summary>
        /// This will update checkout response to database and retrieve checkout infor from gateway
        /// </summary>
        /// <param name="updateRequest">
        /// body{
        /// "Token":"Auth token"
        /// "MPGSOrderID" :"f88bere0",
        /// "CheckOutSessionID" :"SESSION0002391471348N70583782K8",
        /// "Result":"Success"       
        /// }
        /// </param>
        /// <returns>OperationResponse</returns>
        [Route("UpdateCheckOutResponse")]
        [HttpPost]
        public async Task<IActionResult> UpdateCheckOutResponse([FromBody] CheckOutResponseUpdate updateRequest)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(updateRequest.Token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                    {
                        //update checkout details
                        DatabaseResponse updateCheckoutDetailsResponse = await _orderAccess.UpdateCheckOutResponse(updateRequest);

                        // retrieve transaction details from MPGS

                        DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.MPGS.ToString());

                        PaymentHelper gatewayHelper = new PaymentHelper();

                        GridMPGSConfig gatewayConfig = gatewayHelper.GetGridMPGSConfig((List<Dictionary<string, string>>)configResponse.Results);
                         
                        TransactionRetrieveResponseOperation transactionResponse = new TransactionRetrieveResponseOperation();

                        transactionResponse = gatewayHelper.RetrieveCheckOutTransaction(gatewayConfig, updateRequest);

                        // deside on the fields to update to database for payment processing and call SP to update

                        DatabaseResponse paymentProcessingRespose = new DatabaseResponse();

                        paymentProcessingRespose = await _orderAccess.UpdateCheckOutReceipt(transactionResponse.TrasactionResponse);

                        if (paymentProcessingRespose.ResponseCode == (int)DbReturnValue.TransactionSuccess)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(DbReturnValue.TransactionSuccess),
                                IsDomainValidationErrors = false,
                                ReturnedObject = transactionResponse // check if need to return this data 
                            });
                        }
                        else
                        {
                            LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.TransactionFailed));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.TransactionFailed),
                                IsDomainValidationErrors = false
                            });
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

        [Route("RemoveAdditionalLine")]
        [HttpPost]
        public async Task<IActionResult> RemoveAdditionalLine([FromBody] RemoveAdditionalLineRequest request)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(request.Token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                    {
                        //remove additional line
                        DatabaseResponse removeLineResponse = await _orderAccess.RemoveAdditionalLine(request);

                        if (removeLineResponse.ResponseCode == (int)DbReturnValue.DeleteSuccess)
                        {
                            // call bss api to release/unblock the number

                            OrderCustomer customer = new OrderCustomer();

                            DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(request.OrderID);

                            if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                            {
                                customer = (OrderCustomer)customerResponse.Results;

                                BSSAPIHelper helper = new BSSAPIHelper();

                                MiscHelper configHelper = new MiscHelper();

                                DatabaseResponse requestIdToUpdateUnblock = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customer.CustomerId);

                                DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                                GridBSSConfi config = helper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                                // Unblock
                                BSSUpdateResponseObject bssUnblockUpdateResponse = await helper.UpdateAssetBlockNumber(config, ((BSSAssetRequest)requestIdToUpdateUnblock.Results).request_id, request.MobileNumber, true);

                                if (helper.GetResponseCode(bssUnblockUpdateResponse) == "0")
                                {
                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = true,
                                        Message = EnumExtensions.GetDescription(CommonErrors.LineDeleteSuccess),
                                        IsDomainValidationErrors = false
                                    });

                                }

                                else
                                {
                                    /*
                                     revice - check if revert the line deleted in database in case number releasing failed in BSS
                                     */

                                    // unblocking failed -  
                                    LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.UpdateAssetUnBlockingFailed));
                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = false,
                                        Message = EnumExtensions.GetDescription(DbReturnValue.UnBlockingFailed),
                                        IsDomainValidationErrors = false
                                    });
                                }

                            }

                            else
                            {
                                // failed to locate customer
                                LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.UnBlockingFailed),
                                    IsDomainValidationErrors = false
                                });
                            }

                        }
                       
                        else if (removeLineResponse.ResponseCode == (int)DbReturnValue.ActiveTryDelete)
                        {
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.LineDeleteFailed));

                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.ActiveTryDelete),
                                IsDomainValidationErrors = false
                            });
                        }

                        else if (removeLineResponse.ResponseCode == (int)DbReturnValue.PrimaryTryDelete)
                        {
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.LineDeleteFailed));

                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.PrimaryTryDelete),
                                IsDomainValidationErrors = false
                            });
                        }

                        else if (removeLineResponse.ResponseCode == (int)DbReturnValue.CompletedOrderDelete)
                        {
                            LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.CompletedOrderDelete));

                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.CompletedOrderDelete),
                                IsDomainValidationErrors = false
                            });
                        }
                        else 
                        {
                            LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.NotExists));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NotExists),
                                IsDomainValidationErrors = false
                            });
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

        [Route("AssignNewNumberToSubscriber")]
        [HttpPost]
        public async Task<IActionResult> AssignNewNumberToSubscriber([FromBody] AssignNewNumberRequest request)
        {
           try
                {
                    if (!ModelState.IsValid)
                    {
                        return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                        {
                            HasSucceeded = false,
                            IsDomainValidationErrors = true,
                            Message = string.Join("; ", ModelState.Values
                                                     .SelectMany(x => x.Errors)
                                                     .Select(x => x.ErrorMessage))
                        });
                    }

                    OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                    DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(request.Token);

                    if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                    {

                        AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                        if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                        {
                            // call GetAssets BSSAPI

                            BSSAPIHelper helper = new BSSAPIHelper();

                            DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                            GridBSSConfi config = helper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                                config.GridDefaultAssetLimit = 1; // to get only on asset

                            DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                            DatabaseResponse requestIdRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), aTokenResp.CustomerID);

                            ResponseObject res = await helper.GetAssetInventory(config, (((List<ServiceFees>)serviceCAF.Results)).FirstOrDefault().ServiceCode, ((BSSAssetRequest)requestIdRes.Results).request_id);

                            string NewNumber = helper.GetAssetId(res);

                            if (res != null && (int.Parse(res.Response.asset_details.total_record_count) > 0))
                            {
                                //Block number                                    

                                DatabaseResponse requestIdToUpdateRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), aTokenResp.CustomerID);

                                BSSUpdateResponseObject bssUpdateResponse = await helper.UpdateAssetBlockNumber(config, ((BSSAssetRequest)requestIdToUpdateRes.Results).request_id, NewNumber, false);

                                if (helper.GetResponseCode(bssUpdateResponse) == "0")
                                {
                                    // Assign Newnumber
                                    AssignNewNumber newNumbertoAssign = new AssignNewNumber { OrderID = request.OrderID, OldNumber=request.OldNumber,NewNumber= NewNumber };

                                    DatabaseResponse AssignNewNumberResponse = await _orderAccess.AssignNewNumber(newNumbertoAssign);

                                    if (AssignNewNumberResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                                    {
                                        // Get Order Basic Details

                                        DatabaseResponse orderDetailsResponse = await _orderAccess.GetOrderBasicDetails(request.OrderID);

                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = true,
                                            Message = EnumExtensions.GetDescription(CommonErrors.AssignNuewNumberSuccess),
                                            IsDomainValidationErrors = false,
                                            ReturnedObject = orderDetailsResponse.Results
                                        });

                                    }

                                    else
                                    {
                                        // Assign Newnumber failed
                                        LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.AssignNewNumberFailed));

                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = true,
                                            Message = EnumExtensions.GetDescription(CommonErrors.AssignNewNumberFailed),
                                            IsDomainValidationErrors = false

                                        });
                                    }


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