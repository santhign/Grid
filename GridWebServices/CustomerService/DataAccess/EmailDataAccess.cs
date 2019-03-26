using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomerService.Models;
using Core.Helpers;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using InfrastructureService;
using Core.Enums;


namespace CustomerService.DataAccess
{
    public class EmailDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public EmailDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ForgetPassword> GetForgetPassword(Emails emails)
        {
            try
            {
                SqlParameter[] parameters =
                 {
                    new SqlParameter( "@EmailAddress",  SqlDbType.VarChar ) 
                };

                parameters[0].Value = emails.EmailId;

                _DataHelper = new DataAccessHelper("Customer_ForgotPassword", parameters, _configuration);

                DataTable dt = new DataTable();

                _DataHelper.Run(dt);

                List< ForgetPassword> _forgetPassword = new List<ForgetPassword>();

                if (dt != null && dt.Rows.Count > 0)
                {

                    _forgetPassword = (from model in dt.AsEnumerable()
                                 select new ForgetPassword ()
                                 {
                                     CustomerId = model.Field<int>("CustomerId"),
                                     Token = model.Field<string>("Token") 
                                 }).ToList();
                }

                return _forgetPassword[0];

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
