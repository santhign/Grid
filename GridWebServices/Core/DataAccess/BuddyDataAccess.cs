using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Core.Helpers;
using Core.Models;
using Core.Extensions;
using System.Linq;
using Core.Enums;
using Serilog;

namespace Core.DataAccess
{
    public class BuddyDataAccess
    {
        internal DataAccessHelper _DataHelper = null;
        public async Task<DatabaseResponse> GetPendingBuddyList(string connectionString)
        {
            try
            {
                _DataHelper = new DataAccessHelper("Order_GetPendingBuddyOrders", connectionString);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 109 /105

                DatabaseResponse response = new DatabaseResponse();

                List<PendingBuddy> pendingBuddies = new List<PendingBuddy>();

                if (dt != null && dt.Rows.Count > 0)
                {
                    pendingBuddies = (from model in dt.AsEnumerable()
                                      select new PendingBuddy()
                                      {
                                          PendingBuddyID = model.Field<int>("PendingBuddyID"),
                                          OrderID = model.Field<int>("OrderID"),
                                          PendingBuddyOrderListID = model.Field<int>("PendingBuddyOrderListID"),
                                          OrderSubscriberID = model.Field<int>("OrderSubscriberID"),
                                          MobileNumber = model.Field<string>("MobileNumber"),
                                          DateCreated = model.Field<DateTime>("DateCreated"),
                                          IsProcessed = model.Field<bool>("IsProcessed")
                                      }).ToList();

                    response = new DatabaseResponse { ResponseCode = result, Results = pendingBuddies };
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
                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> UpdatePendingBuddyList(string connectionString, PendingBuddy buddy)
        {
            try
            {

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@PendingBuddyOrderListID",  SqlDbType.Int ),

                    new SqlParameter( "@IsProcessed",  SqlDbType.Bit )                   
                };
                 
                parameters[0].Value = buddy.PendingBuddyOrderListID;

                parameters[1].Value = buddy.IsProcessed;
               
                _DataHelper = new DataAccessHelper("Orders_UpdatePendingBuddyList",parameters, connectionString);              

                int result = await _DataHelper.RunAsync(); // 101 /106/102

                return new DatabaseResponse { ResponseCode = result };
            }

            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> GetBssApiRequestId(string source, string apiName, int customerId, int isNewSession, string mobileNumber, string connectionString)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@Source",  SqlDbType.NVarChar ),
                    new SqlParameter( "@APIName",  SqlDbType.NVarChar ),
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@IsNewSession",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar),
                };

                parameters[0].Value = source;
                parameters[1].Value = apiName;
                parameters[2].Value = customerId;
                parameters[3].Value = isNewSession;
                parameters[4].Value = mobileNumber;

                _DataHelper = new DataAccessHelper("Admin_GetRequestIDForBSSAPI", parameters, connectionString);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                BSSAssetRequest assetRequest = new BSSAssetRequest();

                DatabaseResponse response = new DatabaseResponse();

                if (dt.Rows.Count > 0)
                {
                    assetRequest = (from model in dt.AsEnumerable()
                                    select new BSSAssetRequest()
                                    {
                                        request_id = model.Field<string>("RequestID"),
                                        userid = model.Field<string>("UserID"),
                                        BSSCallLogID = model.Field<int>("BSSCallLogID"),

                                    }).FirstOrDefault();
                }

                response = new DatabaseResponse { Results = assetRequest };

                return response;
            }

            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> UpdateBSSCallNumbers(string json, string userID, int callLogId, string connectionString)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter("@UserID",  SqlDbType.NVarChar ),
                     new SqlParameter("@Json",  SqlDbType.NVarChar ),
                     new SqlParameter("@BSSCallLogID",  SqlDbType.Int )
                };

                parameters[0].Value = userID;
                parameters[1].Value = json;
                parameters[2].Value = callLogId;

                _DataHelper = new DataAccessHelper("Orders_UpdateBSSCallNumbers", parameters, connectionString);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync();    // 105 / 102

                DatabaseResponse response = new DatabaseResponse();

                response = new DatabaseResponse { ResponseCode = result };

