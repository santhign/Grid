using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Amazon.SQS.Model;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// var subscriber = new InfrastructureService.MessageQueue.Subscriber(_iconfiguration, ConfigHelper.GetValueByKey("TopicName", _iconfiguration).Results.ToString().Trim(), ConfigHelper.GetValueByKey("QueueName", _iconfiguration).Results.ToString().Trim());
/// await subscriber.ListenAsync(message => {  Console.WriteLine("Received message: " + message);  });// Process the message
/// </summary>
namespace InfrastructureService.MessageQueue
{
    public class Subscriber : IDisposable
    {
        private AmazonSQSClient _sqsClient;
        private AmazonSimpleNotificationServiceClient _snsClient;
        private readonly string _topicName;
        private string _topicArn;
        private readonly string _queueName;
        private string _queueUrl;
        private bool _initialised;
        private bool _disposed;

        public Subscriber(IConfiguration _configuration, string topicName, string queueName)
        {
            DatabaseResponse configResponse = ConfigHelper.GetValue(ConfiType.AWS.ToString(), _configuration);
            List<Dictionary<string, string>> _result = ((List<Dictionary<string, string>>)configResponse.Results);
            var credentials = new BasicAWSCredentials(_result.Single(x => x["key"] == "AWSAccessKey")["value"], _result.Single(x => x["key"] == "AWSSecretKey")["value"]);
            _sqsClient = new AmazonSQSClient(credentials, Amazon.RegionEndpoint.APSoutheast1);
            _snsClient = new AmazonSimpleNotificationServiceClient(credentials, Amazon.RegionEndpoint.APSoutheast1);
            _topicName = topicName;
            _queueName = queueName;
        }

        public async Task Initialise()
        {
            _topicArn = (await _snsClient.CreateTopicAsync(_topicName)).TopicArn;
            _queueUrl = (await _sqsClient.CreateQueueAsync(_queueName)).QueueUrl;
            await SubscribeTopicToQueue();

            _initialised = true;
        }

        private async Task SubscribeTopicToQueue()
        {
            var currentSubscriptions = (await _snsClient.ListSubscriptionsByTopicAsync(_topicArn)).Subscriptions;
            if (currentSubscriptions.Any())
            {
                var queueArn = (await _sqsClient.GetQueueAttributesAsync(_queueUrl, new List<string> { "QueueArn" })).QueueARN;
                var existingSubscription = currentSubscriptions.FirstOrDefault(x => x.Endpoint == queueArn);
                if (existingSubscription != null)
                {
                    return;
                }
            }

            await _snsClient.SubscribeQueueAsync(_topicArn, _sqsClient, _queueUrl);
        }

        public async Task ListenAsync(Action<Message> messageHandler)
        {
            if (!_initialised)
                await Initialise();

            while (true)
            {
                var response = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest { QueueUrl = _queueUrl, WaitTimeSeconds = 10 });
                foreach (var message in response.Messages)
                {
                    messageHandler(message);
                    await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _sqsClient.Dispose();
                    _sqsClient = null;

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
