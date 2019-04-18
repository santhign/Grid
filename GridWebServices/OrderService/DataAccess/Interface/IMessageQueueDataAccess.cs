using Core.Models;
using OrderService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.DataAccess
{

    /// <summary>
    /// IMessageQueueDataAccess interface
    /// </summary>
    public interface IMessageQueueDataAccess
    {
        /// <summary>
        /// Gets the message body by change request.
        /// </summary>
        /// <param name="changeRequestId">The change request identifier.</param>
        /// <returns></returns>
        Task<MessageBodyForCR> GetMessageBodyByChangeRequest(int changeRequestId);

        /// <summary>
        /// Gets the message details.
        /// </summary>
        /// <param name="MPGSOrderID">The MPGS order identifier.</param>
        /// <returns></returns>
        Task<MessageDetailsForCROrOrder> GetMessageDetails(string MPGSOrderID);

        /// <summary>
        /// Publishes the message to message queue.
        /// </summary>
        /// <param name="topicName">Name of the topic.</param>
        /// <param name="msgBody">The MSG body.</param>
        /// <param name="messageAttribute">The message attribute.</param>
        /// <param name="subject">The subject.</param>
        /// <returns></returns>
        Task<string> PublishMessageToMessageQueue(string topicName, object msgBody, Dictionary<string, string> messageAttribute, string subject);

        /// <summary>
        /// Publishes the message to message queue.
        /// </summary>
        /// <param name="topicName">Name of the topic.</param>
        /// <param name="msgBody">The MSG body.</param>
        /// <param name="messageAttribute">The message attribute.</param>
        /// <returns></returns>
        Task<string> PublishMessageToMessageQueue(string topicName, object msgBody, Dictionary<string, string> messageAttribute);

        /// <summary>
        /// Inserts the message in message queue request.
        /// </summary>
        /// <param name="messageQueueRequest">The message queue request.</param>
        /// <returns></returns>
        Task<int> InsertMessageInMessageQueueRequest(MessageQueueRequest messageQueueRequest);
        
    }
}