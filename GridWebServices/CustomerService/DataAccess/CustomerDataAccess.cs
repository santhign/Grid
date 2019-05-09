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
using CustomerService.Models;
using InfrastructureService;
using Swashbuckle.AspNetCore.Swagger;

namespace CustomerService.DataAccess
{
    /// <summary>
    /// Customer Data Access class
    /// </summary>
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
        /// Creates the customer.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> CreateCustomer(RegisterCustomer customer)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@Email",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Password",  SqlDbType.NVarChar ),
                    new SqlParameter( "@ReferralCode",  SqlDbType.NVarChar)
                };

                parameters[0].Value = customer.Email;
                parameters[1].Value = new Sha2().Hash(customer.Password);
                parameters[2].Value = new RandomSG().GetString();

                _DataHelper = new DataAccessHelper("Customer_CreateCustomer", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt);

                Customer newCustomer = new Customer();

                if (dt != null && dt.Rows.Count > 0)
                {

                    newCustomer = (from model in dt.AsEnumerable()
                                   select new Customer()
                                   {
                                       CustomerID = model.Field<int>("CustomerID"),
                                       Email = model.Field<string>("Email"),
                                       Name = model.Field<string>("Name"),
                                       Password = model.Field<string>("Password"),
                                       MobileNumber = model.Field<string>("MobileNumber"),
                                       ReferralCode = model.Field<string>("ReferralCode"),
                                       Nationality = model.Field<string>("Nationality"),
                                       Gender = model.Field<string>("Gender"),
                                       SMSSubscription = model.Field<string>("SMSSubscription"),
                                       EmailSubscription = model.Field<string>("EmailSubscription"),
                                       Status = model.Field<string>("Status")
                                   }).FirstOrDefault();
                }

                return new DatabaseResponse { ResponseCode = result, Results = newCustomer };
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
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.NVarChar )
                };

                parameters[0].Value = customerId;

                _DataHelper = new DataAccessHelper("Customer_GetDetails", parameters, _configuration);

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
                                    Name = model.Field<string>("Name"),
                                    MobileNumber = model.Field<string>("MobileNumber"),
                                    IdentityCardType = model.Field<string>("IdentityCardType"),
                                    IdentityCardNumber = model.Field<string>("IdentityCardNumber"),
                                    ReferralCode = model.Field<string>("ReferralCode"),
                                    Nationality = model.Field<string>("Nationality"),
                                    Gender = model.Field<string>("Gender"),
                                    DOB = model.Field<DateTime?>("DOB"),
                                    SMSSubscription = model.Field<string>("SMSSubscription"),
                                    EmailSubscription = model.Field<string>("EmailSubscription"),
                                    Status = model.Field<string>("Status"),
                                    JoinedOn = model.Field<DateTime>("JoinedOn"),
                                    OrderCount = model.Field<int>("OrderCount"),
                                    PendingAllowedSubscribers = model.Field<int>("PendingAllowedSubscribers")
                                }).FirstOrDefault();
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
        /// Updates the customer profile.
        /// </summary>
        /// <param name="CustomerID"></param>
        /// <param name="customer">The customer.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdateCustomerProfile(int CustomerID, CustomerProfile customer)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Password",  SqlDbType.NVarChar ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@Email",  SqlDbType.NVarChar)
                };

                parameters[0].Value = CustomerID;
                if (customer.Password == null || customer.Password == "")
                { parameters[1].Value = DBNull.Value; }
                else
                { parameters[1].Value = new Sha2().Hash(customer.Password); }
                parameters[2].Value = customer.MobileNumber;
                parameters[3].Value = customer.Email;

                _DataHelper = new DataAccessHelper(DbObjectNames.Customer_UpdateCustomerProfile, parameters, _configuration);

                int result = await _DataHelper.RunAsync();                

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
        /// Gets the customer shared plans.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns>
        /// List of plan associated with Customers along with all subscribers
        /// </returns>
        public async Task<List<CustomerPlans>> GetCustomerSharedPlans(int customerId)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int )
                };

                parameters[0].Value = customerId;

                _DataHelper = new DataAccessHelper("Customers_GetSharedPlans", parameters, _configuration);

                DataTable dt = new DataTable();


                await _DataHelper.RunAsync(dt);

                var customerPlans = new List<CustomerPlans>();

                if (dt.Rows.Count > 0)
                {
                    customerPlans = (from model in dt.AsEnumerable()
                                     select new CustomerPlans()
                                     {
                                         SubscriptionID = model.Field<int>("SubscriptionID"),
                                         CustomerID = model.Field<int>("CustomerID"),
                                         PlanId = model.Field<int>("PlanID"),
                                         PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                         PortalSummaryDescription = model.Field<string>("PortalSummaryDescription"),
                                         PortalDescription = model.Field<string>("PortalDescription"),
                                         SubscriptionType = model.Field<string>("SubscriptionType"),
                                         PlanStatus = model.Field<string>("PlanStatus"),
                                         //MobileNumber = model.Field<string>("MobileNumber"),
                                         
                                         IsRecurring = model.Field<int>("IsRecurring"),
                                         ExpiryDate = model.Field<DateTime?>("ExpiryDate"),
                                         Removable = model.Field<int>("Removable"),
                                         SubscriptionDate = model.Field<DateTime?>("SubscriptionDate"),
                                         SubscriptionFee = model.Field<double?>("SubscriptionFee"),
                                     }).ToList();
                }

                return customerPlans;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw ex;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Gets the customer plans.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="mobileNumber">Mobile Number</param>
        /// <param name="planType">Plan Type</param>
        /// <returns>
        /// List of plan associated with Customers along with all subscribers
        /// </returns>
        public async Task<List<CustomerPlans>> GetCustomerPlans(int customerId, string mobileNumber, int ? planType)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@PlanType",  SqlDbType.Int )
                };

                parameters[0].Value = customerId;
                if (!string.IsNullOrEmpty(mobileNumber))
                    parameters[1].Value = mobileNumber;
                else
                    parameters[1].Value = DBNull.Value;
                if (planType != null)
                    parameters[2].Value = planType;
                else
                    parameters[2].Value = DBNull.Value;
                
                _DataHelper = new DataAccessHelper("Customers_GetPlans", parameters, _configuration);

                DataTable dt = new DataTable();

                
                await _DataHelper.RunAsync(dt);

                var customerPlans = new List<CustomerPlans>();

                if (dt.Rows.Count > 0)
                {
                    customerPlans = (from model in dt.AsEnumerable()
                                     select new CustomerPlans()
                                     {
                                         SubscriptionID = model.Field<int>("SubscriptionID"),
                                         CustomerID = model.Field<int>("CustomerID"),
                                         PlanId = model.Field<int>("PlanID"),
                                         PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                         PortalSummaryDescription = model.Field<string>("PortalSummaryDescription"),
                                         PortalDescription = model.Field<string>("PortalDescription"),
                                         SubscriptionType = model.Field<string>("SubscriptionType"),
                                         IsRecurring = model.Field<int>("IsRecurring"),
                                         MobileNumber = model.Field<string>("MobileNumber"),
                                         ExpiryDate = model.Field<DateTime?>("ExpiryDate"),
                                         PlanStatus = model.Field<string>("PlanStatus"),
                                         Removable = model.Field<int>("Removable"),
                                         SubscriptionDate = model.Field<DateTime?>("SubscriptionDate"),
                                         SubscriptionFee = model.Field<double?>("SubscriptionFee"),
                                     }).ToList();
                }

                return customerPlans;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw ex;
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
        /// Validates the referral code.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="referralCode">The referral code.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> ValidateReferralCode(int customerId, string referralCode)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@ReferralCode",  SqlDbType.NVarChar )
                };

                parameters[0].Value = customerId;
                parameters[1].Value = referralCode;

                _DataHelper = new DataAccessHelper("Customers_ValidateReferralCode", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 105 /119

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    ValidateReferralCodeResponse vrcResponse = new ValidateReferralCodeResponse();

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        vrcResponse = (from model in dt.AsEnumerable()
                                       select new ValidateReferralCodeResponse()
                                       {
                                           CustomerID = model.Field<int>("CustomerID"),
                                           IsReferralCodeValid = true

                                       }).FirstOrDefault();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = vrcResponse };

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
        /// Gets the subscribers.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetSubscribers(int customerId)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),

                };

                parameters[0].Value = customerId;
                _DataHelper = new DataAccessHelper("Customers_GetSubscribers", parameters, _configuration);

                var dt = new DataTable();

                var result = await _DataHelper.RunAsync(dt); // 105 /119

                DatabaseResponse response;

                if (result == 105)
                {

                    var subscriber = new List<Subscriber>();

                    if (dt.Rows.Count > 0)
                    {

                        subscriber = (from model in dt.AsEnumerable()
                                      select new Subscriber()
                                      {
                                          MobileNumber = model.Field<string>("MobileNumber"),
                                          DisplayName = model.Field<string>("DisplayName"),
                                          SIMID = model.Field<string>("SIMID"),
                                          PremiumType = model.Field<int>("PremiumType"),
                                          ActivatedOn = model.Field<DateTime?>("ActivatedOn"),
                                          IsPrimary = model.Field<bool>("IsPrimary"),
                                          LinkedMobileNumber = model.Field<string>("LinkedMobileNumber"),
                                          AccountType = model.Field<string>("AccountType"),
                                          LinkedDisplayName = model.Field<string>("LinkedDisplayName"),
                                          State = model.Field<string>("State"),
                                          SuspensionRaised = model.Field<int>("SuspensionRaised"),
                                          TerminationRaised = model.Field<int>("TerminationRaised"),
                                          PlanChangeRaised = model.Field<int>("PlanChangeRaised"),
                                          PlanChangeMessage = model.Field<string>("PlanChangeMessage")
                                      }).ToList();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = subscriber };

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

                throw;
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


        /// <summary>
        /// Updates the referral code.
        /// </summary>
        /// <param name="customerid">The customerid.</param>
        /// <param name="ReferralCode">The referral code.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdateReferralCode(int customerid,string ReferralCode)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int  ),
                    new SqlParameter( "@ReferralCode",  SqlDbType.NVarChar )
                };

                parameters[0].Value = customerid;
                parameters[1].Value = ReferralCode;

                _DataHelper = new DataAccessHelper("Customers_UpdateReferralCode", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 105 /119

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    ValidateReferralCodeResponse vrcResponse = new ValidateReferralCodeResponse();

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        vrcResponse = (from model in dt.AsEnumerable()
                                       select new ValidateReferralCodeResponse()
                                       {
                                           CustomerID = model.Field<int>("CustomerID"),
                                           IsReferralCodeValid = true

                                       }).FirstOrDefault();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = vrcResponse };

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
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> GetPaymentMethod(int CustomerID)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),

                };

                parameters[0].Value = CustomerID;
                _DataHelper = new DataAccessHelper("Customers_GetPaymentMethods", parameters, _configuration);

                var dt = new DataTable();

                var result = await _DataHelper.RunAsync(dt); // 105 /119

                DatabaseResponse response;

                if (result == 105)
                {

                    var _customerPaymentMethods = new List<customerPaymentMethod>();

                    if (dt.Rows.Count > 0)
                    {

                        _customerPaymentMethods = (from model in dt.AsEnumerable()
                                            select new customerPaymentMethod()
                                            {
                                                CardHolderName = model.Field<string>("CardHolderName"),
                                                MaskedCardNumer = model.Field<string>("MaskedCardNumer"),
                                                CardType = model.Field<string>("CardType"),
                                                IsDefault = model.Field<int>("IsDefault"),
                                                ExpiryMonth = model.Field<int>("ExpiryMonth"),
                                                ExpiryYear = model.Field<int>("ExpiryYear"),
                                                CardFundMethod = model.Field<string>("CardFundMethod"),
                                                CardIssuer = model.Field<string>("CardIssuer")
                                            }).ToList();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = _customerPaymentMethods };

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

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> UpdateEmailSubscriptionDetails(int CustomerID, int EmailSubscription)

        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@EmailSubscription",  SqlDbType.Int )
                };

                parameters[0].Value = CustomerID;
                parameters[1].Value = EmailSubscription;
                _DataHelper = new DataAccessHelper("Customers_UpdateEmailSubscriptionDetails", parameters, _configuration);

                var dt = new DataTable();

                var result = await _DataHelper.RunAsync(dt); // 105 /119

                DatabaseResponse response;

                if (result == 105)
                {
                    response = new DatabaseResponse { ResponseCode = result, Results = EnumExtensions.GetDescription(DbReturnValue.UpdateSuccess) };
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

                throw;
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

                _DataHelper = new DataAccessHelper("Customers_GetOrders", parameters, _configuration);

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
                            orderDetails.ListingStatus = dr["ListingStatus"].ToString();
                            orderDetails.OrderNumber = dr["OrderNumber"].ToString();
                            orderDetails.OrderDate = Convert.ToDateTime(dr["OrderDate"]);
                            orderDetails.OrderStatus = dr["OrderStatus"].ToString();
                            orderDetails.AllowRescheduling = Convert.ToInt32(dr["AllowRescheduling"].ToString());
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
                            orderDetails.InvoiceNumber = dr["InvoiceNumber"].ToString();
                            orderDetails.MaskedCardNumber = dr["MaskedCardNumber"].ToString();
                            orderDetails.CardBrand = dr["CardBrand"].ToString();
                            orderDetails.ExpiryMonth = Convert.ToInt32(dr["ExpiryMonth"].ToString());
                            orderDetails.ExpiryYear = Convert.ToInt32(dr["ExpiryYear"].ToString());
                            try { orderDetails.PaymentOn = Convert.ToDateTime(dr["PaymentOn"]); }
                            catch { }
                            List<Subscribers> orderSubscribers = new List<Subscribers>();
                            foreach (DataRow osdr in ds.Tables[1].Rows)
                            {
                                Subscribers _subscriber = new Subscribers();
                                _subscriber.OrderID = Convert.ToInt32(osdr["OrderID"]);
                                _subscriber.SubscriberID = Convert.ToInt32(osdr["SubscriberID"]);
                                _subscriber.OrderSubscriberID = Convert.ToInt32(osdr["OrderSubscriberID"]);
                                _subscriber.DisplayName = osdr["DisplayName"].ToString();
                                _subscriber.MobileNumber = osdr["MobileNumber"].ToString();
                                _subscriber.IsPrimary = Convert.ToInt32(osdr["IsPrimary"]);
                                try
                                { _subscriber.ActivateOn = Convert.ToDateTime(osdr["ActivateOn"]); }
                                catch { }
                                try
                                { _subscriber.DepositFee = Convert.ToDouble(osdr["DepositFee"]); }
                                catch { }
                                _subscriber.IsBuddyLine = Convert.ToInt32(osdr["IsBuddyLine"]);
                                _subscriber.PremiumType = Convert.ToInt32(osdr["PremiumType"]);
                                _subscriber.PremiumName = osdr["PremiumName"].ToString();
                                _subscriber.IsPorted = Convert.ToInt32(osdr["IsPorted"]);

                                List<Bundle> orderBundles = new List<Bundle>();

                                if (ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                                {
                                    orderBundles = (from model in ds.Tables[2].AsEnumerable()
                                                    select new Bundle()
                                                    {
                                                        OrderID = model.Field<int>("OrderID"),
                                                        OrderSubscriberID = model.Field<int>("OrderSubscriberID"),
                                                        BundleID = model.Field<int>("BundleID"),
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
                                                    }).Where(c => c.OrderSubscriberID == _subscriber.OrderSubscriberID).ToList();

                                    _subscriber.Bundles = orderBundles;
                                }
                                orderSubscribers.Add(_subscriber);
                            }

                            orderDetails.Subscribers = orderSubscribers;

                            List<ServiceCharge> orderServiceCharges = new List<ServiceCharge>();

                            if (ds.Tables[3] != null && ds.Tables[3].Rows.Count > 0)
                            {

                                orderServiceCharges = (from model in ds.Tables[3].AsEnumerable()
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
                            List<OrderStatuses> OrderStatuses = new List<OrderStatuses>();

                            if (ds.Tables[4] != null && ds.Tables[4].Rows.Count > 0)
                            {

                                OrderStatuses = (from model in ds.Tables[4].AsEnumerable()
                                                       select new OrderStatuses()
                                                       {
                                                           OrderID = model.Field<int>("OrderID"),
                                                           OrderStatus = model.Field<string>("OrderStatus"),
                                                           UpdatedOn = model.Field<DateTime?>("UpdatedOn")
                                                       }).Where(c => c.OrderID == orderDetails.OrderID).ToList();

                                orderDetails.OrderStatuses = OrderStatuses;
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

        public async Task<DatabaseResponse> GetRewardSummary(int CustomerID)
        {
            try
            {
                DatabaseResponse response = new DatabaseResponse();
                SqlParameter[] parameters =
                   {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),

                };

                parameters[0].Value = CustomerID;
                _DataHelper = new DataAccessHelper("Customers_GetAccountID", parameters, _configuration);

                var dt = new DataTable();

                var result = await _DataHelper.RunAsync(dt); // 105 /119
                if (result == 105)
                {
                    int AccountID = -1;
                    int.TryParse(dt.Rows[0][0].ToString().Trim(), out AccountID);
                    DatabaseResponse configResponse = ConfigHelper.GetValueByKey("RewardSummaryUrl", _configuration);
                    RewardHelper _helper = new RewardHelper();
                    Rewards _rewards = _helper.GetRewardSummary(configResponse.Results.ToString().Trim(), AccountID);
                    response = new DatabaseResponse { ResponseCode = 105, Results = _rewards };
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

        public async Task<DatabaseResponse> GetRewardDetails(int CustomerID, DateTime FromDate, DateTime ToDate)
        {
            try
            {
                DatabaseResponse response = new DatabaseResponse();
                SqlParameter[] parameters =
                   {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),

                };

                parameters[0].Value = CustomerID;
                _DataHelper = new DataAccessHelper("Customers_GetAccountID", parameters, _configuration);

                var dt = new DataTable();

                var result = await _DataHelper.RunAsync(dt); // 105 /119
                if (result == 105)
                {
                    int AccountID = -1;
                    int.TryParse(dt.Rows[0][0].ToString().Trim(), out AccountID);
                    DatabaseResponse configResponse = ConfigHelper.GetValueByKey("RewardDetailsUrl", _configuration);
                    RewardHelper _helper = new RewardHelper();
                    RewardDetails _rewards = _helper.GetRewardDetails(configResponse.Results.ToString().Trim(), AccountID, FromDate, ToDate);
                    response = new DatabaseResponse { ResponseCode = 105, Results = _rewards };
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

        public async Task<DatabaseResponse> UpdateBillingDetails(int CustomerID, customerBilling _billing)

        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@BillingBuildingNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@BillingBuildingName",  SqlDbType.NVarChar ),
                    new SqlParameter( "@BillingUnit",  SqlDbType.NVarChar ),
                    new SqlParameter( "@BillingFloor",  SqlDbType.NVarChar ),
                    new SqlParameter( "@BillingStreetName",  SqlDbType.NVarChar ),
                    new SqlParameter( "@BillingPostCode",  SqlDbType.NVarChar ),
                    new SqlParameter( "@BillingContactNumber",  SqlDbType.NVarChar ),
                };

                parameters[0].Value = CustomerID;
                parameters[1].Value = _billing.BillingBuildingNumber;
                parameters[2].Value = _billing.BillingBuildingName;
                parameters[3].Value = _billing.BillingUnit;
                parameters[4].Value = _billing.BillingFloor;
                parameters[5].Value = _billing.BillingStreetName;
                parameters[6].Value = _billing.BillingPostCode;
                parameters[7].Value = _billing.BillingContactNumber;
                _DataHelper = new DataAccessHelper("Customers_UpdateBillingDetails", parameters, _configuration);

                var dt = new DataTable();

                var result = await _DataHelper.RunAsync(dt); // 105 /119

                DatabaseResponse response;

                if (result == 105)
                {             
                    response = new DatabaseResponse { ResponseCode = result, Results = EnumExtensions.GetDescription(DbReturnValue.UpdateSuccess) };
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

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> GetBasePlan(int CustomerID, string MobileNumber)
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

                _DataHelper = new DataAccessHelper("Customers_GetBaseBundle", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 105 /102

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {
                    var _plan = new List<BasePlans>();

                    if (dt.Rows.Count > 0)
                    {

                        _plan = (from model in dt.AsEnumerable()
                                     select new BasePlans()
                                     {
                                         BundleID = model.Field<int>("BundleID"),
                                         PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                         PortalSummaryDescription = model.Field<string>("PortalSummaryDescription"),
                                         PortalDescription = model.Field<string>("PortalDescription"),
                                         MobileNumber = model.Field<string>("MobileNumber"),
                                         TotalData = model.Field<double>("TotalData"),
                                         TotalSMS = model.Field<double>("TotalSMS"),
                                         TotalVoice = model.Field<double>("TotalVoice"),
                                         ActualSubscriptionFee = model.Field<double>("ActualSubscriptionFee"),
                                         ApplicableSubscriptionFee = model.Field<double>("ApplicableSubscriptionFee"),
                                     }).ToList();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = _plan };
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

        public async Task<DatabaseResponse> UpdateDisplayName(int CustomerID, DisplayDetails details)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@DisplayName",  SqlDbType.NVarChar )
                };

                parameters[0].Value = CustomerID;
                parameters[1].Value = details.MobileNumber;
                parameters[2].Value = details.DisplayName;
                _DataHelper = new DataAccessHelper("Customers_UpdateDisplayDetails", parameters, _configuration);

                var dt = new DataTable();

                var result = await _DataHelper.RunAsync(dt); // 105 /119

                DatabaseResponse response;

                if (result == 105)
                {
                    response = new DatabaseResponse { ResponseCode = result, Results = EnumExtensions.GetDescription(DbReturnValue.UpdateSuccess) };
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

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> UpdateSMSSubscriptionDetails(int CustomerID, string MobileNumber, int SMSSubscription)

        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.Int ),
                    new SqlParameter( "@SMSSubscription",  SqlDbType.Int )
                };

                parameters[0].Value = CustomerID;
                parameters[1].Value = MobileNumber;
                parameters[2].Value = SMSSubscription;
                _DataHelper = new DataAccessHelper("Customers_UpdateSMSSubscriptionDetails", parameters, _configuration);

                var dt = new DataTable();

                var result = await _DataHelper.RunAsync(dt); // 105 /119

                DatabaseResponse response;

                if (result == 105)
                {
                    response = new DatabaseResponse { ResponseCode = result, Results = EnumExtensions.GetDescription(DbReturnValue.UpdateSuccess) };
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

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> UpdateVoiceSubscriptionDetails(int CustomerID, string MobileNumber, int VoiceSubscription)

        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.Int ),
                    new SqlParameter( "@VoiceSubscription",  SqlDbType.Int )
                };

                parameters[0].Value = CustomerID;
                parameters[1].Value = MobileNumber;
                parameters[2].Value = VoiceSubscription;
                _DataHelper = new DataAccessHelper("Customers_UpdateVoiceSubscriptionDetails", parameters, _configuration);

                var dt = new DataTable();

                var result = await _DataHelper.RunAsync(dt); // 105 /119

                DatabaseResponse response;

                if (result == 105)
                {
                    response = new DatabaseResponse { ResponseCode = result, Results = EnumExtensions.GetDescription(DbReturnValue.UpdateSuccess) };
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

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }
        
        public async Task<DatabaseResponse> GetCustomerShippingDetails(int CustomerID)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),

                };

                parameters[0].Value = CustomerID;
                _DataHelper = new DataAccessHelper("Customers_GetShippingDetails", parameters, _configuration);

                var dt = new DataTable();

                var result = await _DataHelper.RunAsync(dt); // 105 /119

                DatabaseResponse response;

                if (result == 105)
                {

                    var _customerShipping = new customerShipping();

                    if (dt.Rows.Count > 0)
                    {

                        _customerShipping = (from model in dt.AsEnumerable()
                                            select new customerShipping()
                                            {
                                                ShippingUnit = model.Field<string>("ShippingUnit"),
                                                ShippingFloor = model.Field<string>("ShippingFloor"),
                                                ShippingStreetName = model.Field<string>("ShippingStreetName"),
                                                ShippingBuildingNumber = model.Field<string>("ShippingBuildingNumber"),
                                                ShippingBuildingName = model.Field<string>("ShippingBuildingName"),
                                                ShippingContactNumber = model.Field<string>("ShippingContactNumber"),
                                                ShippingPostCode = model.Field<string>("ShippingPostCode")
                                            }).FirstOrDefault();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = _customerShipping };

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

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> ValidatePassword(LoginDto request)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@Email",  SqlDbType.VarChar ),
                    new SqlParameter( "@Password",  SqlDbType.VarChar )
                };

                parameters[0].Value = request.Email;
                parameters[1].Value = new Sha2().Hash(request.Password);

                _DataHelper = new DataAccessHelper("Customer_ValidatePassword", parameters, _configuration);                

                int result = await _DataHelper.RunAsync();

                return new DatabaseResponse { ResponseCode = result };

            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw ex;

            }
            finally
            {
                _DataHelper.Dispose();
            }
        }
    }
}
