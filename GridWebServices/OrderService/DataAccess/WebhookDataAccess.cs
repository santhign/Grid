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
using Core.Extensions;


namespace OrderService.DataAccess
{
    public class WebhookDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public WebhookDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<DatabaseResponse> UpdateMPGSWebhookNotification(WebhookNotificationModel notification)
        {
            try
            {
                SqlParameter[] parameters =
               {
                     new SqlParameter( "@MPGSOrderID",  SqlDbType.NVarChar ),
                     new SqlParameter( "@TransactionID",  SqlDbType.NVarChar ),
                     new SqlParameter( "@Status",  SqlDbType.NVarChar ),
                     new SqlParameter( "@Amount",  SqlDbType.Float ),
                     new SqlParameter( "@TimeStamp",  SqlDbType.NVarChar )

                };

                parameters[0].Value = notification.Order.Id;
                parameters[1].Value = notification.Transaction.Id;
                parameters[2].Value = notification.Order.Status;
                parameters[3].Value = notification.Order.Amount;
                parameters[4].Value = notification.Timestamp.ToString();

                _DataHelper = new DataAccessHelper("Orders_UpdateCheckoutWebhookNotification", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt);

                DatabaseResponse response = new DatabaseResponse();
                TokenSession tokenSession = new TokenSession();
                if (dt != null && dt.Rows.Count > 0)
                {

                    tokenSession = (from model in dt.AsEnumerable()
                                    select new TokenSession()
                                    {
                                        MPGSOrderID = model.Field<string>("MPGSOrderID"),
                                        CheckOutSessionID = model.Field<string>("CheckOutSessionID"),
                                        Amount = model.Field<double>("Amount"),
                                        CustomerID = model.Field<int>("CustomerID"),
                                        RequireTokenization = model.Field<int>("RequireTokenization")
                                    }).FirstOrDefault();

                    response = new DatabaseResponse { ResponseCode = result, Results = tokenSession };
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

    }
}
