using System;
using System.Collections.Generic;
using System.Text;

namespace MessageQueueConsoleAppService
{
    /// <summary>
    /// Message Queue Request class
    /// </summary>
    public class MessageQueueRequest
    {
        /// <summary>
        /// Gets or sets the message queue request identifier.
        /// </summary>
        /// <value>
        /// The message queue request identifier.
        /// </value>
        public int MessageQueueRequestID { get; set; }
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public int Status { get; set; }
        /// <summary>
        /// Gets or sets the published on.
        /// </summary>
        /// <value>
        /// The published on.
        /// </value>
        public DateTime PublishedOn { get; set; }
        /// <summary>
        /// Gets or sets the number of retries.
        /// </summary>
        /// <value>
        /// The number of retries.
        /// </value>
        public int NumberOfRetries { get; set; }
        /// <summary>
        /// Gets or sets the last tried on.
        /// </summary>
        /// <value>
        /// The last tried on.
        /// </value>
        public DateTime LastTriedOn { get; set; }
    }

    /// <summary>
    /// Message Queue Response class
    /// </summary>
    public class MessageQueueResponse
    {
        /// <summary>
        /// Gets or sets the message queue request identifier.
        /// </summary>
        /// <value>
        /// The message queue request identifier.
        /// </value>
        public int MessageQueueRequestID { get; set; }
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public int Status { get; set; }
        /// <summary>
        /// Gets or sets the published on.
        /// </summary>
        /// <value>
        /// The published on.
        /// </value>
        public DateTime PublishedOn { get; set; }
        /// <summary>
        /// Gets or sets the number of retries.
        /// </summary>
        /// <value>
        /// The number of retries.
        /// </value>
        public int NumberOfRetries { get; set; }
        /// <summary>
        /// Gets or sets the last tried on.
        /// </summary>
        /// <value>
        /// The last tried on.
        /// </value>
        public DateTime LastTriedOn { get; set; }
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source { get; set; }
        /// <summary>
        /// Gets or sets the SNS topic.
        /// </summary>
        /// <value>
        /// The SNS topic.
        /// </value>
        public string SNSTopic { get; set; }
        /// <summary>
        /// Gets or sets the message attribute.
        /// </summary>
        /// <value>
        /// The message attribute.
        /// </value>
        public string MessageAttribute { get; set; }
        /// <summary>
        /// Gets or sets the message body.
        /// </summary>
        /// <value>
        /// The message body.
        /// </value>
        public string MessageBody { get; set; }
        /// <summary>
        /// Gets or sets the created on.
        /// </summary>
        /// <value>
        /// The created on.
        /// </value>
        public DateTime CreatedOn { get; set; }

    }
}
