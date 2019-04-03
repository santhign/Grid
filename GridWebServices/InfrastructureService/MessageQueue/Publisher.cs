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
                
            }
            catch (WebException ex)
            {
               
            }
            catch (Exception ex)
            {
                
            }
        }

        public async Task Initialise()
        {
            _topicArn = (await _snsClient.CreateTopicAsync(_topicName)).TopicArn;

            _initialised = true;
        }

        public async Task PublishAsync(object message)
        {
            if (!_initialised)
                await Initialise();

            await _snsClient.PublishAsync(_topicArn, JsonConvert.SerializeObject(message));
        }

        public async Task PublishAsync(object message, string subject)
        {
            if (!_initialised)
                await Initialise();

            await _snsClient.PublishAsync(_topicArn, JsonConvert.SerializeObject(message), subject);
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
