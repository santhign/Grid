using OrderService.Models;
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
using OrderService.Enums;
using Newtonsoft.Json;
using OrderService.Models.Transaction;

namespace OrderService.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public class OrderDataAccess
    {
        /// <summary>
        /// The data helper
        /// </summary>
        internal DataAccessHelper _DataHelper = null;
        /// <summary>
        /// The message queue data access
        /// </summary>
        private readonly IMessageQueueDataAccess _messageQueueDataAccess;
        /// <summary>
        /// The configuration
        /// </summary>
        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="messageQueueDataAccess">The message queue data access.</param>
        public OrderDataAccess(IConfiguration configuration, IMessageQueueDataAccess messageQueueDataAccess)
        {
            _configuration = configuration;
            _messageQueueDataAccess = messageQueueDataAccess;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderDataAccess"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public OrderDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
            _messageQueueDataAccess = new MessageQueueDataAccess(configuration);
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

                int result = await _DataHelper.RunAsync(dt);

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
        /// <param name="sessionId">The session identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> CreateSubscriber(CreateSubscriber subscriber, string sessionId)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@BundleID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@PromotionCode",  SqlDbType.NVarChar),
                    new SqlParameter( "@UserId",  SqlDbType.NVarChar)

                };

                parameters[0].Value = subscriber.OrderID;
                parameters[1].Value = subscriber.BundleID;
                parameters[2].Value = subscriber.MobileNumber;
                parameters[3].Value = subscriber.PromotionCode;
                parameters[4].Value = sessionId;

                _DataHelper = new DataAccessHelper("Order_CreateSubscriber", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync();    // 100 / 107 

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

                int result = await _DataHelper.RunAsync(ds); // 105 /109

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
        /// <param name="isNewSession">The is new session.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetBssApiRequestId(string source, string apiName, int customerId, int isNewSession, string mobileNumber)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@Source",  SqlDbType.NVarChar ),
                    new SqlParameter( "@APIName",  SqlDbType.NVarChar ),
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@IsNewSession",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar),
                };

                parameters[0].Value = source;
                parameters[1].Value = apiName;
                parameters[2].Value = customerId;
                parameters[3].Value = isNewSession;
                parameters[4].Value = mobileNumber;

                _DataHelper = new DataAccessHelper("Admin_GetRequestIDForBSSAPI", parameters, _configuration);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                BSSAssetRequest assetRequest = new BSSAssetRequest();

                DatabaseResponse response = new DatabaseResponse();

                if (dt.Rows.Count > 0)
                {
                    assetRequest = (from model in dt.AsEnumerable()
                                    select new BSSAssetRequest()
                                    {
                                        request_id = model.Field<string>("RequestID"),
                                        userid = model.Field<string>("UserID"),
                                        BSSCallLogID = model.Field<int>("BSSCallLogID"),

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

                int result = await _DataHelper.RunAsync(dt); // 102 /105

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

                int result = await _DataHelper.RunAsync(dt); // 111 /109

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

                int result = await _DataHelper.RunAsync();    // 101 / 102 

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

                int result = await _DataHelper.RunAsync();    // 101 / 109 

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

                int result = await _DataHelper.RunAsync(dt); // 102 /105

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

                int result = await _DataHelper.RunAsync(dt); // 102 /105

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

                int result = await _DataHelper.RunAsync(ds); // 105 /102

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
                                            IdentityCardType = model.Field<string>("IdentityCardType"),
                                            IdentityCardNumber = model.Field<string>("IdentityCardNumber"),
                                            BillingUnit = model.Field<string>("BillingUnit"),
                                            BillingFloor = model.Field<string>("BillingFloor"),
                                            BillingBuildingNumber = model.Field<string>("BillingBuildingNumber"),
                                            BillingBuildingName = model.Field<string>("BillingBuildingName"),
                                            BillingStreetName = model.Field<string>("BillingStreetName"),
                                            BillingPostCode = model.Field<string>("BillingPostCode"),
                                            BillingContactNumber = model.Field<string>("BillingContactNumber"),
                                            ReferralCode = model.Field<string>("ReferralCode"),
                                            PromotionCode = model.Field<string>("PromotionCode"),
                                            HaveDocuments = model.Field<bool>("HaveDocuments"),
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
                                            AlternateRecioientIDType = model.Field<string>("AlternateRecioientIDType"),
                                            AlternateRecioientIDNumber = model.Field<string>("AlternateRecioientIDNumber"),
                                            PortalSlotID = model.Field<string>("PortalSlotID"),
                                            SlotDate = model.Field<DateTime?>("SlotDate"),
                                            SlotFromTime = model.Field<TimeSpan?>("SlotFromTime"),
                                            SlotToTime = model.Field<TimeSpan?>("SlotToTime"),
                                            ScheduledDate = model.Field<DateTime?>("ScheduledDate"),
                                            ServiceFee = model.Field<double?>("ServiceFee"),
                                            RecieptNumber = model.Field<string>("RecieptNumber"),
                                        }).FirstOrDefault();

                        List<Bundle> orderBundles = new List<Bundle>();
                        foreach (DataRow dr in ds.Tables[1].Rows)
                        {
                            Bundle OrderBundle = new Bundle();
                            OrderBundle.OrderSubscriberID = Convert.ToInt32(dr["OrderSubscriberID"].ToString());
                            OrderBundle.BundleID = Convert.ToInt32(dr["BundleID"].ToString());
                            OrderBundle.DisplayName = dr["DisplayName"].ToString();
                            OrderBundle.MobileNumber = dr["MobileNumber"].ToString();
                            OrderBundle.IsPrimaryNumber = Convert.ToInt32(dr["IsPrimaryNumber"].ToString());
                            OrderBundle.PlanMarketingName = dr["PlanMarketingName"].ToString();
                            OrderBundle.PortalDescription = dr["PortalDescription"].ToString();
                            OrderBundle.PortalSummaryDescription = dr["PortalSummaryDescription"].ToString();
                            OrderBundle.TotalData = Convert.ToDouble(dr["TotalData"].ToString());
                            OrderBundle.TotalSMS = Convert.ToDouble(dr["TotalSMS"].ToString());
                            OrderBundle.TotalVoice = Convert.ToDouble(dr["TotalVoice"].ToString());
                            OrderBundle.ActualSubscriptionFee = Convert.ToDouble(dr["ActualSubscriptionFee"].ToString());
                            OrderBundle.ApplicableSubscriptionFee = Convert.ToDouble(dr["ApplicableSubscriptionFee"].ToString());
                            OrderBundle.ServiceName = dr["ServiceName"].ToString();
                            OrderBundle.ActualServiceFee = Convert.ToDouble(dr["ActualServiceFee"].ToString());
                            OrderBundle.ApplicableServiceFee = Convert.ToDouble(dr["ApplicableServiceFee"].ToString());
                            OrderBundle.PremiumType = Convert.ToInt32(dr["PremiumType"].ToString());
                            OrderBundle.IsPorted = Convert.ToInt32(dr["IsPorted"].ToString());
                            OrderBundle.IsOwnNumber = Convert.ToInt32(dr["IsOwnNumber"].ToString());
                            OrderBundle.DonorProvider = dr["DonorProvider"].ToString();
                            OrderBundle.PortedNumberTransferForm = dr["PortedNumberTransferForm"].ToString();
                            OrderBundle.PortedNumberOwnedBy = dr["PortedNumberOwnedBy"].ToString();
                            OrderBundle.PortedNumberOwnerRegistrationID = dr["PortedNumberOwnerRegistrationID"].ToString();
                            OrderBundle.PricingDescription = Convert.ToString(dr["PricingDescription"]);

                            List<ServiceCharge> subscriberServiceCharges = new List<ServiceCharge>();

                            if (ds.Tables[3] != null && ds.Tables[3].Rows.Count > 0)
                            {

                                subscriberServiceCharges = (from model in ds.Tables[3].AsEnumerable()
                                                            select new ServiceCharge()
                                                            {
                                                                OrderSubscriberID = model.Field<int>("OrderSubscriberID"),
                                                                PortalServiceName = model.Field<string>("PortalServiceName"),
                                                                ServiceFee = model.Field<double?>("ServiceFee"),
                                                                IsRecurring = model.Field<int>("IsRecurring"),
                                                                IsGSTIncluded = model.Field<int>("IsGSTIncluded"),
                                                            }).Where(c => c.OrderSubscriberID == Convert.ToInt32(dr["OrderSubscriberID"].ToString())).ToList();

                                OrderBundle.ServiceCharges = subscriberServiceCharges;

                            }
                            orderBundles.Add(OrderBundle);
                        }
                        orderDetails.Bundles = orderBundles;

                        List<ServiceCharge> orderServiceCharges = new List<ServiceCharge>();

                        if (ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                        {

                            orderServiceCharges = (from model in ds.Tables[2].AsEnumerable()
                                                   select new ServiceCharge()
                                                   {
                                                       PortalServiceName = model.Field<string>("PortalServiceName"),
                                                       ServiceFee = model.Field<double?>("ServiceFee"),
                                                       IsRecurring = model.Field<int>("IsRecurring"),
                                                       IsGSTIncluded = model.Field<int>("IsGSTIncluded"),
                                                   }).ToList();

                            orderDetails.ServiceCharges = orderServiceCharges;

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

                int result = await _DataHelper.RunAsync(dt); // 105 /102

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
        /// Gets the customer identifier from change request identifier.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetCustomerIdFromChangeRequestId(int orderId)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@ChangeRequestID",  SqlDbType.Int )

                };

                parameters[0].Value = orderId;

                _DataHelper = new DataAccessHelper("Order_GetCustomerIDByChangeRequestID", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 105 /102

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
        /// Gets the customer identifier from account invoice identifier.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetCustomerIdFromAccountInvoiceId(int orderId)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@AccountInvoiceID",  SqlDbType.Int )

                };

                parameters[0].Value = orderId;

                _DataHelper = new DataAccessHelper("Order_GetCustomerIDByAccountInvoiceID", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 105 /102

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
                _DataHelper = new DataAccessHelper("Orders_GetAvailableSlots", _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 105 /102

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
                    new SqlParameter( "@NameInNRIC",  SqlDbType.NVarChar),
                    new SqlParameter( "@DisplayName",  SqlDbType.NVarChar),
                    new SqlParameter( "@Gender",  SqlDbType.NVarChar),
                    new SqlParameter( "@DOB",  SqlDbType.Date),
                    new SqlParameter( "@ContactNumber",  SqlDbType.NVarChar)
                };

                parameters[0].Value = personalDetails.OrderID;
                parameters[1].Value = personalDetails.NameInNRIC;
                parameters[2].Value = personalDetails.DisplayName;
                parameters[3].Value = personalDetails.Gender;
                parameters[4].Value = personalDetails.DOB;
                parameters[5].Value = personalDetails.ContactNumber;

                _DataHelper = new DataAccessHelper("Orders_UpdateOrderBasicDetails_v2", parameters, _configuration);

                int result = await _DataHelper.RunAsync();    // 101 / 109 

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

                _DataHelper = new DataAccessHelper("Orders_UpdateOrderBillingDetails_v2", parameters, _configuration);

                int result = await _DataHelper.RunAsync();    // 101 / 109 

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

                int result = await _DataHelper.RunAsync();    // 101 / 109 

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

                int result = await _DataHelper.RunAsync();    // 101 / 109 

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

                int result = await _DataHelper.RunAsync();    // 105 / 102

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

                int result = await _DataHelper.RunAsync(dt);    // 105 / 109 
                DatabaseResponse response = new DatabaseResponse();
                if (result == 105)
                {
                    List<OrderedNumbers> numbers = new List<OrderedNumbers>();

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        numbers = (from model in dt.AsEnumerable()
                                   select new OrderedNumbers()
                                   {
                                       MobileNumber = model.Field<string>("MobileNumber"),
                                       IsDefault = model.Field<int>("IsDefault")

                                   }).ToList();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = numbers };
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
                    new SqlParameter( "@EmailMessage",  SqlDbType.Int),
                    new SqlParameter( "@SMSMessage",  SqlDbType.Int),
                    new SqlParameter( "@VoiceMessage",  SqlDbType.Int)

                };

                parameters[0].Value = subscriptionDetails.OrderID;
                parameters[1].Value = subscriptionDetails.ContactNumber;
                parameters[2].Value = subscriptionDetails.Terms;
                parameters[3].Value = subscriptionDetails.PaymentSubscription;
                parameters[4].Value = subscriptionDetails.EmailMessage;
                parameters[5].Value = subscriptionDetails.SMSMessage;
                parameters[6].Value = subscriptionDetails.VoiceMessage;



                _DataHelper = new DataAccessHelper("Orders_UpdateOrderSubscriptions", parameters, _configuration);

                int result = await _DataHelper.RunAsync();    // 101 / 109 

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

                int result = await _DataHelper.RunAsync(dt);    // 105 / 119 
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
                     new SqlParameter( "@TransactionID",  SqlDbType.NVarChar )

                };

                parameters[0].Value = checkOutRequest.Source;
                parameters[1].Value = checkOutRequest.SourceID;
                parameters[2].Value = checkOutRequest.MPGSOrderID;
                parameters[3].Value = checkOutRequest.CheckOutSessionID;
                parameters[4].Value = checkOutRequest.SuccessIndicator;
                parameters[5].Value = checkOutRequest.CheckoutVersion;
                parameters[6].Value = checkOutRequest.TransactionID;

                _DataHelper = new DataAccessHelper("Orders_GetCheckoutRequestDetails", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt);    // 105 / 102
                DatabaseResponse response = new DatabaseResponse();
                if (result == 105)
                {
                    Checkout checkOut = new Checkout();

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        checkOut = (from model in dt.AsEnumerable()
                                    select new Checkout()
                                    {
                                        Amount = model.Field<double>("Amount"),
                                        ReceiptNumber = model.Field<string>("RecieptNumber"),
                                        OrderId = model.Field<string>("MPGSOrderID"),
                                        OrderNumber = model.Field<string>("OrderNumber"),
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

                int result = await _DataHelper.RunAsync(dt);    // 106 / 101

                DatabaseResponse response = new DatabaseResponse();

                TokenSession tokenSession = new TokenSession();

                if (dt != null && dt.Rows.Count > 0)
                {

                    tokenSession = (from model in dt.AsEnumerable()
                                    select new TokenSession()
                                    {
                                        MPGSOrderID = model.Field<string>("MPGSOrderID"),
                                        CheckOutSessionID = model.Field<string>("CheckOutSessionID"),
                                        Amount = model.Field<double>("Amount")
                                    }).FirstOrDefault();

                    response = new DatabaseResponse { ResponseCode = result, Results = tokenSession };

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
        /// Updates the check out receipt.
        /// </summary>
        /// <param name="transactionModel">The transaction model.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdateCheckOutReceipt(TransactionResponseModel transactionModel)
        {
            DatabaseResponse response = new DatabaseResponse();
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

                _DataHelper = new DataAccessHelper("Orders_ProcessPayment_v2", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync();    // 105 / 102

                response = new DatabaseResponse { ResponseCode = result };

                return response;
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return response;
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

                _DataHelper = new DataAccessHelper("Orders_RemoveAdditionalSubscriber_v2", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync();    // 104 /103/ 109 

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

                int result = await _DataHelper.RunAsync();    // 104 /103/ 109 

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

                int result = await _DataHelper.RunAsync(dt);    // 105 /102

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

        /// <summary>
        /// Updates the BSS call numbers.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="userID">The user identifier.</param>
        /// <param name="callLogId">The call log identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdateBSSCallNumbers(string json, string userID, int callLogId)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter("@UserID",  SqlDbType.NVarChar ),
                     new SqlParameter("@Json",  SqlDbType.NVarChar ),
                     new SqlParameter("@BSSCallLogID",  SqlDbType.Int )
                };

                parameters[0].Value = userID;
                parameters[1].Value = json;
                parameters[2].Value = callLogId;

                _DataHelper = new DataAccessHelper("Orders_UpdateBSSCallNumbers", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync();    // 105 / 102

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
        /// Rolls the back order.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <param name="source">The rollback source.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> RollBackOrder(int orderId, string source)
        {
            try
            {
                LogInfo.Information("Order Rollback for " + orderId.ToString() + " - " + source);
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )

                };

                parameters[0].Value = orderId;

                _DataHelper = new DataAccessHelper("Orders_RollBackOldUnfinishedOrder", parameters, _configuration);


                int result = await _DataHelper.RunAsync(); // 103 /104

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
        /// Gets the BSS API request identifier and subscriber session.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="apiName">Name of the API.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetBssApiRequestIdAndSubscriberSession(string source, string apiName, int customerId, string mobileNumber)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@Source",  SqlDbType.NVarChar ),
                    new SqlParameter( "@APIName",  SqlDbType.NVarChar ),
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar),
                };

                parameters[0].Value = source;
                parameters[1].Value = apiName;
                parameters[2].Value = customerId;
                parameters[3].Value = mobileNumber;

                _DataHelper = new DataAccessHelper("Admin_GetBSSRequestIDAndSubscriberSession", parameters, _configuration);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                BSSAssetRequest assetRequest = new BSSAssetRequest();

                DatabaseResponse response = new DatabaseResponse();

                if (dt.Rows.Count > 0)
                {
                    assetRequest = (from model in dt.AsEnumerable()
                                    select new BSSAssetRequest()
                                    {
                                        request_id = model.Field<string>("RequestID"),
                                        userid = model.Field<string>("UserID"),
                                        BSSCallLogID = model.Field<int>("BSSCallLogID"),

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
        /// Gets the order nric details.
        /// </summary>
        /// <param name="OrderID">The order identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetOrderNRICDetails(int OrderID)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int)
                };

                parameters[0].Value = OrderID;

                _DataHelper = new DataAccessHelper("Order_GetCustomerNRICDetails", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt);

                DatabaseResponse response = new DatabaseResponse();

                OrderNRICDetails nRICDetails = new OrderNRICDetails();

                if (dt != null && dt.Rows.Count > 0)
                {
                    nRICDetails = (from model in dt.AsEnumerable()
                                   select new OrderNRICDetails()
                                   {
                                       CustomerID = model.Field<int>("CustomerID"),
                                       DocumentID = model.Field<int>("DocumentID"),
                                       DocumentURL = model.Field<string>("DocumentURL"),
                                       DocumentBackURL = model.Field<string>("DocumentBackURL"),
                                       IdentityCardNumber = model.Field<string>("IdentityCardNumber"),
                                       IdentityCardType = model.Field<string>("IdentityCardType"),
                                       Nationality = model.Field<string>("Nationality"),
                                   }).FirstOrDefault();
                }

                response = new DatabaseResponse { ResponseCode = result, Results = nRICDetails };

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
        /// Updates the order personal identifier details.
        /// </summary>
        /// <param name="personalDetails">The personal details.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdateOrderPersonalIdDetails(UpdateOrderPersonalDetails personalDetails)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@Nationality",  SqlDbType.NVarChar ),
                    new SqlParameter( "@IDType",  SqlDbType.NVarChar ),
                    new SqlParameter( "@IDNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@IDImageUrl",  SqlDbType.NVarChar),
                    new SqlParameter( "@IDImageUrlBack",  SqlDbType.NVarChar),

                };

                parameters[0].Value = personalDetails.OrderID;
                parameters[1].Value = personalDetails.Nationality;
                parameters[2].Value = personalDetails.IDType;
                parameters[3].Value = personalDetails.IDNumber;
                parameters[4].Value = personalDetails.IDFrontImageUrl;
                parameters[5].Value = personalDetails.IDBackImageUrl;

                _DataHelper = new DataAccessHelper("Orders_UpdateOrderPersonalIDDetails", parameters, _configuration);
                DataTable dt = new DataTable("dt");
                int result = await _DataHelper.RunAsync(dt); // 101 / 109 
                IDResponse _response = new IDResponse();  
                if (result == 101)
                {
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        _response = (from model in dt.AsEnumerable()
                                    select new IDResponse()
                                    {
                                        IDNumber = model.Field<string>("IdentityCardNumber"),
                                        IDFrontImageUrl = model.Field<string>("DocumentURL"),
                                        IDBackImageUrl = model.Field<string>("DocumentBackURL"),
                                    }).FirstOrDefault();

                    }
                }
                return new DatabaseResponse { ResponseCode = result, Results = _response};
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
        /// Gets the change card checkout request details.
        /// </summary>
        /// <param name="checkOutRequest">The check out request.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetChangeCardCheckoutRequestDetails(CheckOutRequestDBUpdateModel checkOutRequest)
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
                     new SqlParameter( "@TransactionID",  SqlDbType.NVarChar )
                };

                parameters[0].Value = checkOutRequest.Source;
                parameters[1].Value = checkOutRequest.SourceID;
                parameters[2].Value = checkOutRequest.MPGSOrderID;
                parameters[3].Value = checkOutRequest.CheckOutSessionID;
                parameters[4].Value = checkOutRequest.SuccessIndicator;
                parameters[5].Value = checkOutRequest.CheckoutVersion;
                parameters[6].Value = checkOutRequest.TransactionID;

                _DataHelper = new DataAccessHelper("Orders_GetChangeCardCheckoutRequestDetails", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt);    // 105 / 102
                DatabaseResponse response = new DatabaseResponse();
                if (result == 105)
                {
                    Checkout checkOut = new Checkout();

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        checkOut = (from model in dt.AsEnumerable()
                                    select new Checkout()
                                    {
                                        Amount = model.Field<double>("Amount"),
                                        ReceiptNumber = model.Field<string>("RecieptNumber"),
                                        OrderId = model.Field<string>("MPGSOrderID"),
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
        /// Gets the source type by MPGSS order identifier.
        /// </summary>
        /// <param name="MPGSOrderID">The MPGS order identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetSourceTypeByMPGSSOrderId(string MPGSOrderID)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@MPGSOrderID",  SqlDbType.NVarChar)
                };

                parameters[0].Value = MPGSOrderID;

                _DataHelper = new DataAccessHelper("Orders_GetSourceTypeByMpgsOrderId", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); //105/102

                DatabaseResponse response = new DatabaseResponse();

                OrderSource orderSource = new OrderSource();

                if (dt != null && dt.Rows.Count > 0)
                {
                    orderSource = (from model in dt.AsEnumerable()
                                   select new OrderSource()
                                   {
                                       SourceType = model.Field<string>("SourceType"),
                                       SourceID = model.Field<int>("SourceID")
                                   }).FirstOrDefault();
                }

                response = new DatabaseResponse { ResponseCode = result, Results = orderSource };

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
        /// Gets the customer order count.
        /// </summary>
        /// <param name="CustomerID">The customer identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetCustomerOrderCount(int CustomerID)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int)
                };

                parameters[0].Value = CustomerID;

                _DataHelper = new DataAccessHelper("Orders_GetCustomerOrderCount", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); //105/102

                DatabaseResponse response = new DatabaseResponse();

                OrderCount orderCount = new OrderCount();

                if (dt != null && dt.Rows.Count > 0)
                {
                    orderCount = (from model in dt.AsEnumerable()
                                  select new OrderCount()
                                  {
                                      SuccessfulOrders = model.Field<int>("OrderCount"),

                                  }).FirstOrDefault();
                }

                response = new DatabaseResponse { ResponseCode = result, Results = orderCount };

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
        /// Updates the MPGS create token session details.
        /// </summary>
        /// <param name="tokenResponse">The token response.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdateMPGSCreateTokenSessionDetails(CreateTokenResponse tokenResponse)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter( "@MPGSOrderID",  SqlDbType.NVarChar ),
                     new SqlParameter( "@CheckOutSessionID",  SqlDbType.NVarChar ),
                     new SqlParameter( "@SuccessIndicator",  SqlDbType.NVarChar ),
                     new SqlParameter( "@CheckoutVersion",  SqlDbType.NVarChar ),
                     new SqlParameter( "@TransactionID",  SqlDbType.NVarChar )
                };

                parameters[0].Value = tokenResponse.MPGSOrderID;
                parameters[1].Value = tokenResponse.MPGSResponse.session.id;
                parameters[2].Value = tokenResponse.MPGSResponse.session.updateStatus;
                parameters[3].Value = tokenResponse.MPGSResponse.session.version;
                parameters[4].Value = tokenResponse.TransactionID;

                _DataHelper = new DataAccessHelper("Orders_UpdateMPGSCreateTokenSessionDetails", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt);    // 105 / 102

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {
                    CreateTokenUpdatedDetails detailsToTokenize = new CreateTokenUpdatedDetails();

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        detailsToTokenize = (from model in dt.AsEnumerable()
                                             select new CreateTokenUpdatedDetails()
                                             {
                                                 MPGSOrderID = model.Field<string>("MPGSOrderID"),
                                                 TransactionID = model.Field<string>("TokenizeTransactionID"),
                                                 Amount = model.Field<double>("Amount"),
                                                 SessionID = model.Field<string>("CheckOutSessionID"),
                                                 OrderID = model.Field<int>("OrderID"),
                                                 CustomerID = model.Field<int>("OrderID")

                                             }).FirstOrDefault();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = detailsToTokenize };
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
        /// Updates the subscriber details.
        /// </summary>
        /// <param name="subscriberBasicDetails">The subscriber basic details.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdateSubscriberDetails(UpdateSubscriberBasicDetails subscriberBasicDetails)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter( "@OrderID",  SqlDbType.Int ),
                     new SqlParameter( "@BundleID",  SqlDbType.Int ),
                     new SqlParameter( "@DisplayName",  SqlDbType.NVarChar ),
                     new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar ),
                };

                parameters[0].Value = subscriberBasicDetails.OrderID;
                parameters[1].Value = subscriberBasicDetails.BundleID;
                parameters[2].Value = subscriberBasicDetails.DisplayName;
                parameters[3].Value = subscriberBasicDetails.MobileNumber;

                _DataHelper = new DataAccessHelper("Order_UpdateSubscriberBasicDetails_v2", parameters, _configuration);


                int result = await _DataHelper.RunAsync();    // 101 / 106/ 127/102

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
        /// Gets the reschedule available slots.
        /// </summary>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetRescheduleAvailableSlots()
        {
            try
            {
                _DataHelper = new DataAccessHelper("Orders_Reschedule_GetAvailableSlots", _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 105 /102

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
        /// Removes the loa details.
        /// </summary>
        /// <param name="OrderID">The order identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> RemoveLOADetails(int OrderID)
        {
            try
            {
                SqlParameter[] parameters =
                 {
                     new SqlParameter( "@OrderID",  SqlDbType.Int ),
                };

                parameters[0].Value = OrderID;

                _DataHelper = new DataAccessHelper("Orders_RemoveLOADetails", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); //101/ 106 /102

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
        /// Gets the port type from order identifier.
        /// </summary>
        /// <param name="OrderID">The order identifier.</param>
        /// <param name="MobileNumber">The mobile number.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetPortTypeFromOrderId(int OrderID, string MobileNumber)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar )
                };

                parameters[0].Value = OrderID;
                parameters[1].Value = MobileNumber;

                _DataHelper = new DataAccessHelper("Order_GetPortTypeByOrderID", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 105 /102

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {
                    response = new DatabaseResponse { ResponseCode = result, Results = dt.Rows[0][0].ToString().Trim() };
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
        /// Cancels the order.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="orderId">The order identifier.</param>
        /// <returns></returns>
        public async Task<int> CancelOrder(int customerId, int orderId)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                     new SqlParameter( "@OrderId",  SqlDbType.Int )
                };

                parameters[0].Value = customerId;
                parameters[1].Value = orderId;

                _DataHelper = new DataAccessHelper(DbObjectNames.Orders_CancelOrder, parameters, _configuration);

                return await _DataHelper.RunAsync();    // 105 / 119 

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
        /// Creates the payment method.
        /// </summary>
        /// <param name="tokenResponse">The token response.</param>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="MPGSOrderID">The MPGS order identifier.</param>
        /// <param name="SourceMethodName">Name of the source method.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> CreatePaymentMethod(TokenResponse tokenResponse, int customerID, string MPGSOrderID = "", string SourceMethodName = "")
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter("@CustomerID",  SqlDbType.Int ),
                     new SqlParameter("@MaskedCardNumer",  SqlDbType.NVarChar ),
                     new SqlParameter("@SourceType",  SqlDbType.NVarChar ),
                     new SqlParameter("@Token",  SqlDbType.NVarChar ),
                     new SqlParameter("@CardType",  SqlDbType.NVarChar ),
                     new SqlParameter("@ExpiryMonth",  SqlDbType.Int ),
                     new SqlParameter("@ExpiryYear",  SqlDbType.Int ),
                     new SqlParameter("@CardFundMethod",  SqlDbType.NVarChar ),
                     new SqlParameter("@CardBrand",  SqlDbType.NVarChar )
                };


                parameters[0].Value = customerID;
                parameters[1].Value = tokenResponse.Card.number;
                parameters[2].Value = tokenResponse.Type;
                parameters[3].Value = tokenResponse.Token;
                parameters[4].Value = tokenResponse.Card.scheme;
                parameters[5].Value = tokenResponse.Card.expiry.month;
                parameters[6].Value = tokenResponse.Card.expiry.year;
                parameters[7].Value = tokenResponse.Card.fundingMethod;
                parameters[8].Value = tokenResponse.Card.brand;


                _DataHelper = new DataAccessHelper("Order_CreateCustomerPaymentMethods", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync();    // 100 / 105

                DatabaseResponse response = new DatabaseResponse();

                response = new DatabaseResponse { ResponseCode = result };

                if (!string.IsNullOrWhiteSpace(MPGSOrderID))
                {
                    string topicName = string.Empty;
                    string pushResult = string.Empty;
                    Dictionary<string, string> attribute = new Dictionary<string, string>();
                    ProfileMQ msgBody = new ProfileMQ();
                    try
                    {
                        topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _configuration).Results.ToString().Trim();
                        attribute.Add(EventTypeString.EventType, RequestType.EditPaymentMethod.GetDescription());

                        var sourceTyeResponse = await GetSourceTypeByMPGSSOrderId(MPGSOrderID);
                        DatabaseResponse OrderCountResponse = await GetCustomerOrderCount(customerID);
                        if (((((OrderSource)sourceTyeResponse.Results).SourceType == CheckOutType.Orders.ToString()) && (((OrderCount)OrderCountResponse.Results).SuccessfulOrders >= 1)) || (((OrderSource)sourceTyeResponse.Results).SourceType != CheckOutType.Orders.ToString()))
                        {
                            msgBody = await _messageQueueDataAccess.GetProfileUpdateMessageBody(customerID);
                            pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, msgBody, attribute);

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
                                    MessageAttribute = RequestType.EditPaymentMethod.GetDescription(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
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
                                    MessageAttribute = RequestType.EditPaymentMethod.GetDescription(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 0
                                };
                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                        }
                        else
                        {
                            LogInfo.Information(MPGSOrderID.ToString() + " ID is not having Order as Source Type or it may the first order");
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
                            MessageAttribute = Core.Enums.RequestType.EditPaymentMethod.GetDescription().ToString(),
                            MessageBody = msgBody != null ? JsonConvert.SerializeObject(msgBody) : null,
                            Status = 0,
                            Remark = "Error Occured in " + SourceMethodName + " method while generating message",
                            Exception = new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical)


                        };

                        await _messageQueueDataAccess.InsertMessageInMessageQueueRequestException(queueRequest);
                    }

                    //End Message
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
        /// Updates the payment method details.
        /// </summary>
        /// <param name="transactionModel">The transaction model.</param>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdatePaymentMethodDetails(TransactionResponseModel transactionModel, int customerID, string token)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter("@CustomerID",  SqlDbType.Int ),
                     new SqlParameter("@MaskedCardNumer",  SqlDbType.NVarChar ),
                     new SqlParameter("@Token",  SqlDbType.NVarChar ),
                     new SqlParameter("@CardHolderName",  SqlDbType.NVarChar ),
                     new SqlParameter("@CardIssuer",  SqlDbType.NVarChar )
                };

                parameters[0].Value = customerID;
                parameters[1].Value = transactionModel.CardNumber;
                parameters[2].Value = token;
                parameters[3].Value = transactionModel.CardHolderName;
                parameters[4].Value = transactionModel.CardIssuer;

                _DataHelper = new DataAccessHelper("Order_UpdateCustomerPaymentMethodDetails", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync();    // 101 / 105

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
        /// Gets the payment method token.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetPaymentMethodToken(int customerID)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter( "@CustomerID",  SqlDbType.Int )
                };

                parameters[0].Value = customerID;

                _DataHelper = new DataAccessHelper("Order_GetCustomerPaymentToken", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt);    // 105 / 102

                DatabaseResponse response = new DatabaseResponse();

                PaymentMethod paymentMethod = new PaymentMethod();

                if (dt != null && dt.Rows.Count > 0)
                {

                    paymentMethod = (from model in dt.AsEnumerable()
                                     select new PaymentMethod()
                                     {
                                         PaymentMethodID = model.Field<int>("PaymentMethodID"),
                                         Token = model.Field<string>("Token"),
                                         SourceType = model.Field<string>("SourceType"),
                                         CardHolderName = model.Field<string>("CardHolderName"),
                                         CardType = model.Field<string>("CardType"),
                                     }).FirstOrDefault();

                    response = new DatabaseResponse { ResponseCode = result, Results = paymentMethod };

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
        /// Gets the payment method token by identifier.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="paymentMethodID">The payment method identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetPaymentMethodTokenById(int customerID, int paymentMethodID)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                       new SqlParameter( "@PaymentMethodID",  SqlDbType.Int ),

                };

                parameters[0].Value = customerID;
                parameters[1].Value = paymentMethodID;


                _DataHelper = new DataAccessHelper("Order_GetPaymentTokenByID", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt);    // 105 / 102

                DatabaseResponse response = new DatabaseResponse();

                PaymentMethod paymentMethod = new PaymentMethod();

                if (dt != null && dt.Rows.Count > 0)
                {

                    paymentMethod = (from model in dt.AsEnumerable()
                                     select new PaymentMethod()
                                     {
                                         PaymentMethodID = model.Field<int>("PaymentMethodID"),
                                         Token = model.Field<string>("Token"),
                                         SourceType = model.Field<string>("SourceType"),
                                         CardHolderName = model.Field<string>("CardHolderName"),
                                     }).FirstOrDefault();

                    response = new DatabaseResponse { ResponseCode = result, Results = paymentMethod };

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
        /// Removes the payment method.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="paymentMethodID">The payment method identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> RemovePaymentMethod(int customerID, int paymentMethodID)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                     new SqlParameter( "@PaymentMethodID",  SqlDbType.NVarChar )
                };

                parameters[0].Value = customerID;

                parameters[1].Value = paymentMethodID;

                _DataHelper = new DataAccessHelper("Order_RemovePaymentTokenByID", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt);    // 103 / 102

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
        /// Reschedules the delivery.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="detailsrequest">The detailsrequest.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> RescheduleDelivery(int customerID, OrderRescheduleDeliveryRequest detailsrequest)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter("@CustomerID",  SqlDbType.Int ),
                     new SqlParameter("@OrderID",  SqlDbType.Int ),
                     new SqlParameter("@ShippingContactNumber",  SqlDbType.NVarChar ),
                     new SqlParameter("@ShippingFloor",  SqlDbType.NVarChar ),
                     new SqlParameter("@ShippingUnit",  SqlDbType.NVarChar ),
                     new SqlParameter("@ShippingBuildingName",  SqlDbType.NVarChar ),
                     new SqlParameter("@ShippingBuildingNumber",  SqlDbType.NVarChar ),
                     new SqlParameter("@ShippingStreetName",  SqlDbType.NVarChar ),
                     new SqlParameter("@ShippingPostCode",  SqlDbType.NVarChar ),
                     new SqlParameter("@AlternateRecipientName",  SqlDbType.NVarChar ),
                     new SqlParameter("@AlternateRecipientEmail",  SqlDbType.NVarChar ),
                     new SqlParameter("@AlternateRecipientContact",  SqlDbType.NVarChar ),
                     new SqlParameter("@AlternateRecipientIDNumber",  SqlDbType.NVarChar ),
                     new SqlParameter("@AlternateRecipientIDType",  SqlDbType.NVarChar ),
                     new SqlParameter("@PortalSlotID",  SqlDbType.NVarChar ),
                     new SqlParameter("@ScheduledDate",  SqlDbType.Date ),
                     new SqlParameter("@OrderType",  SqlDbType.Int )
                };


                parameters[0].Value = customerID;
                parameters[1].Value = detailsrequest.OrderID;
                parameters[2].Value = detailsrequest.ShippingContactNumber;
                parameters[3].Value = detailsrequest.ShippingFloor;
                parameters[4].Value = detailsrequest.ShippingUnit;
                parameters[5].Value = detailsrequest.ShippingBuildingName;
                parameters[6].Value = detailsrequest.ShippingBuildingNumber;
                parameters[7].Value = detailsrequest.ShippingStreetName;
                parameters[8].Value = detailsrequest.ShippingPostCode;
                parameters[9].Value = detailsrequest.AlternateRecipientName;
                parameters[10].Value = detailsrequest.AlternateRecipientEmail;
                parameters[11].Value = detailsrequest.AlternateRecipientContact;
                parameters[12].Value = detailsrequest.AlternateRecioientIDNumber;
                parameters[13].Value = detailsrequest.AlternateRecioientIDType;
                parameters[14].Value = detailsrequest.PortalSlotID;
                parameters[15].Value = DBNull.Value;
                parameters[16].Value = detailsrequest.OrderType;




                _DataHelper = new DataAccessHelper(DbObjectNames.Orders_RescheduleDelivery, parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt);    // 100 / 105

                if (result != (int)Core.Enums.DbReturnValue.CreateSuccess)
                    return new DatabaseResponse { ResponseCode = result };

                var rescheduleDeliveryResponse = new Order_RescheduleDeliveryResponse();

                if (dt.Rows.Count > 0)
                {
                    rescheduleDeliveryResponse = (from model in dt.AsEnumerable()
                                                  select new Order_RescheduleDeliveryResponse()
                                                  {
                                                      AccountInvoiceID = model.Field<int>("AccountInvoiceID"),
                                                      PayableAmount = model.Field<double?>("PayableAmount"),

                                                  }).FirstOrDefault();
                }
                else
                {
                    rescheduleDeliveryResponse = null;
                }

                var response = new DatabaseResponse { ResponseCode = result, Results = rescheduleDeliveryResponse };
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
        /// Processes the reschedule delivery.
        /// </summary>
        /// <param name="accountInvoiceID">The account invoice identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> ProcessRescheduleDelivery(int accountInvoiceID)
        {
            try
            {
                SqlParameter[] parameters =
                {

                     new SqlParameter("@AccountInvoiceID",  SqlDbType.Int )

                };


                parameters[0].Value = accountInvoiceID;


                _DataHelper = new DataAccessHelper(DbObjectNames.Orders_ProcessRescheduleDelivery, parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt);    // 100 / 105

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
        /// Orders the buddy check.
        /// </summary>
        /// <param name="orderID">The order identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> OrderBuddyCheck(int orderID)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter( "@OrderID",  SqlDbType.Int )

                };

                parameters[0].Value = orderID;

                _DataHelper = new DataAccessHelper("Orders_RequireBuddy", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt);    // 105 / 102

                DatabaseResponse response = new DatabaseResponse();

                List<BuddyCheckList> buddyCheckList = new List<BuddyCheckList>();

                if (dt != null && dt.Rows.Count > 0)
                {
                    buddyCheckList = (from model in dt.AsEnumerable()
                                      select new BuddyCheckList()
                                      {
                                          CustomerID = model.Field<int>("CustomerID"),
                                          OrderID = orderID,
                                          OrderSubscriberID = model.Field<int>("OrderSubscriberID"),
                                          MobileNumber = model.Field<string>("MobileNumber"),
                                          HasBuddyPromotion = model.Field<int>("HasBuddyPromotion")
                                      }).Where(cl => cl.HasBuddyPromotion == 1).ToList();

                    response = new DatabaseResponse { ResponseCode = result, Results = buddyCheckList };

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
        /// Checks the payment is processed.
        /// </summary>
        /// <param name="MPGSOrderID">The MPGS order identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> CheckPaymentIsProcessed(string MPGSOrderID)
        {
            try
            {
                SqlParameter[] parameters =
                {
                     new SqlParameter( "@MPGSOrderID",  SqlDbType.NVarChar )

                };

                parameters[0].Value = MPGSOrderID;

                _DataHelper = new DataAccessHelper("Orders_IsPaymentProcessed", parameters, _configuration);

                int result = await _DataHelper.RunAsync();    // 126 / 135

                DatabaseResponse response = new DatabaseResponse();

                List<BuddyCheckList> buddyCheckList = new List<BuddyCheckList>();

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
        /// Processes the buddy plan.
        /// </summary>
        /// <param name="updateRequest">The update request.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> ProcessBuddyPlan(BuddyNumberUpdate updateRequest)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter( "@OrderSubscriberID",  SqlDbType.Int ),
                     new SqlParameter( "@UserId",  SqlDbType.NVarChar ),
                      new SqlParameter( "@NewMobileNumber",  SqlDbType.NVarChar ),
                };

                parameters[0].Value = updateRequest.OrderSubscriberID;
                parameters[1].Value = updateRequest.UserId;
                parameters[2].Value = updateRequest.NewMobileNumber;

                _DataHelper = new DataAccessHelper("Orders_ProcessBuddyPlan", parameters, _configuration);

                int result = await _DataHelper.RunAsync();    // 107 / 100/119                

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
        /// Creates the pending buddy list.
        /// </summary>
        /// <param name="updateRequest">The update request.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> CreatePendingBuddyList(BuddyCheckList updateRequest)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter( "@OrderID",  SqlDbType.Int ),
                     new SqlParameter( "@OrderSubscriberID",  SqlDbType.Int ),
                     new SqlParameter( "@NewMobileNumber",  SqlDbType.NVarChar ),
                     new SqlParameter( "@IsProcessed",  SqlDbType.Bit )
                };

                parameters[0].Value = updateRequest.OrderID;
                parameters[1].Value = updateRequest.OrderSubscriberID;
                parameters[2].Value = updateRequest.MobileNumber;
                parameters[3].Value = updateRequest.IsProcessed;

                _DataHelper = new DataAccessHelper("Orders_CreatePendingBuddyList", parameters, _configuration);

                int result = await _DataHelper.RunAsync();    // 107 / 100               

                return new DatabaseResponse { ResponseCode = result }; ;
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
        /// Gets the account identifier from customer identifier.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetAccountIdFromCustomerId(int customerId)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int )

                };

                parameters[0].Value = customerId;

                _DataHelper = new DataAccessHelper("Order_GetAccountIDFromCustomerID", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 105 /102

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    BSSAccount account = new BSSAccount();

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        account = (from model in dt.AsEnumerable()
                                   select new BSSAccount()
                                   {
                                       AccountID = model.Field<int>("AccountID"),

                                   }).FirstOrDefault();

                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = account.AccountID };
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
        /// Creates the account invoice.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> CreateAccountInvoice(AccountInvoice request)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter( "@AccountID",  SqlDbType.Int ),
                     new SqlParameter( "@InvoiceName",  SqlDbType.NVarChar ),
                     new SqlParameter( "@InvoiceUrl",  SqlDbType.NVarChar ),
                     new SqlParameter( "@FinalAmount",  SqlDbType.Float ),
                     new SqlParameter( "@Remarks",  SqlDbType.NVarChar ),
                     new SqlParameter( "@OrderStatus",  SqlDbType.Int ),
                     new SqlParameter( "@PaymentSourceID",  SqlDbType.Int ),
                     new SqlParameter( "@CreatedBy",  SqlDbType.Int ),
                     new SqlParameter( "@BSSBillId",  SqlDbType.NVarChar )
                };

                parameters[0].Value = request.AccountID;
                parameters[1].Value = request.InvoiceName;
                parameters[2].Value = request.InvoiceUrl;
                parameters[3].Value = request.FinalAmount;
                parameters[4].Value = request.Remarks;
                parameters[5].Value = request.OrderStatus;
                parameters[6].Value = request.PaymentSourceID;
                parameters[7].Value = request.CreatedBy;
                parameters[8].Value = request.BSSBillId;
                _DataHelper = new DataAccessHelper("Orders_CreateAccountInvoice", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt);    // 100 / 107

                DatabaseResponse response = new DatabaseResponse();

                if (result == 100)
                {

                    int orderID = 0;

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        orderID = int.Parse(dt.Rows[0].ItemArray[0].ToString());

                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = orderID };
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



        //public async Task<CustomerDetails> GetCustomerDetailByOrder(int customerID)
        //{
        //    try
        //    {
        //        SqlParameter[] parameters =               {

        //             new SqlParameter( "@CustomerID",  SqlDbType.Int )
        //        };

        //        parameters[0].Value = customerID;


        //        _DataHelper = new DataAccessHelper("Order_CheckForOrderCount", parameters, _configuration);



        //        int result = await _DataHelper.RunAsync();    // 105 / 102

        //        DatabaseResponse response = new DatabaseResponse();



        //        return customer;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

        //        throw (ex);
        //    }
        //    finally
        //    {
        //        _DataHelper.Dispose();
        //    }
        //}

        /// <summary>
        /// Removes the reschedule loa details.
        /// </summary>
        /// <param name="RescheduleDeliveryInformationID">The reschedule delivery information identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> RemoveRescheduleLOADetails(int RescheduleDeliveryInformationID)
        {
            try
            {
                SqlParameter[] parameters =
                 {
                     new SqlParameter( "@RescheduleDeliveryInformationID",  SqlDbType.Int )

                };

                parameters[0].Value = RescheduleDeliveryInformationID;

                _DataHelper = new DataAccessHelper("Orders_RemoveLOADetailsRescheduleDelivery", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); //101/ 106 /102

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
        /// Numbers the is ported.
        /// </summary>
        /// <param name="OrderID">The order identifier.</param>
        /// <param name="MobileNumber">The mobile number.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> NumberIsPorted(int OrderID, string MobileNumber)
        {
            try
            {
                SqlParameter[] parameters =
                 {
                     new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar ),
                     new SqlParameter( "@OrderID",  SqlDbType.Int )

                };

                parameters[0].Value = MobileNumber;
                parameters[1].Value = OrderID;

                _DataHelper = new DataAccessHelper("Orders_IsPortedNumber", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 105 /102

                DatabaseResponse response = new DatabaseResponse();

                int isPorted = 0;

                if (dt != null && dt.Rows.Count > 0)
                {
                    isPorted = int.Parse(dt.Rows[0].ItemArray[0].ToString());

                    response = new DatabaseResponse { ResponseCode = result, Results = isPorted };
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

        public async Task<DatabaseResponse> UpdatePaymentResponse(string MPGSOrderID, string PaymentResponse)
        {
            try
            {
                SqlParameter[] parameters =
                 {
                     new SqlParameter( "@MPGSOrderID",  SqlDbType.NVarChar ),
                     new SqlParameter( "@TransactionResponse",  SqlDbType.NVarChar )

                };

                parameters[0].Value = MPGSOrderID;

                parameters[1].Value = PaymentResponse;

                _DataHelper = new DataAccessHelper("Orders_UpdatePaymentTransactionResponse", parameters, _configuration);

                int result = await _DataHelper.RunAsync(); // 101

                DatabaseResponse response = new DatabaseResponse();


                return response = new DatabaseResponse { ResponseCode = result };

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

        public async Task<DatabaseResponse> CheckRescheduleDeliveryCharges(int AccountInvoiceID)
        {
            try
            {
                SqlParameter[] parameters =
                 {
                     new SqlParameter( "@AccountInvoiceID",  SqlDbType.Int )

                };

                parameters[0].Value = AccountInvoiceID;


                _DataHelper = new DataAccessHelper("Orders_CheckRescheduleDeliveryCharges", parameters, _configuration);

                //DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(); // 105 /102

                DatabaseResponse response = new DatabaseResponse();

                response.ResponseCode = result;


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

        public async Task<DatabaseResponse> GetChangeRequestTypeFromID(int changeRequestID)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@ChangeRequestID",  SqlDbType.Int )

                };

                parameters[0].Value = changeRequestID;

                _DataHelper = new DataAccessHelper("Order_GetRequestTypeFromChangeRequestID", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 105 /102

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    string ChangeRequestType = string.Empty;

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        ChangeRequestType = dt.Rows[0].ItemArray[0].ToString();

                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = ChangeRequestType };
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
        public async Task<DatabaseResponse> GetInvoiceRemarksFromInvoiceID(int InvoiceID)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@InvoiceID",  SqlDbType.Int )

                };

                parameters[0].Value = InvoiceID;

                _DataHelper = new DataAccessHelper("Order_GetAccountRemarksByInvoiceID", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 105 /102

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    string Remarks = string.Empty;

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        Remarks = dt.Rows[0].ItemArray[0].ToString();

                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = Remarks };
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

        public async Task<DatabaseResponse> GetOrderIDByCustomerIdAndMobileNumber(int CustomerID, string MobileNumber)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar )
                };

                parameters[0].Value = CustomerID;
                parameters[1].Value = MobileNumber;

                _DataHelper = new DataAccessHelper("Order_GetOrderIDByCustomerIdAndMobileNumber", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 105 /102

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    string Remarks = string.Empty;

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        Remarks = dt.Rows[0].ItemArray[0].ToString();

                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = Remarks };
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

        public async Task<DatabaseResponse> IsBuddyBundle(int bundleID)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter( "@BundleID",  SqlDbType.Int )

                };

                parameters[0].Value = bundleID;

                _DataHelper = new DataAccessHelper("Orders_IsBuddyBundle", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt);    // 105 / 102

                DatabaseResponse response = new DatabaseResponse();

                IsBuddyBundle buddy = new IsBuddyBundle();

                if (dt != null && dt.Rows.Count > 0)
                {
                    buddy = (from model in dt.AsEnumerable()
                                      select new IsBuddyBundle()
                                      {
                                          BundleID   = model.Field<int>("BundleID"),                                         
                                          HasBuddyPromotion = model.Field<int>("HasBuddyPromotion")
                                      }).ToList().FirstOrDefault();

                    response = new DatabaseResponse { ResponseCode = result, Results = buddy};

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

        public async Task<DatabaseResponse> CreateBuddySubscriber(CreateBuddySubscriber subscriber)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@UserId",  SqlDbType.NVarChar ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@MainLineMobileNumber",  SqlDbType.NVarChar)
                };

                parameters[0].Value = subscriber.OrderID;
                parameters[1].Value = subscriber.UserId;
                parameters[2].Value = subscriber.MobileNumber;
                parameters[3].Value = subscriber.MainLineMobileNumber;              

                _DataHelper = new DataAccessHelper("Order_CreateBuddySubscriber", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync();    // 100 / 107 /170 - no buddy for the bundle

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

        public async Task<DatabaseResponse> CreateOrder_V2(CreateOrder order)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@BundleID",  SqlDbType.Int ),
                    new SqlParameter( "@ReferralCode",  SqlDbType.NVarChar),
                    new SqlParameter( "@PromotionCode",  SqlDbType.NVarChar),
                    new SqlParameter( "@UserCode",  SqlDbType.NVarChar)
                };

                parameters[0].Value = order.CustomerID;
                parameters[1].Value = order.BundleID;
                parameters[2].Value = order.ReferralCode;
                parameters[3].Value = order.PromotionCode;
                parameters[4].Value = order.UserCode;

                _DataHelper = new DataAccessHelper("Orders_CreateOrder_v2", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt);

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
        public async Task<DatabaseResponse> GetOrderBasicDetails_V2(int orderId)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )

                };

                parameters[0].Value = orderId;

                _DataHelper = new DataAccessHelper("Order_GetOrderBasicDetails_v2", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds); // 105 /109

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
                                                 IsBuddyLine = model.Field<int>("IsBuddyLine"),
                                                 GroupNumber = model.Field<int>("GroupNumber")
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


        public async Task<DatabaseResponse> CheckBuddyToRemove(int orderId)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )

                };

                parameters[0].Value = orderId;

                _DataHelper = new DataAccessHelper("Orders_HasBuddyToRemove", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds); // 102 /105

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    BuddyToRemove buddyToRemove = new BuddyToRemove();

                    if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {

                        buddyToRemove = (from model in ds.Tables[0].AsEnumerable()
                                        select new BuddyToRemove()
                                        {
                                             BuddyRemovalID = model.Field<int>("BuddyRemovalID"),
                                             MobileNumber = model.Field<string>("MobileNumber"),
                                             IsPorted = model.Field<int?>("IsPorted"),
                                             IsRemoved = model.Field<int?>("IsRemoved"),
                                        }).FirstOrDefault();  
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = buddyToRemove };

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

        public async Task<DatabaseResponse> UpdateBuddyRemoval(int buddyRemovalID)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@BuddyRemovalID",  SqlDbType.Int )

                };

                parameters[0].Value = buddyRemovalID;

                _DataHelper = new DataAccessHelper("Orders_UpdateBuddyRemoval", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds); // 102 /101/106

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


        public async Task<DatabaseResponse> GetOrderDetails_V2(int orderId)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )

                };

                parameters[0].Value = orderId;

                _DataHelper = new DataAccessHelper("Order_GetOrderDetails_v2", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds); // 105 /102

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
                                            IdentityCardType = model.Field<string>("IdentityCardType"),
                                            IdentityCardNumber = model.Field<string>("IdentityCardNumber"),
                                            BillingUnit = model.Field<string>("BillingUnit"),
                                            BillingFloor = model.Field<string>("BillingFloor"),
                                            BillingBuildingNumber = model.Field<string>("BillingBuildingNumber"),
                                            BillingBuildingName = model.Field<string>("BillingBuildingName"),
                                            BillingStreetName = model.Field<string>("BillingStreetName"),
                                            BillingPostCode = model.Field<string>("BillingPostCode"),
                                            BillingContactNumber = model.Field<string>("BillingContactNumber"),
                                            ReferralCode = model.Field<string>("ReferralCode"),
                                            PromotionCode = model.Field<string>("PromotionCode"),
                                            HaveDocuments = model.Field<bool>("HaveDocuments"),
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
                                            AlternateRecioientIDType = model.Field<string>("AlternateRecioientIDType"),
                                            AlternateRecioientIDNumber = model.Field<string>("AlternateRecioientIDNumber"),
                                            PortalSlotID = model.Field<string>("PortalSlotID"),
                                            SlotDate = model.Field<DateTime?>("SlotDate"),
                                            SlotFromTime = model.Field<TimeSpan?>("SlotFromTime"),
                                            SlotToTime = model.Field<TimeSpan?>("SlotToTime"),
                                            ScheduledDate = model.Field<DateTime?>("ScheduledDate"),
                                            ServiceFee = model.Field<double?>("ServiceFee"),
                                            RecieptNumber = model.Field<string>("RecieptNumber"),
                                            EventSalesRepresentativeID = model.Field<int?>("EventSalesRepresentativeID"),
                                            SIMIDPrefix = model.Field<string>("SIMIDPrefix"),
                                            IsELOAUpdateAllowed = model.Field<bool?>("IsELOAUpdateAllowed")
                                        }).FirstOrDefault();

                        List<Bundle> orderBundles = new List<Bundle>();

                        List<ServiceCharge> subscriberServiceCharges = new List<ServiceCharge>();

                        List<PromotionalVAS> promoVASes = new List<PromotionalVAS>();

                        if (ds.Tables[3] != null && ds.Tables[3].Rows.Count > 0)
                        {

                            subscriberServiceCharges = (from model in ds.Tables[3].AsEnumerable()
                                                        select new ServiceCharge()
                                                        {
                                                            OrderSubscriberID = model.Field<int>("OrderSubscriberID"),
                                                            PortalServiceName = model.Field<string>("PortalServiceName"),
                                                            ServiceFee = model.Field<double?>("ServiceFee"),
                                                            IsRecurring = model.Field<int>("IsRecurring"),
                                                            IsGSTIncluded = model.Field<int>("IsGSTIncluded"),
                                                        }).ToList();                           

                        }

                        if (ds.Tables[4] !=null && ds.Tables[4].Rows.Count>0)
                        {
                            promoVASes = (from model in ds.Tables[4].AsEnumerable()
                                                            select new PromotionalVAS()
                                                            {
                                                                 OrderSubscriberID = model.Field<int>("OrderSubscriberID"),
                                                                 VASID = model.Field<int>("VASID"),
                                                                 BSSPlanCode = model.Field<string>("BSSPlanCode"),
                                                                 PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                                                 PortalDescription = model.Field<string>("PortalDescription"),
                                                                 PortalSummaryDescription = model.Field<string>("PortalSummaryDescription"),
                                                                 Data = model.Field<double?>("Data"),
                                                                 SMS = model.Field<double?>("SMS"),
                                                                 Voice = model.Field<double?>("Voice"),
                                                                 SubscriptionFee = model.Field<double?>("SubscriptionFee"),
                                                                 IsRecurring = model.Field<string>("IsRecurring"),
                                                                 SubscriptionCount = model.Field<int>("SubscriptionCount"),
                                                            }).ToList();  
                           
                        }                       

                        if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                        {
                            orderBundles = (from model in ds.Tables[1].AsEnumerable()
                                          select new Bundle()
                                          {
                                              OrderSubscriberID = model.Field<int>("OrderSubscriberID"),
                                              BundleID = model.Field<int>("BundleID"),
                                              DisplayName = model.Field<string>("DisplayName"),
                                              MobileNumber = model.Field<string>("MobileNumber"),
                                              IsPrimaryNumber = model.Field<int>("IsPrimaryNumber"),
                                              PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                              PortalDescription = model.Field<string>("PortalDescription"),
                                              PortalSummaryDescription = model.Field<string>("PortalSummaryDescription"),
                                              TotalData = model.Field<double?>("TotalData"),
                                              TotalSMS = model.Field<double?>("TotalSMS"),
                                              TotalVoice = model.Field<double?>("TotalVoice"),
                                              ActualSubscriptionFee = model.Field<double?>("ActualSubscriptionFee"),
                                              ApplicableSubscriptionFee = model.Field<double?>("ApplicableSubscriptionFee"),
                                              ServiceName = model.Field<string>("ServiceName"),
                                              ActualServiceFee = model.Field<double?>("ActualServiceFee"),
                                              ApplicableServiceFee = model.Field<double?>("ApplicableServiceFee"),
                                              PremiumType = model.Field<int>("PremiumType"),
                                              IsPorted = model.Field<int>("IsPorted"),
                                              IsOwnNumber = model.Field<int>("IsOwnNumber"),
                                              DonorProvider = model.Field<string>("DonorProvider"),
                                              PortedNumberTransferForm = model.Field<string>("PortedNumberTransferForm"),
                                              PortedNumberOwnedBy = model.Field<string>("PortedNumberOwnedBy"),
                                              PortedNumberOwnerRegistrationID = model.Field<string>("PortedNumberOwnerRegistrationID"),
                                              PricingDescription = model.Field<string>("PricingDescription"),
                                              IsBuddyLine = model.Field<int?>("IsBuddyLine"),
                                              GroupNumber = model.Field<int?>("GroupNumber"),
                                              PromotionalVASes= promoVASes!=null && promoVASes.Count>0? promoVASes.Where(v=>v.OrderSubscriberID== model.Field<int>("OrderSubscriberID")).ToList():null,
                                              ServiceCharges= subscriberServiceCharges!=null && subscriberServiceCharges.Count>0? subscriberServiceCharges.Where(c=>c.OrderSubscriberID== model.Field<int>("OrderSubscriberID")).ToList():null
                                          }).ToList();

                        }                        
                        orderDetails.Bundles = orderBundles;

                        List<ServiceCharge> orderServiceCharges = new List<ServiceCharge>();

                        if (ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                        {

                            orderServiceCharges = (from model in ds.Tables[2].AsEnumerable()
                                                   select new ServiceCharge()
                                                   {
                                                       PortalServiceName = model.Field<string>("PortalServiceName"),
                                                       ServiceFee = model.Field<double?>("ServiceFee"),
                                                       IsRecurring = model.Field<int>("IsRecurring"),
                                                       IsGSTIncluded = model.Field<int>("IsGSTIncluded"),
                                                   }).ToList();

                            orderDetails.ServiceCharges = orderServiceCharges;

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

        public async Task<DatabaseResponse> CheckAdditionalBuddy(int orderId)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )

                };

                parameters[0].Value = orderId;

                _DataHelper = new DataAccessHelper("Orders_HasAdditionalBuddy", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds); // 102 /105

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                   List<AdditionalBuddy> additionalBuddies = new List<AdditionalBuddy>();

                    if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {

                        additionalBuddies = (from model in ds.Tables[0].AsEnumerable()
                                         select new AdditionalBuddy()
                                         {
                                             OrderAdditionalBuddyID = model.Field<int>("OrderAdditionalBuddyID"),
                                             MobileNumber = model.Field<string>("MobileNumber"),
                                             IsProcessed = model.Field<int?>("IsProcessed"),
                                             IsPorted = model.Field<int?>("IsPorted")
                                         }).ToList();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = additionalBuddies };

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

        public async Task<DatabaseResponse> UpdateAdditionalBuddyProcessing(int OrderAdditionalBuddyID)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderAdditionalBuddyID",  SqlDbType.Int )

                };

                parameters[0].Value = OrderAdditionalBuddyID;

                _DataHelper = new DataAccessHelper("Orders_UpdateAdditionalBuddyProcessing", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds); // 102 /101/106

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

        public async Task<DatabaseResponse> AddRemoveVas(VasAddRemoveRequest request)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@BundleID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.Int ),
                    new SqlParameter( "@IsRemove",  SqlDbType.Int )
                };

                parameters[0].Value = request.OrderID;
                parameters[1].Value = request.BundleID;
                parameters[2].Value = request.MobileNumber;
                parameters[3].Value = request.IsRemove;

                _DataHelper = new DataAccessHelper("Orders_AddRemoveVAS", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds); // 100 /107, /103/150, 102/164

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
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@BundleID",  SqlDbType.Int),
                    new SqlParameter( "@Quantity",  SqlDbType.Int),
                    new SqlParameter( "@RequestType",  SqlDbType.NVarChar)
                };

                parameters[0].Value = customerId;
                parameters[1].Value = mobileNumber;
                parameters[2].Value = bundleId;
                parameters[3].Value = quantity;
                parameters[4].Value = Core.Enums.RequestType.AddVAS.GetDescription();

                _DataHelper = new DataAccessHelper(DbObjectNames.Orders_CR_BuyVAS, parameters, _configuration);
                DataTable dt = new DataTable();
                var result = await _DataHelper.RunAsync(dt);    // 101 / 102
                if (result != (int)Core.Enums.DbReturnValue.CreateSuccess)
                    return new DatabaseResponse { ResponseCode = result };

                var removeVASResponse = new BuyVASResponse();

                if (dt.Rows.Count > 0)
                {
                    removeVASResponse = (from model in dt.AsEnumerable()
                                         select new BuyVASResponse()
                                         {
                                             ChangeRequestID = model.Field<int>("ChangeRequestID"),
                                             BSSPlanCode = model.Field<string>("BSSPlanCode"),
                                             PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                             SubscriptionFee = model.Field<double>("SubscriptionFee")
                                         }).FirstOrDefault();
                }
                else
                {
                    removeVASResponse = null;
                }

                var response = new DatabaseResponse { ResponseCode = result, Results = removeVASResponse };
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


        public async Task<DatabaseResponse> GetOrderedVASesToProcess(int orderId)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )

                };

                parameters[0].Value = orderId;

                _DataHelper = new DataAccessHelper("Orders_GetVASesToProcess", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds); // 102 /105

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                   List<VasToProcess> vasListToProcess = new List<VasToProcess>();

                    if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {

                        vasListToProcess = (from model in ds.Tables[0].AsEnumerable()
                                         select new VasToProcess()
                                         {
                                              OrderSubscriberVASBundleID = model.Field<int>("OrderSubscriberVASBundleID"),
                                              MobileNumber = model.Field<string>("MobileNumber"),
                                              BundleID = model.Field<int>("BundleID"),
                                            
                                         }).ToList();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = vasListToProcess };

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

        public async Task<DatabaseResponse> RemoveAdditionalBuddyOnRollBackOrder(int orderId)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    
                };

                parameters[0].Value = orderId;
               

                _DataHelper = new DataAccessHelper("Orders_RemoveAdditionalBuddyOnRollBackOrder", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds); //103/150, 

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

        public async Task<DatabaseResponse> LogUnblockFailedMainline(int orderId, AdditionalBuddy subscriber)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@IsPorted",  SqlDbType.Int )
                };

                parameters[0].Value = orderId;
                parameters[1].Value = subscriber.MobileNumber;
                parameters[2].Value = subscriber.IsPorted;

                _DataHelper = new DataAccessHelper("Orders_LogUnblockFailedMainline", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds); //103/150, 

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

        public async Task<DatabaseResponse> GetRequiredNumberCount(int CustomerID, int BundleID)
        {
            try
            {
                SqlParameter[] parameters =
                   {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@BundleID",  SqlDbType.Int )
                };

                parameters[0].Value = CustomerID;
                parameters[1].Value = BundleID;

                _DataHelper = new DataAccessHelper("Orders_GetRequiredNumberCount", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds); //103/150, 

                DatabaseResponse response = new DatabaseResponse();

                response = new DatabaseResponse { ResponseCode = result, Results=ds.Tables[0].Rows[0][0].ToString().Trim() };

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

        public async Task<DatabaseResponse> UpdateSIMCardDetails(int OrderID, SIMCardDetail[] details)
        {
            try
            {
                DataTable SIMList = new DataTable();
                SIMList.Columns.Add(new DataColumn("MobileNumber", typeof(string)));
                SIMList.Columns.Add(new DataColumn("SIMID", typeof(string)));
                foreach (SIMCardDetail detail in details)
                {
                    DataRow dr = SIMList.NewRow();
                    dr["MobileNumber"] = detail.MobileNumber;
                    dr["SIMID"] = detail.SIMNumber;

                    SIMList.Rows.Add(dr);
                }
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@SIMList",  SqlDbType.Structured )
                };

                parameters[0].Value = OrderID;
                parameters[1].Value = SIMList;

                _DataHelper = new DataAccessHelper("Orders_UpdateOrderSubscriberSIMLog", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds); //103/150, 

                List<SIMCardResponse> _SIMCardResponse = new List<SIMCardResponse>();

                if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {

                    _SIMCardResponse = (from model in ds.Tables[0].AsEnumerable()
                                        select new SIMCardResponse()
                                        {
                                            MobileNumber = model.Field<string>("MobileNumber"),
                                            SIMNumber = model.Field<string>("SIMID"),
                                            IsProcessed = model.Field<int>("IsProcessed"),
                                            ErrorReason = model.Field<string>("ErrorReason"),

                                        }).ToList();
                }
                DatabaseResponse response = new DatabaseResponse();

                response = new DatabaseResponse { ResponseCode = result, Results = _SIMCardResponse };

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
        /// Updates the order eloa.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdateOrderELOA(UpdateOrderELOARequest details)
        {
            try
            {
                
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@OrderType",  SqlDbType.Int ),
                    new SqlParameter( "@AlternateRecioientIDNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@AlternateRecioientIDType",  SqlDbType.NVarChar ),
                    new SqlParameter( "@AlternateRecipientContact",  SqlDbType.NVarChar ),
                    new SqlParameter( "@AlternateRecipientEmail",  SqlDbType.NVarChar ),
                    new SqlParameter( "@AlternateRecipientName",  SqlDbType.NVarChar )
                };

                parameters[0].Value = details.OrderID;
                parameters[1].Value = details.OrderType;
                parameters[2].Value = details.AlternateRecioientIDNumber;
                parameters[3].Value = details.AlternateRecioientIDType;
                parameters[4].Value = details.AlternateRecipientContact;
                parameters[5].Value = details.AlternateRecipientEmail;
                parameters[6].Value = details.AlternateRecipientName;

                _DataHelper = new DataAccessHelper("Orders_UpdateLOADetails", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds); 

               

                var response = new DatabaseResponse { ResponseCode = result };

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
        /// Register number for unblocking.
        /// </summary>
        /// <param name="CustomerID">CustomerID.</param>
        /// <param name="number">Mobile Number.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> LogUnblockNumber(int CustomerID, string number)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@Number",  SqlDbType.NVarChar )

                };

                parameters[0].Value = CustomerID;
                parameters[1].Value = number;

                _DataHelper = new DataAccessHelper("Orders_LogUnBlockNumber", parameters, _configuration);

                int result = await _DataHelper.RunAsync(); // 102 /105

                DatabaseResponse response = new DatabaseResponse { ResponseCode = result };

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
        public async Task<DatabaseResponse> UpdateBSSSelectionNumbers(string json, string userID, int callLogId, int customerID, string portalServiceName, double price)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter("@UserID",  SqlDbType.NVarChar ),
                     new SqlParameter("@Json",  SqlDbType.NVarChar ),
                     new SqlParameter("@BSSCallLogID",  SqlDbType.Int ),
                     new SqlParameter("@CustomerID",  SqlDbType.Int ),
                     new SqlParameter("@PortalServiceName",  SqlDbType.NVarChar ),
                     new SqlParameter("@Price",  SqlDbType.Float )
                };

                parameters[0].Value = userID;
                parameters[1].Value = json;
                parameters[2].Value = callLogId;
                parameters[3].Value = customerID;
                parameters[4].Value = portalServiceName;
                parameters[5].Value = price;

                _DataHelper = new DataAccessHelper("Orders_UpdateBSSSelectionNumbers", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync();    // 105 / 102

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
        public async Task<DatabaseResponse> GetSelectionNumbers(int customerID)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter("@CustomerID",  SqlDbType.Int )
                };

                parameters[0].Value = customerID;

                _DataHelper = new DataAccessHelper("Orders_GetBSSSelectionNumbers", parameters, _configuration);

                DataSet ds = new DataSet("ds");

                int result = await _DataHelper.RunAsync(ds);    // 105 / 102
                BSSNumbers numbers = new BSSNumbers();
                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        List<FreeNumber> _numbers = new List<FreeNumber>();
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            FreeNumber _number = new FreeNumber();
                            _number.MobileNumber = dr["Number"].ToString().Trim();
                            _number.ServiceCode = dr["ServiceCode"].ToString().Trim();
                            _numbers.Add(_number);
                        }
                        numbers.FreeNumbers = _numbers;
                    }
                    if (ds.Tables.Count > 1)
                    {
                        List<PremiumNumbers> _premiumnumbers = new List<PremiumNumbers>();
                        foreach (DataRow dr in ds.Tables[1].Rows)
                        {
                            PremiumNumbers _number = new PremiumNumbers();
                            _number.MobileNumber = dr["Number"].ToString().Trim();
                            _number.ServiceCode = ((int)dr["ServiceCode"]);
                            _number.PortalServiceName = dr["PortalServiceName"].ToString().Trim();
                            _number.Price = ((double)dr["Price"]);
                            _premiumnumbers.Add(_number);
                        }
                        numbers.PremiumNumbers = _premiumnumbers;
                    }
                }
                DatabaseResponse response = new DatabaseResponse();

                response = new DatabaseResponse { ResponseCode = result, Results =  numbers};

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

        public async Task<bool> GetBSSNumbersInitially(int customerID)
        {
            BSSAPIHelper bsshelper = new BSSAPIHelper();
            DatabaseResponse systemConfigResponse = await GetConfiguration(ConfiType.System.ToString());

            DatabaseResponse bssConfigResponse = await GetConfiguration(ConfiType.BSS.ToString());

            GridBSSConfi bssConfig = bsshelper.GetGridConfig((List<Dictionary<string, string>>)bssConfigResponse.Results);

            GridSystemConfig systemConfig = bsshelper.GetGridSystemConfig((List<Dictionary<string, string>>)systemConfigResponse.Results);

            DatabaseResponse serviceCAF = await GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

            DatabaseResponse requestIdResForFreeNumber = await GetBssApiRequestId(GridMicroservices.Customer.ToString(), BSSApis.GetAssets.ToString(), customerID, (int)BSSCalls.NewSession, "");

            ResponseObject res = new ResponseObject();

            //Getting FreeNumbers
            try
            {
                res = await bsshelper.GetAssetInventory(bssConfig, ((List<ServiceFees>)serviceCAF.Results).FirstOrDefault().ServiceCode, (BSSAssetRequest)requestIdResForFreeNumber.Results, systemConfig.FreeNumberListCount);
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));
                return false;
            }

            BSSNumbers numbers = new BSSNumbers();

            if (res != null)
            {
                numbers.FreeNumbers = bsshelper.GetFreeNumbers(res);

                //insert these number into database
                string json = bsshelper.GetJsonString(numbers.FreeNumbers); // json insert
                LogInfo.Information("step1 : " + json);
                await UpdateBSSSelectionNumbers(json, ((BSSAssetRequest)requestIdResForFreeNumber.Results).userid, ((BSSAssetRequest)requestIdResForFreeNumber.Results).BSSCallLogID, customerID, "Free", 0);
                DatabaseResponse updateBssCallFeeNumbers = await UpdateBSSCallNumbers(json, ((BSSAssetRequest)requestIdResForFreeNumber.Results).userid, ((BSSAssetRequest)requestIdResForFreeNumber.Results).BSSCallLogID);

                if (updateBssCallFeeNumbers.ResponseCode == (int)DbReturnValue.CreateSuccess)
                {
                    // get Premium Numbers

                    DatabaseResponse serviceCAFPremium = await GetBSSServiceCategoryAndFee(ServiceTypes.Premium.ToString());

                    if (serviceCAFPremium != null && serviceCAFPremium.ResponseCode == (int)DbReturnValue.RecordExists)
                    {
                        List<ServiceFees> premiumServiceFeeList = new List<ServiceFees>();

                        premiumServiceFeeList = (List<ServiceFees>)serviceCAFPremium.Results;

                        int countPerPremium = (systemConfig.PremiumNumberListCount / premiumServiceFeeList.Count);

                        int countBalance = systemConfig.PremiumNumberListCount % premiumServiceFeeList.Count;

                        if (countBalance > 0)
                        {
                            countPerPremium = countPerPremium + countBalance;
                        }

                        int loopCount = premiumServiceFeeList.Count;

                        int iterator = 0;

                        foreach (ServiceFees fee in premiumServiceFeeList)
                        {
                            //get code and call premum 
                            //  fee.PortalServiceName

                            DatabaseResponse requestIdResForPremium = await GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), customerID, (int)BSSCalls.NewSession, "");

                            ResponseObject premumResponse = new ResponseObject();

                            try
                            {
                                premumResponse = await bsshelper.GetAssetInventory(bssConfig, fee.ServiceCode, (BSSAssetRequest)requestIdResForPremium.Results, countPerPremium);
                            }

                            catch (Exception ex)
                            {
                                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                            }

                            if (premumResponse != null && premumResponse.Response != null && premumResponse.Response.asset_details != null)
                            {
                                List<PremiumNumbers> premiumNumbers = bsshelper.GetPremiumNumbers(premumResponse, fee);

                                List<FreeNumber> premiumToLogNumbers = bsshelper.GetFreeNumbers(premumResponse);

                                string jsonPremium = bsshelper.GetJsonString(premiumToLogNumbers);

                                LogInfo.Information("step2 : " + jsonPremium);
                                await UpdateBSSSelectionNumbers(jsonPremium, ((BSSAssetRequest)requestIdResForFreeNumber.Results).userid, ((BSSAssetRequest)requestIdResForFreeNumber.Results).BSSCallLogID, customerID, fee.PortalServiceName, fee.ServiceFee);

                                DatabaseResponse updateBssCallPremiumNumbers = await UpdateBSSCallNumbers(jsonPremium, ((BSSAssetRequest)requestIdResForPremium.Results).userid, ((BSSAssetRequest)requestIdResForPremium.Results).BSSCallLogID);
                                List<PremiumNumbers> pnumbers = new List<PremiumNumbers>();
                                foreach (PremiumNumbers premium in premiumNumbers)
                                {
                                    pnumbers.Add(premium);
                                }
                                numbers.PremiumNumbers = pnumbers;
                            }
                            else
                            {
                                //failed to get premium

                                if (iterator == 0)
                                {
                                    countPerPremium = (systemConfig.PremiumNumberListCount / (premiumServiceFeeList.Count - 1));
                                }
                                else if (iterator == 1)
                                {
                                    if (numbers.PremiumNumbers.Count < countPerPremium * (iterator + 1))

                                        countPerPremium = (systemConfig.PremiumNumberListCount / (premiumServiceFeeList.Count - 2));
                                    else
                                        countPerPremium = (systemConfig.PremiumNumberListCount / (premiumServiceFeeList.Count - 1));
                                }
                            }

                            iterator++;

                        }  // for

                        if (numbers.PremiumNumbers.Count > systemConfig.PremiumNumberListCount)
                        {
                            int extrPremiumCount = numbers.PremiumNumbers.Count - systemConfig.PremiumNumberListCount;

                            numbers.PremiumNumbers.RemoveRange(numbers.PremiumNumbers.Count - (extrPremiumCount + 1), extrPremiumCount);
                        }
                    }
                    return true;
                }
                else
                {
                    //failded to update BSS call numbers so returning
                    return false;
                }
            }
            else
            {
                //failed to get free numbers
                return false;
            }
        }
    }
}
