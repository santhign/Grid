using AdminService.Models;
using Core.Enums;
using Core.Helpers;
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

        public async Task<AdminUsers> GetLoginAuthentication(AdminUserLoginRequest users)
        {
            try
            {
                SqlParameter[] parameters =
                 {
                    new SqlParameter( "@Email",  SqlDbType.VarChar ),
                    new SqlParameter( "@Password",  SqlDbType.VarChar )
                };

                parameters[0].Value = users.Email ;
                parameters[1].Value = users.Password;

                _DataHelper = new DataAccessHelper("Admin_AuthenticateAdminUser", parameters, _configuration);

                DataTable dt = new DataTable();

                _DataHelper.Run(dt);

                  List<AdminUsers> adminuser = new List<AdminUsers>();

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

                                 }).ToList();
                }

                return adminuser[0];

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
