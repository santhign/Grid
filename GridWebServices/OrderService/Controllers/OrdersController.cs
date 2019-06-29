using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.Models.Transaction;
using OrderService.DataAccess;
using OrderService.Helpers;
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
using System.Net.Mail;

namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        IConfiguration _iconfiguration;
        private readonly IMessageQueueDataAccess _messageQueueDataAccess;

        public OrdersController(IConfiguration configuration, IMessageQueueDataAccess messageQueueDataAccess)
        {
            _iconfiguration = configuration;
            _messageQueueDataAccess = messageQueueDataAccess;
        }
        /// <summary>
        /// This will return Order details for specific ID passed 
        /// </summary>       
        /// <param name="token"></param>
        /// <param name="id">OrderID</param>
        /// <returns>OperationsResponse</returns>
        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] int id)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
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

                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(id);

                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
                        {
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
                        else
                        {
                            // failed to locate customer
                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }

                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// This will Create an order for the logged in customer with the bundle selected.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="request">CreateOrderRequest</param>
        ///Body: 
        ///{
        ///	"BundleID" : "1",
        ///	"ReferralCode" : "dkfsdsd" --optional
        ///	"PromotionCode" : "Launch2019" --optional
        ///}
        /// <returns>OperationResponse</returns>
        // POST: api/Orders
        [HttpPost]
        public async Task<IActionResult> Post([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] CreateOrderRequest request)
        {

            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Orders_CreateOrder);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                         .SelectMany(x => x.Errors)
                                                         .Select(x => x.ErrorMessage))
                            });
                        }

                        EmailValidationHelper _helper = new EmailValidationHelper();

                        bool AllowSrubscriber = await _helper.AllowSubscribers(customerID, (int)SubscriberCheckType.CustomerLevel, _iconfiguration);

                        if (!AllowSrubscriber)
                        {
                            LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NotAllowSubscribers) + " for customer:" + customerID + ". Payload:" + JsonConvert.SerializeObject(request)+ "\n");

                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NotAllowSubscribers),
                                IsDomainValidationErrors = false
                            });
                        }
                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        CreateOrder order = new CreateOrder();

                        order = new CreateOrder { BundleID = request.BundleID, PromotionCode = request.PromotionCode, ReferralCode = request.ReferralCode, CustomerID = customerID };

                        DatabaseResponse createOrderRresponse = await _orderAccess.CreateOrder(order);

                        if (createOrderRresponse.ResponseCode == ((int)DbReturnValue.CreationFailed))
                        {
                            // order creation failed

                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.CreateOrderFailed) + " for customer:" + customerID+ ". Payload:" + JsonConvert.SerializeObject(request) + "\n");

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

                                try
                                {

                                    BSSAPIHelper bsshelper = new BSSAPIHelper();

                                    DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());


                                    GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                                    DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                                    DatabaseResponse requestIdRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), customerID, (int)BSSCalls.NewSession, "");

                                    ResponseObject res = new ResponseObject();

                                    try
                                    {
                                        res = await bsshelper.GetAssetInventory(config, (((List<ServiceFees>)serviceCAF.Results)).FirstOrDefault().ServiceCode, (BSSAssetRequest)requestIdRes.Results);
                                    }
                                   
                                    catch(Exception ex)
                                    {
                                        LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                                        DatabaseResponse rollbackResponse = await _orderAccess.RollBackOrder(((OrderInit)createOrderRresponse.Results).OrderID);

                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = false,
                                            Message = EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed) + ". " + EnumExtensions.GetDescription(CommonErrors.OrderRolledBack),
                                            IsDomainValidationErrors = false
                                        });

                                    }

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

                                        DatabaseResponse requestIdToUpdateRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customerID, (int)BSSCalls.ExistingSession, AssetToSubscribe);

                                        BSSUpdateResponseObject bssUpdateResponse = new BSSUpdateResponseObject();

                                        try
                                        {
                                            bssUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateRes.Results, AssetToSubscribe, false);
                                        }

                                        catch (Exception ex)
                                        {
                                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                                            DatabaseResponse rollbackResponse = await _orderAccess.RollBackOrder(((OrderInit)createOrderRresponse.Results).OrderID);

                                            return Ok(new OperationResponse
                                            {
                                                HasSucceeded = false,
                                                Message = EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed) + ". " + EnumExtensions.GetDescription(CommonErrors.OrderRolledBack),
                                                IsDomainValidationErrors = false
                                            });
                                        }


                                        if (bsshelper.GetResponseCode(bssUpdateResponse) == "0")
                                        {
                                            // create subscription
                                            CreateSubscriber subscriberToCreate = new CreateSubscriber { BundleID = request.BundleID, OrderID = ((OrderInit)createOrderRresponse.Results).OrderID, MobileNumber = AssetToSubscribe, PromotionCode = request.PromotionCode }; // verify isPrimary

                                            DatabaseResponse createSubscriberResponse = await _orderAccess.CreateSubscriber(subscriberToCreate, ((BSSAssetRequest)requestIdToUpdateRes.Results).userid);

                                            if (createSubscriberResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                                            {
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
                                                // create subscription failed

                                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.CreateSubscriptionFailed) + "\n");

                                                DatabaseResponse rollbackResponse = await _orderAccess.RollBackOrder(((OrderInit)createOrderRresponse.Results).OrderID);

                                                if (rollbackResponse.ResponseCode == (int)DbReturnValue.DeleteSuccess)
                                                {
                                                    return Ok(new OperationResponse
                                                    {
                                                        HasSucceeded = false,
                                                        Message = EnumExtensions.GetDescription(CommonErrors.CreateSubscriptionFailed) + " " + EnumExtensions.GetDescription(CommonErrors.OrderRolledBack),
                                                        IsDomainValidationErrors = false
                                                    });
                                                }

                                                else
                                                {
                                                    return Ok(new OperationResponse
                                                    {
                                                        HasSucceeded = false,
                                                        Message = EnumExtensions.GetDescription(CommonErrors.CreateSubscriptionFailed) + " " + EnumExtensions.GetDescription(CommonErrors.OrderRolledBackFailed),
                                                        IsDomainValidationErrors = false
                                                    });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //blocking failed

                                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.UpdateAssetBlockingFailed) + "\n");

                                            //ROLLBACK ORDER
                                            DatabaseResponse rollbackResponse = await _orderAccess.RollBackOrder(((OrderInit)createOrderRresponse.Results).OrderID);

                                            if (rollbackResponse.ResponseCode == (int)DbReturnValue.DeleteSuccess)
                                            {
                                                return Ok(new OperationResponse
                                                {
                                                    HasSucceeded = false,
                                                    Message = EnumExtensions.GetDescription(DbReturnValue.BlockingFailed) + " " + EnumExtensions.GetDescription(CommonErrors.OrderRolledBack),
                                                    IsDomainValidationErrors = false
                                                });
                                            }
                                            else
                                            {
                                                return Ok(new OperationResponse
                                                {
                                                    HasSucceeded = false,
                                                    Message = EnumExtensions.GetDescription(DbReturnValue.BlockingFailed) + " " + EnumExtensions.GetDescription(CommonErrors.OrderRolledBackFailed),
                                                    IsDomainValidationErrors = false
                                                });
                                            }
                                        }

                                    }
                                    else
                                    {
                                        // no assets returned                                   

                                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.GetAssetFailed) + "\n");

                                        DatabaseResponse rollbackResponse = await _orderAccess.RollBackOrder(((OrderInit)createOrderRresponse.Results).OrderID);

                                        if (rollbackResponse.ResponseCode == (int)DbReturnValue.DeleteSuccess)
                                        {
                                            return Ok(new OperationResponse
                                            {
                                                HasSucceeded = false,
                                                Message = EnumExtensions.GetDescription(CommonErrors.GetAssetFailed) + " " + EnumExtensions.GetDescription(CommonErrors.OrderRolledBack),
                                                IsDomainValidationErrors = false
                                            });
                                        }
                                        else
                                        {
                                            return Ok(new OperationResponse
                                            {
                                                HasSucceeded = false,
                                                Message = EnumExtensions.GetDescription(CommonErrors.GetAssetFailed) + " " + EnumExtensions.GetDescription(CommonErrors.OrderRolledBackFailed),
                                                IsDomainValidationErrors = false
                                            });
                                        }

                                    }
                                }

                                catch (Exception ex)
                                {
                                    LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.GetAssetFailed) + "\n");

                                    LogInfo.Fatal(ex, EnumExtensions.GetDescription(CommonErrors.GetAssetFailed) + "\n");

                                    DatabaseResponse rollbackResponse = await _orderAccess.RollBackOrder(((OrderInit)createOrderRresponse.Results).OrderID);

                                    if (rollbackResponse.ResponseCode == (int)DbReturnValue.DeleteSuccess)
                                    {
                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = false,
                                            Message = EnumExtensions.GetDescription(CommonErrors.GetAssetFailed) + " " + EnumExtensions.GetDescription(CommonErrors.OrderRolledBack),
                                            IsDomainValidationErrors = false
                                        });
                                    }
                                    else
                                    {
                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = false,
                                            Message = EnumExtensions.GetDescription(CommonErrors.GetAssetFailed) + " " + EnumExtensions.GetDescription(CommonErrors.OrderRolledBackFailed),
                                            IsDomainValidationErrors = false
                                        });
                                    }
                                }

                            }

                            else
                            {
                                // old order-- return order details

                                LogInfo.Information(EnumExtensions.GetDescription(CommonErrors.UnfishedOrderExists) + "\n");

                                DatabaseResponse orderDetailsResponse = await _orderAccess.GetOrderBasicDetails(((OrderInit)createOrderRresponse.Results).OrderID);

                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = true,
                                    Message = EnumExtensions.GetDescription(CommonErrors.UnfishedOrderExists),
                                    IsDomainValidationErrors = false,
                                    ReturnedObject = orderDetailsResponse.Results

                                });
                            }
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken) + "\n");

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
                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed) + "\n");

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
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + " for customer token:" + token + ". Payload:" + JsonConvert.SerializeObject(request) + "\n");

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
        /// <param name="token"></param>
        /// <param name="request">
        /// body{
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
        public async Task<IActionResult> UpdateSubscriberNumber([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] UpdateSubscriberNumber request)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Orders_UpdateSubscriberNumber);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        BSSAPIHelper bsshelper = new BSSAPIHelper();

                        OrderCustomer customer = new OrderCustomer();

                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(request.OrderID);

                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
                        {
                            customer = (OrderCustomer)customerResponse.Results;

                            DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                            GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                            // check if the number is ported

                            DatabaseResponse isPortedResponse = await _orderAccess.NumberIsPorted(request.OrderID, request.OldMobileNumber);

                            if (isPortedResponse != null && isPortedResponse.ResponseCode == (int)DbReturnValue.RecordExists && isPortedResponse.Results != null)
                            {
                                if (((int)isPortedResponse.Results) != 1)
                                {
                                    DatabaseResponse requestIdToUpdateUnblock = await _orderAccess.GetBssApiRequestIdAndSubscriberSession(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customer.CustomerId, request.OldMobileNumber);

                                    // Unblock

                                    BSSUpdateResponseObject bssUnblockUpdateResponse = new BSSUpdateResponseObject();

                                    try
                                    {
                                        bssUnblockUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateUnblock.Results, request.OldMobileNumber, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + " " + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = false,
                                            Message = EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed),
                                            IsDomainValidationErrors = false
                                        });

                                    }

                                    if (bsshelper.GetResponseCode(bssUnblockUpdateResponse) == "0")
                                    {
                                        //Block

                                        DatabaseResponse requestIdToUpdateBlock = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customer.CustomerId, (int)BSSCalls.ExistingSession, request.NewNumber.MobileNumber);

                                        BSSUpdateResponseObject bssUpdateResponse = new BSSUpdateResponseObject();

                                        try
                                        {
                                            bssUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateBlock.Results, request.NewNumber.MobileNumber, false);
                                        }
                                        catch (Exception ex)
                                        {
                                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + " " + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                                            return Ok(new OperationResponse
                                            {
                                                HasSucceeded = false,
                                                Message = EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed),
                                                IsDomainValidationErrors = false
                                            });

                                        }

                                        if (bsshelper.GetResponseCode(bssUpdateResponse) == "0")
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
                                                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToLocateUpdatedSubscription));
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
                                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.UpdateSubscriptionFailed));
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

                                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.UpdateAssetBlockingFailed) + ". Customer:" + customerID);
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
                                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.UpdateAssetUnBlockingFailed) + ". Customer:" + customerID);
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
                                    // ported number just block new number

                                    DatabaseResponse requestIdToUpdateBlock = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customer.CustomerId, (int)BSSCalls.ExistingSession, request.NewNumber.MobileNumber);

                                    BSSUpdateResponseObject bssUpdateResponse = new BSSUpdateResponseObject();

                                    try
                                    {
                                        bssUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateBlock.Results, request.NewNumber.MobileNumber, false);
                                    }
                                    catch (Exception ex)
                                    {
                                        LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + " " + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = false,
                                            Message = EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed),
                                            IsDomainValidationErrors = false
                                        });

                                    }

                                    if (bsshelper.GetResponseCode(bssUpdateResponse) == "0")
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
                                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToLocateUpdatedSubscription));
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
                                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.UpdateSubscriptionFailed));
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

                                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.UpdateAssetBlockingFailed) + ". Customer:" + customerID);
                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = false,
                                            Message = EnumExtensions.GetDescription(DbReturnValue.BlockingFailed),
                                            IsDomainValidationErrors = false
                                        });
                                    }
                                }
                            }

                            else
                            {
                                // Old number not found in database

                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.OldNumberNotExists));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(CommonErrors.OldNumberNotExists),
                                    IsDomainValidationErrors = false
                                });
                            }

                        }

                        else
                        {
                            // failed to locate customer
                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + " for token:" + token);

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
        /// <param name="token"></param>
        /// <param name="request">
        /// Form{
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
        public async Task<IActionResult> PortingNumber([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromForm] UpdateSubscriberPortingNumberRequest request)
        {

            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Orders_number_portin);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                            .SelectMany(x => x.Errors)
                                                            .Select(x => x.ErrorMessage))
                            });
                        }
                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        IFormFile file = request.PortedNumberTransferForm;

                        BSSAPIHelper bsshelper = new BSSAPIHelper();

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

                                string fileName = "Grid_PNTF_" + portingRequest.OrderID + "_" + DateTime.Now.ToString("yyyymmddhhmmss") + Path.GetExtension(file.FileName);

                                UploadResponse s3UploadResponse = await s3Helper.UploadFile(file, fileName);

                                if (s3UploadResponse.HasSucceed)
                                {
                                    portingRequest.PortedNumberTransferForm = s3UploadResponse.FileName;
                                }
                                else
                                {
                                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.S3UploadFailed));
                                }
                            }
                            else
                            {
                                // unable to get aws config
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToGetConfiguration));

                            }
                        }

                        OrderCustomer customer = new OrderCustomer();

                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(request.OrderID);

                        DatabaseResponse portResponse = await _orderAccess.GetPortTypeFromOrderId(request.OrderID, request.OldMobileNumber);

                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
                        {
                            if (portResponse.Results != null && portResponse.Results.ToString().Trim() == "0")
                            {
                                customer = (OrderCustomer)customerResponse.Results;

                                DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                                GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                                DatabaseResponse requestIdToUpdateUnblock = await _orderAccess.GetBssApiRequestIdAndSubscriberSession(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customer.CustomerId, request.OldMobileNumber);

                                // Unblock

                                BSSUpdateResponseObject bssUnblockUpdateResponse = new BSSUpdateResponseObject();

                                try
                                {
                                    bssUnblockUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateUnblock.Results, request.OldMobileNumber, true);
                                }
                                catch (Exception ex)
                                {
                                    LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + " " + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = false,
                                        Message = EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed),
                                        IsDomainValidationErrors = false
                                    });

                                }

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
                                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToLocateUpdatedSubscription));
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
                                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.UpdateSubscriptionFailed));
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
                                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToLocateUpdatedSubscription));
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
                                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.UpdateSubscriptionFailed));
                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = false,
                                        Message = EnumExtensions.GetDescription(DbReturnValue.UpdationFailed),
                                        IsDomainValidationErrors = false
                                    });
                                }
                            }
                        }

                        else
                        {
                            // failed to locate customer
                              LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }

                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + ". token:" + token);

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
        /// <param name="token"></param>
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
        public async Task<IActionResult> CreateSubscriber([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] AdditionalSubscriberRequest request)
        {

            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Orders_subscriber_create);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                          .SelectMany(x => x.Errors)
                                                          .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(request.OrderID);

                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
                        {
                            // call GetAssets BSSAPI
                            Core.Helpers.EmailValidationHelper _helper = new EmailValidationHelper();
                            bool AllowSrubscriber = await _helper.AllowSubscribers(customerID, (int)SubscriberCheckType.OrderLevel, _iconfiguration);
                            if (!AllowSrubscriber)
                            {
                                LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NotAllowSubscribers));

                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.NotAllowSubscribers),
                                    IsDomainValidationErrors = false
                                });
                            }
                            BSSAPIHelper bsshelper = new BSSAPIHelper();

                            DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                            GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                            DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                            DatabaseResponse requestIdRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), customerID, (int)BSSCalls.NewSession, "");

                            ResponseObject res = new ResponseObject();

                            try
                            {
                                res = await bsshelper.GetAssetInventory(config, (((List<ServiceFees>)serviceCAF.Results)).FirstOrDefault().ServiceCode, (BSSAssetRequest)requestIdRes.Results);
                            }

                            catch (Exception ex)
                            {
                                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed) + ". " + EnumExtensions.GetDescription(CommonErrors.OrderRolledBack),
                                    IsDomainValidationErrors = false
                                });

                            }


                            string AssetToSubscribe = bsshelper.GetAssetId(res);

                            if (res != null)
                            {
                                BSSNumbers numbers = new BSSNumbers();

                                numbers.FreeNumbers = bsshelper.GetFreeNumbers(res);

                                //insert this AssetToSubscribe into database

                                string json = bsshelper.GetJsonString(numbers.FreeNumbers); // json insert

                                DatabaseResponse updateBssCallFeeNumbers = await _orderAccess.UpdateBSSCallNumbers(json, ((BSSAssetRequest)requestIdRes.Results).userid, ((BSSAssetRequest)requestIdRes.Results).BSSCallLogID);
                            }

                            if (res != null && (int.Parse(res.Response.asset_details.total_record_count) > 0))
                            {
                                //Block number                                   

                                DatabaseResponse requestIdToUpdateRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customerID, (int)BSSCalls.ExistingSession, AssetToSubscribe);

                                BSSUpdateResponseObject bssUpdateResponse = new BSSUpdateResponseObject();

                                try
                                {
                                    bssUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateRes.Results, AssetToSubscribe, false);
                                }

                                catch (Exception ex)
                                {
                                    LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = false,
                                        Message = EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed) + ". " + EnumExtensions.GetDescription(CommonErrors.OrderRolledBack),
                                        IsDomainValidationErrors = false
                                    });

                                }


                                if (bsshelper.GetResponseCode(bssUpdateResponse) == "0")
                                {
                                    // create subscription
                                    CreateSubscriber subscriberToCreate = new CreateSubscriber { BundleID = request.BundleID, OrderID = request.OrderID, MobileNumber = AssetToSubscribe, PromotionCode = "" };

                                    DatabaseResponse createSubscriberResponse = await _orderAccess.CreateSubscriber(subscriberToCreate, ((BSSAssetRequest)requestIdToUpdateRes.Results).userid);

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
                                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.CreateSubscriptionFailed));

                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = true,
                                            Message = EnumExtensions.GetDescription(DbReturnValue.CreationFailed),
                                            IsDomainValidationErrors = false

                                        });
                                    }


                                }
                                else
                                {
                                    //blocking failed

                                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.UpdateAssetBlockingFailed));
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

                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.GetAssetFailed));

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
                            // failed to locate customer
                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical)  +". token:" + token);

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
        public async Task<IActionResult> GetAvailableSlots([FromHeader(Name = "Grid-Authorization-Token")] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

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
                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.DeliverySlotNotExists));
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

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + ". token:" + token);

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
        /// <param name="token"></param>
        /// <param name="request">
        /// Form{
        /// "OrderID" :1,
        /// "NameInNRIC" : "Name as in NRIC",
        /// "DisplayName" : "DisplayName",
        /// "Gender":"Male",
        /// "DOB":"15/12/2000", //dd/MM/yyyy
        /// "ContactNumber":"95421232"
        /// 
        /// }
        /// </param>
        /// <returns>OperationResponse</returns>
        [Route("updateorderpersonaldetails")]    
        [HttpPost]
        public async Task<IActionResult> UpdateOrderPersonalDetails([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] UpdateOrderPersonalDetailsRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Orders_personaldetails_update);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;

                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                          .SelectMany(x => x.Errors)
                                                          .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(request.OrderID);

                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
                        {
                            BSSAPIHelper bsshelper = new BSSAPIHelper();

                            MiscHelper configHelper = new MiscHelper();

                            UpdateOrderPersonalDetails personalDetails = new UpdateOrderPersonalDetails
                            {
                                OrderID = request.OrderID,
                                ContactNumber = request.ContactNumber,
                                DOB = request.DOB,
                                Gender = request.Gender,
                                NameInNRIC = request.NameInNRIC,
                                DisplayName = request.DisplayName

                            };

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
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedUpdatePersonalDetails));
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
                            // failed to locate customer
                              LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + ". token:" + token);

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
        /// <param name="token"></param>
        /// <param name="request">
        /// body{
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
        public async Task<IActionResult> UpdateOrderBillingDetails([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] UpdateOrderBillingDetailsRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Orders_billingaddress_update);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;

                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(request.OrderID);

                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
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
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedUpdateBillingDetails));
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
                            // failed to locate customer
                              LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// <param name="token"></param>
        /// <param name="request">
        /// body{
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
        public async Task<IActionResult> UpdateOrderShippingDetails([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] UpdateOrderShippingDetailsRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Orders_shippingaddress_update);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(request.OrderID);

                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
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
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedUpdateShippingDetails));
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
                            // failed to locate customer
                              LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// <param name="token"></param>
        /// <param name="request">
        /// body{
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
        public async Task<IActionResult> UpdateOrderLOADetails([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] UpdateOrderLOADetailsRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Orders_loa_update);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }


                        if (!string.IsNullOrEmpty(request.EmailAdddress))
                        {
                            try

                            {
                                MailAddress m = new MailAddress(request.EmailAdddress);
                            }
                            catch
                            {
                                LogInfo.Error(StatusMessages.DomainValidationError);

                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    IsDomainValidationErrors = true,
                                    Message = EnumExtensions.GetDescription(CommonErrors.InvalidEmail),
                                });

                            }
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(request.OrderID);

                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
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
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedUpdateLOADetails));
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
                            // failed to locate customer
                              LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// <param name="token"></param>
        /// <param name="request">
        /// body{
        /// "OrderID" :1,
        /// "ReferralCode" :"A4EDFE23",       
        /// }
        /// </param>
        /// <returns>OperationResponse</returns>
        [Route("validateorderreferralcode")]
        [HttpPost]
        public async Task<IActionResult> ValidateOrderReferralCode([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] ValidateOrderReferralCodeRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Orders_validate_order_referral);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
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
                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(request.OrderID);
                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
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
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ReferralCodeNotExists));
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
                            // failed to locate customer
                              LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// <param name="token"></param>
        /// <param name="request">
        /// body{
        /// "OrderID" :1             
        /// }
        /// </param>
        /// <returns>OperationResponse</returns>

        [Route("getorderednumbers")]
        [HttpPost]
        public async Task<IActionResult> GetOrderedNumbers([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] OrderedNumberRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {

                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);
                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(request.OrderID);
                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
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
                                LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NoRecords));
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
                            // failed to locate customer
                              LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }
                    }
                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// <param name="token"></param>
        /// <param name="request">
        /// body{
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
        public async Task<IActionResult> UpdateOrderSubscriptionDetails([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] UpdateOrderSubcriptionDetailsRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Orders_UpdateSubscription);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(request.OrderID);
                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
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
                            else if (updatePersoanDetailsResponse.ResponseCode == (int)DbReturnValue.DeliverySlotUnavailability)
                            {
                                LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.DeliverySlotUnavailability));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.DeliverySlotUnavailability),
                                    IsDomainValidationErrors = false
                                });
                            }
                            else if (updatePersoanDetailsResponse.ResponseCode == (int)DbReturnValue.OrderDeliveryInformationMissing)
                            {
                                LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.OrderDeliveryInformationMissing));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.OrderDeliveryInformationMissing),
                                    IsDomainValidationErrors = false
                                });
                            }
                            else if (updatePersoanDetailsResponse.ResponseCode == (int)DbReturnValue.OrderIDDocumentsMissing)
                            {
                                LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.OrderIDDocumentsMissing));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.OrderIDDocumentsMissing),
                                    IsDomainValidationErrors = false
                                });
                            }
                            else if (updatePersoanDetailsResponse.ResponseCode == (int)DbReturnValue.OrderNationalityMissing)
                            {
                                LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.OrderNationalityMissing));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.OrderNationalityMissing),
                                    IsDomainValidationErrors = false
                                });
                            }
                            else
                            {
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToUpdatedSubscriptionDetails));
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
                            // failed to locate customer
                              LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        [HttpGet("GetPendingOrderDetails")]
        public async Task<IActionResult> GetPendingOrderDetails([FromHeader(Name = "Grid-Authorization-Token")] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                      .SelectMany(x => x.Errors)
                                                      .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        //get ordered numbers
                        DatabaseResponse pendingOrderDetailsResponse = await _orderAccess.GetPendingOrderDetails(customerID);

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
                            LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NoRecords));
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

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// <param name="token" in="Header"></param>     
        /// <param name="orderId">Initial OrderID/ChangeRequestID in case of sim replacement/planchange/numberchange</param>
        /// <param name="orderType"> Initial Order = 1, ChangeRequest = 2, AccountInvoices = 3</param>
        /// <returns>OperationsResponse</returns>
        [HttpGet("GetCheckOutDetails/{orderId}/{orderType}")]
        public async Task<IActionResult> GetCheckOutDetails([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute]int orderId, [FromRoute]int orderType)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Orders_get_checkout_details);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;

                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                      .SelectMany(x => x.Errors)
                                                      .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);
                        DatabaseResponse customerResponse;
                        if (orderType == 1)
                        {
                            customerResponse = await _orderAccess.GetCustomerIdFromOrderId(orderId);
                        }
                        else if (orderType == 2)
                        {
                            customerResponse = await _orderAccess.GetCustomerIdFromChangeRequestId(orderId);
                        }
                        else
                        {
                            customerResponse = await _orderAccess.GetCustomerIdFromAccountInvoiceId(orderId);
                        }

                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
                        {
                            // Call MPGS to create a checkout session and retuen details

                            PaymentHelper gatewayHelper = new PaymentHelper();

                            Checkout checkoutDetails = new Checkout();

                            DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.MPGS.ToString());

                            GridMPGSConfig gatewayConfig = gatewayHelper.GetGridMPGSConfig((List<Dictionary<string, string>>)configResponse.Results);

                            customerBilling billingAddress = new customerBilling();

                            DatabaseResponse billingResponse = new DatabaseResponse();

                            CommonDataAccess commonAccess = new CommonDataAccess(_iconfiguration);

                            billingResponse = await commonAccess.GetCustomerBillingDetails(customerID);

                            if(billingResponse!=null)
                            {
                                billingAddress = (customerBilling)billingResponse.Results;
                            }

                           // checkoutDetails.OrderId = PaymentHelper.GenerateOrderId();

                            checkoutDetails.TransactionID = PaymentHelper.GenerateOrderId();

                            CheckOutRequestDBUpdateModel createcheckOutModel = new CheckOutRequestDBUpdateModel
                            {
                                Source = ((CheckOutType)orderType).ToString(),

                                SourceID = orderId,                              

                              //  MPGSOrderID = checkoutDetails.OrderId,

                                TransactionID = checkoutDetails.TransactionID
                            };

                            DatabaseResponse checkOutAmountResponse = await _orderAccess.GetCheckoutRequestDetails(createcheckOutModel);

                            if(checkOutAmountResponse.ResponseCode==(int)DbReturnValue.RecordExists)
                            {
                                checkoutDetails.OrderId = ((Checkout)checkOutAmountResponse.Results).OrderId;

                                checkoutDetails = gatewayHelper.CreateCheckoutSession(gatewayConfig, billingAddress, checkoutDetails.OrderId, checkoutDetails.TransactionID, ((Checkout)checkOutAmountResponse.Results).ReceiptNumber, ((Checkout)checkOutAmountResponse.Results).OrderNumber);

                                CheckOutRequestDBUpdateModel checkoutUpdateModel = new CheckOutRequestDBUpdateModel
                                {
                                    Source = ((CheckOutType)orderType).ToString(),

                                    SourceID = orderId,

                                    CheckOutSessionID = checkoutDetails.CheckoutSession.Id,

                                    CheckoutVersion = checkoutDetails.CheckoutSession.Version,

                                    SuccessIndicator = checkoutDetails.CheckoutSession.SuccessIndicator,

                                    MPGSOrderID = checkoutDetails.OrderId,

                                    TransactionID = checkoutDetails.TransactionID,
                                };

                                //Update checkout details and return amount

                                checkOutAmountResponse = await _orderAccess.GetCheckoutRequestDetails(checkoutUpdateModel);

                                if (checkOutAmountResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                                {
                                    checkoutDetails.Amount = ((Checkout)checkOutAmountResponse.Results).Amount;

                                    checkoutDetails.ReceiptNumber = ((Checkout)checkOutAmountResponse.Results).ReceiptNumber;

                                    checkoutDetails.OrderNumber= ((Checkout)checkOutAmountResponse.Results).OrderNumber;

                                    checkoutDetails.MerchantName = gatewayConfig.GridMerchantName;

                                    checkoutDetails.MerchantLogo = gatewayConfig.GridMerchantLogo;

                                    checkoutDetails.MerchantEmail = gatewayConfig.GridMerchantEmail;

                                    checkoutDetails.MerchantAddressLine1 = gatewayConfig.GridMerchantAddress1;

                                    checkoutDetails.MerchantAddressLine2 = gatewayConfig.GridMerchantAddress2;

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
                                    LogInfo.Warning(EnumExtensions.GetDescription(checkOutAmountResponse.ResponseCode));
                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = false,
                                        Message = EnumExtensions.GetDescription(checkOutAmountResponse.ResponseCode),
                                        IsDomainValidationErrors = false
                                    });
                                }
                            }
                            else if(checkOutAmountResponse.ResponseCode==(int)DbReturnValue.PaymentAlreadyProcessed)
                            {
                                // already processed order

                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.AlreadyProcessedOrder));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(CommonErrors.AlreadyProcessedOrder),
                                    IsDomainValidationErrors = false
                                });
                            }
                            else
                            {
                                LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NotExists));
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
                            // failed to locate customer
                              LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// This will remove the added additional lines from the order
        /// </summary>
        /// <param name="token"></param>
        /// <param name="request">
        /// body{
        /// "OrderID" :1,
        /// "MobileNumber" :"99999999"
        /// }
        /// </param>
        /// <returns>OperationResponse</returns>
        [Route("RemoveAdditionalLine")]
        [HttpPost]
        public async Task<IActionResult> RemoveAdditionalLine([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] RemoveAdditionalLineRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Orders_subscriber_remove);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                          .SelectMany(x => x.Errors)
                                                          .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(request.OrderID);

                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
                        {
                            //remove additional line

                            DatabaseResponse isPortedResponse = await _orderAccess.NumberIsPorted(request.OrderID, request.MobileNumber);

                            if (isPortedResponse != null && isPortedResponse.ResponseCode == (int)DbReturnValue.RecordExists && isPortedResponse.Results != null)
                            {
                                if (((int)isPortedResponse.Results) != 1)
                                {
                                    // unblock needed only if number not ported

                                    BSSAPIHelper bsshelper = new BSSAPIHelper();

                                    MiscHelper configHelper = new MiscHelper();

                                    DatabaseResponse requestIdToUpdateUnblock = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customerID, (int)BSSCalls.ExistingSession, request.MobileNumber);

                                    DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                                    GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                                    // Unblock
                                    BSSUpdateResponseObject bssUnblockUpdateResponse = new BSSUpdateResponseObject();

                                    try
                                    {
                                        bssUnblockUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateUnblock.Results, request.MobileNumber, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + " " + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = false,
                                            Message = EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed),
                                            IsDomainValidationErrors = false
                                        });

                                    }

                                    if (bsshelper.GetResponseCode(bssUnblockUpdateResponse) == "0")
                                    {
                                        DatabaseResponse removeLineResponse = await _orderAccess.RemoveAdditionalLine(request);

                                        if (removeLineResponse.ResponseCode == (int)DbReturnValue.DeleteSuccess)
                                        {

                                            return Ok(new OperationResponse
                                            {
                                                HasSucceeded = true,
                                                Message = EnumExtensions.GetDescription(CommonErrors.LineDeleteSuccess),
                                                IsDomainValidationErrors = false
                                            });

                                        }

                                        else if (removeLineResponse.ResponseCode == (int)DbReturnValue.ActiveTryDelete)
                                        {
                                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.LineDeleteFailed));

                                            return Ok(new OperationResponse
                                            {
                                                HasSucceeded = false,
                                                Message = EnumExtensions.GetDescription(DbReturnValue.ActiveTryDelete),
                                                IsDomainValidationErrors = false
                                            });
                                        }

                                        else if (removeLineResponse.ResponseCode == (int)DbReturnValue.PrimaryTryDelete)
                                        {
                                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.LineDeleteFailed));

                                            return Ok(new OperationResponse
                                            {
                                                HasSucceeded = false,
                                                Message = EnumExtensions.GetDescription(DbReturnValue.PrimaryTryDelete),
                                                IsDomainValidationErrors = false
                                            });
                                        }

                                        else if (removeLineResponse.ResponseCode == (int)DbReturnValue.CompletedOrderDelete)
                                        {
                                            LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.CompletedOrderDelete));

                                            return Ok(new OperationResponse
                                            {
                                                HasSucceeded = false,
                                                Message = EnumExtensions.GetDescription(DbReturnValue.CompletedOrderDelete),
                                                IsDomainValidationErrors = false
                                            });
                                        }
                                        else
                                        {
                                            LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NotExists));
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
                                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.UpdateAssetUnBlockingFailed));
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
                                    // ported number - just remove the line

                                    DatabaseResponse removeLineResponse = await _orderAccess.RemoveAdditionalLine(request);

                                    if (removeLineResponse.ResponseCode == (int)DbReturnValue.DeleteSuccess)
                                    {
                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = true,
                                            Message = EnumExtensions.GetDescription(CommonErrors.LineDeleteSuccess),
                                            IsDomainValidationErrors = false
                                        });

                                    }

                                    else if (removeLineResponse.ResponseCode == (int)DbReturnValue.ActiveTryDelete)
                                    {
                                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.LineDeleteFailed));

                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = false,
                                            Message = EnumExtensions.GetDescription(DbReturnValue.ActiveTryDelete),
                                            IsDomainValidationErrors = false
                                        });
                                    }

                                    else if (removeLineResponse.ResponseCode == (int)DbReturnValue.PrimaryTryDelete)
                                    {
                                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.LineDeleteFailed));

                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = false,
                                            Message = EnumExtensions.GetDescription(DbReturnValue.PrimaryTryDelete),
                                            IsDomainValidationErrors = false
                                        });
                                    }

                                    else if (removeLineResponse.ResponseCode == (int)DbReturnValue.CompletedOrderDelete)
                                    {
                                        LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.CompletedOrderDelete));

                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = false,
                                            Message = EnumExtensions.GetDescription(DbReturnValue.CompletedOrderDelete),
                                            IsDomainValidationErrors = false
                                        });
                                    }
                                    else
                                    {
                                        LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NotExists));
                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = false,
                                            Message = EnumExtensions.GetDescription(DbReturnValue.NotExists),
                                            IsDomainValidationErrors = false
                                        });
                                    }

                                }
                            }

                            else
                            {
                                // Old number not found in database

                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.OldNumberNotExists));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(CommonErrors.OldNumberNotExists),
                                    IsDomainValidationErrors = false
                                });
                            }
                         
                        }
                        else
                        {
                            // failed to locate customer
                              LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// This will remove the added additional lines from the order
        /// </summary>
        /// <param name="token"></param>
        /// <param name="request">
        /// body{
        /// "OrderID" :1,
        /// "OldMobileNumber" :"99999999"
        /// }
        /// </param>
        /// <returns>OperationResponse</returns>
        [Route("AssignNewNumberToSubscriber")]
        [HttpPost]
        public async Task<IActionResult> AssignNewNumberToSubscriber([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] AssignNewNumberRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(request.OrderID);
                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
                        {
                            // call GetAssets BSSAPI

                            BSSAPIHelper bsshelper = new BSSAPIHelper();

                            DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                            GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                            config.GridDefaultAssetLimit = 1; // to get only on asset

                            DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                            DatabaseResponse requestIdRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), customerID, (int)BSSCalls.NewSession, "");

                            ResponseObject res = await bsshelper.GetAssetInventory(config, (((List<ServiceFees>)serviceCAF.Results)).FirstOrDefault().ServiceCode, (BSSAssetRequest)requestIdRes.Results);

                            string NewNumber = bsshelper.GetAssetId(res);

                            if (res != null)
                            {
                                BSSNumbers numbers = new BSSNumbers();

                                numbers.FreeNumbers = bsshelper.GetFreeNumbers(res);

                                //insert this AssetToSubscribe into database

                                string json = bsshelper.GetJsonString(numbers.FreeNumbers); // json insert

                                DatabaseResponse updateBssCallFeeNumbers = await _orderAccess.UpdateBSSCallNumbers(json, ((BSSAssetRequest)requestIdRes.Results).userid, ((BSSAssetRequest)requestIdRes.Results).BSSCallLogID);
                            }


                            if (res != null && (int.Parse(res.Response.asset_details.total_record_count) > 0))
                            {
                                //Block number                                    

                                DatabaseResponse requestIdToUpdateRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customerID, (int)BSSCalls.ExistingSession, NewNumber);

                                BSSUpdateResponseObject bssUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateRes.Results, NewNumber, false);

                                if (bsshelper.GetResponseCode(bssUpdateResponse) == "0")
                                {
                                    // Assign Newnumber
                                    AssignNewNumber newNumbertoAssign = new AssignNewNumber { OrderID = request.OrderID, OldNumber = request.OldNumber, NewNumber = NewNumber };

                                    DatabaseResponse AssignNewNumberResponse = await _orderAccess.AssignNewNumber(newNumbertoAssign);

                                    if (AssignNewNumberResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                                    {
                                        //unblock oldnumber
                                        DatabaseResponse isPortedResponse = await _orderAccess.NumberIsPorted(request.OrderID, request.OldNumber);

                                        if (isPortedResponse != null && isPortedResponse.ResponseCode == (int)DbReturnValue.RecordExists && isPortedResponse.Results != null)
                                        {
                                            if (((int)isPortedResponse.Results) != 1)
                                            {
                                                DatabaseResponse requestIdToUpdateUnblock = await _orderAccess.GetBssApiRequestIdAndSubscriberSession(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customerID, request.OldNumber);

                                                // Unblock

                                                BSSUpdateResponseObject bssUnblockUpdateResponse = new BSSUpdateResponseObject();

                                                try
                                                {
                                                    bssUnblockUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateUnblock.Results, request.OldNumber, true);
                                                }
                                                catch (Exception ex)
                                                {
                                                    LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + " " + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                                                    return Ok(new OperationResponse
                                                    {
                                                        HasSucceeded = false,
                                                        Message = EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed),
                                                        IsDomainValidationErrors = false
                                                    });

                                                }
                                            }
                                        }

                                        if (bsshelper.GetResponseCode(bssUpdateResponse) == "0")
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
                                            DatabaseResponse orderDetailsResponse = await _orderAccess.GetOrderBasicDetails(request.OrderID);

                                            return Ok(new OperationResponse
                                            {
                                                HasSucceeded = true,
                                                Message = EnumExtensions.GetDescription(CommonErrors.AssignNuewNumberSuccess) + EnumExtensions.GetDescription(CommonErrors.UpdateAssetUnBlockingFailed),
                                                IsDomainValidationErrors = false,
                                                ReturnedObject = orderDetailsResponse.Results
                                            });
                                        }

                                    }
                                    else if (AssignNewNumberResponse.ResponseCode == (int)DbReturnValue.NotExists)
                                    {
                                        // Assign Newnumber failed
                                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.OldNumberNotExists));

                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = true,
                                            Message = EnumExtensions.GetDescription(CommonErrors.OldNumberNotExists),
                                            IsDomainValidationErrors = false

                                        });
                                    }

                                    else 
                                    {
                                        // Assign Newnumber failed
                                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.AssignNewNumberFailed));

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

                                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.UpdateAssetBlockingFailed));
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

                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.GetAssetFailed));

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
                            // failed to locate customer
                              LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// This will returns Customer ID Image as well as details of the ID
        /// </summary>
        /// <param name="token" in="Header"></param>
        /// <param name="OrderID"></param>
        /// <returns>OperationResponse</returns>
        [Route("GetCustomerIDImages/{OrderID}")]
        [HttpGet]
        public async Task<IActionResult> GetCustomerIDImages([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] int OrderID)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        //first get order NRIC details order documents  

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);
                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(OrderID);
                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
                        {
                            DatabaseResponse nRICresponse = await _orderAccess.GetOrderNRICDetails(OrderID);

                            if ((nRICresponse.ResponseCode == (int)DbReturnValue.RecordExists) && ((OrderNRICDetails)nRICresponse.Results).DocumentID > 0)
                            {
                                //get image bytes from s3

                                // DownloadFile

                                DatabaseResponse awsConfigResponse = await _orderAccess.GetConfiguration(ConfiType.AWS.ToString());

                                if (awsConfigResponse != null && awsConfigResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                                {
                                    MiscHelper configHelper = new MiscHelper();

                                    GridAWSS3Config awsConfig = configHelper.GetGridAwsConfig((List<Dictionary<string, string>>)awsConfigResponse.Results);

                                    AmazonS3 s3Helper = new AmazonS3(awsConfig);


                                    DownloadResponse FrontImageDownloadResponse = new DownloadResponse();

                                    DownloadResponse BackImageDownloadResponse = new DownloadResponse();

                                    if (!string.IsNullOrEmpty(((OrderNRICDetails)nRICresponse.Results).DocumentURL))
                                    {
                                        FrontImageDownloadResponse = await s3Helper.DownloadFile(((OrderNRICDetails)nRICresponse.Results).DocumentURL.Remove(0, awsConfig.AWSEndPoint.Length));
                                    }

                                    if (!string.IsNullOrEmpty(((OrderNRICDetails)nRICresponse.Results).DocumentBackURL))
                                    {
                                        BackImageDownloadResponse =  await s3Helper.DownloadFile(((OrderNRICDetails)nRICresponse.Results).DocumentBackURL.Remove(0, awsConfig.AWSEndPoint.Length));
                                    }
                                   
                                    DownloadNRIC nRICDownloadObject = new DownloadNRIC { OrderID = OrderID, FrontImage = FrontImageDownloadResponse.FileObject != null ? configHelper.GetBase64StringFromByteArray(FrontImageDownloadResponse.FileObject, ((OrderNRICDetails)nRICresponse.Results).DocumentURL.Remove(0, awsConfig.AWSEndPoint.Length)) : null, BackImage = BackImageDownloadResponse.FileObject != null ? configHelper.GetBase64StringFromByteArray(BackImageDownloadResponse.FileObject, ((OrderNRICDetails)nRICresponse.Results).DocumentBackURL.Remove(0, awsConfig.AWSEndPoint.Length)) : null, IdentityCardNumber = ((OrderNRICDetails)nRICresponse.Results).IdentityCardNumber, IdentityCardType = ((OrderNRICDetails)nRICresponse.Results).IdentityCardType, Nationality = ((OrderNRICDetails)nRICresponse.Results).Nationality };

                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = true,
                                        Message = EnumExtensions.GetDescription(DbReturnValue.RecordExists),
                                        ReturnedObject = nRICDownloadObject

                                    });
                                }
                                else
                                {
                                    // unable to get aws config
                                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToGetConfiguration));

                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = false,
                                        Message = EnumExtensions.GetDescription(CommonErrors.FailedToGetConfiguration)

                                    });
                                }

                            }
                            else
                            {
                                // NRIC details not exists
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = true,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.NotExists),
                                    IsDomainValidationErrors = false
                                });
                            }
                        }
                        else
                        {
                            // failed to locate customer
                              LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// This will update personal ID details of the customer for the order
        /// </summary>
        /// <param name="token"></param>
        /// <param name="request">
        /// Form{
        /// "OrderID" :1,
        /// "Nationality": "Singaporean"
        /// "IDType" :"PAN",
        /// "IDNumber":"P23FD",
        /// "IDImageFront" : FileInput,
        /// "IDImageBack" : FileInput        
        /// }
        /// </param>
        /// <returns>OperationResponse</returns>
        [Route("updateorderpersonalIDdetails")]
        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> UpdateOrderPersonalIDDetails([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromForm] UpdateOrderPersonalIDDetailsRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Orders_personaldetails_update);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;

                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                          .SelectMany(x => x.Errors)
                                                          .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(request.OrderID);

                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
                        {
                            string nricwarning = "";
                            EmailValidationHelper _helper = new EmailValidationHelper();
                            if(!_helper.NRICValidation((request.IDType == "NRIC" ? "S" : "F"), request.IDNumber, out nricwarning))
                            {
                                LogInfo.Error(nricwarning);
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.InvalidNRIC),
                                    IsDomainValidationErrors = false
                                });
                            }
                            IFormFile frontImage = request.IDImageFront;

                            IFormFile backImage = request.IDImageBack;

                            BSSAPIHelper bsshelper = new BSSAPIHelper();

                            MiscHelper configHelper = new MiscHelper();

                            UpdateOrderPersonalDetails personalDetails = new UpdateOrderPersonalDetails
                            {
                                OrderID = request.OrderID,
                                Nationality = request.Nationality,
                                IDNumber = request.IDNumber,
                                IDType = request.IDType
                            };

                            //process file if uploaded - non null

                            if (frontImage != null && backImage != null)
                            {
                                DatabaseResponse awsConfigResponse = await _orderAccess.GetConfiguration(ConfiType.AWS.ToString());

                                if (awsConfigResponse != null && awsConfigResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                                {
                                    GridAWSS3Config awsConfig = configHelper.GetGridAwsConfig((List<Dictionary<string, string>>)awsConfigResponse.Results);

                                    AmazonS3 s3Helper = new AmazonS3(awsConfig);

                                    string fileNameFront = request.IDNumber.Substring(1, request.IDNumber.Length - 2) + "_Front_" + DateTime.Now.ToString("yyMMddhhmmss") + Path.GetExtension(frontImage.FileName); //Grid_IDNUMBER_yyyymmddhhmmss.extension

                                    UploadResponse s3UploadResponse = await s3Helper.UploadFile(frontImage, fileNameFront);

                                    if (s3UploadResponse.HasSucceed)
                                    {
                                        personalDetails.IDFrontImageUrl = awsConfig.AWSEndPoint + s3UploadResponse.FileName;
                                    }
                                    else
                                    {
                                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.S3UploadFailed));
                                    }

                                    string fileNameBack = request.IDNumber.Substring(1, request.IDNumber.Length - 2) + "_Back_" + DateTime.Now.ToString("yyMMddhhmmss") + Path.GetExtension(frontImage.FileName); //Grid_IDNUMBER_yyyymmddhhmmss.extension

                                    s3UploadResponse = await s3Helper.UploadFile(backImage, fileNameBack);

                                    if (s3UploadResponse.HasSucceed)
                                    {
                                        personalDetails.IDBackImageUrl = awsConfig.AWSEndPoint + s3UploadResponse.FileName;
                                    }
                                    else
                                    {
                                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.S3UploadFailed));
                                    }
                                }
                                else
                                {
                                    // unable to get aws config
                                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToGetConfiguration));

                                }
                            }    //file     



                            //update personal details
                            DatabaseResponse updatePersoanDetailsResponse = await _orderAccess.UpdateOrderPersonalIdDetails(personalDetails);

                            if (updatePersoanDetailsResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
                            {
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = true,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.UpdateSuccess),
                                    IsDomainValidationErrors = false
                                });
                            }
                            else if (updatePersoanDetailsResponse.ResponseCode == (int)DbReturnValue.DuplicateNRICNotAllowed)
                            {
                                LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.DuplicateNRICNotAllowed) + "for " + request.OrderID + "Order");
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.DuplicateNRICNotAllowed),
                                    IsDomainValidationErrors = false
                                });
                            }
                            else
                            {
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedUpdatePersonalDetails));
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
                            // failed to locate customer
                              LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// <returns>OperationsResponse</returns>
      

        /// <summary>
        /// This will update Order subscription details
        /// </summary>
        /// <param name="token"></param>
        /// <param name="request">
        /// body{
        /// "OrderID" :1,
        /// "BundleID" :1, 
        /// "DisplayName" : "test",      
        /// "MobileNumber":"98745632",             
        /// }
        /// </param>
        /// <returns>OperationResponse</returns>
        [Route("UpdateSubscriberBasicDetails")]
        [HttpPost]
        public async Task<IActionResult> UpdateSubscriberBasicDetails([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] UpdateSubscriberBasicDetails request)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Orders_subscriber_update);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(request.OrderID);
                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
                        {
                            //update shipping details
                            DatabaseResponse updatePersoanDetailsResponse = await _orderAccess.UpdateSubscriberDetails(request);

                            if (updatePersoanDetailsResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
                            {
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = true,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.UpdateSuccess),
                                    IsDomainValidationErrors = false
                                });
                            }
                            else if (updatePersoanDetailsResponse.ResponseCode == (int)DbReturnValue.UpdateNotAllowed)
                            {
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToUpdatedSubscriptionDetails));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.UpdateNotAllowed),
                                    IsDomainValidationErrors = false
                                });
                            }
                            else
                            {
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToUpdatedSubscriptionDetails));
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
                            // failed to locate customer
                              LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        [Route("GetRescheduleAvailableSlots")]
        public async Task<IActionResult> GetRescheduleAvailableSlots([FromHeader(Name = "Grid-Authorization-Token")] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse deliveryDetailsResponse = await _orderAccess.GetRescheduleAvailableSlots();

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
                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.DeliverySlotNotExists));
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

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// <param name="OrderID"></param>
        /// <returns>OperationResponse</returns>
        [HttpGet]
        [Route("RemoveLOADetails/{OrderID}")]
        public async Task<IActionResult> RemoveLOADetails([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] int OrderID)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;

                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse LOAResponse = await _orderAccess.RemoveLOADetails(OrderID);

                        if (LOAResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(CommonErrors.LOARemoved),
                                Result = LOAResponse.Results

                            });
                        }

                        else if(LOAResponse.ResponseCode == (int)DbReturnValue.UpdationFailed)
                        {
                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedUpdateLOADetails) + " token:" + token + ", orderID:"+ OrderID);

                            return Ok(new ServerResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.FailedToRemoveLoa)
                            });

                        }

                        else
                        { 
                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.DeliveryInfoNotExists) + " token:" + token + ", orderID:" + OrderID);

                            return Ok(new ServerResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.DeliveryInfoNotExists)
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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

        [HttpPost]
        [Route("CancelOrder/{orderId}")]
        public async Task<IActionResult> CancelOrder([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] int orderId)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                if (!ModelState.IsValid)
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage))
                    });
                }

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);
                var helper = new AuthHelper(_iconfiguration);
                var tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Orders_cancel_order);
                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;
                    DatabaseResponse statusResponse = new DatabaseResponse();
                    statusResponse.ResponseCode =
                        await _orderAccess.CancelOrder(aTokenResp.CustomerID, orderId);

                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        {
                            DatabaseResponse orderMqResponse = new DatabaseResponse();

                            orderMqResponse = await _messageQueueDataAccess.GetOrderMessageQueueBody(orderId);

                            OrderQM orderDetails = new OrderQM();

                            string topicName = string.Empty;

                            string pushResult = string.Empty;

                            if (orderMqResponse != null && orderMqResponse.Results != null)
                            {
                                orderDetails = (OrderQM)orderMqResponse.Results;

                                try
                                {
                                    Dictionary<string, string> attribute = new Dictionary<string, string>();

                                    topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration).Results.ToString().Trim();


                                    attribute.Add(EventTypeString.EventType, RequestType.CancelOrder.GetDescription());

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
                                            MessageAttribute = RequestType.CancelOrder.GetDescription(),
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
                                            MessageAttribute = RequestType.CancelOrder.GetDescription(),
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
                                        MessageAttribute = Core.Enums.RequestType.CancelOrder.GetDescription().ToString(),
                                        MessageBody = orderDetails != null ? JsonConvert.SerializeObject(orderDetails) : null,
                                        Status = 0,
                                        Remark = "Error Occured in Cancel Order ",
                                        Exception = new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical)


                                    };

                                    await _messageQueueDataAccess.InsertMessageInMessageQueueRequestException(queueRequest);
                                }
                            }

                        }

                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = statusResponse
                        });
                    }
                    else if (statusResponse.ResponseCode == (int)DbReturnValue.DuplicateCRExists)
                    {
                        LogInfo.Warning(DbReturnValue.DuplicateCRExists.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.DuplicateCRExists.GetDescription(),
                            IsDomainValidationErrors = false
                        });
                    }
                    else
                    {
                        LogInfo.Warning(DbReturnValue.NoRecords.GetDescription());

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
                    LogInfo.Warning(CommonErrors.ExpiredToken.GetDescription());
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
        /// Update authorization status after a payment checkout with new card details, generate token for the card, and capture the authorized amount
        /// </summary>
        /// <param name="token"></param>
        /// <param name="updateRequest"></param>
        /// <returns>OperationResponse</returns>
        [Route("UpdateTokenizeCheckOutResponse")]
        [HttpPost]
        public async Task<IActionResult> UpdateTokenizeCheckOutResponse([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] CheckOutResponseUpdate updateRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                          .SelectMany(x => x.Errors)
                                                          .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);
                        //update checkout details
                        DatabaseResponse updateCheckoutDetailsResponse = await _orderAccess.UpdateCheckOutResponse(updateRequest);
                        //Function to check if the payment amount is same as one from order
                        if (updateCheckoutDetailsResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
                        {
                            if ((TokenSession)updateCheckoutDetailsResponse.Results != null)
                            {
                                PaymentHelper gatewayHelper = new PaymentHelper();

                                DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.MPGS.ToString());

                                GridMPGSConfig gatewayConfig = gatewayHelper.GetGridMPGSConfig((List<Dictionary<string, string>>)configResponse.Results);

                                TokenResponse tokenizeResponse = new TokenResponse();

                                TokenSession tokenSession = new TokenSession();

                                tokenSession = (TokenSession)updateCheckoutDetailsResponse.Results;

                                tokenizeResponse = gatewayHelper.Tokenize(gatewayConfig, tokenSession);

                                if (tokenizeResponse != null && !string.IsNullOrEmpty(tokenizeResponse.Token))
                                {
                                   // insert token response to payment methods table

                                     DatabaseResponse tokenDetailsCreateResponse = new DatabaseResponse();

                                    LogInfo.Information(JsonConvert.SerializeObject(tokenizeResponse));

                                    tokenDetailsCreateResponse = await _orderAccess.CreatePaymentMethod(tokenizeResponse, customerID, updateRequest.MPGSOrderID, "UpdateTokenizeCheckOutResponse");

                                    if (tokenDetailsCreateResponse.ResponseCode == (int)DbReturnValue.CreateSuccess || tokenDetailsCreateResponse.ResponseCode == (int)DbReturnValue.ExistingCard)
                                    {
                                        tokenSession.SourceOfFundType = tokenizeResponse.Type;

                                        tokenSession.Token = tokenizeResponse.Token;

                                        string captureResponse = gatewayHelper.Capture(gatewayConfig, tokenSession);

                                        if (captureResponse == MPGSAPIResponse.SUCCESS.ToString())
                                        {
                                            LogInfo.Information(captureResponse);
                                            //  get the session details and transaction details

                                            TransactionRetrieveResponseOperation transactionResponse = new TransactionRetrieveResponseOperation();

                                            string receipt = gatewayHelper.RetrieveCheckOutTransaction(gatewayConfig, updateRequest);

                                            LogInfo.Information(receipt);

                                            transactionResponse = gatewayHelper.GetCapturedTransaction(receipt);

                                            LogInfo.Information(transactionResponse.TrasactionResponse.ApiResult + transactionResponse.TrasactionResponse.PaymentStatus + transactionResponse.TrasactionResponse.OrderId);

                                            DatabaseResponse tokenDetailsUpdateResponse = new DatabaseResponse();

                                            DatabaseResponse paymentProcessingRespose = new DatabaseResponse();

                                            transactionResponse.TrasactionResponse.Token = tokenSession.Token;
                                            LogInfo.Information("Processing the order payment: and calling UpdateCheckOutReceipt");
                                            paymentProcessingRespose = await _orderAccess.UpdateCheckOutReceipt(transactionResponse.TrasactionResponse);
                                            DatabaseResponse updatePaymentResponse = await _orderAccess.UpdatePaymentResponse(updateRequest.MPGSOrderID, receipt);

                                            tokenDetailsUpdateResponse = await _orderAccess.UpdatePaymentMethodDetails(transactionResponse.TrasactionResponse, customerID, tokenSession.Token);

                                            //Get Order Type
                                            var sourceTyeResponse = await _orderAccess.GetSourceTypeByMPGSSOrderId(updateRequest.MPGSOrderID);

                                            PaymentSuccessResponse paymentResponse = new PaymentSuccessResponse { Source = ((OrderSource)sourceTyeResponse.Results).SourceType, MPGSOrderID = updateRequest.MPGSOrderID, Amount = tokenSession.Amount, Currency = gatewayConfig.Currency };

                                            if (paymentProcessingRespose.ResponseCode == (int)DbReturnValue.TransactionSuccess)
                                            {
                                                LogInfo.Information(EnumExtensions.GetDescription(DbReturnValue.TransactionSuccess));

                                                QMHelper qMHelper = new QMHelper(_iconfiguration, _messageQueueDataAccess);

                                                int processResult = await qMHelper.ProcessSuccessTransaction(updateRequest);

                                                if (processResult == 1)
                                                {
                                                    LogInfo.Information(EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(CommonErrors.BuddyProcessed));

                                                    return Ok(new OperationResponse
                                                    {
                                                        HasSucceeded = true,
                                                        Message = EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(DbReturnValue.TransactionSuccess),
                                                        IsDomainValidationErrors = false,
                                                        ReturnedObject = paymentResponse
                                                    });
                                                }
                                                else if (processResult == 2)
                                                {
                                                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(CommonErrors.BuddyProcessingFailed));

                                                    return Ok(new OperationResponse
                                                    {
                                                        HasSucceeded = true,
                                                        Message = EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(DbReturnValue.TransactionSuccess),
                                                        IsDomainValidationErrors = false,
                                                        ReturnedObject = paymentResponse
                                                    });
                                                }
                                                else if (processResult == 3)
                                                {
                                                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(CommonErrors.MQSent));

                                                    return Ok(new OperationResponse
                                                    {
                                                        HasSucceeded = true,
                                                        Message = EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(DbReturnValue.TransactionSuccess),
                                                        IsDomainValidationErrors = false,
                                                        ReturnedObject = paymentResponse
                                                    });
                                                }

                                                else if (processResult == 4)
                                                {
                                                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". But while processing Buddy/MQ/EML/SMS " + EnumExtensions.GetDescription(CommonErrors.SourceTypeNotFound) + " for MPGSOrderID" + updateRequest.MPGSOrderID);
                                                    return Ok(new OperationResponse
                                                    {
                                                        HasSucceeded = true,
                                                        Message = EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(DbReturnValue.TransactionSuccess),
                                                        IsDomainValidationErrors = false,
                                                        ReturnedObject = paymentResponse
                                                    });
                                                }

                                                else if (processResult == 5)
                                                {
                                                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". But while processing Buddy/MQ/EML/SMS " + EnumExtensions.GetDescription(CommonErrors.InvalidCheckoutType) + " for MPGSOrderID" + updateRequest.MPGSOrderID);
                                                    return Ok(new OperationResponse
                                                    {
                                                        HasSucceeded = true,
                                                        Message = EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(DbReturnValue.TransactionSuccess),
                                                        IsDomainValidationErrors = false,
                                                        ReturnedObject = paymentResponse
                                                    });
                                                }

                                                else
                                                {
                                                    // entry for exceptions from QM Helper, but need to send payment success message to UI as payment already processed
                                                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". But while processing Buddy/MQ/EML/SMS " + EnumExtensions.GetDescription(CommonErrors.SystemExceptionAfterPayment) + " for MPGSOrderID" + updateRequest.MPGSOrderID);
                                                    return Ok(new OperationResponse
                                                    {
                                                        HasSucceeded = true,
                                                        Message = EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(DbReturnValue.TransactionSuccess),
                                                        IsDomainValidationErrors = false,
                                                        ReturnedObject = paymentResponse
                                                    });

                                                }
                                            }
                                            else if (paymentProcessingRespose.ResponseCode == (int)DbReturnValue.PaymentAlreadyProcessed)
                                            {
                                                return Ok(new OperationResponse
                                                {
                                                    HasSucceeded = true,
                                                    Message = EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(DbReturnValue.TransactionSuccess),
                                                    IsDomainValidationErrors = false,
                                                    ReturnedObject = paymentResponse
                                                });
                                            }
                                            else
                                            {
                                                LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TransactionFailed));
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
                                            return Ok(new OperationResponse
                                            {
                                                HasSucceeded = false,
                                                Message = EnumExtensions.GetDescription(CommonErrors.CaptureFailed),
                                                IsDomainValidationErrors = false
                                            });

                                        }
                                    }

                                    else
                                    {
                                        // token details update failed

                                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToCreatePaymentMethod));
                                        LogInfo.Warning("Create payment method failed - " + JsonConvert.SerializeObject(tokenDetailsCreateResponse));
                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = false,
                                            Message = EnumExtensions.GetDescription(CommonErrors.FailedToCreatePaymentMethod),
                                            IsDomainValidationErrors = false
                                        });
                                    }
                                }

                                else
                                {
                                    //failed to create payment token

                                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenGenerationFailed) + " for customer:" + customerID);
                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = false,
                                        Message = EnumExtensions.GetDescription(CommonErrors.TokenGenerationFailed),
                                        IsDomainValidationErrors = false
                                    });
                                }

                            }

                            else
                            {
                                //unable to get token session

                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.UnableToGetTokenSession));

                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(CommonErrors.UnableToGetTokenSession),
                                    IsDomainValidationErrors = false
                                });
                            }

                        }
                        else
                        {
                            // checkout response update failed

                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.CheckOutDetailsUpdationFailed));

                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.CheckOutDetailsUpdationFailed),
                                IsDomainValidationErrors = false
                            });
                        }

                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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

        [NonAction]
        public async Task<IActionResult> RemovePaymentMethod([FromHeader(Name = "Grid-Authorization-Token")] string token, int paymentMethodId)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                          .SelectMany(x => x.Errors)
                                                          .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);
                        //Get token from paymentmethodID
                        DatabaseResponse paymentMethodResponse = await _orderAccess.GetPaymentMethodTokenById(customerID, paymentMethodId);

                        if (paymentMethodResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                        {
                            PaymentHelper gatewayHelper = new PaymentHelper();

                            DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.MPGS.ToString());

                            GridMPGSConfig gatewayConfig = gatewayHelper.GetGridMPGSConfig((List<Dictionary<string, string>>)configResponse.Results);

                            string response = gatewayHelper.RemoveToken(gatewayConfig, ((PaymentMethod)paymentMethodResponse.Results).Token);

                            if (response == MPGSAPIResponse.SUCCESS.ToString())
                            {
                                DatabaseResponse databaseResponse = await _orderAccess.RemovePaymentMethod(customerID, paymentMethodId);

                                if (databaseResponse.ResponseCode == (int)DbReturnValue.DeleteSuccess)
                                {

                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = true,
                                        Message = EnumExtensions.GetDescription(CommonErrors.PaymentMethodSuccessfullyRemoved),
                                        IsDomainValidationErrors = false
                                    });
                                }

                                else
                                {
                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = true,
                                        Message = EnumExtensions.GetDescription(CommonErrors.PaymentMethodSuccessfullyRemoved) + ". " + EnumExtensions.GetDescription(CommonErrors.FailedToRemovePaymentMethodDb),
                                        IsDomainValidationErrors = false
                                    });
                                }
                            }

                            else
                            {
                                // failed to remove payment details from gateway
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToRemovePaymentMethod));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(CommonErrors.FailedToRemovePaymentMethod),
                                    IsDomainValidationErrors = false
                                });
                            }


                        }
                        else
                        {
                            // payment method does not exists

                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotExists));
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

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// To pay the chekout amount with the token against the default payment method of the customer
        /// </summary>
        /// <param name="token"></param>
        /// <param name="orderId"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        [HttpGet("PaywithToken/{orderId}/{orderType}")]
        public async Task<IActionResult> PaywithToken([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute]int orderId, [FromRoute]int orderType)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;

                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                      .SelectMany(x => x.Errors)
                                                      .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse customerResponse;

                        if (orderType == 1)
                        {
                            customerResponse = await _orderAccess.GetCustomerIdFromOrderId(orderId);
                        }
                        else if (orderType == 2)
                        {
                            customerResponse = await _orderAccess.GetCustomerIdFromChangeRequestId(orderId);
                        }
                        else
                        {
                            customerResponse = await _orderAccess.GetCustomerIdFromAccountInvoiceId(orderId);
                        }

                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
                        {
                            DatabaseResponse paymentMethodResponse = await _orderAccess.GetPaymentMethodToken(customerID);
                            //Get token from paymentmethodID
                            PaymentMethod paymentMethod = new PaymentMethod();

                            paymentMethod = (PaymentMethod)paymentMethodResponse.Results;

                            if (paymentMethod != null && (!string.IsNullOrEmpty(paymentMethod.Token)))
                            {
                                PaymentHelper gatewayHelper = new PaymentHelper();

                                Checkout checkoutDetails = new Checkout();

                                DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.MPGS.ToString());

                                GridMPGSConfig gatewayConfig = gatewayHelper.GetGridMPGSConfig((List<Dictionary<string, string>>)configResponse.Results);

                                customerBilling billingAddress = new customerBilling();

                                DatabaseResponse billingResponse = new DatabaseResponse();

                                CommonDataAccess commonAccess = new CommonDataAccess(_iconfiguration);

                                billingResponse = await commonAccess.GetCustomerBillingDetails(customerID);

                                if (billingResponse != null)
                                {
                                    billingAddress = (customerBilling)billingResponse.Results;
                                }

                               // checkoutDetails.OrderId = PaymentHelper.GenerateOrderId();

                                checkoutDetails.TransactionID = PaymentHelper.GenerateOrderId();

                                CheckOutRequestDBUpdateModel createcheckOutModel = new CheckOutRequestDBUpdateModel
                                {
                                    Source = ((CheckOutType)orderType).ToString(),

                                    SourceID = orderId,                                  

                                   // MPGSOrderID = checkoutDetails.OrderId,

                                    TransactionID = checkoutDetails.TransactionID
                                };                               

                                DatabaseResponse checkOutAmountResponse = await _orderAccess.GetCheckoutRequestDetails(createcheckOutModel);

                                if (checkOutAmountResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                                {
                                    checkoutDetails.OrderId = ((Checkout)checkOutAmountResponse.Results).OrderId;

                                    checkoutDetails = gatewayHelper.CreateCheckoutSession(gatewayConfig, billingAddress, checkoutDetails.OrderId, checkoutDetails.TransactionID, ((Checkout)checkOutAmountResponse.Results).ReceiptNumber, ((Checkout)checkOutAmountResponse.Results).OrderNumber);

                                    // Call MPGS to create a checkout session and retuen details
                                    CheckOutRequestDBUpdateModel checkoutUpdateModel = new CheckOutRequestDBUpdateModel
                                    {
                                        Source = ((CheckOutType)orderType).ToString(),

                                        SourceID = orderId,

                                        CheckOutSessionID = checkoutDetails.CheckoutSession.Id,

                                        CheckoutVersion = checkoutDetails.CheckoutSession.Version,

                                        SuccessIndicator = checkoutDetails.CheckoutSession.SuccessIndicator,

                                        MPGSOrderID = checkoutDetails.OrderId,

                                        TransactionID = checkoutDetails.TransactionID
                                    };

                                    //Update checkout details and return amount

                                    checkOutAmountResponse = await _orderAccess.GetCheckoutRequestDetails(checkoutUpdateModel);

                                    if (checkOutAmountResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                                    {
                                        checkoutDetails.Amount = ((Checkout)checkOutAmountResponse.Results).Amount;

                                        checkoutDetails.ReceiptNumber = ((Checkout)checkOutAmountResponse.Results).ReceiptNumber;

                                        checkoutDetails.OrderNumber= ((Checkout)checkOutAmountResponse.Results).OrderNumber;

                                        string authorizeResponse = gatewayHelper.Authorize(gatewayConfig, checkoutDetails, paymentMethod);

                                        if (authorizeResponse == MPGSAPIResponse.SUCCESS.ToString())
                                        {
                                            string captureResponse = gatewayHelper.Capture(gatewayConfig, new TokenSession { Amount = checkoutDetails.Amount, MPGSOrderID = checkoutDetails.OrderId, Token = paymentMethod.Token, SourceOfFundType = paymentMethod.SourceType });

                                            if (captureResponse == MPGSAPIResponse.SUCCESS.ToString())
                                            {
                                                TransactionRetrieveResponseOperation transactionResponse = new TransactionRetrieveResponseOperation();

                                                CheckOutResponseUpdate updateRequest = new CheckOutResponseUpdate { MPGSOrderID = checkoutDetails.OrderId, Result = captureResponse };
                                                    
                                                string receipt = gatewayHelper.RetrieveCheckOutTransaction(gatewayConfig, updateRequest);

                                                transactionResponse = gatewayHelper.GetCapturedTransaction(receipt);                                                

                                                transactionResponse.TrasactionResponse.CardType = paymentMethod.CardType;

                                                transactionResponse.TrasactionResponse.CardHolderName = paymentMethod.CardHolderName;

                                                transactionResponse.TrasactionResponse.Token = paymentMethod.Token;

                                                DatabaseResponse paymentProcessingRespose = new DatabaseResponse();

                                                paymentProcessingRespose = await _orderAccess.UpdateCheckOutReceipt(transactionResponse.TrasactionResponse);

                                                DatabaseResponse updatePaymentResponse = await _orderAccess.UpdatePaymentResponse(updateRequest.MPGSOrderID, receipt);

                                                if (paymentProcessingRespose.ResponseCode == (int)DbReturnValue.TransactionSuccess)
                                                {
                                                    LogInfo.Information(EnumExtensions.GetDescription(DbReturnValue.TransactionSuccess));

                                                    //Get Order Type
                                                    var sourceTyeResponse = await _orderAccess.GetSourceTypeByMPGSSOrderId(updateRequest.MPGSOrderID);                                                  

                                                    QMHelper qMHelper = new QMHelper(_iconfiguration, _messageQueueDataAccess);

                                                    PaymentSuccessResponse paymentResponse = new PaymentSuccessResponse { Source = ((CheckOutType)orderType).ToString(), MPGSOrderID = updateRequest.MPGSOrderID, Amount = checkoutDetails.Amount, Currency = gatewayConfig.Currency };

                                                    int processResult = await qMHelper.ProcessSuccessTransaction(updateRequest);

                                                    if (processResult==1)
                                                    {   
                                                        LogInfo.Information(EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(CommonErrors.BuddyProcessed));

                                                        return Ok(new OperationResponse
                                                        {
                                                            HasSucceeded = true,
                                                            Message = EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(DbReturnValue.TransactionSuccess),
                                                            IsDomainValidationErrors = false,
                                                            ReturnedObject = paymentResponse
                                                        });
                                                    }
                                                    else if (processResult == 2)
                                                    {
                                                        LogInfo.Information(EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(CommonErrors.BuddyProcessingFailed));

                                                        return Ok(new OperationResponse
                                                        {
                                                            HasSucceeded = true,
                                                            Message = EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(DbReturnValue.TransactionSuccess),
                                                            IsDomainValidationErrors = false,
                                                            ReturnedObject = paymentResponse
                                                        });
                                                    }
                                                    else if (processResult == 3)
                                                    {
                                                        LogInfo.Information(EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(CommonErrors.MQSent));

                                                        return Ok(new OperationResponse
                                                        {
                                                            HasSucceeded = true,
                                                            Message = EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(DbReturnValue.TransactionSuccess),
                                                            IsDomainValidationErrors = false,
                                                            ReturnedObject = paymentResponse
                                                        });
                                                    }

                                                    else if (processResult == 4)
                                                    {
                                                        LogInfo.Information(EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". But while processing Buddy/MQ/EML/SMS " + EnumExtensions.GetDescription(CommonErrors.SourceTypeNotFound) + " for MPGSOrderID" + updateRequest.MPGSOrderID);
                                                        return Ok(new OperationResponse
                                                        {
                                                            HasSucceeded = true,
                                                            Message = EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(DbReturnValue.TransactionSuccess),
                                                            IsDomainValidationErrors = false,
                                                            ReturnedObject = paymentResponse
                                                        });
                                                    }

                                                    else if (processResult == 5)
                                                    {
                                                        LogInfo.Information(EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". But while processing Buddy/MQ/EML/SMS " + EnumExtensions.GetDescription(CommonErrors.InvalidCheckoutType) + " for MPGSOrderID" + updateRequest.MPGSOrderID);
                                                        return Ok(new OperationResponse
                                                        {
                                                            HasSucceeded = true,
                                                            Message = EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(DbReturnValue.TransactionSuccess),
                                                            IsDomainValidationErrors = false,
                                                            ReturnedObject = paymentResponse
                                                        });
                                                    }

                                                    else
                                                    {
                                                        // entry for exceptions from QM Helper, but need to send payment success message to UI as payment already processed
                                                        LogInfo.Information(EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". But while processing Buddy/MQ/EML/SMS " + EnumExtensions.GetDescription(CommonErrors.SystemExceptionAfterPayment) + " for MPGSOrderID" + updateRequest.MPGSOrderID);
                                                        return Ok(new OperationResponse
                                                        {
                                                            HasSucceeded = true,
                                                            Message = EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(DbReturnValue.TransactionSuccess),
                                                            IsDomainValidationErrors = false,
                                                            ReturnedObject = paymentResponse
                                                        });

                                                    }                                                   
                                                }
                                                else
                                                {
                                                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.PaymentProcessed) + ". " + EnumExtensions.GetDescription(DbReturnValue.TransactionFailed));
                                                    return Ok(new OperationResponse
                                                    {
                                                        HasSucceeded = false,
                                                        Message = EnumExtensions.GetDescription(DbReturnValue.TransactionFailed),
                                                        IsDomainValidationErrors = false,                                                        
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                return Ok(new OperationResponse
                                                {
                                                    HasSucceeded = false,
                                                    Message = EnumExtensions.GetDescription(CommonErrors.CaptureFailed),
                                                    IsDomainValidationErrors = false
                                                });

                                            }
                                        }
                                        else
                                        {
                                            //authorize failed

                                            return Ok(new OperationResponse
                                            {
                                                HasSucceeded = false,
                                                Message = EnumExtensions.GetDescription(CommonErrors.AuthorizeFailed),
                                                IsDomainValidationErrors = false
                                            });
                                        }

                                    }
                                    else
                                    {
                                        LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NoRecords));
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
                                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NoRecords));
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
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.PaymentMethodNotExists));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(CommonErrors.PaymentMethodNotExists),
                                    IsDomainValidationErrors = false
                                });
                            }

                        }
                        else
                        {
                            // failed to locate customer
                              LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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

        [HttpPost]
        [Route("RescheduleDelivery")]
        public async Task<IActionResult> RescheduleDelivery([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] OrderRescheduleDeliveryRequest detailsrequest)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                if (!ModelState.IsValid)
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage))
                    });
                }
               
                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                var helper = new AuthHelper(_iconfiguration);

                var tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Orders_RescheduleDelivery);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;
                 
                    var statusResponse =
                        await _orderAccess.RescheduleDelivery(aTokenResp.CustomerID, detailsrequest);

                    var order_Reschedule = (Order_RescheduleDeliveryResponse)statusResponse.Results;

                    if (order_Reschedule != null && order_Reschedule.PayableAmount == 0)
                    {
                        var confirmOrder = await _orderAccess.ProcessRescheduleDelivery(order_Reschedule.AccountInvoiceID);

                        var chargesExists = await _orderAccess.CheckRescheduleDeliveryCharges(order_Reschedule.AccountInvoiceID);
                        //var orderDetailsForSMS = await _orderAccess.GetOrderDetails(detailsrequest.OrderID);
                        //var orderObjectForSMS = (Order)orderDetailsForSMS.Results;
                        if (confirmOrder.ResponseCode == (int)DbReturnValue.CreateSuccess)
                        {
                            //Send SMS
                            
                            string orderNumberForSMS = string.Empty, Name = string.Empty, Email = string.Empty, MobileNumber = string.Empty;
                            DateTime ? SlotDateForSMS;
                            TimeSpan ? SlotFromTime, SlotToTime;
                            
                            if (detailsrequest.OrderType == (int)CheckOutType.Orders)
                            {
                                DatabaseResponse orderMqResponseForSMS = new DatabaseResponse();
                                orderMqResponseForSMS = await _messageQueueDataAccess.GetOrderMessageQueueBody(detailsrequest.OrderID);
                                var orderObject = (OrderQM)orderMqResponseForSMS.Results;
                                orderNumberForSMS = orderObject.orderNumber;
                                SlotDateForSMS = orderObject.slotDate;
                                SlotFromTime = orderObject.slotFromTime;
                                    SlotToTime = orderObject.slotToTime;
                                Name = orderObject.name;
                                Email = orderObject.email;
                                MobileNumber = orderObject.shippingContactNumber;
                            }
                            else
                            {
                                var messageForSMS = await _messageQueueDataAccess.GetMessageBodyByChangeRequest(detailsrequest.OrderID);
                                //var orderObject = (OrderQM)orderMqResponseForSMS.Results;
                                orderNumberForSMS = messageForSMS.OrderNumber;
                                SlotDateForSMS = messageForSMS.SlotDate;
                                SlotFromTime = messageForSMS.SlotFromTime;
                                SlotToTime = messageForSMS.SlotToTime;
                                Name = messageForSMS.Name;
                                Email = messageForSMS.Email;
                                MobileNumber = messageForSMS.MobileNumber;
                            }
                            ConfigDataAccess _configAccess = new ConfigDataAccess(_iconfiguration);

                            DatabaseResponse smsTemplateResponse = await _configAccess.GetSMSNotificationTemplate(NotificationEvent.RescheduleDelivery.ToString());

                            var notificationMessage = MessageHelper.GetSMSMessage(NotificationEvent.RescheduleDelivery.ToString(),
                                ((SMSTemplates)smsTemplateResponse.Results).TemplateName, Name,Email,MobileNumber, orderNumberForSMS,
                                 SlotDateForSMS != null ? SlotDateForSMS.Value.ToString("dd MMM yyyy") : null,
                                SlotFromTime != null && SlotToTime != null ? new DateTime(SlotFromTime.Value.Ticks).ToString("hh mm tt") +
                                " to " + new DateTime(SlotToTime.Value.Ticks).ToString("hh mm tt") : null);

                            DatabaseResponse notificationResponse = await _configAccess.GetConfiguration(ConfiType.Notification.ToString());

                            MiscHelper parser = new MiscHelper();

                            var notificationConfig = parser.GetNotificationConfig((List<Dictionary<string, string>>)notificationResponse.Results);

                            Publisher orderSuccessSMSNotificationPublisher = new Publisher(_iconfiguration, notificationConfig.SNSTopic);

                            var status = await orderSuccessSMSNotificationPublisher.PublishAsync(notificationMessage);

                            LogInfo.Information("SMS send status : " + status + " " + JsonConvert.SerializeObject(notificationMessage));
                            //End
                            if (chargesExists.ResponseCode == (int)DbReturnValue.RecordExists)                            
                            {
                                //Start - Send MQ if Successfully Reschedule delivery Order processed with payment.
                                DatabaseResponse orderMqResponse = new DatabaseResponse();
                                orderMqResponse = await _messageQueueDataAccess.GetRescheduleMessageQueueBody(order_Reschedule.AccountInvoiceID);

                                QMHelper qMHelper = new QMHelper(_iconfiguration, _messageQueueDataAccess);
                                var result = await qMHelper.SendMQ(orderMqResponse);

                            }
                            //End
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = statusResponse
                            });
                        }
                        else
                        {
                            LogInfo.Warning(DbReturnValue.NoRecords.GetDescription());

                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = DbReturnValue.UpdationFailed.GetDescription(),
                                IsDomainValidationErrors = false
                            });
                        }
                    }
                    else if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                       
                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = statusResponse
                        });
                    }
                    else if (statusResponse.ResponseCode == (int)DbReturnValue.DuplicateCRExists)
                    {
                        LogInfo.Warning(DbReturnValue.DuplicateCRExists.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.DuplicateCRExists.GetDescription(),
                            IsDomainValidationErrors = false
                        });
                    }
                    else if (statusResponse.ResponseCode == (int)DbReturnValue.RescheduleOrderStatusNotCorrect)
                    {
                        LogInfo.Warning(DbReturnValue.RescheduleOrderStatusNotCorrect.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.RescheduleOrderStatusNotCorrect.GetDescription(),
                            IsDomainValidationErrors = false
                        });
                    }
                    else if (statusResponse.ResponseCode == (int)DbReturnValue.DeliverySlotUnavailability)
                    {
                        LogInfo.Warning(DbReturnValue.DeliverySlotUnavailability.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.DeliverySlotUnavailability.GetDescription(),
                            IsDomainValidationErrors = false
                        });
                    }
                    else
                    {
                        LogInfo.Warning(DbReturnValue.NoRecords.GetDescription());

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
                    LogInfo.Warning(CommonErrors.ExpiredToken.GetDescription());
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
        /// Create a payment gateway session for the customer to change existing card details
        /// </summary>
        /// <param name="token"></param>
        /// <param name="customerID"></param>
        /// <returns></returns>
        [HttpGet("GetChangePaymentMethodSession/{customerID}")]
        public async Task<IActionResult> GetChangePaymentMethodSession([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] int customerID)
        {
            try
            {

                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int tokencustomerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;

                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                      .SelectMany(x => x.Errors)
                                                      .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        if (tokencustomerID == customerID)
                        {
                            // Call MPGS to create a checkout session and retuen details

                            PaymentHelper gatewayHelper = new PaymentHelper();

                            Checkout checkoutDetails = new Checkout();

                            DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.MPGS.ToString());

                            GridMPGSConfig gatewayConfig = gatewayHelper.GetGridMPGSConfig((List<Dictionary<string, string>>)configResponse.Results);

                            customerBilling billingAddress = new customerBilling();

                            DatabaseResponse billingResponse = new DatabaseResponse();

                            CommonDataAccess commonAccess = new CommonDataAccess(_iconfiguration);

                            billingResponse = await commonAccess.GetCustomerBillingDetails(customerID);

                            if (billingResponse != null)
                            {
                                billingAddress = (customerBilling)billingResponse.Results;
                            }

                           // checkoutDetails.OrderId = PaymentHelper.GenerateOrderId();

                            checkoutDetails.TransactionID = PaymentHelper.GenerateOrderId();

                            CheckOutRequestDBUpdateModel createcheckOutModel = new CheckOutRequestDBUpdateModel
                            {
                                Source = CheckOutType.ChangeCard.ToString(),

                                SourceID = 0,

                                //  MPGSOrderID = checkoutDetails.OrderId,

                                TransactionID = checkoutDetails.TransactionID
                            };

                            DatabaseResponse checkOutAmountResponse = await _orderAccess.GetChangeCardCheckoutRequestDetails(createcheckOutModel);

                            if (checkOutAmountResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                            {
                                checkoutDetails.OrderId = ((Checkout)checkOutAmountResponse.Results).OrderId;

                                checkoutDetails.ReceiptNumber = ((Checkout)checkOutAmountResponse.Results).ReceiptNumber;

                                checkoutDetails = gatewayHelper.CreateCheckoutSession(gatewayConfig, billingAddress, checkoutDetails.OrderId, checkoutDetails.TransactionID, checkoutDetails.ReceiptNumber, ((Checkout)checkOutAmountResponse.Results).OrderNumber);

                                CheckOutRequestDBUpdateModel checkoutUpdateModel = new CheckOutRequestDBUpdateModel
                                {
                                    Source = CheckOutType.ChangeCard.ToString(),

                                    SourceID = 0,

                                    CheckOutSessionID = checkoutDetails.CheckoutSession.Id,

                                    CheckoutVersion = checkoutDetails.CheckoutSession.Version,

                                    SuccessIndicator = checkoutDetails.CheckoutSession.SuccessIndicator,

                                    MPGSOrderID = checkoutDetails.OrderId,

                                    TransactionID = checkoutDetails.TransactionID
                                };

                                //Update checkout details and return amount

                                checkOutAmountResponse = await _orderAccess.GetChangeCardCheckoutRequestDetails(checkoutUpdateModel);

                                if (checkOutAmountResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                                {
                                    checkoutDetails.Amount = ((Checkout)checkOutAmountResponse.Results).Amount;

                                    checkoutDetails.MerchantName = gatewayConfig.GridMerchantName;

                                    checkoutDetails.MerchantLogo = gatewayConfig.GridMerchantLogo;

                                    checkoutDetails.MerchantEmail = gatewayConfig.GridMerchantEmail;

                                    checkoutDetails.MerchantAddressLine1 = gatewayConfig.GridMerchantAddress1;

                                    checkoutDetails.MerchantAddressLine2 = gatewayConfig.GridMerchantAddress2;

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
                                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NoRecords));
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
                                LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NoRecords));
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
                            // failed to locate customer
                              LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenNotMatching));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.TokenNotMatching),
                                IsDomainValidationErrors = false
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// Update change card details status, and upon success, will tokenize the new card and remove the existing one from gateway
        /// </summary>
        /// <param name="token"></param>
        /// <param name="updateRequest"></param>
        /// <returns></returns>
        [HttpPost("UpdateChangePaymentMethodStatus")]
        public async Task<IActionResult> UpdateChangePaymentMethodStatus([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] CheckOutResponseUpdate updateRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;

                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                          .SelectMany(x => x.Errors)
                                                          .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);
                        //update checkout details
                        DatabaseResponse updateCheckoutDetailsResponse = await _orderAccess.UpdateCheckOutResponse(updateRequest);

                        //Get token from existing payment method
                        DatabaseResponse existingPaymentMethodResponse = await _orderAccess.GetPaymentMethodToken(customerID);

                        if (updateCheckoutDetailsResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
                        {
                            if ((TokenSession)updateCheckoutDetailsResponse.Results != null)
                            {
                                PaymentHelper gatewayHelper = new PaymentHelper();

                                DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.MPGS.ToString());

                                GridMPGSConfig gatewayConfig = gatewayHelper.GetGridMPGSConfig((List<Dictionary<string, string>>)configResponse.Results);

                                TokenResponse tokenizeResponse = new TokenResponse();

                                TokenSession tokenSession = new TokenSession();

                                tokenSession = (TokenSession)updateCheckoutDetailsResponse.Results;

                                tokenizeResponse = gatewayHelper.Tokenize(gatewayConfig, tokenSession);

                                if (tokenizeResponse != null && !string.IsNullOrEmpty(tokenizeResponse.Token))
                                {
                                    // insert token response to payment methods table

                                    DatabaseResponse tokenDetailsCreateResponse = new DatabaseResponse();

                                    tokenDetailsCreateResponse = await _orderAccess.CreatePaymentMethod(tokenizeResponse, customerID, updateRequest.MPGSOrderID, "UpdateChangePaymentMethodStatus");

                                    if (tokenDetailsCreateResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                                    {                                       
                                            PaymentMethod paymentMethod = new PaymentMethod();

                                            paymentMethod = (PaymentMethod)existingPaymentMethodResponse.Results;

                                            if(paymentMethod!=null && paymentMethod.Token!=null)
                                            {
                                                string response = gatewayHelper.RemoveToken(gatewayConfig, paymentMethod.Token);

                                                if (response == MPGSAPIResponse.SUCCESS.ToString())
                                                {
                                                    DatabaseResponse databaseResponse = await _orderAccess.RemovePaymentMethod(customerID, paymentMethod.PaymentMethodID);

                                                    if (databaseResponse.ResponseCode == (int)DbReturnValue.DeleteSuccess)
                                                    {
                                                        return Ok(new OperationResponse
                                                        {
                                                            HasSucceeded = true,
                                                            Message = EnumExtensions.GetDescription(CommonErrors.PaymentMethodSuccessfullyChanged),
                                                            IsDomainValidationErrors = false
                                                        });
                                                    }

                                                    else
                                                    {
                                                    // remove from db failed
                                                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.PaymentMethodSuccessfullyRemoved) + ". " + EnumExtensions.GetDescription(CommonErrors.FailedToRemovePaymentMethodDb));
                                                        return Ok(new OperationResponse
                                                        {
                                                            HasSucceeded = true,
                                                            Message = EnumExtensions.GetDescription(CommonErrors.PaymentMethodSuccessfullyChanged),
                                                            IsDomainValidationErrors = false
                                                        });
                                                    }
                                                }

                                                else
                                                {
                                                    // remove from gateway failed, but not 
                                                   
                                                        return Ok(new OperationResponse
                                                        {
                                                            HasSucceeded = true,
                                                            Message = EnumExtensions.GetDescription(CommonErrors.PaymentMethodSuccessfullyChanged),
                                                            IsDomainValidationErrors = false
                                                        });                                                    
                                                }
                                            }
                                        else
                                        {
                                            //no existng method to remove
                                            return Ok(new OperationResponse
                                            {
                                                HasSucceeded = true,
                                                Message = EnumExtensions.GetDescription(CommonErrors.PaymentMethodSuccessfullyChanged),
                                                IsDomainValidationErrors = false
                                            });
                                        }   
                                    }

                                    else if (tokenDetailsCreateResponse.ResponseCode == (int)DbReturnValue.ExistingCard)
                                    {
                                        PaymentMethod paymentMethod = new PaymentMethod();

                                        paymentMethod = (PaymentMethod)existingPaymentMethodResponse.Results;

                                        string response = gatewayHelper.RemoveToken(gatewayConfig, paymentMethod.Token);

                                        if (response == MPGSAPIResponse.SUCCESS.ToString())
                                        {
                                            return Ok(new OperationResponse
                                            {
                                                HasSucceeded = true,
                                                Message = EnumExtensions.GetDescription(CommonErrors.PaymentMethodSuccessfullyChanged),
                                                IsDomainValidationErrors = false
                                            });
                                        }

                                        else
                                        {

                                            return Ok(new OperationResponse
                                            {
                                                HasSucceeded = true,
                                                Message = EnumExtensions.GetDescription(CommonErrors.PaymentMethodSuccessfullyChanged),
                                                IsDomainValidationErrors = false
                                            });
                                        }

                                    }

                                    else 
                                    {
                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = true,
                                            Message = EnumExtensions.GetDescription(CommonErrors.PaymentMethodSuccessfullyChanged),
                                            IsDomainValidationErrors = false
                                        });
                                    }
                                    

                                }

                                else
                                {
                                    //failed to create payment token

                                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.TokenGenerationFailed));

                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = false,
                                        Message = EnumExtensions.GetDescription(CommonErrors.TokenGenerationFailed),
                                        IsDomainValidationErrors = false
                                    });
                                }


                            }

                            else
                            {
                                //unable to get token session

                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.UnableToGetTokenSession));

                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(CommonErrors.UnableToGetTokenSession),
                                    IsDomainValidationErrors = false
                                });
                            }

                        }
                        else
                        {
                            // checkout response update failed

                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.CheckOutDetailsUpdationFailed));

                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.CheckOutDetailsUpdationFailed),
                                IsDomainValidationErrors = false
                            });
                        }

                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// Create an account Invoce Entry, which inturn is used for outstanding invoice/bill payment
        /// </summary>
        /// <param name="token"></param>
        /// <param name="accountInvoiceRequest">
        /// body{
        /// "InvoiceID" :"3000001", //bill_id
        /// "InvoiceName" :"3123201", 
        /// "FinalAmount":20       
        /// }
        /// </param>
        /// <returns></returns>
        [HttpPost("CreateAccountInvoice")]
        public async Task<IActionResult> CreateAccountInvoice([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] CreateAccountInvoiceRequest accountInvoiceRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                          .SelectMany(x => x.Errors)
                                                          .Select(x => x.ErrorMessage))
                            });
                        }

                        //
                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);
                        //update checkout details

                        DatabaseResponse accountIdResponse = await _orderAccess.GetAccountIdFromCustomerId(customerID);

                        if (accountIdResponse != null && accountIdResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                        {
                            int AccountID = (int)accountIdResponse.Results;

                            DatabaseResponse downloadLinkResponse = ConfigHelper.GetValueByKey(ConfigKeys.BSSInvoiceDownloadLink.ToString(), _iconfiguration);

                            string downloadLinkPrefix = (string)downloadLinkResponse.Results;

                            DatabaseResponse accountResponse = await _orderAccess.GetCustomerBSSAccountNumber(customerID);

                            AccountInvoice accountInvoice = new AccountInvoice
                            {
                                AccountID = AccountID,
                                CreatedBy = customerID,
                                BSSBillId = accountInvoiceRequest.InvoiceID,
                                InvoiceName = accountInvoiceRequest.InvoiceName,
                                FinalAmount = accountInvoiceRequest.FinalAmount,
                                InvoiceUrl = downloadLinkPrefix + accountInvoiceRequest.InvoiceID,
                                Remarks = Misc.Account.ToString(),
                                PaymentSourceID = AccountID, // for outstanding payment its accountID for rescheduling its deliveryIfo ID
                                OrderStatus = 0

                            };

                            DatabaseResponse createAccountInvoiceResponse = await _orderAccess.CreateAccountInvoice(accountInvoice);

                            if (createAccountInvoiceResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                            {
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = true,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.CreateSuccess),
                                    IsDomainValidationErrors = false,
                                    ReturnedObject = new InvoiceOrder { OrderID = (int)createAccountInvoiceResponse.Results }
                                });
                            }

                            else
                            {
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.CreationFailed),
                                    IsDomainValidationErrors = false
                                });
                            }

                        }
                        else
                        {
                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.AccountNotExists) + ". customer:" + customerID);

                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.AccountNotExists),
                                IsDomainValidationErrors = true
                            });
                            //account does not exists
                        }

                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken) + ". token:" + token);

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
                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed) + ". token:" + token);

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
        /// This will remove the LOA details from rescheduled delivery information
        /// </summary>
        /// <param name="token"></param>
        /// <param name="rescheduleDeliveryInformationID"></param>       
        /// <returns>OperationResponse</returns>
        [HttpGet]
        [Route("RemoveLOADetailsForRescheduledDelivery/{rescheduleDeliveryInformationID}")]
        public async Task<IActionResult> RemoveLOADetailsForRescheduledDelivery([FromHeader(Name = "Grid-Authorization-Token")] string token, int rescheduleDeliveryInformationID)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });


                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;

                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse rescheduleLOAResponse = await _orderAccess.RemoveRescheduleLOADetails(rescheduleDeliveryInformationID);

                        if (rescheduleLOAResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(CommonErrors.LOARemoved),
                                Result = rescheduleLOAResponse.Results

                            });
                        }

                        else if (rescheduleLOAResponse.ResponseCode == (int)DbReturnValue.UpdationFailed)
                        {
                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedUpdateLOADetails) + " token:" + token + ", RescheduleDeliveryInformationID:" + rescheduleDeliveryInformationID);

                            return Ok(new ServerResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.FailedToRemoveRescheduleLoa)
                            });

                        }

                        else
                        {
                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.DeliveryInfoNotExists) + " token:" + token + ", RescheduleDeliveryInformationID:" + rescheduleDeliveryInformationID);

                            return Ok(new ServerResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.DeliveryInfoNotExists)
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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


        [HttpGet]
        [Route("GetMQBody/{OrderID}")]
        public async Task<IActionResult> GetMQBody([FromHeader(Name = "Grid-Authorization-Token")] string token, int OrderID)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });


                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;

                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }

                        MessageQueueDataAccess _orderAccess = new MessageQueueDataAccess(_iconfiguration);

                        DatabaseResponse mqResponse = await _orderAccess.GetOrderMessageQueueBody(OrderID);

                        if (mqResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = "Retrieved Successfully",
                                Result = mqResponse.Results

                            });
                        }
                        else
                        {                            
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.DeliveryInfoNotExists)
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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