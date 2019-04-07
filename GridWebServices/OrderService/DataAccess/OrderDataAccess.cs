﻿using OrderService.Models;
using Core.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Core.Enums;
using InfrastructureService;
using Core.Models;
using Core.Extensions;


namespace OrderService.DataAccess
{
    /// <summary>
    /// Order Data Access class
    /// </summary>
    public class OrderDataAccess
    {
        /// <summary>
        /// The data helper
        /// </summary>
        internal DataAccessHelper _DataHelper = null;

        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public OrderDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// This method will create a new order for createOrder endpoint
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> CreateOrder(CreateOrder order)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@BundleID",  SqlDbType.Int ),
                    new SqlParameter( "@ReferralCode",  SqlDbType.NVarChar),
                    new SqlParameter( "@PromotionCode",  SqlDbType.NVarChar)
                };

                parameters[0].Value = order.CustomerID;
                parameters[1].Value = order.BundleID;
                parameters[2].Value = order.ReferralCode;
                parameters[3].Value = order.PromotionCode;

                _DataHelper = new DataAccessHelper("Orders_CreateOrder", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run(dt);

                OrderInit orderCreated = new OrderInit();

                if (dt != null && dt.Rows.Count > 0)
                {
                    orderCreated = (from model in dt.AsEnumerable()
                                    select new OrderInit()
                                    {
                                        OrderID = model.Field<int>("OrderID"),
                                        Status = model.Field<string>("Status")
                                    }).FirstOrDefault();

                }

                return new DatabaseResponse { ResponseCode = result, Results = orderCreated };
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Creates the subscriber.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> CreateSubscriber(CreateSubscriber subscriber)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@BundleID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@PromotionCode",  SqlDbType.NVarChar)
                };

                parameters[0].Value = subscriber.OrderID;
                parameters[1].Value = subscriber.BundleID;
                parameters[2].Value = subscriber.MobileNumber;
                parameters[3].Value = subscriber.PromotionCode;


