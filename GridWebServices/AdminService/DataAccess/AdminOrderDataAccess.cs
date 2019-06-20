using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminService.DataAccess.Interfaces;
using Core.Helpers;
using Microsoft.Extensions.Configuration;
using System.Data;
using AdminService.Models;
using InfrastructureService;
using Core.Enums;

namespace AdminService.DataAccess
{
    public class AdminOrderDataAccess : IAdminOrderDataAccess
    {
        /// <summary>
        /// The data helper
        /// </summary>
        internal DataAccessHelper _DataHelper = null;

        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public AdminOrderDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<List<AdminService.Models.OrderList>> GetOrdersList(int? deliveryStatus, DateTime? fromDate, DateTime? toDate)
        {
            try
            {

                _DataHelper = new DataAccessHelper(DbObjectNames.Admin_GetOrderList, _configuration);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                List<OrderList> customerList = new List<OrderList>();

                if (dt.Rows.Count > 0)
                {

                    customerList = (from model in dt.AsEnumerable()
                                    select new OrderList()
                                    {
                                        OrderID = model.Field<int>("OrderID"),
                                        OrderNumber = model.Field<string>("OrderNumber"),
                                        IDVerificationStatus = model.Field<string>("IDVerificationStatus"),
                                        IDVerificationStatusNumber = model.Field<int?>("IDVerificationStatusNumber"),
                                        OrderStatus = model.Field<string>("OrderStatus"),
                                        OrderStatusNumber = model.Field<int?>("OrderStatusNumber"),
                                        RejectionCount = model.Field<int?>("RejectionCount"),
                                        Name = model.Field<string>("Name"),
                                        OrderDate = model.Field<DateTime?>("OrderDate"),
                                        DeliveryDate = model.Field<DateTime?>("DeliveryDate"),
                                        DeliveryFromTime = model.Field<DateTime?>("DeliveryFromTime"),
                                        DeliveryToTime = model.Field<DateTime?>("DeliveryToTime")
                                    }).ToList();
                }

                return customerList;
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
