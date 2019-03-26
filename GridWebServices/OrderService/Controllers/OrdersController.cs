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

                                //GridBSSConfi config = LinqExtensions.GeObjectFromDictionary<GridBSSConfi>((Dictionary<string, string>)configResponse.Results);

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

                                        //Start Remove while going live -- only for testing
                                        // Unblock blocked number 
                                        DatabaseResponse requestIdToUpdateUnblock = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), aTokenResp.CustomerID);

                                        BSSUpdateResponseObject bssUnblockUpdateResponse = await helper.UpdateAssetBlockNumber(config, ((BSSAssetRequest)requestIdToUpdateRes.Results).request_id, AssetToSubscribe, true);

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

        /// <summary>
        /// This will Update subscribers existing number with new number selected.
        /// </summary>
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
        public async Task<IActionResult> UpdateSubscriberNumber([FromBody] UpdateSubscriberNumber request)
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
        [HttpPost]
        [Route("PortingNumber")]
        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> PortingNumber([FromForm] UpdateSubscriberPortingNumberRequest request)
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

                // var files = Request.Form.Files;

                IFormFile file = request.PortedNumberTransferForm;

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

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

                        UploadResponse s3UploadResponse = await s3Helper.UploadFile(file);

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