                _DataHelper = new DataAccessHelper("Order_CreateSubscriber", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run();    // 100 / 107 

                return new DatabaseResponse { ResponseCode = result };
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Gets the order basic details.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetOrderBasicDetails(int orderId)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )

                };

                parameters[0].Value = orderId;

                _DataHelper = new DataAccessHelper("Order_GetOrderBasicDetails", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = _DataHelper.Run(ds); // 105 /109

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    OrderBasicDetails orderDetails = new OrderBasicDetails();

                    if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {

                        orderDetails = (from model in ds.Tables[0].AsEnumerable()
                                        select new OrderBasicDetails()
                                        {
                                            OrderID = model.Field<int>("OrderID"),
                                            OrderNumber = model.Field<string>("OrderNumber"),
                                            OrderDate = model.Field<DateTime>("OrderDate"),
                                        }).FirstOrDefault();

                        List<OrderSubscription> subscriptions = new List<OrderSubscription>();

                        if (ds.Tables[1].Rows.Count > 0)
                        {

                            subscriptions = (from model in ds.Tables[1].AsEnumerable()
                                             select new OrderSubscription()
                                             {
                                                 BundleID = model.Field<int>("BundleID"),
                                                 DisplayName = model.Field<string>("DisplayName"),
                                                 MobileNumber = model.Field<string>("MobileNumber"),
                                             }).ToList();

                            orderDetails.OrderSubscriptions = subscriptions;

                        }
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = orderDetails };

                }

                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                }

                return response;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Gets the BSS API request identifier.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="apiName">Name of the API.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetBssApiRequestId(string source, string apiName, int customerId)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@Source",  SqlDbType.NVarChar ),
                    new SqlParameter( "@APIName",  SqlDbType.NVarChar ),
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                };

                parameters[0].Value = source;
                parameters[1].Value = apiName;
                parameters[2].Value = customerId;

                _DataHelper = new DataAccessHelper("Admin_GetRequestIDForBSSAPI", parameters, _configuration);

                DataTable dt = new DataTable();

                _DataHelper.Run(dt);

                BSSAssetRequest assetRequest = new BSSAssetRequest();

                DatabaseResponse response = new DatabaseResponse();

                if (dt.Rows.Count > 0)
                {

                    assetRequest = (from model in dt.AsEnumerable()
                                    select new BSSAssetRequest()
                                    {
                                        request_id = model.Field<string>("RequestID")

                                    }).FirstOrDefault();

                }



                response = new DatabaseResponse { Results = assetRequest };




                return response;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Gets the service fee.
        /// </summary>
        /// <param name="serviceCode">The service code.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetServiceFee(int serviceCode)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@ServiceCode",  SqlDbType.Int )

                };

                parameters[0].Value = serviceCode;

                _DataHelper = new DataAccessHelper("Admin_GetServiceFee", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run(dt); // 102 /105

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    ServiceFees serviceFee = new ServiceFees();

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        serviceFee = (from model in dt.AsEnumerable()
                                      select new ServiceFees()
                                      {
                                          ServiceCode = model.Field<int>("ServiceCode"),

                                          ServiceFee = model.Field<double>("ServiceFee")

                                      }).FirstOrDefault();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = serviceFee };

                }

                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                }

                return response;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Sims the replacement request.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> SimReplacementRequest(int customerId, string mobileNumber)
        {
            try
            {
                var ds = new DataSet();
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@RequestType",  SqlDbType.NVarChar)
                };

                parameters[0].Value = customerId;
                parameters[1].Value = mobileNumber;
                parameters[2].Value = Core.Enums.RequestType.ChangeSim.GetDescription();
                    
                _DataHelper = new DataAccessHelper("Order_CR_SIMReplacementRequest", parameters, _configuration);

                var result = await _DataHelper.RunAsync(ds);

                if (result != (int)Core.Enums.DbReturnValue.CreateSuccess)
                    return new DatabaseResponse { ResponseCode = result };

                var TorSresponse = new ChangeSimResponse();

                if (ds.Tables.Count > 0)
                {
                    TorSresponse = (from model in ds.Tables[0].AsEnumerable()
                        select new ChangeSimResponse()
                        {
                            ChangeRequestId = model.Field<int>("ChangeRequestId"),
                            OrderNumber = model.Field<string>("OrderNumber"),
                            RequestOn = model.Field<DateTime>("RequestOn"),
                            RequestTypeDescription = model.Field<string>("RequestTypeDescription"),
                            BillingUnit = model.Field<string>("BillingUnit"),
                            BillingFloor = model.Field<string>("BillingFloor"),
                            BillingBuildingNumber = model.Field<string>("BillingBuildingNumber"),
                            BillingStreetName = model.Field<string>("BillingStreetName"),
                            BillingPostCode = model.Field<string>("BillingPostCode"),
                            BillingContactNumber = model.Field<string>("BillingContactNumber"),
                            Name = model.Field<string>("Name"),
                            Email = model.Field<string>("Email"),
                            //IsSameAsBilling = model.Field<string>("IsSameAsBilling"),
                            ShippingUnit = model.Field<string>("ShippingUnit"),
                            ShippingFloor = model.Field<string>("ShippingFloor"),
                            ShippingBuildingNumber = model.Field<string>("ShippingBuildingNumber"),
                            ShippingStreetName = model.Field<string>("ShippingStreetName"),
                            ShippingPostCode = model.Field<string>("ShippingPostCode"),
                            ShippingContactNumber = model.Field<string>("ShippingContactNumber")//,
                            
                        }).FirstOrDefault();

                    if (TorSresponse != null)
                        TorSresponse.ChangeRequestChargesList = (from model in ds.Tables[1].AsEnumerable()
                            select new ChangeRequestCharges()
                            {
                                PortalServiceName = model.Field<string>("PortalServiceName"),
                                ServiceFee = model.Field<double>("ServiceFee"),
                                IsRecurring = model.Field<bool>("IsRecurring"),
                                IsGstIncluded = model.Field<bool>("IsGSTIncluded")
                            }).ToList();
                }

                var response = new DatabaseResponse { ResponseCode = result, Results = TorSresponse };
                return response;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Terminations the or suspension request.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <param name="request">The request.</param>
        /// <param name="remark">The remark.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> TerminationOrSuspensionRequest(int customerId, string mobileNumber, string request, string remark)
        {
            try
            {
                var ds = new DataSet();
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@RequestTypeDescription",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Remarks",  SqlDbType.NVarChar)
                };

                parameters[0].Value = customerId;
                parameters[1].Value = mobileNumber;
                parameters[2].Value = request;
                parameters[3].Value = remark;

                _DataHelper = new DataAccessHelper("Orders_CR_RaiseRequest", parameters, _configuration);

                var result = await _DataHelper.RunAsync(ds);

                if (result != (int) Core.Enums.DbReturnValue.CreateSuccess)
                    return new DatabaseResponse {ResponseCode = result};

                var TorSresponse = new TerminationOrSuspensionResponse();

                if (ds.Tables.Count > 0)
                {
                    TorSresponse = (from model in ds.Tables[0].AsEnumerable()
                        select new TerminationOrSuspensionResponse()
                        {
                            ChangeRequestId = model.Field<int>("ChangeRequestId"),
                            OrderNumber = model.Field<string>("OrderNumber"),
                            RequestOn = model.Field<DateTime>("RequestOn"),
                            RequestTypeDescription = model.Field<string>("RequestTypeDescription")
                        }).FirstOrDefault();

                    if (TorSresponse != null)
                        TorSresponse.ChangeRequestChargesList = (from model in ds.Tables[1].AsEnumerable()
                            select new ChangeRequestCharges()
                            {
                                PortalServiceName = model.Field<string>("PortalServiceName"),
                                ServiceFee = model.Field<double>("ServiceFee")
                            }).ToList();
                }

                var response = new DatabaseResponse { ResponseCode = result, Results = TorSresponse };
                return response;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Removes the vas service.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> RemoveVasService(int customerId, string mobileNumber, int planId)
        {
            try
            {

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerId",  SqlDbType.Int ),
                    new SqlParameter( "@OldMobileNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@PlanId",  SqlDbType.Int)
                };

                parameters[0].Value = customerId;
                parameters[1].Value = mobileNumber;
                parameters[2].Value = planId;
                

                _DataHelper = new DataAccessHelper("Orders_CR_InsertRemoveVAS", parameters, _configuration);

                

                var result = await _DataHelper.RunAsync();    // 101 / 102 

                return new DatabaseResponse { ResponseCode = result };
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Buys the vas service.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <param name="bundleId">The bundle identifier.</param>
        /// <param name="quantity">The quantity.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> BuyVasService(int customerId, string mobileNumber, int bundleId, int quantity)
        {
            try
            {

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerId",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@BundleID",  SqlDbType.Int),
                    new SqlParameter( "@Quantity",  SqlDbType.Int)
                };

                parameters[0].Value = customerId;
                parameters[1].Value = mobileNumber;
                parameters[2].Value = bundleId;
                parameters[3].Value = quantity;


                _DataHelper = new DataAccessHelper("Orders_CR_BuyVAS", parameters, _configuration);



                var result = await _DataHelper.RunAsync();    // 101 / 102 

                return new DatabaseResponse { ResponseCode = result };
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }
        /// <summary>
        /// Authenticates the customer token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> AuthenticateCustomerToken(string token)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@Token",  SqlDbType.NVarChar )

                };

                parameters[0].Value = token;

                _DataHelper = new DataAccessHelper("Customer_AuthenticateToken", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run(dt); // 111 /109

                DatabaseResponse response = new DatabaseResponse();

                if (result == 111)
                {

                    AuthTokenResponse tokenResponse = new AuthTokenResponse();

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        tokenResponse = (from model in dt.AsEnumerable()
                                         select new AuthTokenResponse()
                                         {
                                             CustomerID = model.Field<int>("CustomerID"),

                                             CreatedOn = model.Field<DateTime>("CreatedOn")


                                         }).FirstOrDefault();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = tokenResponse };

                }

                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                }

                return response;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Updates the subscriber number.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdateSubscriberNumber(UpdateSubscriberNumber subscriber)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@OldMobileNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@NewMobileNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@DisplayName",  SqlDbType.NVarChar),
                    new SqlParameter( "@PremiumTypeSericeCode",  SqlDbType.Int)
                };

                parameters[0].Value = subscriber.OrderID;
                parameters[1].Value = subscriber.OldMobileNumber;
                parameters[2].Value = subscriber.NewNumber.MobileNumber;
                parameters[3].Value = subscriber.DisplayName;
                parameters[4].Value = subscriber.NewNumber.ServiceCode;

                _DataHelper = new DataAccessHelper("Orders_UpdateSubscriberNumber", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run();    // 101 / 102 

                return new DatabaseResponse { ResponseCode = result };
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Updates the subscriber porting number.
        /// </summary>
        /// <param name="portingNumber">The porting number.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdateSubscriberPortingNumber(UpdateSubscriberPortingNumber portingNumber)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@OldMobileNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@NewMobileNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@DisplayName",  SqlDbType.NVarChar),
                    new SqlParameter( "@IsOwnNumber",  SqlDbType.Int),
                    new SqlParameter( "@DonorProvider",  SqlDbType.NVarChar),
                    new SqlParameter( "@PortedNumberTransferForm",  SqlDbType.NVarChar),
                    new SqlParameter( "@PortedNumberOwnedBy",  SqlDbType.NVarChar),
                    new SqlParameter( "@PortedNumberOwnerRegistrationID",  SqlDbType.NVarChar),
                };

                parameters[0].Value = portingNumber.OrderID;
                parameters[1].Value = portingNumber.OldMobileNumber;
                parameters[2].Value = portingNumber.NewMobileNumber;
                parameters[3].Value = portingNumber.DisplayName;
                parameters[4].Value = portingNumber.IsOwnNumber;
                parameters[5].Value = portingNumber.DonorProvider;
                parameters[6].Value = portingNumber.PortedNumberTransferForm;
                parameters[7].Value = portingNumber.PortedNumberOwnedBy;
                parameters[8].Value = portingNumber.PortedNumberOwnerRegistrationID;

                _DataHelper = new DataAccessHelper("Orders_UpdateSubscriberPortingNumber", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run();    // 101 / 109 

                return new DatabaseResponse { ResponseCode = result };
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <param name="configType">Type of the configuration.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetConfiguration(string configType)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@ConfigType",  SqlDbType.NVarChar )

                };

                parameters[0].Value = configType;

                _DataHelper = new DataAccessHelper("Admin_GetConfigurations", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run(dt); // 102 /105

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    List<Dictionary<string, string>> configDictionary = new List<Dictionary<string, string>>();

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        configDictionary = LinqExtensions.GetDictionary(dt);
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = configDictionary };

                }

                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                }

                return response;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Gets the BSS service category and fee.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetBSSServiceCategoryAndFee(string serviceType)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@ServiceType",  SqlDbType.NVarChar )

                };

                parameters[0].Value = serviceType;

                _DataHelper = new DataAccessHelper("Admin_GetNumberTypeCodes", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run(dt); // 102 /105

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    List<ServiceFees> serviceFees = new List<ServiceFees>();

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        serviceFees = (from model in dt.AsEnumerable()

                                       select new ServiceFees()
                                       {
                                           PortalServiceName = model.Field<string>("PortalServiceName"),

                                           ServiceCode = model.Field<int>("ServiceCode"),

                                           ServiceFee = model.Field<double>("ServiceFee")

                                       }).ToList();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = serviceFees };

                }

                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                }

                return response;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }


        /// <summary>
        /// Gets the order details.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetOrderDetails(int orderId)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )

                };

                parameters[0].Value = orderId;

                _DataHelper = new DataAccessHelper("Order_GetOrderDetails", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = _DataHelper.Run(ds); // 105 /102

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    Order orderDetails = new Order();

                    if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {

                        orderDetails = (from model in ds.Tables[0].AsEnumerable()
                                        select new Order()
                                        {
                                            OrderID = model.Field<int>("OrderID"),
                                            OrderNumber = model.Field<string>("OrderNumber"),
                                            OrderDate = model.Field<DateTime>("OrderDate"),
                                            BillingUnit = model.Field<string>("BillingUnit"),
                                            BillingFloor = model.Field<string>("BillingFloor"),
                                            BillingBuildingNumber = model.Field<string>("BillingBuildingNumber"),
                                            BillingBuildingName = model.Field<string>("BillingBuildingName"),
                                            BillingStreetName = model.Field<string>("BillingStreetName"),
                                            BillingPostCode = model.Field<string>("BillingPostCode"),
                                            BillingContactNumber = model.Field<string>("BillingContactNumber"),
                                            ReferralCode = model.Field<string>("ReferralCode"),
                                            Name = model.Field<string>("Name"),
                                            Email = model.Field<string>("Email"),
                                            IDType = model.Field<string>("IDType"),
                                            IDNumber = model.Field<string>("IDNumber"),
                                            IsSameAsBilling = model.Field<int?>("IsSameAsBilling"),
                                            ShippingUnit = model.Field<string>("ShippingUnit"),
                                            ShippingFloor = model.Field<string>("ShippingFloor"),
                                            ShippingBuildingNumber = model.Field<string>("ShippingBuildingNumber"),
                                            ShippingBuildingName = model.Field<string>("ShippingBuildingName"),
                                            ShippingStreetName = model.Field<string>("ShippingStreetName"),
                                            ShippingPostCode = model.Field<string>("ShippingPostCode"),
                                            ShippingContactNumber = model.Field<string>("ShippingContactNumber"),
                                            AlternateRecipientContact = model.Field<string>("AlternateRecipientContact"),
                                            AlternateRecipientName = model.Field<string>("AlternateRecipientName"),
                                            AlternateRecipientEmail = model.Field<string>("AlternateRecipientEmail"),
                                            PortalSlotID = model.Field<string>("PortalSlotID"),
                                            SlotDate = model.Field<DateTime?>("SlotDate"),
                                            SlotFromTime = model.Field<TimeSpan?>("SlotFromTime"),
                                            SlotToTime = model.Field<TimeSpan?>("SlotToTime"),
                                            ScheduledDate = model.Field<DateTime?>("ScheduledDate"),
                                            ServiceFee = model.Field<double?>("ServiceFee")
                                        }).FirstOrDefault();

                        List<Bundle> orderBundles = new List<Bundle>();

                        if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                        {

                            orderBundles = (from model in ds.Tables[1].AsEnumerable()
                                            select new Bundle()
                                            {
                                                BundleID = model.Field<int>("BundleID"),
                                                DisplayName = model.Field<string>("DisplayName"),
                                                MobileNumber = model.Field<string>("MobileNumber"),
                                                IsPrimaryNumber = model.Field<int>("IsPrimaryNumber"),
                                                PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                                PortalDescription = model.Field<string>("PortalDescription"),
                                                TotalData = model.Field<string>("TotalData"),
                                                TotalSMS = model.Field<double?>("TotalSMS"),
                                                TotalVoice = model.Field<double?>("TotalVoice"),
                                                ApplicableSubscriptionFee = model.Field<double?>("ApplicableSubscriptionFee"),
                                                ServiceName = model.Field<string>("ServiceName"),
                                                ApplicableServiceFee = model.Field<double?>("ApplicableServiceFee"),
                                                PremiumType = model.Field<int>("PremiumType"),
                                                IsPorted = model.Field<int>("IsPorted"),
                                                IsOwnNumber = model.Field<int>("IsOwnNumber"),
                                                DonorProvider = model.Field<string>("DonorProvider"),
                                                PortedNumberTransferForm = model.Field<string>("PortedNumberTransferForm"),
                                                PortedNumberOwnedBy = model.Field<string>("PortedNumberOwnedBy"),
                                                PortedNumberOwnerRegistrationID = model.Field<string>("PortedNumberOwnerRegistrationID"),
                                            }).ToList();

                            orderDetails.Bundles = orderBundles;

                        }
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = orderDetails };

                }

                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                }

                return response;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Gets the customer identifier from order identifier.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetCustomerIdFromOrderId(int orderId)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )

                };

                parameters[0].Value = orderId;

                _DataHelper = new DataAccessHelper("Order_GetCustomerIDByOrderID", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run(dt); // 105 /102

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    OrderCustomer customer = new OrderCustomer();

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        customer = (from model in dt.AsEnumerable()
                                    select new OrderCustomer()
                                    {
                                        CustomerId = model.Field<int>("CustomerID"),

                                    }).FirstOrDefault();




                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = customer };
                }

                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                }

                return response;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Gets the available slots.
        /// </summary>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetAvailableSlots()
        {
            try
            {
                _DataHelper = new DataAccessHelper("Orders_GetAvailableSlots",  _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run(dt); // 105 /102

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                  List<DeliverySlot> deliverySlots = new List<DeliverySlot>();

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        deliverySlots = (from model in dt.AsEnumerable()
                                         select new DeliverySlot()
                                         {
                                              PortalSlotID = model.Field<string>("PortalSlotID"),
                                              SlotDate = model.Field<DateTime>("SlotDate"),
                                              SlotFromTime = model.Field<TimeSpan>("SlotFromTime"),
                                              SlotToTime = model.Field<TimeSpan>("SlotToTime"),
                                              Slot = model.Field<string>("Slot"),
                                              AdditionalCharge = model.Field<double>("AdditionalCharge"),

                                         }).ToList();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = deliverySlots };

                }

                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                }

                return response;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Updates the order personal details.
        /// </summary>
        /// <param name="personalDetails">The personal details.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdateOrderPersonalDetails(UpdateOrderPersonalDetails personalDetails)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@IDType",  SqlDbType.NVarChar ),
                    new SqlParameter( "@IDNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@IDImageUrl",  SqlDbType.NVarChar),
                    new SqlParameter( "@NameInNRIC",  SqlDbType.NVarChar),
                    new SqlParameter( "@Gender",  SqlDbType.NVarChar),
                    new SqlParameter( "@DOB",  SqlDbType.Date),
                    new SqlParameter( "@ContactNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@Nationality",  SqlDbType.NVarChar),
                };



                parameters[0].Value = personalDetails.OrderID;
                parameters[1].Value = personalDetails.IDType;
                parameters[2].Value = personalDetails.IDNumber;
                parameters[3].Value = personalDetails.IDImageUrl;
                parameters[4].Value = personalDetails.NameInNRIC;
                parameters[5].Value = personalDetails.Gender;
                parameters[6].Value = personalDetails.DOB;
                parameters[7].Value = personalDetails.ContactNumber;
                parameters[8].Value = personalDetails.Nationality;

                _DataHelper = new DataAccessHelper("Orders_UpdateOrderBasicDetails", parameters, _configuration);               

                int result = _DataHelper.Run();    // 101 / 109 

                return new DatabaseResponse { ResponseCode = result };
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Updates the order billing details.
        /// </summary>
        /// <param name="billingDetails">The billing details.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdateOrderBillingDetails(UpdateOrderBillingDetailsRequest billingDetails)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@Postcode",  SqlDbType.NVarChar ),
                    new SqlParameter( "@BlockNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@Unit",  SqlDbType.NVarChar),
                    new SqlParameter( "@Floor",  SqlDbType.NVarChar),
                    new SqlParameter( "@BuildingName",  SqlDbType.NVarChar),
                    new SqlParameter( "@StreetName",  SqlDbType.NVarChar),
                    new SqlParameter( "@ContactNumber",  SqlDbType.NVarChar)                    
                };

                parameters[0].Value = billingDetails.OrderID;
                parameters[1].Value = billingDetails.Postcode;
                parameters[2].Value = billingDetails.BlockNumber;
                parameters[3].Value = billingDetails.Unit;
                parameters[4].Value = billingDetails.Floor;
                parameters[5].Value = billingDetails.BuildingName;
                parameters[6].Value = billingDetails.StreetName;
                parameters[7].Value = billingDetails.ContactNumber;               

                _DataHelper = new DataAccessHelper("Orders_UpdateOrderBillingDetails", parameters, _configuration);              

                int result = _DataHelper.Run();    // 101 / 109 

                return new DatabaseResponse { ResponseCode = result };
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Updates the order shipping details.
        /// </summary>
        /// <param name="shippingDetails">The shipping details.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdateOrderShippingDetails(UpdateOrderShippingDetailsRequest shippingDetails)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@Postcode",  SqlDbType.NVarChar ),
                    new SqlParameter( "@BlockNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@Unit",  SqlDbType.NVarChar),
                    new SqlParameter( "@Floor",  SqlDbType.NVarChar),
                    new SqlParameter( "@BuildingName",  SqlDbType.NVarChar),
                    new SqlParameter( "@StreetName",  SqlDbType.NVarChar),
                    new SqlParameter( "@ContactNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@IsBillingSame",  SqlDbType.Int),
                    new SqlParameter( "@PortalSlotID",  SqlDbType.NVarChar)
                };

                parameters[0].Value = shippingDetails.OrderID;
                parameters[1].Value = shippingDetails.Postcode;
                parameters[2].Value = shippingDetails.BlockNumber;
                parameters[3].Value = shippingDetails.Unit;
                parameters[4].Value = shippingDetails.Floor;
                parameters[5].Value = shippingDetails.BuildingName;
                parameters[6].Value = shippingDetails.StreetName;
                parameters[7].Value = shippingDetails.ContactNumber;
                parameters[8].Value = shippingDetails.IsBillingSame;
                parameters[9].Value = shippingDetails.PortalSlotID;

                _DataHelper = new DataAccessHelper("Orders_UpdateOrderShippingDetails", parameters, _configuration);               

                int result = _DataHelper.Run();    // 101 / 109 

                return new DatabaseResponse { ResponseCode = result };
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Updates the order loa details.
        /// </summary>
        /// <param name="loaDetails">The loa details.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdateOrderLOADetails(UpdateOrderLOADetailsRequest loaDetails)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@RecipientName",  SqlDbType.NVarChar ),
                    new SqlParameter( "@IDType",  SqlDbType.NVarChar),
                    new SqlParameter( "@IDNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@ContactNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@EmailAdddress",  SqlDbType.NVarChar)                  
                };

                parameters[0].Value = loaDetails.OrderID;
                parameters[1].Value = loaDetails.RecipientName;
                parameters[2].Value = loaDetails.IDType;
                parameters[3].Value = loaDetails.IDNumber;
                parameters[4].Value = loaDetails.ContactNumber;
                parameters[5].Value = loaDetails.EmailAdddress;              

                _DataHelper = new DataAccessHelper("Orders_UpdateOrderLOADetails", parameters, _configuration);              

                int result = _DataHelper.Run();    // 101 / 109 

                return new DatabaseResponse { ResponseCode = result };
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Validates the order referral code.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> ValidateOrderReferralCode(ValidateOrderReferralCodeRequest order)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@ReferralCode",  SqlDbType.NVarChar )                   
                };

                parameters[0].Value = order.OrderID;

                parameters[1].Value = order.ReferralCode;   

                _DataHelper = new DataAccessHelper("Orders_ValidateReferralCode", parameters, _configuration);               

                int result = _DataHelper.Run();    // 105 / 109 

                return new DatabaseResponse { ResponseCode = result };
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Gets the ordered numbers.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetOrderedNumbers(OrderedNumberRequest order)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )

                };

                parameters[0].Value = order.OrderID;

                _DataHelper = new DataAccessHelper("Orders_GetOrderSubscribers", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run(dt);    // 105 / 109 
                DatabaseResponse response = new DatabaseResponse();
                if (result == 105)
                {
                    List<FreeNumber> numbers = new List<FreeNumber>();

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        numbers = (from model in dt.AsEnumerable()
                                         select new FreeNumber()
                                         {
                                              MobileNumber = model.Field<string>("MobileNumber")                                            

                                         }).ToList();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = numbers };
                }
                else
                {
                    response = new DatabaseResponse { ResponseCode = result};
                }

                return response;
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Updates the order subcription details.
        /// </summary>
        /// <param name="subscriptionDetails">The subscription details.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdateOrderSubcriptionDetails(UpdateOrderSubcriptionDetailsRequest subscriptionDetails)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@ContactNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Terms",  SqlDbType.Int),
                    new SqlParameter( "@PaymentSubscription",  SqlDbType.Int),
                    new SqlParameter( "@PromotionMessage",  SqlDbType.Int)
                  
                };   

                parameters[0].Value = subscriptionDetails.OrderID;
                parameters[1].Value = subscriptionDetails.ContactNumber;
                parameters[2].Value = subscriptionDetails.Terms;
                parameters[3].Value = subscriptionDetails.PaymentSubscription;
                parameters[4].Value = subscriptionDetails.PromotionMessage;
              


                _DataHelper = new DataAccessHelper("Orders_UpdateOrderSubscriptions", parameters, _configuration);

                int result = _DataHelper.Run();    // 101 / 109 

                return new DatabaseResponse { ResponseCode = result };
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Gets the pending order details.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetPendingOrderDetails(int customerId)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter( "@CustomerID",  SqlDbType.Int )
                };

                parameters[0].Value = customerId;

                _DataHelper = new DataAccessHelper("Orders_GetPendingOrderDetails", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run(dt);    // 105 / 119 
                DatabaseResponse response = new DatabaseResponse();
                if (result == 105)
                {
                    OrderPending pendingOrderDetails = new OrderPending();
                   
                    if (dt != null && dt.Rows.Count > 0)
                    {

                        pendingOrderDetails = (from model in dt.AsEnumerable()
                                               select new OrderPending()
                                        {
                                            OrderID = model.Field<int>("OrderID"),
                                            OrderNumber = model.Field<string>("OrderNumber"),
                                            OrderDate = model.Field<DateTime>("OrderDate")
                                           }).FirstOrDefault();

                      }
                    

                    response = new DatabaseResponse { ResponseCode = result, Results = pendingOrderDetails };
                }
                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                }

                return response;
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Gets the checkout request details.
        /// </summary>
        /// <param name="checkOutRequest">The check out request.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetCheckoutRequestDetails(CheckOutRequestDBUpdateModel checkOutRequest)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter( "@Source",  SqlDbType.NVarChar ),
                     new SqlParameter( "@SourceID",  SqlDbType.Int ),
                     new SqlParameter( "@MPGSOrderID",  SqlDbType.NVarChar ),
                     new SqlParameter( "@CheckOutSessionID",  SqlDbType.NVarChar ),
                     new SqlParameter( "@SuccessIndicator",  SqlDbType.NVarChar ),
                     new SqlParameter( "@CheckoutVersion",  SqlDbType.NVarChar ),
                };

                parameters[0].Value = checkOutRequest.Source;
                parameters[1].Value = checkOutRequest.SourceID;
                parameters[2].Value = checkOutRequest.MPGSOrderID;
                parameters[3].Value = checkOutRequest.CheckOutSessionID;
                parameters[4].Value = checkOutRequest.SuccessIndicator;
                parameters[5].Value = checkOutRequest.CheckoutVersion;

                _DataHelper = new DataAccessHelper("Orders_GetCheckoutRequestDetails", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run(dt);    // 105 / 102
                DatabaseResponse response = new DatabaseResponse();
                if (result == 105)
                {
                    Checkout checkOut = new  Checkout();

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        checkOut = (from model in dt.AsEnumerable()
                                               select new Checkout()
                                               {
                                                  Amount= model.Field<double>("Amount"),
                                               }).FirstOrDefault();

                    }


                    response = new DatabaseResponse { ResponseCode = result, Results = checkOut };
                }
                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                }

                return response;
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Updates the check out response.
        /// </summary>
        /// <param name="checkOutResponse">The check out response.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdateCheckOutResponse(CheckOutResponseUpdate checkOutResponse)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter( "@MPGSOrderID",  SqlDbType.NVarChar ),
                     new SqlParameter( "@Status",  SqlDbType.NVarChar )                   
                };
   
                parameters[0].Value = checkOutResponse.MPGSOrderID;

                parameters[1].Value = checkOutResponse.Result;              

               _DataHelper = new DataAccessHelper("Orders_UpdateCheckoutResponse", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run();    // 105 / 102

                DatabaseResponse response = new DatabaseResponse();

                response = new DatabaseResponse { ResponseCode = result };              

                return response;
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Updates the check out receipt.
        /// </summary>
        /// <param name="transactionModel">The transaction model.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdateCheckOutReceipt(TransactionResponseModel transactionModel)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter("@MPGSOrderID",  SqlDbType.NVarChar ),
                     new SqlParameter("@TransactionID",  SqlDbType.NVarChar ),
                     new SqlParameter("@PaymentRequest",  SqlDbType.NVarChar ),
                     new SqlParameter("@PaymentResponse",  SqlDbType.NVarChar ),
                     new SqlParameter("@Amount",  SqlDbType.Float ),
                     new SqlParameter("@MaskedCardNumber",  SqlDbType.NVarChar ),
                     new SqlParameter("@CardFundMethod",  SqlDbType.NVarChar ),
                     new SqlParameter("@CardBrand",  SqlDbType.NVarChar ),
                     new SqlParameter("@CardType",  SqlDbType.NVarChar ),
                     new SqlParameter("@CardIssuer",  SqlDbType.NVarChar ),
                     new SqlParameter("@CardHolderName",  SqlDbType.NVarChar ),
                     new SqlParameter("@ExpiryYear",  SqlDbType.Int ),
                     new SqlParameter("@ExpiryMonth",  SqlDbType.Int ),
                     new SqlParameter("@Token",  SqlDbType.NVarChar ),
                     new SqlParameter("@PaymentStatus",  SqlDbType.NVarChar ),
                     new SqlParameter("@ApiResult",  SqlDbType.NVarChar ),
                      new SqlParameter("@GatewayCode",  SqlDbType.NVarChar ),
                     new SqlParameter("@CustomerIP",  SqlDbType.NVarChar ),
                     new SqlParameter("@PaymentMethodSubscription",  SqlDbType.Int ),                    
                };


                parameters[0].Value = transactionModel.OrderId;
                parameters[1].Value = transactionModel.TransactionID;
                parameters[2].Value = null; // revice
                parameters[3].Value = null;// revice
                parameters[4].Value = transactionModel.OrderAmount;
                parameters[5].Value = transactionModel.CardNumber;
                parameters[6].Value = transactionModel.CardFundMethod;
                parameters[7].Value = transactionModel.CardBrand;
                parameters[8].Value = transactionModel.CardType;
                parameters[9].Value = transactionModel.CardIssuer;
                parameters[10].Value = transactionModel.CardHolderName;
                parameters[11].Value = transactionModel.ExpiryYear;
                parameters[12].Value = transactionModel.ExpiryMonth;
                parameters[13].Value = transactionModel.Token;
                parameters[14].Value = transactionModel.PaymentStatus;
                parameters[15].Value = transactionModel.ApiResult;
                parameters[16].Value = transactionModel.GatewayCode;
                parameters[17].Value = transactionModel.CustomerIP;
                parameters[18].Value = 0; // revice

                _DataHelper = new DataAccessHelper("Orders_ProcessPayment", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run();    // 105 / 102

                DatabaseResponse response = new DatabaseResponse();

                response = new DatabaseResponse { ResponseCode = result };

                return response;
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Removes the additional line.
        /// </summary>
        /// <param name="removeRequest">The remove request.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> RemoveAdditionalLine(RemoveAdditionalLineRequest removeRequest)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                     new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar )

                };

                parameters[0].Value = removeRequest.OrderID;
                parameters[1].Value = removeRequest.MobileNumber;

                _DataHelper = new DataAccessHelper("Orders_RemoveAdditionalSubscriber", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run();    // 104 /103/ 109 

                DatabaseResponse response = new DatabaseResponse();

                response = new DatabaseResponse { ResponseCode = result };

                return response;
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }

        }

        /// <summary>
        /// Assigns the new number.
        /// </summary>
        /// <param name="newNumberRequest">The new number request.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> AssignNewNumber(AssignNewNumber newNumberRequest)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                     new SqlParameter( "@OldMobileNumber",  SqlDbType.NVarChar ),
                      new SqlParameter( "@NewMobileNumber",  SqlDbType.NVarChar )

                };

                parameters[0].Value = newNumberRequest.OrderID;
                parameters[1].Value = newNumberRequest.OldNumber;
                parameters[2].Value = newNumberRequest.NewNumber;

                _DataHelper = new DataAccessHelper("Orders_UpdateSubscriberNumber ", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run();    // 104 /103/ 109 

                DatabaseResponse response = new DatabaseResponse();

                response = new DatabaseResponse { ResponseCode = result };

                return response;
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }

        }

        /// <summary>
        /// Gets the customer BSS account number.
        /// </summary>
        /// <param name="CustomerId">The customer identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetCustomerBSSAccountNumber(int CustomerId)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int )                   
                };

                parameters[0].Value = CustomerId;               

                _DataHelper = new DataAccessHelper("Order_GetBSSAccountNumberByCustomerId ", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run(dt);    // 105 /102

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {
                    BSSAccount account = new BSSAccount();

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        account = (from model in dt.AsEnumerable()
                                    select new BSSAccount()
                                    {
                                         AccountNumber = model.Field<string>("AccountName"),
                                    }).FirstOrDefault();
                    }


                    response = new DatabaseResponse { ResponseCode = result, Results = account };
                }
                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                } 

                return response;
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }

        }

    }
}
