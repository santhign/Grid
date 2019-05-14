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
using Serilog;
using InfrastructureService;

namespace AdminService.DataAccess
{
    public class BannerDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public BannerDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>       
        /// <returns></returns>
        public async Task<List<Banners>> GetBannerDetails(BannerDetailsRequest request)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@LocationName",  SqlDbType.VarChar ),
                    new SqlParameter( "@PageName",  SqlDbType.VarChar )
                };

                parameters[0].Value = request.LocationName;
                parameters[1].Value = request.PageName;

                _DataHelper = new DataAccessHelper("Admin_GetBannerDetails", parameters, _configuration);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                List<Banners> statusList = new List<Banners>();

                if (dt != null && dt.Rows.Count > 0)
                {

                    statusList = (from model in dt.AsEnumerable()
                                  select new Banners()
                                  {
                                      BannerImage = model.Field<string>("BannerImage"),
                                      BannerUrl = model.Field<string>("BannerUrl"),
                                      UrlType = model.Field<int>("UrlType").ToString(),

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
