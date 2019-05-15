using Core.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using System.IO;
using Core;
using System.Data.SqlClient;
using Core.Models;
using Core.Extensions;
using Core.Enums;
using System.Linq;
using Newtonsoft.Json;
using InfrastructureService;

namespace MessageQueueConsoleAppService
{
    /// <summary>
    /// Publish Message To Queue class
    /// </summary>
    public class PublishMessageToQueueDataAccess
    {
        /// <summary>
        /// The connection string
        /// </summary>
        private readonly string _connectionString;
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishMessageToQueueDataAccess"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public PublishMessageToQueueDataAccess(string connectionString)
        {            
            _connectionString = connectionString;
        }
        /// <summary>
        /// Gets the message from message queue table.
        /// </summary>
        /// <returns></returns>
        private async Task<MessageQueueResponse> GetMessageFromMessageQueueTable()
        {
            var _DataHelper = new DataAccessHelper(Core.Enums.DbObjectNames.z_GetSingleMessageQueueRecord, _connectionString);
            try
            {

                DataTable dt = new DataTable();
                return await Task.Run(() =>
                {
                    MessageQueueResponse message = new MessageQueueResponse();

                    _DataHelper.Run(dt);

                    if (dt.Rows.Count > 0)
                    {
                        message = (from model in dt.AsEnumerable()
                                   select new MessageQueueResponse()
                                   {
                                       MessageQueueRequestID = model.Field<int>("MessageQueueRequestID"),
                                       LastTriedOn = model.Field<DateTime>("LastTriedOn"),
                                       Status = model.Field<int>("Status"),
                                       CreatedOn = model.Field<DateTime>("CreatedOn"),
                                       MessageAttribute = model.Field<string>("MessageAttribute"),
                                       MessageBody = model.Field<string>("MessageBody"),
                                       NumberOfRetries = model.Field<int>("NumberOfRetries"),
                                       PublishedOn = model.Field<DateTime>("PublishedOn"),
                                       SNSTopic = model.Field<string>("SNSTopic"),
                                       Source = model.Field<string>("Source")

                                   }).FirstOrDefault();

                        return message;

                    }
                    return null;
                });
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                Console.WriteLine("Critical error in GetMessageFromMessageQueueTable. Exception is as follows \n " + ex);
                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Publishes the message to queue.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="messageBody">The message body.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="topic">The topic.</param>
        /// <param name="messageAttribute">The message attribute.</param>
        /// <returns></returns>
        private async Task<string> PublishMessageToQueue(string source, string messageBody, string subject, string topic, string messageAttribute)
        {

            var configResponse = await GetValue(ConfiType.AWS.ToString());

            List<Dictionary<string, string>> _result = ((List<Dictionary<string, string>>)configResponse.Results);
            var accessKey = _result.Single(x => x["key"] == "AWSAccessKey")["value"];
            var secretKey = _result.Single(x => x["key"] == "AWSSecretKey")["value"];


            var publisher = new InfrastructureService.MessageQueue.Publisher(accessKey, secretKey, topic);

            var messageDict = new Dictionary<string, string>();
            messageDict.Add(EventTypeString.EventType, messageAttribute);
            return await publisher.PublishAsync(messageBody, messageDict, subject);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="serviceCode">The service code.</param>
        /// <returns></returns>
        private async Task<DatabaseResponse> GetValue(string serviceCode)
        {
            DataAccessHelper _DataHelper = new DataAccessHelper("Admin_GetConfigurations", _connectionString);
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@ConfigType",  SqlDbType.NVarChar )

                };

                parameters[0].Value = serviceCode;

                _DataHelper = new DataAccessHelper("Admin_GetConfigurations", parameters, _connectionString);

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

        /// <summary>
        /// Gets the value by key.
        /// </summary>
        /// <param name="serviceKey">The service key.</param>
        /// <returns></returns>
        private async Task<DatabaseResponse> GetValueByKey(string serviceKey)
        {

            DataAccessHelper _DataHelper = new DataAccessHelper("Admin_GetConfigurationByKey", _connectionString);
            try
            {

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@ConfigKey",  SqlDbType.NVarChar )

                };

                parameters[0].Value = serviceKey;

                _DataHelper = new DataAccessHelper("Admin_GetConfigurationByKey", parameters, _connectionString);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 102 /105

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {
                    string ConfigValue = "";
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        ConfigValue = dt.Rows[0]["value"].ToString().Trim();
                    }
                    response = new DatabaseResponse { ResponseCode = result, Results = ConfigValue };
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
        /// Updates the message queue object to table.
        /// </summary>
        /// <param name="updateQueue">The update queue.</param>
        /// <returns></returns>
        private async Task<int> UpdateMessageQueueObjectToTable(MessageQueueRequest updateQueue)
        {
            var _DataHelper = new DataAccessHelper(Core.Enums.DbObjectNames.z_UpdateStatusInMessageQueueRequests, _connectionString);
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@MessageQueueRequestID",  SqlDbType.Int ),
                    new SqlParameter( "@Status",  SqlDbType.Int ),
                    new SqlParameter( "@PublishedOn",  SqlDbType.DateTime),
                    new SqlParameter( "@NumberOfRetries",  SqlDbType.Int),
                    new SqlParameter( "@LastTriedOn",  SqlDbType.DateTime),
                };

                parameters[0].Value = updateQueue.MessageQueueRequestID;
                parameters[1].Value = updateQueue.Status;
                parameters[2].Value = updateQueue.PublishedOn;
                parameters[3].Value = updateQueue.NumberOfRetries;
                parameters[4].Value = updateQueue.LastTriedOn;

                _DataHelper = new DataAccessHelper(Core.Enums.DbObjectNames.z_UpdateStatusInMessageQueueRequests, parameters, _connectionString);
                return await _DataHelper.RunAsync();

            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                Console.WriteLine("Critical error in UpdateMessageQueueObjectToTable. Exception is as follows \n " + ex);
                return 0;
            }
            finally
            {
                _DataHelper.Dispose();
            }

        }

        /// <summary>
        /// Pushes the messages from message queue table.
        /// </summary>
        /// <returns></returns>
        public async Task PushMessagesFromMessageQueueTable()
        {
            MessageQueueResponse responseData = new MessageQueueResponse();
            try
            {
                //Get 1 first message from table
                responseData = await GetMessageFromMessageQueueTable();
                if (responseData != null)
                {//Push message
                    var pushResult = await PublishMessageToQueue(responseData.Source, responseData.MessageBody, null, responseData.SNSTopic, responseData.MessageAttribute);
                    if (pushResult.Trim().ToUpper() == "OK")
                    {//Update the message queue
                        MessageQueueRequest messageQueueRequest = new MessageQueueRequest();
                        messageQueueRequest.LastTriedOn = DateTime.Now;
                        messageQueueRequest.MessageQueueRequestID = responseData.MessageQueueRequestID;
                        messageQueueRequest.NumberOfRetries = responseData.NumberOfRetries + 1;
                        messageQueueRequest.PublishedOn = DateTime.Now;
                        messageQueueRequest.Status = 1;

                        await UpdateMessageQueueObjectToTable(messageQueueRequest);
                    }
                    else
                    {
                        MessageQueueRequest messageQueueRequest = new MessageQueueRequest();
                        messageQueueRequest.LastTriedOn = DateTime.Now;
                        messageQueueRequest.MessageQueueRequestID = responseData.MessageQueueRequestID;
                        messageQueueRequest.NumberOfRetries = responseData.NumberOfRetries + 1;
                        messageQueueRequest.PublishedOn = responseData.PublishedOn;
                        messageQueueRequest.Status = 0;

                        await UpdateMessageQueueObjectToTable(messageQueueRequest);
                    }
                }
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                MessageQueueRequest messageQueueRequest = new MessageQueueRequest();
                messageQueueRequest.LastTriedOn = DateTime.Now;
                messageQueueRequest.MessageQueueRequestID = responseData.MessageQueueRequestID;
                messageQueueRequest.NumberOfRetries = responseData.NumberOfRetries + 1;
                messageQueueRequest.PublishedOn = responseData.PublishedOn;
                messageQueueRequest.Status = 0;

                await UpdateMessageQueueObjectToTable(messageQueueRequest);
                throw ex;
            }


        }
    }

}

