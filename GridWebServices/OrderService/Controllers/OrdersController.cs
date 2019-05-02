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
using Newtonsoft.Json;

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

                                try
                                {

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

                                catch (Exception ex)
                                {
                                    LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.GetAssetFailed));
                                    LogInfo.Fatal(ex, EnumExtensions.GetDescription(CommonErrors.GetAssetFailed));
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

                        DatabaseResponse portResponse = await _orderAccess.GetPortTypeFromOrderId(request.OrderID, request.OldMobileNumber);

                        if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((OrderCustomer)customerResponse.Results).CustomerId)
                        {
                            if (portResponse.Results.ToString().Trim() == "0")
                            {
                                customer = (OrderCustomer)customerResponse.Results;

                                //DatabaseResponse requestIdToUpdateUnblock = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), customer.CustomerId, (int)BSSCalls.ExistingSession, request.OldMobileNumber);

                                //DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                                //GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                                //// Unblock
                                //BSSUpdateResponseObject bssUnblockUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateUnblock.Results, request.OldMobileNumber, true);

                                //if (bsshelper.GetResponseCode(bssUnblockUpdateResponse) == "0")
                                //{
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
                                //}

                                //else
                                //{
                                //    // unblocking failed
                                //    LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.UpdateAssetUnBlockingFailed));
                                //    return Ok(new OperationResponse
                                //    {
                                //        HasSucceeded = false,
                                //        Message = EnumExtensions.GetDescription(DbReturnValue.UnBlockingFailed),
                                //        IsDomainValidationErrors = false
                                //    });
                                //}
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

                                //insert this AssetToSubscribe into database

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
                            else if(updatePersoanDetailsResponse.ResponseCode == (int)DbReturnValue.DeliverySlotUnavailability)
                            {
                                LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.DeliverySlotUnavailability));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.DeliverySlotUnavailability),
                                    IsDomainValidationErrors = false
                                });
                            }
                            else if (updatePersoanDetailsResponse.ResponseCode == (int)DbReturnValue.OrderDeliveryInformationMissing)
                            {
                                LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.OrderDeliveryInformationMissing));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.OrderDeliveryInformationMissing),
                                    IsDomainValidationErrors = false
                                });
                            }
                            else if (updatePersoanDetailsResponse.ResponseCode == (int)DbReturnValue.OrderIDDocumentsMissing)
                            {
                                LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.OrderIDDocumentsMissing));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.OrderIDDocumentsMissing),
                                    IsDomainValidationErrors = false
                                });
                            }
                            else if (updatePersoanDetailsResponse.ResponseCode == (int)DbReturnValue.OrderNationalityMissing)
                            {
                                LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.OrderNationalityMissing));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.OrderNationalityMissing),
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
        /// <param name="orderType"> Initial Order = 1, ChangeRequest = 2, AccountInvoices = 4</param>
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

                            checkoutDetails = gatewayHelper.CreateCheckoutSession(gatewayConfig);

                            CheckOutRequestDBUpdateModel checkoutUpdateModel = new CheckOutRequestDBUpdateModel
                            {
                                Source = ((CheckOutType)orderType).ToString(),

                                SourceID = orderId,

                                CheckOutSessionID = checkoutDetails.CheckoutSession.Id,

                                CheckoutVersion = checkoutDetails.CheckoutSession.Version,

                                SuccessIndicator = checkoutDetails.CheckoutSession.SuccessIndicator,

                                MPGSOrderID = checkoutDetails.OrderId,

                                TransactionID=checkoutDetails.TransactionID
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
                                LogInfo.Error(EnumExtensions.GetDescription(checkOutAmountResponse.ResponseCode));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(checkOutAmountResponse.ResponseCode),
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
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Orders_update_checkout_response);

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
                        //Preeti : Validatechckoutdetails against customer ID
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
                            DatabaseResponse sourceTyeResponse = new DatabaseResponse();

                            sourceTyeResponse = await _orderAccess.GetSourceTypeByMPGSSOrderId(updateRequest.MPGSOrderID);

                            if (sourceTyeResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                            {
                                if (((OrderSource)sourceTyeResponse.Results).SourceType == CheckOutType.ChangeRequest.ToString())
                                {
                                    var details = await _messageQueueDataAccess.GetMessageDetails(updateRequest.MPGSOrderID);

                                    if (details != null)
                                    {
                                        MessageBodyForCR msgBody = new MessageBodyForCR();

                                        string topicName = string.Empty, pushResult = string.Empty; string ReasonType = string.Empty;

                                        try
                                        {
                                            Dictionary<string, string> attribute = new Dictionary<string, string>();

                                            msgBody = await _messageQueueDataAccess.GetMessageBodyByChangeRequest(details.ChangeRequestID);
                                            if (msgBody == null)
                                            {
                                                throw new NullReferenceException("message body is null for ChangeRequest (" + details.ChangeRequestID + ") for ChangeSIM in UpdateCheckout Response Request Service API");
                                            }
                                            if (details.RequestTypeID == (int)Core.Enums.RequestType.ReplaceSIM)
                                            {
                                                ReasonType = Core.Enums.RequestType.ReplaceSIM.GetDescription();
                                                topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration).Results.ToString().Trim();
                                                if (string.IsNullOrWhiteSpace(topicName))
                                                {
                                                    throw new NullReferenceException("topicName is null for ChangeRequest (" + details.ChangeRequestID + ") for ChangeSIM in UpdateCheckout Response Request Service API");
                                                }
                                                attribute.Add(EventTypeString.EventType, Core.Enums.RequestType.ReplaceSIM.GetDescription());
                                                pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, msgBody, attribute);
                                            }
                                            else if (details.RequestTypeID == (int)Core.Enums.RequestType.ChangePlan)
                                            {
                                                ReasonType = Core.Enums.RequestType.ChangePlan.GetDescription();
                                                topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration).Results.ToString().Trim();
                                                if (string.IsNullOrWhiteSpace(topicName))
                                                {
                                                    throw new NullReferenceException("topicName is null for ChangeRequest (" + details.ChangeRequestID + ") for ChangePlan in UpdateCheckout Response Request Service API");
                                                }
                                                attribute.Add(EventTypeString.EventType, Core.Enums.RequestType.ChangePlan.GetDescription());
                                                pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, msgBody, attribute);
                                            }
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
                                                    MessageAttribute = ReasonType,
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
                                                    MessageAttribute = ReasonType,
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
                                                MessageAttribute = ReasonType,
                                                MessageBody = msgBody != null ? JsonConvert.SerializeObject(msgBody) : null,
                                                Status = 0,
                                                Remark = "Error Occured in ReplaceSIM from UpdateCheckoutResponse",
                                                Exception = new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical)


                                            };


                                            await _messageQueueDataAccess.InsertMessageInMessageQueueRequestException(queueRequest);
                                        }

                                    }
                                }
                                else if (((OrderSource)sourceTyeResponse.Results).SourceType == CheckOutType.Orders.ToString())
                                {
                                    DatabaseResponse orderMqResponse = new DatabaseResponse();

                                    orderMqResponse = await _messageQueueDataAccess.GetOrderMessageQueueBody(((OrderSource)sourceTyeResponse.Results).SourceID);

                                    OrderQM orderDetails = new OrderQM();

                                    string topicName = string.Empty;

                                    string pushResult = string.Empty;

                                    if (orderMqResponse != null && orderMqResponse.Results != null)
                                    {
                                        orderDetails = (OrderQM)orderMqResponse.Results;

                                        DatabaseResponse OrderCountResponse = await _orderAccess.GetCustomerOrderCount(orderDetails.customerID);

                                        try
                                        {
                                            Dictionary<string, string> attribute = new Dictionary<string, string>();

                                            topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration).Results.ToString().Trim();


                                            attribute.Add(EventTypeString.EventType, ((OrderCount)OrderCountResponse.Results).SuccessfulOrders == 1 ? Core.Enums.RequestType.NewCustomer.GetDescription() : Core.Enums.RequestType.NewService.GetDescription());

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
                                                    MessageAttribute = ((OrderCount)OrderCountResponse.Results).SuccessfulOrders == 1 ? Core.Enums.RequestType.NewCustomer.GetDescription() : Core.Enums.RequestType.NewService.GetDescription(),
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
                                                    MessageAttribute = ((OrderCount)OrderCountResponse.Results).SuccessfulOrders == 1 ? Core.Enums.RequestType.NewCustomer.GetDescription() : Core.Enums.RequestType.NewService.GetDescription(),
                                                    MessageBody = JsonConvert.SerializeObject(orderDetails),
                                                    Status = 0
                                                };
                                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                                            MessageQueueRequest queueRequest = new MessageQueueRequest
                                            {
                                                Source = CheckOutType.Orders.ToString(),
                                                NumberOfRetries = 1,
                                                SNSTopic = topicName,
                                                CreatedOn = DateTime.Now,
                                                LastTriedOn = DateTime.Now,
                                                PublishedOn = DateTime.Now,
                                                MessageAttribute = ((OrderCount)OrderCountResponse.Results).SuccessfulOrders == 1 ? Core.Enums.RequestType.NewCustomer.GetDescription() : Core.Enums.RequestType.NewService.GetDescription(),
                                                MessageBody = JsonConvert.SerializeObject(orderDetails),
                                                Status = 0
                                            };
                                            await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                                        }
                                    }

                                }

                            }

                            else
                            {
                                // unable to get sourcetype form db

                            }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token" in="Header"></param>
        /// <param name="OrderID"></param>
        /// <returns></returns>
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

                                    DownloadResponse FrontImageDownloadResponse = await s3Helper.DownloadFile(((OrderNRICDetails)nRICresponse.Results).DocumentURL.Remove(0, awsConfig.AWSEndPoint.Length));

                                    DownloadResponse BackImageDownloadResponse = await s3Helper.DownloadFile(((OrderNRICDetails)nRICresponse.Results).DocumentBackURL.Remove(0, awsConfig.AWSEndPoint.Length));

                                    DownloadNRIC nRICDownloadObject = new DownloadNRIC { OrderID= OrderID, FrontImage = FrontImageDownloadResponse.FileObject != null ? configHelper.GetBase64StringFromByteArray(FrontImageDownloadResponse.FileObject, ((OrderNRICDetails)nRICresponse.Results).DocumentURL.Remove(0, awsConfig.AWSEndPoint.Length)) : null, BackImage = BackImageDownloadResponse.FileObject != null ? configHelper.GetBase64StringFromByteArray(BackImageDownloadResponse.FileObject, ((OrderNRICDetails)nRICresponse.Results).DocumentBackURL.Remove(0, awsConfig.AWSEndPoint.Length)) : null, IdentityCardNumber= ((OrderNRICDetails)nRICresponse.Results).IdentityCardNumber, IdentityCardType= ((OrderNRICDetails)nRICresponse.Results).IdentityCardType, Nationality= ((OrderNRICDetails)nRICresponse.Results).Nationality };

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
                                    LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetConfiguration));

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
                            IFormFile frontImage = request.IDImageFront;

                            IFormFile backImage = request.IDImageBack;

                            BSSAPIHelper bsshelper = new BSSAPIHelper();

                            MiscHelper configHelper = new MiscHelper();

                            UpdateOrderPersonalDetails personalDetails = new UpdateOrderPersonalDetails
                            {
                                OrderID = request.OrderID,  
                                Nationality=request.Nationality,
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

                                    string fileNameFront = "Grid_IDNUMBER_Front_" + DateTime.UtcNow.ToString("yyyymmddhhmmss") + Path.GetExtension(frontImage.FileName); //Grid_IDNUMBER_yyyymmddhhmmss.extension

                                    UploadResponse s3UploadResponse = await s3Helper.UploadFile(frontImage, fileNameFront);

                                    if (s3UploadResponse.HasSucceed)
                                    {
                                        personalDetails.IDFrontImageUrl = awsConfig.AWSEndPoint + s3UploadResponse.FileName;
                                    }
                                    else
                                    {
                                        LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.S3UploadFailed));
                                    }

                                    string fileNameBack = "Grid_IDNUMBER_Back_" + DateTime.UtcNow.ToString("yyyymmddhhmmss") + Path.GetExtension(frontImage.FileName); //Grid_IDNUMBER_yyyymmddhhmmss.extension

                                    s3UploadResponse = await s3Helper.UploadFile(backImage, fileNameBack);

                                    if (s3UploadResponse.HasSucceed)
                                    {
                                        personalDetails.IDBackImageUrl = awsConfig.AWSEndPoint + s3UploadResponse.FileName;
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
        /// This will create a checkout session and returns the details to call MPGS 
        /// </summary>
        /// <param name="token" in="Header"></param>  
        /// <param name="request"></param>      
        /// <returns>OperationsResponse</returns>
      
        [HttpPost("Tokenize")]
        public async Task<IActionResult> Tokenize([FromHeader(Name = "Grid-Authorization-Token")] string token)
        {
            try
            {

                CreateTokenResponse request = new CreateTokenResponse();

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

                        DatabaseResponse updateTokenSesisonDetails = new DatabaseResponse();

                       // updateTokenSesisonDetails = await _orderAccess.UpdateMPGSCreateTokenSessionDetails(request);
                        //updateTokenSesisonDetails.ResponseCode == (int)DbReturnValue.RecordExists && customerID == ((CreateTokenUpdatedDetails)updateTokenSesisonDetails.Results).CustomerID
                        if (1==1)
                        {
                            // Call MPGS to create a checkout session and retuen details

                            PaymentHelper gatewayHelper = new PaymentHelper();

                            DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.MPGS.ToString());

                            GridMPGSConfig gatewayConfig = gatewayHelper.GetGridMPGSConfig((List<Dictionary<string, string>>)configResponse.Results);

                            TokenResponse tokenizeResponse = new TokenResponse();
                            TransactionResponseModel transactionResponse = new TransactionResponseModel();
                            //

                           // tokenizeResponse = gatewayHelper.TokenizeTest(gatewayConfig);
                            // transactionResponse = gatewayHelper.PayWithToken(gatewayConfig, request, (CreateTokenUpdatedDetails)updateTokenSesisonDetails.Results, tokenizeResponse);

                            string response = gatewayHelper.VoidTransaction(gatewayConfig);
                           // string response = gatewayHelper.Capture(gatewayConfig);
                           // string response = gatewayHelper.CaptureTest(gatewayConfig);//transactionResponse = gatewayHelper.PayWithToken(gatewayConfig, request, (CreateTokenUpdatedDetails)updateTokenSesisonDetails.Results, tokenizeResponse); 
                            if (tokenizeResponse != null)
                            {
                                // update token reponse in database and then call gatewayHelper.PayWithToken to pay the amount

                              //  TransactionResponseModel transactionResponse = new TransactionResponseModel();

//transactionResponse = gatewayHelper.PayWithToken(gatewayConfig, request, (CreateTokenUpdatedDetails)updateTokenSesisonDetails.Results, tokenizeResponse); 

                                // update transaction
                                // push order message to queue
                                return Ok(new OperationResponse
                                {
                                    // add message and result here
                                    HasSucceeded = true,
                                   // Message = 
                                    IsDomainValidationErrors = false
                                });
                            }
                            else
                            {
                                // failed to tokenize the payment details

                                LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToTokenizeCustomerAccount));

                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(CommonErrors.FailedToTokenizeCustomerAccount) + ". " + EnumExtensions.GetDescription(CommonErrors.PayWithTokenFailed),
                                    IsDomainValidationErrors = false
                                });
                            }
                        }
                        //else
                        //{
                        //    // CustomerID not matching
                        //    LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer));

                        //    return Ok(new OperationResponse
                        //    {
                        //        HasSucceeded = false,
                        //        Message = EnumExtensions.GetDescription(DbReturnValue.NotExists),
                        //        IsDomainValidationErrors = false
                        //    });
                        //}
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
                                LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToUpdatedSubscriptionDetails));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.UpdateNotAllowed),
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
                                Message = EnumExtensions.GetDescription(DbReturnValue.UpdateSuccess),
                                Result = LOAResponse.Results

                            });
                        }

                        else
                        {
                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedUpdateLOADetails));
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.UpdationFailed)
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
                                        Source = Source.ChangeRequest,
                                        NumberOfRetries = 1,
                                        SNSTopic = string.IsNullOrWhiteSpace(topicName) ? null : topicName,
                                        CreatedOn = DateTime.Now,
                                        LastTriedOn = DateTime.Now,
                                        PublishedOn = DateTime.Now,
                                        MessageAttribute = Core.Enums.RequestType.CancelOrder.GetDescription().ToString(),
                                        MessageBody = orderDetails != null ? JsonConvert.SerializeObject(orderDetails) : null,
                                        Status = 0,
                                        Remark = "Error Occured in BuyVASService",
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
                        LogInfo.Error(DbReturnValue.DuplicateCRExists.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.DuplicateCRExists.GetDescription(),
                            IsDomainValidationErrors = false
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

                                    tokenDetailsCreateResponse = await _orderAccess.CreatePaymentMethod(tokenizeResponse, customerID);

                                    if (tokenDetailsCreateResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                                    {
                                        tokenSession.SourceOfFundType = tokenizeResponse.Type;

                                        tokenSession.Token = tokenizeResponse.Token;

                                        string captureResponse = gatewayHelper.Capture(gatewayConfig, tokenSession);

                                        //TransactionResponseModel transactionResponse = new TransactionResponseModel();

                                        // get the session details and transaction details

                                        TransactionRetrieveResponseOperation transactionResponse = new TransactionRetrieveResponseOperation();

                                        transactionResponse = gatewayHelper.RetrieveCheckOutTransaction(gatewayConfig, updateRequest);

                                        // deside on the fields to update to database for payment processing and call SP to update


                                        DatabaseResponse tokenDetailsUpdateResponse = new DatabaseResponse();

                                        DatabaseResponse paymentProcessingRespose = new DatabaseResponse();

                                        paymentProcessingRespose = await _orderAccess.UpdateCheckOutReceipt(transactionResponse.TrasactionResponse);

                                        tokenDetailsUpdateResponse = await _orderAccess.UpdatePaymentMethodDetails(transactionResponse.TrasactionResponse, customerID, tokenSession.Token);


                                        if (paymentProcessingRespose.ResponseCode == (int)DbReturnValue.TransactionSuccess)
                                        {
                                            DatabaseResponse sourceTyeResponse = new DatabaseResponse();

                                            sourceTyeResponse = await _orderAccess.GetSourceTypeByMPGSSOrderId(updateRequest.MPGSOrderID);

                                            if (sourceTyeResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                                            {
                                                if (((OrderSource)sourceTyeResponse.Results).SourceType == CheckOutType.ChangeRequest.ToString())
                                                {
                                                    var details = await _messageQueueDataAccess.GetMessageDetails(updateRequest.MPGSOrderID);

                                                    if (details != null)
                                                    {
                                                        MessageBodyForCR msgBody = new MessageBodyForCR();

                                                        string topicName = string.Empty, pushResult = string.Empty;

                                                        try
                                                        {
                                                            Dictionary<string, string> attribute = new Dictionary<string, string>();

                                                            msgBody = await _messageQueueDataAccess.GetMessageBodyByChangeRequest(details.ChangeRequestID);

                                                            if (details.RequestTypeID == (int)Core.Enums.RequestType.ReplaceSIM)
                                                            {
                                                                topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration).Results.ToString().Trim();
                                                                attribute.Add(EventTypeString.EventType, Core.Enums.RequestType.ReplaceSIM.GetDescription());
                                                                pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, msgBody, attribute);
                                                            }
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
                                                                    MessageAttribute = Core.Enums.RequestType.ReplaceSIM.GetDescription(),
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
                                                                    MessageAttribute = Core.Enums.RequestType.ReplaceSIM.GetDescription(),
                                                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                                                    Status = 0
                                                                };
                                                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                                                            }

                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                                                            MessageQueueRequest queueRequest = new MessageQueueRequest
                                                            {
                                                                Source = Source.ChangeRequest,
                                                                NumberOfRetries = 1,
                                                                SNSTopic = topicName,
                                                                CreatedOn = DateTime.Now,
                                                                LastTriedOn = DateTime.Now,
                                                                PublishedOn = DateTime.Now,
                                                                MessageAttribute = Core.Enums.RequestType.ReplaceSIM.GetDescription(),
                                                                MessageBody = JsonConvert.SerializeObject(msgBody),
                                                                Status = 0
                                                            };


                                                            await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                                                        }

                                                    }
                                                }
                                                else if (((OrderSource)sourceTyeResponse.Results).SourceType == CheckOutType.Orders.ToString())
                                                {
                                                    DatabaseResponse orderMqResponse = new DatabaseResponse();

                                                    orderMqResponse = await _messageQueueDataAccess.GetOrderMessageQueueBody(((OrderSource)sourceTyeResponse.Results).SourceID);

                                                    OrderQM orderDetails = new OrderQM();

                                                    string topicName = string.Empty;

                                                    string pushResult = string.Empty;

                                                    if (orderMqResponse != null && orderMqResponse.Results != null)
                                                    {
                                                        orderDetails = (OrderQM)orderMqResponse.Results;

                                                        DatabaseResponse OrderCountResponse = await _orderAccess.GetCustomerOrderCount(orderDetails.customerID);

                                                        try
                                                        {
                                                            Dictionary<string, string> attribute = new Dictionary<string, string>();

                                                            topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration).Results.ToString().Trim();


                                                            attribute.Add(EventTypeString.EventType, ((OrderCount)OrderCountResponse.Results).SuccessfulOrders == 1 ? Core.Enums.RequestType.NewCustomer.GetDescription() : Core.Enums.RequestType.NewService.GetDescription());

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
                                                                    MessageAttribute = ((OrderCount)OrderCountResponse.Results).SuccessfulOrders == 1 ? Core.Enums.RequestType.NewCustomer.GetDescription() : Core.Enums.RequestType.NewService.GetDescription(),
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
                                                                    MessageAttribute = ((OrderCount)OrderCountResponse.Results).SuccessfulOrders == 1 ? Core.Enums.RequestType.NewCustomer.GetDescription() : Core.Enums.RequestType.NewService.GetDescription(),
                                                                    MessageBody = JsonConvert.SerializeObject(orderDetails),
                                                                    Status = 0
                                                                };
                                                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                                                            }

                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                                                            MessageQueueRequest queueRequest = new MessageQueueRequest
                                                            {
                                                                Source = CheckOutType.Orders.ToString(),
                                                                NumberOfRetries = 1,
                                                                SNSTopic = topicName,
                                                                CreatedOn = DateTime.Now,
                                                                LastTriedOn = DateTime.Now,
                                                                PublishedOn = DateTime.Now,
                                                                MessageAttribute = ((OrderCount)OrderCountResponse.Results).SuccessfulOrders == 1 ? Core.Enums.RequestType.NewCustomer.GetDescription() : Core.Enums.RequestType.NewService.GetDescription(),
                                                                MessageBody = JsonConvert.SerializeObject(orderDetails),
                                                                Status = 0
                                                            };
                                                            await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                                                        }
                                                    }

                                                }

                                            }

                                            else
                                            {
                                                // unable to get sourcetype form db

                                            }

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
                                        // token details update failed

                                        LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToCreatePaymentMethod));

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

                                    LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.TokenGenerationFailed));

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

                                LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.UnableToGetTokenSession));

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

                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.CheckOutDetailsUpdationFailed));

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
                                LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToRemovePaymentMethod));
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

                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.TokenNotExists));
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

                                MPGSOrderID = checkoutDetails.OrderId,

                                TransactionID = checkoutDetails.TransactionID
                            };

                            //Update checkout details and return amount

                            DatabaseResponse checkOutAmountResponse = await _orderAccess.GetCheckoutRequestDetails(checkoutUpdateModel);

                            //Get token from paymentmethodID
                            DatabaseResponse paymentMethodResponse = await _orderAccess.GetPaymentMethodToken(customerID);

                            PaymentMethod paymentMethod = new PaymentMethod();

                            paymentMethod = (PaymentMethod)paymentMethodResponse.Results;

                            if (checkOutAmountResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                            {
                                checkoutDetails.Amount = ((Checkout)checkOutAmountResponse.Results).Amount;

                                string authorizeResponse = gatewayHelper.Authorize(gatewayConfig, checkoutDetails, paymentMethod);

                                if (authorizeResponse == MPGSAPIResponse.SUCCESS.ToString())
                                {
                                    string captureResponse = gatewayHelper.Capture(gatewayConfig, new TokenSession { Amount = checkoutDetails.Amount, MPGSOrderID = checkoutDetails.OrderId, Token = paymentMethod.Token, SourceOfFundType = paymentMethod.SourceType });

                                    if (captureResponse == MPGSAPIResponse.SUCCESS.ToString())
                                    {
                                        TransactionRetrieveResponseOperation transactionResponse = new TransactionRetrieveResponseOperation();

                                        CheckOutResponseUpdate updateRequest = new CheckOutResponseUpdate { MPGSOrderID = checkoutDetails.OrderId, Result = captureResponse };

                                        transactionResponse = gatewayHelper.RetrieveCheckOutTransaction(gatewayConfig, updateRequest);

                                        DatabaseResponse paymentProcessingRespose = new DatabaseResponse();

                                        paymentProcessingRespose = await _orderAccess.UpdateCheckOutReceipt(transactionResponse.TrasactionResponse);

                                        if (paymentProcessingRespose.ResponseCode == (int)DbReturnValue.TransactionSuccess)
                                        {
                                            DatabaseResponse sourceTyeResponse = new DatabaseResponse();

                                            sourceTyeResponse = await _orderAccess.GetSourceTypeByMPGSSOrderId(updateRequest.MPGSOrderID);

                                            if (sourceTyeResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                                            {
                                                if (((OrderSource)sourceTyeResponse.Results).SourceType == CheckOutType.ChangeRequest.ToString())
                                                {
                                                    var details = await _messageQueueDataAccess.GetMessageDetails(updateRequest.MPGSOrderID);

                                                    if (details != null)
                                                    {
                                                        MessageBodyForCR msgBody = new MessageBodyForCR();

                                                        string topicName = string.Empty, pushResult = string.Empty;

                                                        try
                                                        {
                                                            Dictionary<string, string> attribute = new Dictionary<string, string>();

                                                            msgBody = await _messageQueueDataAccess.GetMessageBodyByChangeRequest(details.ChangeRequestID);

                                                            if (details.RequestTypeID == (int)Core.Enums.RequestType.ReplaceSIM)
                                                            {
                                                                topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration).Results.ToString().Trim();
                                                                attribute.Add(EventTypeString.EventType, Core.Enums.RequestType.ReplaceSIM.GetDescription());
                                                                pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, msgBody, attribute);
                                                            }
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
                                                                    MessageAttribute = Core.Enums.RequestType.ReplaceSIM.GetDescription(),
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
                                                                    MessageAttribute = Core.Enums.RequestType.ReplaceSIM.GetDescription(),
                                                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                                                    Status = 0
                                                                };
                                                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                                                            }

                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                                                            MessageQueueRequest queueRequest = new MessageQueueRequest
                                                            {
                                                                Source = Source.ChangeRequest,
                                                                NumberOfRetries = 1,
                                                                SNSTopic = topicName,
                                                                CreatedOn = DateTime.Now,
                                                                LastTriedOn = DateTime.Now,
                                                                PublishedOn = DateTime.Now,
                                                                MessageAttribute = Core.Enums.RequestType.ReplaceSIM.GetDescription(),
                                                                MessageBody = JsonConvert.SerializeObject(msgBody),
                                                                Status = 0
                                                            };


                                                            await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                                                        }

                                                    }
                                                }
                                                else if (((OrderSource)sourceTyeResponse.Results).SourceType == CheckOutType.Orders.ToString())
                                                {
                                                    DatabaseResponse orderMqResponse = new DatabaseResponse();

                                                    orderMqResponse = await _messageQueueDataAccess.GetOrderMessageQueueBody(((OrderSource)sourceTyeResponse.Results).SourceID);

                                                    OrderQM orderDetails = new OrderQM();

                                                    string topicName = string.Empty;

                                                    string pushResult = string.Empty;

                                                    if (orderMqResponse != null && orderMqResponse.Results != null)
                                                    {
                                                        orderDetails = (OrderQM)orderMqResponse.Results;

                                                        DatabaseResponse OrderCountResponse = await _orderAccess.GetCustomerOrderCount(orderDetails.customerID);

                                                        try
                                                        {
                                                            Dictionary<string, string> attribute = new Dictionary<string, string>();

                                                            topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration).Results.ToString().Trim();


                                                            attribute.Add(EventTypeString.EventType, ((OrderCount)OrderCountResponse.Results).SuccessfulOrders == 1 ? Core.Enums.RequestType.NewCustomer.GetDescription() : Core.Enums.RequestType.NewService.GetDescription());

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
                                                                    MessageAttribute = ((OrderCount)OrderCountResponse.Results).SuccessfulOrders == 1 ? Core.Enums.RequestType.NewCustomer.GetDescription() : Core.Enums.RequestType.NewService.GetDescription(),
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
                                                                    MessageAttribute = ((OrderCount)OrderCountResponse.Results).SuccessfulOrders == 1 ? Core.Enums.RequestType.NewCustomer.GetDescription() : Core.Enums.RequestType.NewService.GetDescription(),
                                                                    MessageBody = JsonConvert.SerializeObject(orderDetails),
                                                                    Status = 0
                                                                };
                                                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                                                            }

                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                                                            MessageQueueRequest queueRequest = new MessageQueueRequest
                                                            {
                                                                Source = CheckOutType.Orders.ToString(),
                                                                NumberOfRetries = 1,
                                                                SNSTopic = topicName,
                                                                CreatedOn = DateTime.Now,
                                                                LastTriedOn = DateTime.Now,
                                                                PublishedOn = DateTime.Now,
                                                                MessageAttribute = ((OrderCount)OrderCountResponse.Results).SuccessfulOrders == 1 ? Core.Enums.RequestType.NewCustomer.GetDescription() : Core.Enums.RequestType.NewService.GetDescription(),
                                                                MessageBody = JsonConvert.SerializeObject(orderDetails),
                                                                Status = 0
                                                            };
                                                            await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                                                        }
                                                    }

                                                }

                                            }

                                            else
                                            {
                                                // unable to get sourcetype form db

                                            }

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

        [HttpPost]
        [Route("RescheduleDelivery")]
        public async Task<IActionResult> RescheduleDelivery([FromHeader(Name = "Grid-Authorization-Token")] string token, Order_RescheduleDeliveryRequest detailsrequest)
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
                    //DatabaseResponse statusResponse = new DatabaseResponse();
                    var statusResponse =
                        await _orderAccess.RescheduleDelivery(aTokenResp.CustomerID, detailsrequest);

                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
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
                        LogInfo.Error(DbReturnValue.DuplicateCRExists.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.DuplicateCRExists.GetDescription(),
                            IsDomainValidationErrors = false
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


        [HttpPost]
        [Route("UpdateAddressForRescheduleDelivery/{orderId}")]
        public async Task<IActionResult> UpdateAddressForRescheduleDelivery([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] int orderId, UpdateCRShippingDetailsRequest detailsrequest)
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
                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = statusResponse
                        });
                    }
                    else if (statusResponse.ResponseCode == (int)DbReturnValue.DuplicateCRExists)
                    {
                        LogInfo.Error(DbReturnValue.DuplicateCRExists.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.DuplicateCRExists.GetDescription(),
                            IsDomainValidationErrors = false
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

                            checkoutDetails = gatewayHelper.CreateCheckoutSession(gatewayConfig);

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

                            DatabaseResponse checkOutAmountResponse = await _orderAccess.GetChangeCardCheckoutRequestDetails(checkoutUpdateModel);

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

                                    tokenDetailsCreateResponse = await _orderAccess.CreatePaymentMethod(tokenizeResponse, customerID);

                                    if (tokenDetailsCreateResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                                    {
                                        if (existingPaymentMethodResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                                        {
                                            PaymentMethod paymentMethod = new PaymentMethod();

                                            paymentMethod = (PaymentMethod)existingPaymentMethodResponse.Results;

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
                                                LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToRemovePaymentMethod));
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
                                            //failed to get existing payment method

                                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.PaymentMethodNotExists));
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
                                        // token details update failed

                                        LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.FailedToCreatePaymentMethod));

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

                                    LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.TokenGenerationFailed));

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

                                LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.UnableToGetTokenSession));

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

                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.CheckOutDetailsUpdationFailed));

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