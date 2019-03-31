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

        public  DatabaseResponse UpdateMPGSWebhookNotification(WebhookNotificationModel notification)
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

                parameters[0].Value = notification.OrderId;
                parameters[1].Value = notification.TransactionId;
                parameters[2].Value = notification.OrderStatus;
                parameters[3].Value = notification.Amount;
                parameters[4].Value = notification.Timestamp.ToString();

                _DataHelper = new DataAccessHelper("Orders_UpdateCheckoutWebhookNotification", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = _DataHelper.Run(dt);    // 105 / 107

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

    }
}
