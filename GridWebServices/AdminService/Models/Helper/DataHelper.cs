using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using AdminService.DataAccess;
using Microsoft.Extensions.Configuration;



namespace AdminService.Models.Helper
{
    /// <summary>
    /// Helper class to pass SAP helper to call
    /// </summary>
    public class DataHelper
    {
        internal SPHelper spHelper = null;

        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public DataHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>       
        /// <returns></returns>
        public async Task<List<BannerDetails>> GetBannerDetails(BannerDetailsRequest request)
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

                spHelper = new SPHelper("Admin_GetBannerDetails", parameters, _configuration);

                DataTable dt = new DataTable();

                spHelper.Run(dt);                

                List<BannerDetails> statusList = new List<BannerDetails>();

                if (dt.Rows.Count > 0)
                {

                    statusList = (from model in dt.AsEnumerable()
                                  select new BannerDetails()
                                  {
                                      BannerImage = model.Field<string>("BannerImage"),
                                      BannerUrl = model.Field<string>("BannerUrl"),
                                      UrlType= model.Field<int>("UrlType").ToString(),

                                  }).ToList();
                }

                return statusList;
            }

            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                spHelper.Dispose();
            }
        }
       
    }
}
