using System;
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
    public class AccountDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public AccountDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<DatabaseResponse> AuthenticateCustomer(LoginDto request)
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
 
               _DataHelper =  new DataAccessHelper("Customer_AuthenticateCustomer", parameters, _configuration);

                DataTable dt = new DataTable();

                int result=  _DataHelper.Run(dt);

                Customer customer = new Customer();

                if (dt!=null && dt.Rows.Count > 0)
                {

                    customer = (from model in dt.AsEnumerable()
                                select new Customer()
                                {
                                   CustomerID = model.Field<int>("CustomerID"),
                                   Email =  model.Field<string>("Email"),
                                   Password= model.Field<string>("Password"),
                                   MobileNumber = model.Field<string>("MobileNumber"),
                                    ReferralCode = model.Field<string>("ReferralCode"),
                                    Nationality = model.Field<string>("Nationality"),
                                    Gender = model.Field<string>("Gender"),
                                    SMSSubscription = model.Field<string>("SMSSubscription"),
                                    EmailSubscription = model.Field<string>("EmailSubscription"),
                                    Status = model.Field<string>("Status")
                                }).FirstOrDefault();                    
                }


                    return  new DatabaseResponse {   ResponseCode=result, Results= customer };
                
            }

            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw ex;
                
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

    }
}
