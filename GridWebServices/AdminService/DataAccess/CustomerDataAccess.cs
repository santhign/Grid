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
using AdminService.Models;
using InfrastructureService;
using Swashbuckle.AspNetCore.Swagger;

namespace AdminService.DataAccess
{
    public class CustomerDataAccess
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
        public CustomerDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gets the customers.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Customer>> GetCustomers()
        {
            try
            {

                _DataHelper = new DataAccessHelper("Admin_GetCustomerListing", _configuration);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                List<Customer> customerList = new List<Customer>();

                if (dt.Rows.Count > 0)
                {

                    customerList = (from model in dt.AsEnumerable()
                                    select new Customer()
                                    {
                                        CustomerID = model.Field<int>("CustomerID"),
                                        Email = model.Field<string>("Email"),
                                        Password = model.Field<string>("Password"),
                                        MobileNumber = model.Field<string>("MobileNumber"),
                                        ReferralCode = model.Field<string>("ReferralCode"),
                                        Nationality = model.Field<string>("Nationality"),
                                        Gender = model.Field<string>("Gender"),
                                        DOB = model.Field<DateTime?>("DOB"),
                                        SMSSubscription = model.Field<string>("SMSSubscription"),
                                        EmailSubscription = model.Field<string>("EmailSubscription"),
                                        Status = model.Field<string>("Status"),
                                        JoinedOn = model.Field<DateTime>("JoinedOn")
                                    }).ToList();
                }

                return customerList;
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
        /// Gets the customer.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public async Task<Customer> GetCustomer(int customerId)
        {
            try
            {

                _DataHelper = new DataAccessHelper("Admin_GetCustomerListing", _configuration);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                Customer customer = new Customer();

                if (dt.Rows.Count > 0)
                {

                    customer = (from model in dt.AsEnumerable()
                                select new Customer()
                                {
                                    CustomerID = model.Field<int>("CustomerID"),
                                    Email = model.Field<string>("Email"),
                                    Password = model.Field<string>("Password"),
                                    MobileNumber = model.Field<string>("MobileNumber"),
                                    ReferralCode = model.Field<string>("ReferralCode"),
                                    Nationality = model.Field<string>("Nationality"),
                                    Gender = model.Field<string>("Gender"),
                                    DOB = model.Field<DateTime?>("DOB"),
                                    SMSSubscription = model.Field<string>("SMSSubscription"),
                                    EmailSubscription = model.Field<string>("EmailSubscription"),
                                    Status = model.Field<string>("Status"),
                                    JoinedOn = model.Field<DateTime>("JoinedOn")
                                }).Where(c => c.CustomerID == customerId).FirstOrDefault();
                }

                return customer;
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
        /// Gets the search customers.
        /// </summary>
        /// <param name="SearchValue">The search value.</param>
        /// <returns></returns>
        public async Task<List<CustomerSearch>> GetSearchCustomers(string SearchValue)
        {

            try
            {


                SqlParameter[] parameters =
                    {
                    new SqlParameter("@SearchValue", SqlDbType.NVarChar)
                    };

                parameters[0].Value = SearchValue;
                _DataHelper = new DataAccessHelper("Customer_SearchCustomers", parameters, _configuration);


                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                List<CustomerSearch> customerList = new List<CustomerSearch>();

                if (dt.Rows.Count > 0)
                {

                    customerList = (from model in dt.AsEnumerable()
                                    select new CustomerSearch()
                                    {
                                        CustomerId = model.Field<int>("CustomerID"),
                                        CustomerName = model.Field<string>("Name"),
                                        Email = model.Field<string>("Email"),
                                        PhoneNumber = model.Field<string>("MobileNumber"),
                                        Plan = model.Field<string>("PlanName"),
                                        AdditionalLines = model.Field<int>("AdditionalLines"),
                                        JoinedOn = model.Field<DateTime>("JoinedOn"),
                                        Status = model.Field<string>("Status")
                                    }).ToList();
                }

                return customerList;
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

        public async Task<DatabaseResponse> GetCustomerOrders(int CustomerID)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int )

                };

                parameters[0].Value = CustomerID;

                _DataHelper = new DataAccessHelper("Admin_GetCustomerOrders", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds); // 105 /102

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {
                    List<Order> orders = new List<Order>();
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        Order orderDetails = new Order();

                        if (dr != null)
                        {

                            orderDetails.OrderID = Convert.ToInt32(dr["OrderID"]);
                            orderDetails.OrderNumber = dr["OrderNumber"].ToString();
                            orderDetails.OrderDate = Convert.ToDateTime(dr["OrderDate"]);
                            orderDetails.OrderStatus = dr["OrderStatus"].ToString();
                            orderDetails.IdentityCardType = dr["IdentityCardType"].ToString();
                            orderDetails.IdentityCardNumber = dr["IdentityCardNumber"].ToString();
                            orderDetails.BillingUnit = dr["BillingUnit"].ToString();
                            orderDetails.BillingFloor = dr["BillingFloor"].ToString();
                            orderDetails.BillingBuildingNumber = dr["BillingBuildingNumber"].ToString();
                            orderDetails.BillingBuildingName = dr["BillingBuildingName"].ToString();
                            orderDetails.BillingStreetName = dr["BillingStreetName"].ToString();
                            orderDetails.BillingPostCode = dr["BillingPostCode"].ToString();
                            orderDetails.BillingContactNumber = dr["BillingContactNumber"].ToString();
                            orderDetails.ReferralCode = dr["ReferralCode"].ToString();
                            orderDetails.PromotionCode = dr["PromotionCode"].ToString();
                            orderDetails.HaveDocuments = Convert.ToBoolean(dr["HaveDocuments"]);
                            orderDetails.Name = dr["Name"].ToString();
                            orderDetails.Email = dr["Email"].ToString();
                            orderDetails.IDType = dr["IDType"].ToString();
                            orderDetails.IDNumber = dr["IDNumber"].ToString();
                            orderDetails.IsSameAsBilling = Convert.ToInt32(dr["IsSameAsBilling"]);
                            orderDetails.ShippingUnit = dr["ShippingUnit"].ToString();
                            orderDetails.ShippingFloor = dr["ShippingFloor"].ToString();
                            orderDetails.ShippingBuildingNumber = dr["ShippingBuildingNumber"].ToString();
                            orderDetails.ShippingBuildingName = dr["ShippingBuildingName"].ToString();
                            orderDetails.ShippingStreetName = dr["ShippingStreetName"].ToString();
                            orderDetails.ShippingPostCode = dr["ShippingPostCode"].ToString();
                            orderDetails.ShippingContactNumber = dr["ShippingContactNumber"].ToString();
                            orderDetails.AlternateRecipientContact = dr["AlternateRecipientContact"].ToString();
                            orderDetails.AlternateRecipientName = dr["AlternateRecipientName"].ToString();
                            orderDetails.AlternateRecipientEmail = dr["AlternateRecipientEmail"].ToString();
                            orderDetails.PortalSlotID = dr["PortalSlotID"].ToString();
                            orderDetails.SlotDate = Convert.ToDateTime(dr["SlotDate"]);
                            TimeSpan val;
                            TimeSpan.TryParse(dr["SlotFromTime"].ToString(), out val);
                            orderDetails.SlotFromTime = val;
                            TimeSpan.TryParse(dr["SlotToTime"].ToString(), out val);
                            orderDetails.SlotToTime = val;
                            try { orderDetails.ScheduledDate = Convert.ToDateTime(dr["ScheduledDate"]); }
                            catch { }
                            try { orderDetails.ServiceFee = Convert.ToDouble(dr["ServiceFee"]); }
                            catch { }

                            List<Bundle> orderBundles = new List<Bundle>();

                            if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                            {

                                orderBundles = (from model in ds.Tables[1].AsEnumerable()
                                                select new Bundle()
                                                {
                                                    OrderID = model.Field<int>("OrderID"),
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
                                                }).Where(c => c.OrderID == orderDetails.OrderID).ToList();

                                orderDetails.Bundles = orderBundles;

                            }

                            List<ServiceCharge> orderServiceCharges = new List<ServiceCharge>();

                            if (ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                            {

                                orderServiceCharges = (from model in ds.Tables[2].AsEnumerable()
                                                       select new ServiceCharge()
                                                       {
                                                           OrderID = model.Field<int>("OrderID"),
                                                           PortalServiceName = model.Field<string>("PortalServiceName"),
                                                           ServiceFee = model.Field<double?>("ServiceFee"),
                                                           IsRecurring = model.Field<int>("IsRecurring"),
                                                           IsGSTIncluded = model.Field<int>("IsGSTIncluded"),
                                                       }).Where(c => c.OrderID == orderDetails.OrderID).ToList();

                                orderDetails.ServiceCharges = orderServiceCharges;

                            }
                        }
                        orders.Add(orderDetails);
                    }
                    response = new DatabaseResponse { ResponseCode = result, Results = orders };

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
