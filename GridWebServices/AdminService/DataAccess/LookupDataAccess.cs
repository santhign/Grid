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
    public class LookupDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public LookupDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="LookupType"></param>       
        /// <returns></returns>
        public async Task<List<Lookup>> GetLookupList(string LookupType)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@LookupType",  SqlDbType.VarChar )
                };

                parameters[0].Value = LookupType;

                _DataHelper = new DataAccessHelper("Admin_GetLookup", parameters, _configuration);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                List<Lookup> statusList = new List<Lookup>();

                if (dt.Rows.Count > 0)
                {

                    statusList = (from model in dt.AsEnumerable()
                                  select new Lookup()
                                  {
                                      LookupText = model.Field<string>("LookupText"),

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

