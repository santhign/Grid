using Core.Enums;
using Core.Helpers;
using CustomerService.Models;
using InfrastructureService;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;

namespace CustomerService.DataAccess
{
    public class MQDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public MQDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gets the message body by change request.
        /// </summary>
        /// <param name="CustomerID">The change request identifier.</param>
        /// <returns></returns>
        public async Task<ProfileMQ> GetProfileUpdateMessageBody(int CustomerID)
        {
            try
            {

                DataSet ds = new DataSet();
                SqlParameter[] parameters =
                 {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                     };

                parameters[0].Value = CustomerID;

                _DataHelper = new DataAccessHelper("Customers_GetProfileMessageQueue", parameters, _configuration);


                var result = await _DataHelper.RunAsync(ds);


                var msgBody = new ProfileMQ();

                if (ds.Tables.Count > 0)
                {
                    msgBody = (from model in ds.Tables[0].AsEnumerable()
                               select new ProfileMQ()
                               {
                                   accountID = model.Field<int>("accountID"),
                                   customerID = model.Field<int>("customerID"),
                                   subscriberID = model.Field<int?>("subscriberID"),
                                   mobilenumber = model.Field<string>("mobilenumber"),
                                   MaskedCardNumber = model.Field<string>("MaskedCardNumber"),
                                   Token = model.Field<string>("Token"),
                                   CardType = model.Field<string>("CardType"),
                                   IsDefault = model.Field<int?>("IsDefault"),
                                   CardHolderName = model.Field<string>("CardHolderName"),
                                   ExpiryMonth = model.Field<int?>("ExpiryMonth"),
                                   ExpiryYear = model.Field<int?>("ExpiryYear"),
                                   CardFundMethod = model.Field<string>("CardFundMethod"),
                                   CardBrand = model.Field<string>("CardBrand"),
                                   CardIssuer = model.Field<string>("CardIssuer"),
                                   billingUnit = model.Field<string>("billingUnit"),
                                   billingFloor = model.Field<string>("billingFloor"),
                                   billingBuildingNumber = model.Field<string>("billingBuildingNumber"),
                                   billingBuildingName = model.Field<string>("billingBuildingName"),
                                   billingStreetName = model.Field<string>("billingStreetName"),
                                   billingPostCode = model.Field<string>("billingPostCode"),
                                   billingContactNumber = model.Field<string>("billingContactNumber"),
                                   email = model.Field<string>("email"),
                                   displayname = model.Field<string>("displayname"),
                                   paymentmode = model.Field<string>("paymentmode"),
                                   amountpaid = model.Field<double?>("amountpaid"),
                                   MPGSOrderID = model.Field<string>("MPGSOrderID"),
                                   invoicelist = model.Field<string>("invoicelist"),
                                   invoiceamounts = model.Field<string>("invoiceamounts")
                               }).FirstOrDefault();
                }

                return msgBody;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<string> PublishMessageToMessageQueue(string topicName, object msgBody, Dictionary<string, string> messageAttribute, string subject)
        {
            try
            {
                var publisher = new InfrastructureService.MessageQueue.Publisher(_configuration, topicName);
                return await publisher.PublishAsync(msgBody, messageAttribute, subject);

            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                throw;
            }

        }

        public async Task<string> PublishMessageToMessageQueue(string topicName, object msgBody, Dictionary<string, string> messageAttribute)
        {
            try
            {
                var publisher = new InfrastructureService.MessageQueue.Publisher(_configuration, topicName);
                return await publisher.PublishAsync(msgBody, messageAttribute);

            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                throw;
            }

        }

        public async Task<int> InsertMessageInMessageQueueRequest(MessageQueueRequest messageQueueRequest)
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


                _DataHelper = new DataAccessHelper(DbObjectNames.z_InsertIntoMessageQueueRequests, parameters, _configuration);


                return await _DataHelper.RunAsync();
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }
    }
}
