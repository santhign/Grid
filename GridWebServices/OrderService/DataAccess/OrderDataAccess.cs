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

namespace OrderService.DataAccess
{
    public class OrderDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public OrderDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// This method will create a new order for createOrder endpoint
        /// </summary>
        /// <param name="order"></param>
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

                if(dt!=null && dt.Rows.Count > 0)
                {
                    orderCreated = (from model in dt.AsEnumerable()
                                    select new OrderInit()
                                    {
                                        OrderID = model.Field<int>("OrderID"),
                                        Status = model.Field<string>("Status")
                                    }).FirstOrDefault();

                }

                return new DatabaseResponse { ResponseCode = result, Results=orderCreated};
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

        public async Task<DatabaseResponse> CreateSubscriber(CreateSubscriber subscriber)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@BundleID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@IsPrimary",  SqlDbType.Int),
                    new SqlParameter( "@PromotionCode",  SqlDbType.NVarChar)                  

                };

                parameters[0].Value = subscriber.OrderID;
                parameters[1].Value = subscriber.BundleID;
                parameters[2].Value = subscriber.MobileNumber;
                parameters[3].Value = subscriber.IsPrimary;
                parameters[4].Value = subscriber.PromotionCode;

                _DataHelper = new DataAccessHelper("Order_CreateSubscriber", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run();    // 100 / 107 

                return new DatabaseResponse { ResponseCode = result};
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

                int result = _DataHelper.Run(ds); // 100 /109

                DatabaseResponse response = new DatabaseResponse();

                if (result == 100)
                {              

                OrderBasicDetails orderDetails = new OrderBasicDetails();

                if(ds!=null && ds.Tables[0] !=null && ds.Tables[0].Rows.Count>0)
                {

                    orderDetails = (from model in ds.Tables[0].AsEnumerable()
                                        select new OrderBasicDetails()
                                        {
                                            OrderID = model.Field<int>("OrderID"),
                                            OrderNumber = model.Field<string>("OrderNumber"),
                                            OrderDate =model.Field<DateTime>("OrderDate"),
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

        public async Task<DatabaseResponse> GetBssApiRequestId(int orderId)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )

                };

                parameters[0].Value = orderId;

                _DataHelper = new DataAccessHelper("Admin_GetRequestIDForBSSAPI", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = _DataHelper.Run(ds); // 100 /109

                DatabaseResponse response = new DatabaseResponse();

                if (result == 100)
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

        public async Task<DatabaseResponse> GetServiceFee(int serviceCode)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@ServiceCode",  SqlDbType.Int )

                };

                parameters[0].Value = serviceCode;

                _DataHelper = new DataAccessHelper("Order_GetServiceFee", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run(dt); // 102 /105

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    ServiceFees serviceFee = new ServiceFees();

                    if (dt != null  && dt.Rows.Count > 0)
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


        public async Task<DatabaseResponse> AuthenticateCustomerToken(string token)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@Customer_AuthenticateToken",  SqlDbType.Int )

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


        public async Task<DatabaseResponse> LogCustomerToken(string token)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                     new SqlParameter( "@Token",  SqlDbType.Int )

                };

                parameters[0].Value = token;

                _DataHelper = new DataAccessHelper("Customer_CreateToken", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run(dt); // 100 /105

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


        

    }
}
