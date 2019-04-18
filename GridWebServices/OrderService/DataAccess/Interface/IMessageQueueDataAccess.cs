using Core.Models;
using OrderService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMessageQueueDataAccess
    {
        Task<MessageBodyForCR> GetMessageBodyByChangeRequest(int changeRequestId);

        Task<MessageDetailsForCROrOrder> GetMessageDetails(string MPGSOrderID);

        Task<string> PublishMessageToMessageQueue(string topicName, object msgBody, Dictionary<string, string> messageAttribute, string subject);

        Task<string> PublishMessageToMessageQueue(string topicName, object msgBody, Dictionary<string, string> messageAttribute);

        Task<int> InsertMessageInMessageQueueRequest(MessageQueueRequest messageQueueRequest);
    }
}