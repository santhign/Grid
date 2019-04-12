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
                                        SMSSubscription = model.Field<string>("SMSSubscription"),
                                        EmailSubscription = model.Field<string>("EmailSubscription"),
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
                                    SMSSubscription = model.Field<string>("SMSSubscription"),
                                    EmailSubscription = model.Field<string>("EmailSubscription"),
                                    Status = model.Field<string>("Status")
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
    }
}
