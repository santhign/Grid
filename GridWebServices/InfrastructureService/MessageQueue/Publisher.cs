using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService.Model;

namespace InfrastructureService.MessageQueue
{
    public class Publisher : IDisposable
    {
        private AmazonSimpleNotificationServiceClient _snsClient;
        private readonly string _topicName;
        private string _topicArn;
        private bool _initialised;
        private bool _disposed;

        public Publisher(IConfiguration _configuration, string topicName)
        {
            try
            {
                DatabaseResponse configResponse = ConfigHelper.GetValue(ConfiType.AWS.ToString(), _configuration);
                List<Dictionary<string, string>> _result = ((List<Dictionary<string, string>>)configResponse.Results);
                var credentials = new BasicAWSCredentials(_result.Single(x => x["key"] == "AWSAccessKey")["value"], _result.Single(x => x["key"] == "AWSSecretKey")["value"]);

                _snsClient = new AmazonSimpleNotificationServiceClient(credentials, Amazon.RegionEndpoint.APSoutheast1);
                _topicName = topicName;
            }
            catch (ArgumentNullException ex)
            {
                LogInfo.Fatal(ex, "Error while publishing the message queue");
            }
            catch (WebException ex)
            {
                LogInfo.Fatal(ex, "Error while publishing the message queue");
            }
            catch (Exception ex)
            {
                LogInfo.Fatal(ex, "Error while publishing the message queue");
            }
        }

        public Publisher(string connectionString, string topicName)
        {
            try
            {
                DatabaseResponse configResponse =  ConfigHelper.GetValue(ConfiType.AWS.ToString(), connectionString);
                List<Dictionary<string, string>> _result = ((List<Dictionary<string, string>>)configResponse.Results);
                var credentials = new BasicAWSCredentials(_result.Single(x => x["key"] == "AWSAccessKey")["value"], _result.Single(x => x["key"] == "AWSSecretKey")["value"]);

                _snsClient = new AmazonSimpleNotificationServiceClient(credentials, Amazon.RegionEndpoint.APSoutheast1);
                _topicName = topicName;
            }
            catch (ArgumentNullException ex)
            {
                LogInfo.Fatal(ex, "Error while publishing the message queue");
            }
            catch (WebException ex)
            {
                LogInfo.Fatal(ex, "Error while publishing the message queue");
            }
            catch (Exception ex)
            {
                LogInfo.Fatal(ex, "Error while publishing the message queue");
            }
        }
        public Publisher(string accessKey, string secretKey, string topicName)
        {
            try
            {               
                var credentials = new BasicAWSCredentials(accessKey, secretKey);

                _snsClient = new AmazonSimpleNotificationServiceClient(credentials, Amazon.RegionEndpoint.APSoutheast1);
                _topicName = topicName;
            }
            catch (ArgumentNullException ex)
            {
                LogInfo.Fatal(ex, "Error while publishing the message queue");
            }
            catch (WebException ex)
            {
                LogInfo.Fatal(ex, "Error while publishing the message queue");
            }
            catch (Exception ex)
            {
                LogInfo.Fatal(ex, "Error while publishing the message queue");
            }
        }

        public async Task Initialise()
        {
            _topicArn = (await _snsClient.CreateTopicAsync(_topicName)).TopicArn;

            _initialised = true;
        }

        public async Task<string> PublishAsync(object message)
        {
            if (!_initialised)
                await Initialise();
            string msg = JsonConvert.SerializeObject(message);

            PublishResponse response = await _snsClient.PublishAsync(_topicArn, JsonConvert.SerializeObject(message));

            return response.HttpStatusCode.ToString();
        }

        public async Task<string> PublishAsync(object message, string subject)
        {
            if (!_initialised)
                await Initialise();

            string msg = JsonConvert.SerializeObject(message);

            await _snsClient.PublishAsync(_topicArn, JsonConvert.SerializeObject(message));
            PublishResponse response = await _snsClient.PublishAsync(_topicArn, JsonConvert.SerializeObject(message), subject);
            return response.HttpStatusCode.ToString();
        }

        public async Task<string> PublishAsync(object message, Dictionary<string, string> attributes, string subject)
        {
            Dictionary<string, MessageAttributeValue> _messageattributes = new Dictionary<string, MessageAttributeValue>();
            foreach (string key in attributes.Keys)
            {
                MessageAttributeValue _attributeValue = new MessageAttributeValue();
                _attributeValue.DataType = "String";
                _attributeValue.StringValue = attributes[key];
                _messageattributes.Add(key, _attributeValue);
            }
            if (!_initialised)
                await Initialise();
            PublishRequest request = new PublishRequest
            {
                TopicArn = _topicArn,
                MessageAttributes = _messageattributes,
                Message = JsonConvert.SerializeObject(message),
                Subject = subject
            };
            PublishResponse response = await _snsClient.PublishAsync(request);
            return response.HttpStatusCode.ToString();
        }

        /// <summary>
        /// Publishes the asynchronous.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="subject">The subject.</param>
        /// <returns>HttpStatusCode</returns>
        public async Task<string> PublishAsync(string message, Dictionary<string, string> attributes, string subject)
        {
            Dictionary<string, MessageAttributeValue> _messageattributes = new Dictionary<string, MessageAttributeValue>();
            foreach (string key in attributes.Keys)
            {
                MessageAttributeValue _attributeValue = new MessageAttributeValue();
                _attributeValue.DataType = "String";
                _attributeValue.StringValue = attributes[key];
                _messageattributes.Add(key, _attributeValue);
            }
            if (!_initialised)
                await Initialise();
            PublishRequest request = new PublishRequest
            {
                TopicArn = _topicArn,
                MessageAttributes = _messageattributes,
                Message = message,
                Subject = subject
            };
            PublishResponse response = await _snsClient.PublishAsync(request);
            return response.HttpStatusCode.ToString();
        }

        public async Task<string> PublishAsync(object message, Dictionary<string, string> attributes)
        {
            Dictionary<string, MessageAttributeValue> _messageattributes = new Dictionary<string, MessageAttributeValue>();
            foreach (string key in attributes.Keys)
            {
                MessageAttributeValue _attributeValue = new MessageAttributeValue();
                _attributeValue.DataType = "String";
                _attributeValue.StringValue = attributes[key];
                _messageattributes.Add(key, _attributeValue);
            }
            if (!_initialised)
                await Initialise();
            PublishRequest request = new PublishRequest
            {
                TopicArn = _topicArn,
                MessageAttributes = _messageattributes,
                Message = JsonConvert.SerializeObject(message)
                
            };
            PublishResponse response = await _snsClient.PublishAsync(request);
            return response.HttpStatusCode.ToString();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _snsClient.Dispose();
                    _snsClient = null;
                }

                _disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
