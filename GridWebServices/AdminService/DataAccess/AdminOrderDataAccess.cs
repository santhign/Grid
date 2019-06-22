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
using Core.Extensions;


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
        public async Task<List<OrderList>> GetOrdersList(int? deliveryStatus, DateTime? fromDate, DateTime? toDate)
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
                                      DeliveryToTime = model.Field<TimeSpan?>("DeliveryToTime"),
                                      IdentityCardNumber = model.Field<string>("IdentityCardNumber"),
                                      IdentityCardType = model.Field<string>("IdentityCardType")
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
        /// Gets the email notification template.
        /// </summary>
        /// <param name="templateName">Name of the template.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetEmailNotificationTemplate(string templateName)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@TemplateName",  SqlDbType.NVarChar )

                };

                parameters[0].Value = templateName;

                _DataHelper = new DataAccessHelper("z_GetEmailTemplateByName", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 109 /105

                DatabaseResponse response = new DatabaseResponse();

                EmailTemplate template = new EmailTemplate();

                if (dt != null && dt.Rows.Count > 0)
                {
                    template = (from model in dt.AsEnumerable()
                                select new EmailTemplate()
                                {
                                    EmailTemplateID = model.Field<int>("EmailTemplateID"),
                                    EmailBody = model.Field<string>("EmailBody"),
                                    EmailSubject = model.Field<string>("EmailSubject"),
                                    TemplateName = model.Field<string>("TemplateName")
                                }).FirstOrDefault();

                    response = new DatabaseResponse { ResponseCode = result, Results = template };
                }


                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                }

                return response;
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
        /// Creates the e mail notification log.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> CreateEMailNotificationLog(NotificationLog log)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),

                    new SqlParameter( "@Email",  SqlDbType.NVarChar ),

                    new SqlParameter( "@EmailSubject",  SqlDbType.NVarChar ),

                    new SqlParameter( "@EmailBody",  SqlDbType.NVarChar ),

                    new SqlParameter( "@ScheduledOn",  SqlDbType.DateTime ),

                    new SqlParameter( "@EmailTemplateID",  SqlDbType.Int ),

                    new SqlParameter( "@SendOn",  SqlDbType.DateTime ),

                    new SqlParameter( "@Status",  SqlDbType.Int )

                };

                parameters[0].Value = log.CustomerID;
                parameters[1].Value = log.Email;
                parameters[2].Value = log.EmailSubject;
                parameters[3].Value = log.EmailBody;
                parameters[4].Value = log.ScheduledOn;
                parameters[5].Value = log.EmailTemplateID;
                parameters[6].Value = log.SendOn;
                parameters[7].Value = log.Status;


                _DataHelper = new DataAccessHelper("z_EmailNotificationsLogEntry", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 107 /100

                DatabaseResponse response = new DatabaseResponse();

                response = new DatabaseResponse { ResponseCode = result };


                return response;
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
        /// Creates the e mail notification log for dev purpose.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> CreateEMailNotificationLogForDevPurpose(NotificationLogForDevPurpose log)
        {
            try
            {

                SqlParameter[] parameters =
               {

                    new SqlParameter( "@EventType",  SqlDbType.NVarChar ),

                    new SqlParameter( "@Message",  SqlDbType.NVarChar )
                };

                parameters[0].Value = log.EventType;
                parameters[1].Value = log.Message;



                _DataHelper = new DataAccessHelper(DbObjectNames.z_EmailNotificationsLogEntryForDevPurpose, parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 107 /100

                DatabaseResponse response = new DatabaseResponse();

                response = new DatabaseResponse { ResponseCode = result };


                return response;
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

        public async Task<DatabaseResponse> GetConfiguration(string configType)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@ConfigType",  SqlDbType.NVarChar )

                };

                parameters[0].Value = configType;

                _DataHelper = new DataAccessHelper("Admin_GetConfigurations", parameters, _configuration);

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
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> CreateTokenForVerificationRequests(int orderId)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )
                };

                parameters[0].Value = orderId;


                _DataHelper = new DataAccessHelper(DbObjectNames.Orders_InsertIDVerificationRequests, parameters, _configuration);
                DataTable dt = new DataTable();

                var result = await _DataHelper.RunAsync(dt);

                if (result != (int)DbReturnValue.UpdateSuccess && result != (int)DbReturnValue.UpdateSuccessSendEmail)
                    return new DatabaseResponse() { ResponseCode = result };

                DatabaseResponse response = new DatabaseResponse();
                VerificationRequestResponse requestDetails = new VerificationRequestResponse();
                response.ResponseCode = result;
                if (dt.Rows.Count > 0)
                {
                    requestDetails = (from model in dt.AsEnumerable()
                                      select new VerificationRequestResponse()
                                      {
                                          VerificationRequestID = model.Field<int>("VerificationRequestID"),
                                          OrderID = model.Field<int>("OrderID"),
                                          RequestToken = model.Field<string>("RequestToken"),
                                          CreatedOn = model.Field<DateTime>("CreatedOn"),
                                          IsUsed = model.Field<int>("IsUsed")
                                      }).FirstOrDefault();
                }
                response.Results = requestDetails;
                return response;
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

        //    public async Task<DatabaseResponse> UpdateTokenForVerificationRequests(int orderId)
        //    {
        //        try
        //        {
        //            SqlParameter[] parameters =
        //            {
        //                new SqlParameter( "@OrderID",  SqlDbType.Int )
        //            };

        //            parameters[0].Value = orderId;


        //            _DataHelper = new DataAccessHelper(DbObjectNames.Orders_UpdateIDVerificationRequests, parameters, _configuration);
        //            DataTable dt = new DataTable();

        //            var result = await _DataHelper.RunAsync(dt);

        //            if (result != (int)DbReturnValue.UpdateSuccess && result != (int)DbReturnValue.UpdateSuccessSendEmail)
        //                return new DatabaseResponse() { ResponseCode = result };

        //            DatabaseResponse response = new DatabaseResponse();
        //            VerificationRequestResponse requestDetails = new VerificationRequestResponse();
        //            response.ResponseCode = result;
        //            if (dt.Rows.Count > 0)
        //            {
        //                requestDetails = (from model in dt.AsEnumerable()
        //                                  select new VerificationRequestResponse()
        //                                  {
        //                                      VerificationRequestID = model.Field<int>("VerificationRequestID"),
        //                                      OrderID = model.Field<int>("OrderID"),
        //                                      RequestToken = model.Field<string>("RequestToken"),
        //                                      CreatedOn = model.Field<DateTime>("CreatedOn"),
        //                                      IsUsed = model.Field<int>("IsUsed")
        //                                  }).FirstOrDefault();
        //            }
        //            response.Results = requestDetails;
        //            return response;
        //        }
        //        catch (Exception ex)
        //        {
        //            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

        //            throw (ex);
        //        }
        //        finally
        //        {
        //            _DataHelper.Dispose();
        //        }
        //    }

    }
}
