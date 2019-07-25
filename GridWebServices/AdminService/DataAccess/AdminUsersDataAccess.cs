using AdminService.Models;
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

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds);

                AdminUsers adminuser = new AdminUsers();

                if (ds != null && ds.Tables[0]!=null && ds.Tables[0].Rows.Count > 0)
                {

                    adminuser = (from model in ds.Tables[0].AsEnumerable()
                                 select new AdminUsers()
                                 {
                                     AdminUserID = model.Field<int>("AdminUserID"),
                                     Email = model.Field<string>("Email"),
                                     Password = model.Field<string>("Password"),
                                     Name = model.Field<string>("Name"),
                                     Role = model.Field<string>("Role"),

                                 }).FirstOrDefault();
                    List<Permission> permissionList = new List<Permission>();

                    if (ds.Tables[1]!=null && ds.Tables[0].Rows.Count>0)
                    {
                      permissionList= (from model in ds.Tables[1].AsEnumerable()
                                                  select new Permission()
                                                  {
                                                       RolePermission = model.Field<string>("Permission"),

                                                  }).ToList();
                    }

                    adminuser.Permissions= permissionList.Select(item => item.RolePermission).ToList();

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
                    new SqlParameter( "@FullName",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Email",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Password",  SqlDbType.NVarChar ),
                    new SqlParameter( "@RoleID", SqlDbType.Int),
                    new SqlParameter( "@CreatedBy", SqlDbType.Int)
                };

                parameters[0].Value = adminuser.FullName;
                parameters[1].Value = adminuser.Email;
                parameters[2].Value = new Sha2().Hash(adminuser.Password);
                parameters[3].Value = adminuser.RoleID;
                parameters[4].Value = AdminUserID;

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
                                        Role = model.Field<string>("Role"),
                                        Status = model.Field<int>("Status")
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


        public async Task<DatabaseResponse> UpdateAdminUser(AdminUserProfile adminuser)
        {
            try
            {

                SqlParameter[] parameters =
               {
                     new SqlParameter("@AdminID", SqlDbType.Int),
                    new SqlParameter( "@Name",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Email",  SqlDbType.NVarChar ),
                    new SqlParameter( "@NewPassword",  SqlDbType.NVarChar ) ,
                    new SqlParameter( "@RoleID",  SqlDbType.Int )
                };
  
                parameters[0].Value = adminuser.AdminUserID;
                parameters[1].Value = adminuser.Name;
                parameters[2].Value = adminuser.Email;
                parameters[3].Value = new Sha2().Hash(adminuser.NewPassword);
                parameters[4].Value = adminuser.RoleID; 

                _DataHelper = new DataAccessHelper("Admin_UpdateUserProfile", parameters, _configuration);
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


        public async Task<DatabaseResponse> UpdateAdminProfile(int AdminUserID, AdminProfile adminuser)
        {
            try
            {

                SqlParameter[] parameters =
               {
                     new SqlParameter("@AdminID", SqlDbType.Int),
                    new SqlParameter( "@Name",  SqlDbType.NVarChar ),
                    new SqlParameter( "@NewPassword",  SqlDbType.NVarChar ) 
                };

                parameters[0].Value = AdminUserID;
                parameters[1].Value = adminuser.Name;
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

                    DatabaseResponse configResponse = ConfigHelper.GetValueByKey(ConfigKeys.CustomerTokenExpiryInDays.ToString(), _configuration);

                    if (configResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                    {
                        if (tokenResponse.CreatedOn < DateTime.Now.AddDays(-int.Parse(configResponse.Results.ToString())))
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
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
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

        public async Task<DatabaseResponse> UpdateAdminAccountAccessibility(string Token, int AdminUserID, int Status)
        {
            try
            {
                SqlParameter[] parameters =
                 {
                    new SqlParameter( "@AdminUserID",  SqlDbType.Int ),
                    new SqlParameter( "@UpdatorToken",  SqlDbType.VarChar ),
                    new SqlParameter( "@Status",  SqlDbType.Int )
                };               

                parameters[0].Value = AdminUserID;
                parameters[1].Value = Token;
                parameters[2].Value = Status;

                _DataHelper = new DataAccessHelper("Admin_UpdateAdminAccountAccessibility", parameters, _configuration);

               int result = await _DataHelper.RunAsync(); //101,106,102,143 - admin token not matching
              
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

        public DatabaseResponse GetAdminUserPermissionsByToken(string token)
        {
            try
            {
                SqlParameter[] parameters =
                 {
                    new SqlParameter( "@AuthToken",  SqlDbType.VarChar ),
                };

                parameters[0].Value = token;

                _DataHelper = new DataAccessHelper("Admin_GetPermissionsByToken", parameters, _configuration);

                DataTable dt = new DataTable();

                int result =  _DataHelper.Run(dt);

                DatabaseResponse response = new DatabaseResponse();

                if (dt != null && dt.Rows.Count > 0)
                {

                    List<Permission> permissionList = new List<Permission>();

                    permissionList = (from model in dt.AsEnumerable()
                                      select new Permission()
                                      {
                                          RolePermission = model.Field<string>("Permission"),

                                      }).ToList();


                    List<string> permissions = permissionList.Select(item => item.RolePermission).ToList();

                    response = new DatabaseResponse { ResponseCode = result, Results = permissions };

                }

                else response = new DatabaseResponse { ResponseCode = result };

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