                return response;
            }
            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> ProcessBuddyPlan(BuddyNumberUpdate updateRequest, string connectionString)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter( "@OrderSubscriberID",  SqlDbType.Int ),
                     new SqlParameter( "@UserId",  SqlDbType.NVarChar ),
                      new SqlParameter( "@NewMobileNumber",  SqlDbType.NVarChar ),
                };

                parameters[0].Value = updateRequest.OrderSubscriberID;
                parameters[1].Value = updateRequest.UserId;
                parameters[2].Value = updateRequest.NewMobileNumber;

                _DataHelper = new DataAccessHelper("Orders_ProcessBuddyPlan", parameters, connectionString);

                int result = await _DataHelper.RunAsync();    // 107 / 100/119                

                return new DatabaseResponse { ResponseCode = result };
            }

            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> GetCustomerIdFromOrderId(int orderId, string connectionString)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )

                };

                parameters[0].Value = orderId;

                _DataHelper = new DataAccessHelper("Order_GetCustomerIDByOrderID", parameters, connectionString);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 105 /102

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {
                    OrderCust cust = new OrderCust();

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        cust = (from model in dt.AsEnumerable()
                                select new OrderCust()
                                {
                                    CustomerID = model.Field<int>("CustomerID"),

                                }).FirstOrDefault();

                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = cust };
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

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> GetConfiguration(string configType, string connectionString)
        {
            try
            {


                SqlParameter[] parameters =
               {
                    new SqlParameter( "@ConfigType",  SqlDbType.NVarChar )

                };

                parameters[0].Value = configType;

                _DataHelper = new DataAccessHelper("Admin_GetConfigurations", parameters, connectionString);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 102 /105

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
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> GetBSSServiceCategoryAndFee(string serviceType, string connectionString)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@ServiceType",  SqlDbType.NVarChar )

                };

                parameters[0].Value = serviceType;

                _DataHelper = new DataAccessHelper("Admin_GetNumberTypeCodes", parameters, connectionString);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 102 /105

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    List<ServiceFees> serviceFees = new List<ServiceFees>();

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        serviceFees = (from model in dt.AsEnumerable()

                                       select new ServiceFees()
                                       {
                                           PortalServiceName = model.Field<string>("PortalServiceName"),

                                           ServiceCode = model.Field<int>("ServiceCode"),

                                           ServiceFee = model.Field<double>("ServiceFee")

                                       }).ToList();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = serviceFees };

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

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<int> InsertMessageInMessageQueueRequest(MessageQueueRequest messageQueueRequest, string connectionString)
        {
            try
            {



                SqlParameter[] parameters =
                {
                    new SqlParameter( "@Source",  SqlDbType.NVarChar ),
                    new SqlParameter( "@SNSTopic",  SqlDbType.NVarChar ),
                    new SqlParameter( "@MessageAttribute",  SqlDbType.NVarChar ),
                    new SqlParameter( "@MessageBody",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Status",  SqlDbType.Int ),
                    new SqlParameter( "@PublishedOn",  SqlDbType.DateTime ),
                    new SqlParameter( "@CreatedOn",  SqlDbType.DateTime ),
                    new SqlParameter( "@NumberOfRetries",  SqlDbType.Int ),
                    new SqlParameter( "@LastTriedOn",  SqlDbType.DateTime)
            };

                parameters[0].Value = messageQueueRequest.Source;
                parameters[1].Value = messageQueueRequest.SNSTopic;
                parameters[2].Value = messageQueueRequest.MessageAttribute;
                parameters[3].Value = messageQueueRequest.MessageBody;
                parameters[4].Value = messageQueueRequest.Status;
                parameters[5].Value = messageQueueRequest.PublishedOn;
                parameters[6].Value = messageQueueRequest.CreatedOn;
                parameters[7].Value = messageQueueRequest.NumberOfRetries;
                parameters[8].Value = messageQueueRequest.LastTriedOn;


                _DataHelper = new DataAccessHelper(DbObjectNames.z_InsertIntoMessageQueueRequests, parameters, connectionString);


                return await _DataHelper.RunAsync();
            }           

            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> RemoveProcessedBuddyList(string connectionString, int orderID)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )                  

                };

                parameters[0].Value = orderID;               

                _DataHelper = new DataAccessHelper("Orders_RemoveProcessedBuddyList", parameters, connectionString);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync();    // 102 /103/ 150

                DatabaseResponse response = new DatabaseResponse();

                response = new DatabaseResponse { ResponseCode = result };

                return response;
            }
            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }

        }


        public async Task<DatabaseResponse> CheckBuddyLocked( int orderID, string connectionString)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )

                };

                parameters[0].Value = orderID;

                _DataHelper = new DataAccessHelper("Order_CheckBuddyOrderLocked", parameters, connectionString);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync();    // 102 /156/ 157

                DatabaseResponse response = new DatabaseResponse();

                response = new DatabaseResponse { ResponseCode = result };

                return response;
            }
            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }

        }

        public async Task<DatabaseResponse> UnLockPendingBuddy(int orderID, string connectionString)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )

                };

                parameters[0].Value = orderID;

                _DataHelper = new DataAccessHelper("Order_UnLockPendingBuddy", parameters, connectionString);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync();    // 102 / 157

                DatabaseResponse response = new DatabaseResponse();

                response = new DatabaseResponse { ResponseCode = result };

                return response;
            }
            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }

        }
        public async Task InsertMessageInMessageQueueRequestException(MessageQueueRequestException messageQueueRequestException, string connectionString)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@Source",  SqlDbType.NVarChar ),
                    new SqlParameter( "@SNSTopic",  SqlDbType.NVarChar ),
                    new SqlParameter( "@MessageAttribute",  SqlDbType.NVarChar ),
                    new SqlParameter( "@MessageBody",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Status",  SqlDbType.Int ),
                    new SqlParameter( "@PublishedOn",  SqlDbType.DateTime ),
                    new SqlParameter( "@CreatedOn",  SqlDbType.DateTime ),
                    new SqlParameter( "@NumberOfRetries",  SqlDbType.Int ),
                    new SqlParameter( "@LastTriedOn",  SqlDbType.DateTime),
                    new SqlParameter( "@Remark",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Exception",  SqlDbType.NVarChar )

            };

                parameters[0].Value = messageQueueRequestException.Source;
                parameters[1].Value = messageQueueRequestException.SNSTopic;
                parameters[2].Value = messageQueueRequestException.MessageAttribute;
                parameters[3].Value = messageQueueRequestException.MessageBody;
                parameters[4].Value = messageQueueRequestException.Status;
                parameters[5].Value = messageQueueRequestException.PublishedOn;
                parameters[6].Value = messageQueueRequestException.CreatedOn;
                parameters[7].Value = messageQueueRequestException.NumberOfRetries;
                parameters[8].Value = messageQueueRequestException.LastTriedOn;
                parameters[9].Value = messageQueueRequestException.Remark;
                parameters[10].Value = messageQueueRequestException.Exception;


                _DataHelper = new DataAccessHelper(DbObjectNames.z_UpdateStatusInMessageQueueRequestsException, parameters, connectionString);


                await _DataHelper.RunAsync();
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

    public class OrderCust
    {
        public int CustomerID { get; set; }
    }
}
