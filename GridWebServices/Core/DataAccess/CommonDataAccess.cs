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

    }
}
