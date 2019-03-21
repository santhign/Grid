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
    public class FaqPageDataAccess
    {

        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        public FaqPageDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<FaqPages>> GetPageFAQ(string Pagename)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@PageName",  SqlDbType.VarChar )
                };

                parameters[0].Value = Pagename;

                _DataHelper = new DataAccessHelper("Admin_GetPageFAQ", parameters, _configuration);

                DataTable dt = new DataTable();

                _DataHelper.Run(dt);

                List<FaqPages> FaqPagesList = new List<FaqPages>(); 

                if (dt.Rows.Count > 0)
                {

                    FaqPagesList = (from model in dt.AsEnumerable()
                                  select new FaqPages()
                                  {
                                      Title= model.Field<string>("Title"),
                                      Description= model.Field<string>("Description"),
                                      FAQCategory= model.Field<string>("FAQCategory"),
                                      SortOrder = model.Field<Int32>("SortOrder"),

                                  }).ToList();
                }

                return FaqPagesList;
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
