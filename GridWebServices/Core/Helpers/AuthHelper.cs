using System;
using System.Collections.Generic;
using System.Text;
using Core.Models;
using Core.Enums;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

using System.Linq;
using Core.Extensions;
using Serilog;

namespace Core.Helpers
{
   public class AuthHelper
    {
        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        public AuthHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<OperationResponse> AuthenticateLoginToken(string token, string source = "")
        {
            var response = new OperationResponse();
            DatabaseResponse tokenAuthResponse;

            if (string.IsNullOrEmpty(token))
            {
                response.HasSucceeded = false;
                response.IsDomainValidationErrors = true;
                response.Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty);
                return response;
            }

            //Authenticate            
            try
            {
                tokenAuthResponse = await AuthenticateCustomerToken(token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Authentication DB call Failed");
                response.HasSucceeded = false;
                response.IsDomainValidationErrors = false;
                response.Message = StatusMessages.ServerError;
                return response;
            }
            
            if (tokenAuthResponse.ResponseCode != (int)DbReturnValue.AuthSuccess)
            {
                response.HasSucceeded = false;
                response.IsDomainValidationErrors = false;
                response.Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed);                
            }
            else if (((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
            {
                response.HasSucceeded = false;
                response.IsDomainValidationErrors = true;
                response.Message = EnumExtensions.GetDescription(DbReturnValue.TokenExpired);                
            }
            else
            {
                int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                response.HasSucceeded = true;
                response.IsDomainValidationErrors = true;
                response.Message = "Success";
                response._loginUserId = customerID;
            }
            return response;
        }

        public async Task<DatabaseResponse> AuthenticateCustomerToken(string token, string source="")
        {
            try
            {                
                

                if (string.IsNullOrWhiteSpace(source))
                {
                    SqlParameter[] parameters =
                    {
                        new SqlParameter( "@Token",  SqlDbType.NVarChar )
                    };
                    parameters[0].Value = token;
                    _DataHelper = new DataAccessHelper("Customer_AuthenticateToken", parameters, _configuration);
                }
                else {
                    SqlParameter[] parameters =
                    {
                        new SqlParameter("@Token",  SqlDbType.NVarChar),
                        new SqlParameter("@Source", SqlDbType.NVarChar)
                    };                    
                    parameters[0].Value = token;
                    parameters[1].Value = source;
                    _DataHelper = new DataAccessHelper("Customer_AuthenticateTokenwithSource", parameters, _configuration);
                }

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 111 /109

                DatabaseResponse response = new DatabaseResponse();

                AuthTokenResponse tokenResponse = new AuthTokenResponse();

                if (result == 111)
                {
                    if (dt != null && dt.Rows.Count > 0)
                    {

                        tokenResponse = (from model in dt.AsEnumerable()
                                         select new AuthTokenResponse()
                                         {
                                             CustomerID = model.Field<int>("CustomerID"),

                                             CreatedOn = model.Field<DateTime>("CreatedOn")


                                         }).FirstOrDefault();
                    }

                    DatabaseResponse configResponse = ConfigHelper.GetValueByKey(ConfigKeys.CustomerTokenExpiryInDays.ToString(), _configuration);

                    if (configResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                    {
                        if (tokenResponse.CreatedOn < DateTime.Now.AddDays(- int.Parse(configResponse.Results.ToString())))
                        {
                            tokenResponse.IsExpired = true;
                        }
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

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }
    }
}
