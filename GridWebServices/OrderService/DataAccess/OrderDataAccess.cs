using OrderService.Models;
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
using Core.Models;

namespace OrderService.DataAccess
{
    public class OrderDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public OrderDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// This method will create a new order for createOrder endpoint
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<DatabaseResponse> CreateOrder(CreateOrder order)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.NVarChar ),
                    new SqlParameter( "@BundleID",  SqlDbType.NVarChar ),
                    new SqlParameter( "@ReferralCode",  SqlDbType.NVarChar)
                };

                parameters[0].Value = order.CustomerID;
                parameters[1].Value = order.BundleID;
                parameters[2].Value = order.ReferralCode;

                _DataHelper = new DataAccessHelper("Orders_CreateOrder", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run();

                return new DatabaseResponse { ResponseCode = result};
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
