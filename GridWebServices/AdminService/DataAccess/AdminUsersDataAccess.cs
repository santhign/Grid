﻿using AdminService.Models;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using InfrastructureService;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AdminService.DataAccess
{
    public class AdminUsersDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        public AdminUsersDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<DatabaseResponse> GetLoginAuthentication(AdminUserLoginRequest users)
        {
            try
            {
                SqlParameter[] parameters =
                 {
                    new SqlParameter( "@Email",  SqlDbType.VarChar ),
                    new SqlParameter( "@Password",  SqlDbType.VarChar )
                };

                parameters[0].Value = users.Email;
                parameters[1].Value = new Sha2().Hash(users.Password);

                _DataHelper = new DataAccessHelper("Admin_AuthenticateAdminUser", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt);

                AdminUsers adminuser = new AdminUsers();

                if (dt != null && dt.Rows.Count > 0)
                {

                    adminuser = (from model in dt.AsEnumerable()
                                 select new AdminUsers()
                                 {
                                     AdminUserID = model.Field<int>("AdminUserID"),
                                     Email = model.Field<string>("Email"),
                                     Password = model.Field<string>("Password"),
                                     Name = model.Field<string>("Name"),
                                     Role = model.Field<string>("Role"),

                                 }).FirstOrDefault();
                }

                return new DatabaseResponse { ResponseCode = result, Results = adminuser };
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
        public async Task<DatabaseResponse> LogAdminUserToken(int adminuserId, string token)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@AdminUserID",  SqlDbType.Int ),
                     new SqlParameter( "@Token",  SqlDbType.NVarChar )

                };
                parameters[0].Value = adminuserId;
                parameters[1].Value = token;

                _DataHelper = new DataAccessHelper("AdminUser_CreateToken", parameters, _configuration);

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
                                             CustomerID = model.Field<int>("AdminUserID"),

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

        public async Task<DatabaseResponse> CreateAdminUser(RegisterAdminUser adminuser, int AdminUserID)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@Email",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Password",  SqlDbType.NVarChar ),
                    new SqlParameter( "@RoleID", SqlDbType.Int),
                    new SqlParameter( "@CreatedBy", SqlDbType.Int)
                };

                parameters[0].Value = adminuser.Email;
                parameters[1].Value = new Sha2().Hash(adminuser.Password);
                parameters[2].Value = adminuser.RoleID;
                parameters[3].Value = AdminUserID;

                _DataHelper = new DataAccessHelper("Admin_CreateAdminUser", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt);

                AdminUsers newCustomer = new AdminUsers();

                if (dt != null && dt.Rows.Count > 0)
                {

                    newCustomer = (from model in dt.AsEnumerable()
                                   select new AdminUsers()
                                   {
                                       AdminUserID = model.Field<int>("AdminUserID"),
                                       Email = model.Field<string>("Email"),
                                       Password = model.Field<string>("Password"),
                                       Name = model.Field<string>("Name"),
                                       Role = model.Field<string>("Role"),
                                   }).FirstOrDefault();
                }

                return new DatabaseResponse { ResponseCode = result, Results = adminuser };
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

        public async Task<AdminUsers> GetAdminUser(int userId)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@AdminUserID",  SqlDbType.Int )
                };

                parameters[0].Value = userId;

                _DataHelper = new DataAccessHelper("Admin_GetAdminUserByID", parameters,_configuration);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                AdminUsers adminuser = new AdminUsers();

                if (dt.Rows.Count > 0)
                {


                    adminuser = (from model in dt.AsEnumerable()
                                 select new AdminUsers()
                                 {
                                     AdminUserID = model.Field<int>("AdminUserID"),
                                     Name = model.Field<string>("Name"),
                                     Email = model.Field<string>("Email"),
                                     Password = model.Field<string>("Password"),
                                     Role = model.Field<string>("Role")
                                 }).Where(c => c.AdminUserID == userId).FirstOrDefault();
                }

                return adminuser;
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

        public async Task<List<AdminUsers>> GetAdminusers()
        {
            try
            {                
                _DataHelper = new DataAccessHelper("Admin_GetAllAdminUsers", _configuration);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                List<AdminUsers> adminusersList = new List<AdminUsers>();

                if (dt.Rows.Count > 0)
                {

                    adminusersList = (from model in dt.AsEnumerable()
                                    select new AdminUsers ()
                                    {
                                        AdminUserID = model.Field<int>("AdminUserID"),
                                        Name = model.Field<string>("Name"),
                                        Email = model.Field<string>("Email"),
                                        Password = model.Field<string>("Password"),
                                        Role = model.Field<string>("Role")
                                    }).ToList();
                }

                return adminusersList;
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

        public async Task<List<Roles>> GetAdminRoles()
        {
            try
            {
                _DataHelper = new DataAccessHelper("Admin_GetAllAdminRoles", _configuration);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                List<Roles> adminRoles = new List<Roles>();

                if (dt.Rows.Count > 0)
                {

                    adminRoles = (from model in dt.AsEnumerable()
                                      select new Roles()
                                      {
                                          RoleID = model.Field<int>("RoleID"),
                                          Role = model.Field<string>("Role")
                                      }).ToList();
                }

                return adminRoles;
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


        public async Task<DatabaseResponse> UpdateAdminUser(AdminProfile adminuser)
        {
            try
            {

                SqlParameter[] parameters =
               {
                     new SqlParameter("@AdminID", SqlDbType.Int),
                    new SqlParameter( "@ExistingPassword",  SqlDbType.NVarChar ),
                    new SqlParameter( "@NewPassword",  SqlDbType.NVarChar ) 
                };
  
                parameters[0].Value = adminuser.AdminUserID;
                parameters[1].Value = new Sha2().Hash(adminuser.ExistingPassword);
                parameters[2].Value = new Sha2().Hash(adminuser.NewPassword); 

                _DataHelper = new DataAccessHelper("Admin_UpdateProfile", parameters, _configuration);
                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); 

                return new DatabaseResponse { ResponseCode = result, Results = adminuser };
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

        public async Task<DatabaseResponse> AuthenticateAdminUserToken(string token)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@Token",  SqlDbType.NVarChar )

                };

                parameters[0].Value = token;

                _DataHelper = new DataAccessHelper("AdminUser_AuthenticateToken", parameters, _configuration);

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
                                             CustomerID = model.Field<int>("AdminUserID"),

                                             CreatedOn = model.Field<DateTime>("CreatedOn")


                                         }).FirstOrDefault();
                    }


                    if (tokenResponse.CreatedOn < DateTime.UtcNow.AddDays(-7))
                    {
                        tokenResponse.IsExpired = true;
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

        public async Task<DatabaseResponse> ValidatePassword(int AdminUserID, string Password)
        {
            try
            {
                SqlParameter[] parameters =
                 {
                    new SqlParameter( "@AdminUserID",  SqlDbType.VarChar ),
                    new SqlParameter( "@Password",  SqlDbType.VarChar )
                };

                parameters[0].Value = AdminUserID;
                parameters[1].Value = new Sha2().Hash(Password);

                _DataHelper = new DataAccessHelper("Admin_ValidateAdminUserPassword", parameters, _configuration);
                
                int result = await _DataHelper.RunAsync();

                AdminUsers adminuser = new AdminUsers();

                if (result == 105)
                {
                    return new DatabaseResponse { ResponseCode = result, Results = "valid" };
                }
                else
                {
                    return new DatabaseResponse { ResponseCode = result, Results = "invalid" };
                }
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
