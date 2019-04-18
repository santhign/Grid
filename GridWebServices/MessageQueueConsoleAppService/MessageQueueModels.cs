using System;
using System.Collections.Generic;
using System.Text;

namespace MessageQueueConsoleAppService
{
    public class MessageQueueRequest
    {
       public int MessageQueueRequestID { get; set; }
        public int Status { get; set; }
        public DateTime PublishedOn { get; set; }
        public int NumberOfRetries { get; set; }
        public DateTime LastTriedOn { get; set; }
    }

    public class MessageQueueResponse
    {
        public int MessageQueueRequestID { get; set; }
        public int Status { get; set; }
        public DateTime PublishedOn { get; set; }
        public int NumberOfRetries { get; set; }
        public DateTime LastTriedOn { get; set; }
        public string Source { get; set; }
        public string SNSTopic { get; set; }
        public string MessageAttribute { get; set; }
        public string MessageBody { get; set; }
        public DateTime CreatedOn { get; set; }
        
    }    
}
