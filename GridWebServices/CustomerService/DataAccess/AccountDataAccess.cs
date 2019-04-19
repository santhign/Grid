﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Core.Helpers;
using Core.Models;
using Core.Enums;
using CustomerService.Models;
using InfrastructureService;


namespace CustomerService.DataAccess
{
    public class AccountDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private readonly IConfiguration _configuration;

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

                _DataHelper = new DataAccessHelper("Customer_AuthenticateCustomer", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt);

                Customer customer = new Customer();

                if (dt != null && dt.Rows.Count > 0)
                {

                    customer = (from model in dt.AsEnumerable()
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
                                }).FirstOrDefault();
                }


                return new DatabaseResponse { ResponseCode = result, Results = customer };

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
       
        public async Task<DatabaseResponse> LogCustomerToken(int customerId, string token)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                     new SqlParameter( "@Token",  SqlDbType.NVarChar )

                };
                parameters[0].Value = customerId;
                parameters[1].Value = token;

                _DataHelper = new DataAccessHelper("Customer_CreateToken", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 100 /105

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

        public async Task<DatabaseResponse> ValidateResetPasswordToken(string token)
        {
            try
            {
                SqlParameter[] parameters =
               {                  
                     new SqlParameter( "@Token",  SqlDbType.NVarChar )

                };

                 parameters[0].Value = token;               

                _DataHelper = new DataAccessHelper("Customer_ValidateResetPasswordToken", parameters, _configuration);               

                int result = await _DataHelper.RunAsync(); // 124 /125

                DatabaseResponse response = new DatabaseResponse();

                response = new DatabaseResponse { ResponseCode = result };                

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
        public async Task<DatabaseResponse> ResetPassword(ResetPassword resetPassword)
        {
            try
            {

                SqlParameter[] parameters =
               {                   
                    new SqlParameter( "@Token",  SqlDbType.NVarChar ),

                    new SqlParameter( "@Password",  SqlDbType.NVarChar)
                };

                parameters[0].Value = resetPassword.ResetToken;

                parameters[1].Value = new Sha2().Hash(resetPassword.NewPassword);                

                _DataHelper = new DataAccessHelper("Customer_ResetPassword", parameters, _configuration);               

                int result = await _DataHelper.RunAsync(); 
                
                return new DatabaseResponse { ResponseCode = result };
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
