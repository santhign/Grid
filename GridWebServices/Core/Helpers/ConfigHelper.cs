using Core.Extensions;
using Core.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Core.Helpers
{
    public static class ConfigHelper
    {
        public static DatabaseResponse GetValue(string serviceCode, IConfiguration _configuration)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@ConfigType",  SqlDbType.NVarChar )

                };

                parameters[0].Value = serviceCode;

                DataAccessHelper _DataHelper = new DataAccessHelper("Admin_GetConfigurations", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run(dt); // 102 /105

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    List<Dictionary<string, string>> configDictionary = new List<Dictionary<string, string>>();

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        configDictionary = LinqExtensions.GetDictionary(dt);

                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = configDictionary };

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
        }

        public static DatabaseResponse GetValueByKey(string serviceKey, IConfiguration _configuration)
        {
            try
            {

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@ConfigKey",  SqlDbType.NVarChar )

                };

                parameters[0].Value = serviceKey;

                DataAccessHelper _DataHelper = new DataAccessHelper("Admin_GetConfigurationByKey", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run(dt); // 102 /105

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {
                    string ConfigValue = "";
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        ConfigValue = dt.Rows[0]["value"].ToString().Trim();
                    }
                    response = new DatabaseResponse { ResponseCode = result, Results = ConfigValue };
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
        }
    }
}
