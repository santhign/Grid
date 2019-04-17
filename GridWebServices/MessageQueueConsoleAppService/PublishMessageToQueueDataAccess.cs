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

namespace MessageQueueConsoleAppService
{
    public class PublishMessageToQueueDataAccess
    {
        private readonly string _connectionString;
        public PublishMessageToQueueDataAccess()
        {
            var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        private async Task<MessageQueueResponse> GetMessageFromMessageQueueTable()
        {
            try
            {
                return await Task.Run(() =>
                {
                    DataTable dt = new DataTable();
                    MessageQueueResponse message = new MessageQueueResponse();
                    var _DataHelper = new DataAccessHelper(Core.Enums.DbObjectNames.z_GetSingleMessageQueueRecord, _connectionString);
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
                                       MessageBody = model.Field<object>("MessageBody"),
                                       NumberOfRetries = model.Field<int>("NumberOfRetries"),
                                       PublishedOn = model.Field<DateTime>("PublishedOn"),
                                       SNSTopic = model.Field<string>("SNSTopic"),
                                       Source = model.Field<string>("Source")

                                   }).FirstOrDefault();

                    }
                    return message;
                });
            }

            catch (Exception ex)
            {
                Console.WriteLine("Critical error in GetMessageFromMessageQueueTable. Exception is as follows \n " + ex);
                throw;
            }
        }

        private async Task PublishMessageToQueue(string source, object messageBody, string subject, string topic, string messageAttribute)
        {

            var configResponse = await GetValue(ConfiType.AWS.ToString());

            List<Dictionary<string, string>> _result = ((List<Dictionary<string, string>>)configResponse.Results);
            var accessKey = _result.Single(x => x["key"] == "AWSAccessKey")["value"];
            var secretKey = _result.Single(x => x["key"] == "AWSSecretKey")["value"];


            var publisher = new InfrastructureService.MessageQueue.Publisher(accessKey, secretKey, topic);
            //another approach.
            //var messageDict = messageAttribute.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
            //   .Select(part => part.Split('='))
            //   .ToDictionary(split => split[0], split => split[1]);

            var messageDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(messageAttribute);
            await publisher.PublishAsync(messageBody, messageDict, subject);
        }

        private async Task<DatabaseResponse> GetValue(string serviceCode)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@ConfigType",  SqlDbType.NVarChar )

                };

                parameters[0].Value = serviceCode;

                DataAccessHelper _DataHelper = new DataAccessHelper("Admin_GetConfigurations", parameters, _connectionString);

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
                throw (ex);
            }
        }

        private async Task<DatabaseResponse> GetValueByKey(string serviceKey)
        {
            try
            {

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@ConfigKey",  SqlDbType.NVarChar )

                };

                parameters[0].Value = serviceKey;

                DataAccessHelper _DataHelper = new DataAccessHelper("Admin_GetConfigurationByKey", parameters, _connectionString);

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
                throw (ex);
            }
        }

        private async Task<int> UpdateMessageQueueObjectToTable(MessageQueueRequest updateQueue)
        {
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

                var _DataHelper = new DataAccessHelper(Core.Enums.DbObjectNames.z_UpdateStatusInMessageQueueRequests, parameters, _connectionString);
                return await _DataHelper.RunAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Critical error in UpdateMessageQueueObjectToTable. Exception is as follows \n " + ex);
                return 0;
            }
        }

        public async Task PushMessagesFromMessageQueueTable()
        {
            MessageQueueResponse responseData = new MessageQueueResponse();
            try
            {
                //Get 1 first message from table
                responseData = await GetMessageFromMessageQueueTable();

                //Push message
                await PublishMessageToQueue(responseData.Source, responseData.MessageBody, null, responseData.SNSTopic, responseData.MessageAttribute);


                //Update the message queue
                MessageQueueRequest messageQueueRequest = new MessageQueueRequest();
                messageQueueRequest.LastTriedOn = DateTime.Now;
                messageQueueRequest.MessageQueueRequestID = responseData.MessageQueueRequestID;
                messageQueueRequest.NumberOfRetries = responseData.NumberOfRetries+1;
                messageQueueRequest.PublishedOn = DateTime.Now;
                messageQueueRequest.Status = 1;

                await UpdateMessageQueueObjectToTable(messageQueueRequest);
            }
            catch(Exception)
            {
                MessageQueueRequest messageQueueRequest = new MessageQueueRequest();
                messageQueueRequest.LastTriedOn = DateTime.Now;
                messageQueueRequest.MessageQueueRequestID = responseData.MessageQueueRequestID;
                messageQueueRequest.NumberOfRetries = responseData.NumberOfRetries + 1;
                messageQueueRequest.PublishedOn = responseData.PublishedOn;
                messageQueueRequest.Status = 0;

                await UpdateMessageQueueObjectToTable(messageQueueRequest);
                throw;
            }


        }
    }

}
    
