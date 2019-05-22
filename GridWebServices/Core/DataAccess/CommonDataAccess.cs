using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Core.Helpers;
using Core.Models;
using Core.Enums;
using Serilog;


namespace Core.DataAccess
{
    public class CommonDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public CommonDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<DatabaseResponse> GetCustomerBillingDetails(int CustomerID)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),

                };

                parameters[0].Value = CustomerID;

                _DataHelper = new DataAccessHelper("Customers_GetBillingDetails", parameters, _configuration);

                var dt = new DataTable();

                var result = await _DataHelper.RunAsync(dt); // 105 /119

                DatabaseResponse response;

                if (result == 105)
                {

                    var _customerBilling = new customerBilling();

                    if (dt.Rows.Count > 0)
                    {

                        _customerBilling = (from model in dt.AsEnumerable()
                                            select new customerBilling()
                                            {
                                                Name = model.Field<string>("Name"),
                                                BillingUnit = model.Field<string>("BillingUnit"),
                                                BillingFloor = model.Field<string>("BillingFloor"),
                                                BillingStreetName = model.Field<string>("BillingStreetName"),
                                                BillingBuildingNumber = model.Field<string>("BillingBuildingNumber"),
                                                BillingBuildingName = model.Field<string>("BillingBuildingName"),
                                                BillingContactNumber = model.Field<string>("BillingContactNumber"),
                                                BillingPostCode = model.Field<string>("BillingPostCode")
                                            }).FirstOrDefault();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = _customerBilling };

                }

                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                }

                return response;
            }

            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }
    }
}
