using AdminService.Models;
using Core.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Core.Enums;
using InfrastructureService;

namespace AdminService.DataAccess
{
    public class ConfigDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public ConfigDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        /// <summary>
        /// API to get the generic config values.
        /// </summary>
        /// <param name="ConfigKey"></param>        
        /// <returns></returns>
        public async Task<List<Config>> GetConfigValue(string ConfigKey)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@ConfigKey",  SqlDbType.VarChar )
                };

                parameters[0].Value = ConfigKey;

                _DataHelper = new DataAccessHelper("Admin_GetGenericConfigValue", parameters, _configuration);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                List<Config> statusList = new List<Config>();

                if (dt.Rows.Count > 0)
                {

                    statusList = (from model in dt.AsEnumerable()
                                  select new Config()
                                  {
                                      ConfigValue = model.Field<string>("ConfigValue"),

                                  }).ToList();
                }

                return statusList;
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

        public async Task<List<Config>> GetConfigValue(string ConfigKey, string Token)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@ConfigKey",  SqlDbType.VarChar ),

                    new SqlParameter( "@Token",  SqlDbType.VarChar )
                };

                parameters[0].Value = ConfigKey;

                parameters[1].Value = Token;

                _DataHelper = new DataAccessHelper("Admin_GetConfigValue", parameters, _configuration);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                List<Config> statusList = new List<Config>();

                if (dt.Rows.Count > 0)
                {

                    statusList = (from model in dt.AsEnumerable()
                                  select new Config()
                                  {
                                      ConfigValue = model.Field<string>("ConfigValue"),

                                  }).ToList();
                }

                return statusList;
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
