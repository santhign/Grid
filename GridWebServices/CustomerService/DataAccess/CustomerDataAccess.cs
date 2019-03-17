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
using CustomerService.Models;
using Serilog;


namespace CustomerService.DataAccess
{
    public class CustomerDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public CustomerDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<DatabaseResponse> CreateCustomer( RegisterCustomer customer)
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

                int result = _DataHelper.Run(dt);

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

                return new DatabaseResponse { ResponseCode = result, Results = customer };
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

        public async Task<List<Customer>> GetCustomers()
        {
            try
            {

                _DataHelper = new DataAccessHelper("Admin_GetCustomerListing", _configuration);

                DataTable dt = new DataTable();

                _DataHelper.Run(dt);

                List<Customer> customerList = new List<Customer>();

                if (dt.Rows.Count > 0)
                {

                    customerList  = (from model in dt.AsEnumerable()
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
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<Customer> GetCustomer(int customerId)
        {
            try
            {

                _DataHelper = new DataAccessHelper("Admin_GetCustomerListing", _configuration);

                DataTable dt = new DataTable();

                _DataHelper.Run(dt);

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
                                    }).Where(c=>c.CustomerID==customerId).FirstOrDefault();
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
