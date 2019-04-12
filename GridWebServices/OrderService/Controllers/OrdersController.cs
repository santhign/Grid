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
using OrderService.Enums;


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
        /// <param name="token"></param>
        /// <param name="id">OrderID</param>
        /// <returns>OperationsResponse</returns>
        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] int id)
        {
            try
            {
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
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer));
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

                        CreateOrder order = new CreateOrder();

                        order = new CreateOrder { BundleID = request.BundleID, PromotionCode = request.PromotionCode, ReferralCode = request.ReferralCode, CustomerID = customerID };

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

                                BSSAPIHelper bsshelper = new BSSAPIHelper();

                                DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());


                                GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                                DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                                DatabaseResponse requestIdRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), customerID, (int)BSSCalls.NewSession, "");

                                ResponseObject res = await bsshelper.GetAssetInventory(config, (((List<ServiceFees>)serviceCAF.Results)).FirstOrDefault().ServiceCode, (BSSAssetRequest)requestIdRes.Results);

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

                                    BSSUpdateResponseObject bssUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateRes.Results, AssetToSubscribe, false);

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

                                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.CreateSubscriptionFailed));

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

                                        LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.UpdateAssetBlockingFailed));

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

                                    LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.GetAssetFailed));

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

                        BSSAPIHelper bsshelper = new BSSAPIHelper();

                        OrderCustomer customer = new OrderCustomer();

                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(request.OrderID);

                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
                        {
                            customer = (OrderCustomer)customerResponse.Results;


                            DatabaseResponse requestIdToUpdateUnblock = await _orderAccess.GetBssApiRequestIdAndSubscriberSession(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customer.CustomerId, request.OldMobileNumber);

                            DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                            GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                            // Unblock
                            BSSUpdateResponseObject bssUnblockUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateUnblock.Results, request.OldMobileNumber, true);

                            if (bsshelper.GetResponseCode(bssUnblockUpdateResponse) == "0")
                            {
                                //Block

                                DatabaseResponse requestIdToUpdateBlock = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customer.CustomerId, (int)BSSCalls.ExistingSession, request.NewNumber.MobileNumber);

                                BSSUpdateResponseObject bssUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateBlock.Results, request.NewNumber.MobileNumber, false);

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

                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
                        {
                            customer = (OrderCustomer)customerResponse.Results;

                            DatabaseResponse requestIdToUpdateUnblock = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customer.CustomerId, (int)BSSCalls.ExistingSession, request.OldMobileNumber);

                            DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                            GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                            // Unblock
                            BSSUpdateResponseObject bssUnblockUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateUnblock.Results, request.OldMobileNumber, true);

                            if (bsshelper.GetResponseCode(bssUnblockUpdateResponse) == "0")
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

                            DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                            DatabaseResponse requestIdRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), customerID, (int)BSSCalls.NewSession, "");

                            ResponseObject res = await bsshelper.GetAssetInventory(config, (((List<ServiceFees>)serviceCAF.Results)).FirstOrDefault().ServiceCode, (BSSAssetRequest)requestIdRes.Results);

                            string AssetToSubscribe = bsshelper.GetAssetId(res);

                            if (res != null && (int.Parse(res.Response.asset_details.total_record_count) > 0))
                            {
                                //Block number                                    

                                DatabaseResponse requestIdToUpdateRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customerID, (int)BSSCalls.NewSession, "");

                                BSSUpdateResponseObject bssUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateRes.Results, AssetToSubscribe, false);

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
                                        LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.CreateSubscriptionFailed));

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
                            // failed to locate customer
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer));
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
        /// <param name="token"></param>
        /// <param name="request">
        /// Form{
        /// "OrderID" :1,
        /// "IDType" :"PAN",
        /// "IDNumber":"P23FD",
        /// "IDImageFront" : FileInput,
        /// "IDImageBack" : FileInput,
        /// "NameInNRIC" : "Name as in NRIC",
        /// "DisplayName" : "DisplayName",
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
        public async Task<IActionResult> UpdateOrderPersonalDetails([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromForm] UpdateOrderPersonalDetailsRequest request)
        {
            try
            {
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
                            IFormFile frontImage = request.IDImageFront;

                            IFormFile backImage = request.IDImageBack;

                            BSSAPIHelper bsshelper = new BSSAPIHelper();

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
                                DisplayName = request.DisplayName,
                                Nationality = request.Nationality
                            };

                            //process file if uploaded - non null

                            if (frontImage != null && backImage != null)
                            {
                                DatabaseResponse awsConfigResponse = await _orderAccess.GetConfiguration(ConfiType.AWS.ToString());

                                if (awsConfigResponse != null && awsConfigResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                                {
                                    GridAWSS3Config awsConfig = configHelper.GetGridAwsConfig((List<Dictionary<string, string>>)awsConfigResponse.Results);

                                    AmazonS3 s3Helper = new AmazonS3(awsConfig);

                                    string fileNameFront = "Grid_IDNUMBER_Front_" + DateTime.UtcNow.ToString("yyyymmddhhmmss") + Path.GetExtension(frontImage.FileName); //Grid_IDNUMBER_yyyymmddhhmmss.extension

                                    UploadResponse s3UploadResponse = await s3Helper.UploadFile(frontImage, fileNameFront);

                                    if (s3UploadResponse.HasSucceed)
                                    {
                                        personalDetails.IDFrontImageUrl = "http://gridproject.s3.amazonaws.com/gridproject/" + s3UploadResponse.FileName;
                                    }
                                    else
                                    {
                                        LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.S3UploadFailed));
                                    }

                                    string fileNameBack = "Grid_IDNUMBER_Back_" + DateTime.UtcNow.ToString("yyyymmddhhmmss") + Path.GetExtension(frontImage.FileName); //Grid_IDNUMBER_yyyymmddhhmmss.extension

                                    s3UploadResponse = await s3Helper.UploadFile(backImage, fileNameBack);

                                    if (s3UploadResponse.HasSucceed)
                                    {
                                        personalDetails.IDBackImageUrl = "http://gridproject.s3.amazonaws.com/gridproject/" + s3UploadResponse.FileName;
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
                            // failed to locate customer
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer));
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
                            // failed to locate customer
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer));
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
                            // failed to locate customer
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer));
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
                            // failed to locate customer
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer));
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
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

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
                            // failed to locate customer
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer));
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
                            // failed to locate customer
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer));
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
                            // failed to locate customer
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer));
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
        /// <param name="token" in="Header"></param>     
        /// <param name="orderId">Initial OrderID/ChangeRequestID in case of sim replacement/planchange/numberchange</param>
        /// <param name="orderType"> Initial Order = 1, ChangeSim = 2, ChangeNumber = 3, ChangePlan = 4</param>
        /// <returns>OperationsResponse</returns>
        [HttpGet("GetCheckOutDetails/{orderId}/{orderType}")]
        public async Task<IActionResult> GetCheckOutDetails([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute]int orderId, [FromRoute]int orderType)
        {
            try
            {
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

                        DatabaseResponse customerResponse = await _orderAccess.GetCustomerIdFromOrderId(orderId);

                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
                        {
                            // Call MPGS to create a checkout session and retuen details

                            PaymentHelper gatewayHelper = new PaymentHelper();

                            Checkout checkoutDetails = new Checkout();

                            DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.MPGS.ToString());

                            GridMPGSConfig gatewayConfig = gatewayHelper.GetGridMPGSConfig((List<Dictionary<string, string>>)configResponse.Results);

                            checkoutDetails = gatewayHelper.CreateCheckoutSession(gatewayConfig);

                            CheckOutRequestDBUpdateModel checkoutUpdateModel = new CheckOutRequestDBUpdateModel
                            {
                                Source = ((CheckOutType)orderType).ToString(),

                                SourceID = orderId,

                                CheckOutSessionID = checkoutDetails.CheckoutSession.Id,

                                CheckoutVersion = checkoutDetails.CheckoutSession.Version,

                                SuccessIndicator = checkoutDetails.CheckoutSession.SuccessIndicator,

                                MPGSOrderID = checkoutDetails.OrderId
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
                            // failed to locate customer
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer));
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

        /// <summary>
        /// This will update checkout response to database and retrieve checkout infor from gateway
        /// </summary>
        /// <param name="token"></param>
        /// <param name="updateRequest">
        /// body{
        /// "MPGSOrderID" :"f88bere0",
        /// "CheckOutSessionID" :"SESSION0002391471348N70583782K8",
        /// "Result":"Success"       
        /// }
        /// </param>
        /// <returns>OperationResponse</returns>
        [Route("UpdateCheckOutResponse")]
        [HttpPost]
        public async Task<IActionResult> UpdateCheckOutResponse([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] CheckOutResponseUpdate updateRequest)
        {
            try
            {
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
                            //remove additional line
                            DatabaseResponse removeLineResponse = await _orderAccess.RemoveAdditionalLine(request);

                            if (removeLineResponse.ResponseCode == (int)DbReturnValue.DeleteSuccess)
                            {
                                // call bss api to release/unblock the number

                                BSSAPIHelper bsshelper = new BSSAPIHelper();

                                MiscHelper configHelper = new MiscHelper();

                                DatabaseResponse requestIdToUpdateUnblock = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customerID, (int)BSSCalls.ExistingSession, request.MobileNumber);

                                DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                                GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                                // Unblock
                                BSSUpdateResponseObject bssUnblockUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateUnblock.Results, request.MobileNumber, true);

                                if (bsshelper.GetResponseCode(bssUnblockUpdateResponse) == "0")
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
                            // failed to locate customer
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer));
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

                            if (res != null && (int.Parse(res.Response.asset_details.total_record_count) > 0))
                            {
                                //Block number                                    

                                DatabaseResponse requestIdToUpdateRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customerID, (int)BSSCalls.ExistingSession, request.OldNumber);

                                BSSUpdateResponseObject bssUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateRes.Results, NewNumber, false);

                                if (bsshelper.GetResponseCode(bssUpdateResponse) == "0")
                                {
                                    // Assign Newnumber
                                    AssignNewNumber newNumbertoAssign = new AssignNewNumber { OrderID = request.OrderID, OldNumber = request.OldNumber, NewNumber = NewNumber };

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
                            // failed to locate customer
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer));
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
    }
}