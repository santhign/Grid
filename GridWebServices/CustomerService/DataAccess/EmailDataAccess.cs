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
using Core.Models;


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

        public async Task<DatabaseResponse> GetForgetPassword(string email)
        {
            try
            {
                SqlParameter[] parameters =
                 {
                    new SqlParameter( "@EmailAddress",  SqlDbType.VarChar )
                };

                parameters[0].Value = email;

                _DataHelper = new DataAccessHelper("Customer_ForgotPassword", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); //111/107/109

                ForgetPassword changePasswordToken = new ForgetPassword();

                DatabaseResponse response = new DatabaseResponse();

                if (dt != null && dt.Rows.Count > 0)
                {

                    changePasswordToken = (from model in dt.AsEnumerable()
                                           select new ForgetPassword()
                                           {
                                               CustomerId = model.Field<int>("CustomerID"),
                                               Token = model.Field<string>("Token"),
                                               Name= model.Field<string>("Name"),
                                               Email = model.Field<string>("Email")
                                           }).FirstOrDefault();

                    response = new DatabaseResponse { ResponseCode = result, Results = changePasswordToken };
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

                throw ex;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

     
    }
}
