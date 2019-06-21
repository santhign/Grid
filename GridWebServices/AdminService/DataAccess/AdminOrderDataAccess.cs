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
using System.Data.SqlClient;

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
                SqlParameter[] parameters =
              {
                    new SqlParameter( "@IDVerificationStatus",  SqlDbType.Int ),
                    new SqlParameter( "@FromDate",  SqlDbType.DateTime ),
                    new SqlParameter( "@ToDate",  SqlDbType.DateTime )

                };

                parameters[0].Value = deliveryStatus;
                parameters[1].Value = fromDate;
                parameters[2].Value = toDate;
                _DataHelper = new DataAccessHelper(DbObjectNames.Admin_GetOrderListForNRIC, parameters, _configuration);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                List<OrderList> orderLists = new List<OrderList>();

                if (dt.Rows.Count > 0)
                {
                    orderLists = (from model in dt.AsEnumerable()
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
                                        DeliveryFromTime = model.Field<TimeSpan?>("DeliveryFromTime"),
                                        DeliveryToTime = model.Field<TimeSpan?>("DeliveryToTime")
                                    }).ToList();
                }

                return orderLists;
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

        /// <summary>
        /// Gets the order details.
        /// </summary>
        /// <param name="orderID">The order identifier.</param>
        /// <returns></returns>
        public async Task<AdminService.Models.OrderDetails> GetOrderDetails(int orderID)
        {
            try
            {
                SqlParameter[] parameters =
              {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),

                };

                parameters[0].Value = orderID;


                _DataHelper = new DataAccessHelper(DbObjectNames.Admin_GetOrderDetailsForNRIC, parameters, _configuration);

                DataSet ds = new DataSet();

                await _DataHelper.RunAsync(ds);

                OrderDetails details = new OrderDetails();
                details.VerificaionHistories = new List<IDVerificaionHistory>();
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    details = (from model in ds.Tables[0].AsEnumerable()
                               select new OrderDetails()
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
                                   DeliveryFromTime = model.Field<TimeSpan?>("DeliveryFromTime"),
                                   DeliveryToTime = model.Field<TimeSpan?>("DeliveryToTime"),
                                   IdentityCardNumber = model.Field<string>("IdentityCardNumber"),
                                   IdentityCardType = model.Field<string>("IdentityCardType"),
                                   ExpiryDate = model.Field<DateTime?>("ExpiryDate"),
                                   DOB = model.Field<DateTime?>("DOB"),
                                   Nationality = model.Field<string>("Nationality"),
                                   DocumentURL = model.Field<string>("DocumentURL"),
                                   DocumentBackURL = model.Field<string>("DocumentBackURL"),
                               }).FirstOrDefault();
                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                    {
                        details.VerificaionHistories = (from model in ds.Tables[1].AsEnumerable()
                                                        select new IDVerificaionHistory()
                                                        {
                                                            VerificationLogID = model.Field<int>("VerificationLogID"),
                                                            OrderID = model.Field<int>("OrderID"),
                                                            IDVerificationStatusNumber = model.Field<int>("IDVerificationStatusNumber"),
                                                            IDVerificationStatus = model.Field<string>("IDVerificationStatus"),
                                                            ChangeLog = model.Field<string>("ChangeLog"),
                                                            Remarks = model.Field<string>("Remarks"),
                                                            UpdatedOn = model.Field<DateTime?>("UpdatedOn"),
                                                            UpdatedBy = model.Field<int?>("UpdatedBy")
                                                        }).ToList();
                    }
                    else
                    {
                        details.VerificaionHistories = null;
                    }
                }

                return details;
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
