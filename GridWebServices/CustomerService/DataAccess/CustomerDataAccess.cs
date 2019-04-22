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
                                    JoinedOn = model.Field<DateTime>("JoinedOn")
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
        /// <param name="customer">The customer.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> UpdateCustomerProfile(CustomerProfile customer)
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

                parameters[0].Value = customer.CustomerId;
                parameters[1].Value = new Sha2().Hash(customer.Password);
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
                                         CustomerID = model.Field<int>("CustomerID"),
                                         PlanId = model.Field<int>("PlanID"),
                                         PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                         PortalSummaryDescription = model.Field<string>("PortalSummaryDescription"),
                                         PortalDescription = model.Field<string>("PortalDescription"),
                                         PlanStatus = model.Field<string>("PlanStatus"),
                                         MobileNumber = model.Field<string>("MobileNumber"),
                                         SubscriptionType = model.Field<string>("SubscriptionType"),
                                         IsRecurring = model.Field<int>("IsRecurring"),
                                         ExpiryDate = model.Field<DateTime?>("ExpiryDate"),
                                         Removable = model.Field<int>("Removable"),
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
                                         CustomerID = model.Field<int>("CustomerID"),
                                         PlanId = model.Field<int>("PlanID"),
                                         PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                         PortalSummaryDescription = model.Field<string>("PortalSummaryDescription"),
                                         PortalDescription = model.Field<string>("PortalDescription"),
                                         PlanStatus = model.Field<string>("PlanStatus"),
                                         MobileNumber = model.Field<string>("MobileNumber"),
                                         SubscriptionType = model.Field<string>("SubscriptionType"),
                                         IsRecurring = model.Field<int>("IsRecurring"),
                                         ExpiryDate = model.Field<DateTime?>("ExpiryDate"),
                                         Removable = model.Field<int>("Removable"),
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
                                          State = model.Field<string>("State")
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

        public async Task<DatabaseResponse> UpdateSubscriptionDetails(int CustomerID, customerSubscription _subscription)

        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@EmailSubscription",  SqlDbType.Int ),
                    new SqlParameter( "@SMSSubscription",  SqlDbType.Int ),
                };

                parameters[0].Value = CustomerID;
                _DataHelper = new DataAccessHelper("Customers_UpdateSubscriptionDetails", parameters, _configuration);

                var dt = new DataTable();

                var result = await _DataHelper.RunAsync(dt); // 105 /119

                DatabaseResponse response;

                if (result == 105)
                {

                    var _customer = new List<Customer>();

                    if (dt.Rows.Count > 0)
                    {

                        _customer = (from model in dt.AsEnumerable()
                                                   select new Customer()
                                                   {
                                                       CustomerID = model.Field<int>("CustomerID"),
                                                       Email = model.Field<string>("Email"),
                                                       Password = model.Field<string>("Password"),
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
                                                       JoinedOn = model.Field<DateTime>("JoinedOn")
                                                   }).ToList();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = _customer };

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
    }
}
