using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Core.Helpers;
using Core.Models;
using Core.Enums;
using Core.Extensions;
using Serilog;


namespace Core.DataAccess
{
    public class CommonDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public CommonDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<DatabaseResponse> GetCustomerBillingDetails(int CustomerID)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),

                };

                parameters[0].Value = CustomerID;

                _DataHelper = new DataAccessHelper("Customers_GetBillingDetails", parameters, _configuration);

                var dt = new DataTable();

                var result = await _DataHelper.RunAsync(dt); // 105 /119

                DatabaseResponse response;

                if (result == 105)
                {

                    var _customerBilling = new customerBilling();

                    if (dt.Rows.Count > 0)
                    {

                        _customerBilling = (from model in dt.AsEnumerable()
                                            select new customerBilling()
                                            {
                                                Name = model.Field<string>("Name"),
                                                BillingUnit = model.Field<string>("BillingUnit"),
                                                BillingFloor = model.Field<string>("BillingFloor"),
                                                BillingStreetName = model.Field<string>("BillingStreetName"),
                                                BillingBuildingNumber = model.Field<string>("BillingBuildingNumber"),
                                                BillingBuildingName = model.Field<string>("BillingBuildingName"),
                                                BillingContactNumber = model.Field<string>("BillingContactNumber"),
                                                BillingPostCode = model.Field<string>("BillingPostCode")
                                            }).FirstOrDefault();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = _customerBilling };

                }

                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                }

                return response;
            }

            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }


        /// <summary>
        /// Gets the customer detail by order.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="orderID">The order identifier.</param>
        /// <returns></returns>
        public async Task<CustomerDetails> GetCustomerDetailByOrder(int customerID, int orderID)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter( "@OrderID",  SqlDbType.Int ),
                     new SqlParameter( "@CustomerID",  SqlDbType.Int )

                };

                parameters[0].Value = orderID;

                parameters[1].Value = customerID;

                _DataHelper = new DataAccessHelper("Orders_GetCustomerOrderDetails", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds);    // 105 / 102

                DatabaseResponse response = new DatabaseResponse();

                var customer = new CustomerDetails();

                if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    customer = (from model in ds.Tables[0].AsEnumerable()
                                select new CustomerDetails()
                                {
                                    Name = model.Field<string>("Name"),
                                    DeliveryEmail = model.Field<string>("DeliveryEmail"),
                                    ToEmailList = model.Field<string>("ToEmailList"),
                                    ReferralCode = model.Field<string>("ReferralCode"),
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
                                    SlotDate = model.Field<DateTime>("SlotDate"),
                                    SlotFromTime = model.Field<TimeSpan>("SlotFromTime"),
                                    SlotToTime = model.Field<TimeSpan>("SlotToTime"),
                                    OrderNumber = model.Field<string>("OrderNumber")
                                }).FirstOrDefault();



                    if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                    {
                        List<OrderNumber> orderNumbers = new List<OrderNumber>();

                        orderNumbers = (from model in ds.Tables[1].AsEnumerable()
                                        select new OrderNumber()
                                        {
                                            MobileNumber = model.Field<string>("MobileNumber"),
                                            PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                            IsBuddyLine = model.Field<int>("IsBuddyLine"),
                                            ApplicableSubscriptionFee = model.Field<double>("ApplicableSubscriptionFee"),
                                            PricingDescription = model.Field<string>("PricingDescription")
                                        }).ToList();


                        customer.OrderedNumbers = orderNumbers;

                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = customer };
                }
                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                }

                return customer;
            }
            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }


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
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

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
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

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
        /// <param name="orderID">The order identifier.</param>
        /// <returns></returns>
        public async Task<OrderDetails> GetOrderDetails(int orderID)
        {
            try
            {
                SqlParameter[] parameters =
              {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),

                };

                parameters[0].Value = orderID;


                _DataHelper = new DataAccessHelper(DbObjectNames.Admin_GetOrderDetailsForNRIC, parameters, _configuration);

                DataSet ds = new DataSet();

                await _DataHelper.RunAsync(ds);

                OrderDetails details = new OrderDetails();
                details.VerificaionHistories = new List<IDVerificaionHistory>();
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    details = (from model in ds.Tables[0].AsEnumerable()
                               select new OrderDetails()
                               {
                                   OrderID = model.Field<int>("OrderID"),
                                   OrderNumber = model.Field<string>("OrderNumber"),
                                   IDVerificationStatus = model.Field<string>("IDVerificationStatus"),
                                   IDVerificationStatusNumber = model.Field<int?>("IDVerificationStatusNumber"),
                                   OrderStatus = model.Field<string>("OrderStatus"),
                                   OrderStatusNumber = model.Field<int?>("OrderStatusNumber"),
                                   RejectionCount = model.Field<int?>("RejectionCount"),
                                   Name = model.Field<string>("Name"),
                                   OrderDate = model.Field<DateTime?>("OrderDate"),
                                   DeliveryDate = model.Field<DateTime?>("DeliveryDate"),
                                   DeliveryFromTime = model.Field<TimeSpan?>("DeliveryFromTime"),
                                   DeliveryToTime = model.Field<TimeSpan?>("DeliveryToTime"),
                                   IdentityCardNumber = model.Field<string>("IdentityCardNumber"),
                                   IdentityCardType = model.Field<string>("IdentityCardType"),
                                   ExpiryDate = model.Field<DateTime?>("ExpiryDate"),
                                   DOB = model.Field<DateTime?>("DOB"),
                                   Nationality = model.Field<string>("Nationality"),
                                   DocumentURL = model.Field<string>("DocumentURL"),
                                   DocumentBackURL = model.Field<string>("DocumentBackURL"),
                               }).FirstOrDefault();
                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                    {
                        details.VerificaionHistories = (from model in ds.Tables[1].AsEnumerable()
                                                        select new IDVerificaionHistory()
                                                        {
                                                            VerificationLogID = model.Field<int>("VerificationLogID"),
                                                            OrderID = model.Field<int>("OrderID"),
                                                            IDVerificationStatusNumber = model.Field<int>("IDVerificationStatusNumber"),
                                                            IDVerificationStatus = model.Field<string>("IDVerificationStatus"),
                                                            ChangeLog = model.Field<string>("ChangeLog"),
                                                            Remarks = model.Field<string>("Remarks"),
                                                            UpdatedOn = model.Field<DateTime?>("UpdatedOn"),
                                                            UpdatedBy = model.Field<int?>("UpdatedBy")
                                                        }).ToList();
                    }
                    else
                    {
                        details.VerificaionHistories = null;
                    }
                }

                return details;
            }

            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> UpdateNRICDetails(int ? adminUserId, int verificationStatus, NRICDetailsRequest request)
        {
            try
            {
                SqlParameter[] parameters =
              {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@VerificationStatus",  SqlDbType.Int ),
                    new SqlParameter( "@IDNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@IDType",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Nationality",  SqlDbType.NVarChar ),
                    new SqlParameter( "@NameInNRIC",  SqlDbType.NVarChar ),
                    new SqlParameter( "@DOB",  SqlDbType.Date ),
                    new SqlParameter( "@Expiry",  SqlDbType.Date ),
                    new SqlParameter( "@BackImage",  SqlDbType.NVarChar ),
                    new SqlParameter( "@FrontImage",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Remarks",  SqlDbType.NVarChar ),
                    new SqlParameter( "@AdminUserID",  SqlDbType.Int )


                };

                parameters[0].Value = request.OrderID;
                parameters[1].Value = verificationStatus;
                parameters[2].Value = request.IdentityCardNumber;
                parameters[3].Value = request.IdentityCardType;
                parameters[4].Value = request.Nationality;
                parameters[5].Value = request.NameInNRIC;
                parameters[6].Value = request.DOB;
                parameters[7].Value = request.Expiry;
                parameters[8].Value = request.BackImage;
                parameters[9].Value = request.FrontImage;
                parameters[10].Value = request.Remarks;
                parameters[11].Value = adminUserId;

                _DataHelper = new DataAccessHelper(DbObjectNames.Orders_IDVerificationCapture, parameters, _configuration);
                DataTable dt = new DataTable();

                var result = await _DataHelper.RunAsync(dt);

                if (result != (int)DbReturnValue.UpdateSuccess && result != (int)DbReturnValue.UpdateSuccessSendEmail)
                    return new DatabaseResponse() { ResponseCode = result };

                DatabaseResponse response = new DatabaseResponse();
                EmailResponse emailDetails = new EmailResponse();
                response.ResponseCode = result;
                if (dt.Rows.Count > 0)
                {
                    emailDetails = (from model in dt.AsEnumerable()
                                    select new EmailResponse()
                                    {
                                        Email = model.Field<string>("Email"),
                                        Name = model.Field<string>("Name"),
                                        VerificationStatus = model.Field<int>("VerificationStatus")
                                    }).FirstOrDefault();
                }
                response.Results = emailDetails;
                return response;
            }
            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> UpdateTokenForVerificationRequests(int orderId)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )
                };

                parameters[0].Value = orderId;


                _DataHelper = new DataAccessHelper(DbObjectNames.Orders_UpdateIDVerificationRequests, parameters, _configuration);
                DataTable dt = new DataTable();

                var result = await _DataHelper.RunAsync(dt);

                if (result != (int)DbReturnValue.UpdateSuccess && result != (int)DbReturnValue.UpdateSuccessSendEmail)
                    return new DatabaseResponse() { ResponseCode = result };

                DatabaseResponse response = new DatabaseResponse();
                VerificationRequestResponse requestDetails = new VerificationRequestResponse();
                response.ResponseCode = result;
                if (dt.Rows.Count > 0)
                {
                    requestDetails = (from model in dt.AsEnumerable()
                                      select new VerificationRequestResponse()
                                      {
                                          VerificationRequestID = model.Field<int>("VerificationRequestID"),
                                          OrderID = model.Field<int>("OrderID"),
                                          RequestToken = model.Field<string>("RequestToken"),
                                          CreatedOn = model.Field<DateTime>("CreatedOn"),
                                          IsUsed = model.Field<int>("IsUsed")
                                      }).FirstOrDefault();
                }
                response.Results = requestDetails;
                return response;
            }
            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }
    }


}
