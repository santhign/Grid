using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Enums
{
    /// <summary>
    /// Database object names
    /// </summary>
    public static class DbObjectNames
    {
        /// <summary>
        /// The orders cr insert remove vas
        /// </summary>
        public const string Orders_CR_InsertRemoveVAS = "Orders_CR_InsertRemoveVAS";
        /// <summary>
        /// The customer authenticate token
        /// </summary>
        public const string Customer_AuthenticateToken = "Customer_AuthenticateToken";

        /// <summary>
        /// The orders create order
        /// </summary>
        public const string Orders_CreateOrder = "Orders_CreateOrder";

        /// <summary>
        /// The orders cr buy vas
        /// </summary>
        public const string Orders_CR_BuyVAS = "Orders_CR_BuyVAS";

        /// <summary>
        /// The order cr sim replacement request
        /// </summary>
        public const string Order_CR_SIMReplacementRequest = "Order_CR_SIMReplacementRequest";

        /// <summary>
        /// The orders cr raise request
        /// </summary>
        public const string Orders_CR_RaiseRequest = "Orders_CR_RaiseRequest";

        /// <summary>
        /// The customer cr change phone request
        /// </summary>
        public const string Customer_CR_ChangePhoneRequest = "Customer_CR_ChangePhoneRequest";

        /// <summary>
        /// The orders cr update cr shipping details
        /// </summary>
        public const string Orders_CR_UpdateCRShippingDetails = "Orders_CR_UpdateCRShippingDetails";
        /// <summary>
        /// The customer update customer profile
        /// </summary>
        public const string Customer_UpdateCustomerProfile = "Customer_UpdateCustomerProfile";

        /// <summary>
        /// The cr get message body
        /// </summary>
        public const string CR_GetMessageBody = "CR_GetMessageBody";

        /// <summary>
        /// The z insert into message queue requests
        /// </summary>
        public const string z_InsertIntoMessageQueueRequests = "z_InsertIntoMessageQueueRequests";

        /// <summary>
        /// The z get single message queue record
        /// </summary>
        public const string z_GetSingleMessageQueueRecord = "z_GetSingleMessageQueueRecord";

        /// <summary>
        /// The z update status in message queue requests
        /// </summary>
        public const string z_UpdateStatusInMessageQueueRequests = "z_UpdateStatusInMessageQueueRequests";

        /// <summary>
        /// The z update status in message queue requests exception
        /// </summary>
        public const string z_UpdateStatusInMessageQueueRequestsException = "z_UpdateStatusInMessageQueueRequestsException";

        /// <summary>
        /// The orders cr buy shared vas
        /// </summary>
        public const string Orders_CR_BuySharedVAS = "Orders_CR_BuySharedVAS";

        /// <summary>
        /// The orders cr insert remove shared vas
        /// </summary>
        public const string Orders_CR_InsertRemoveSharedVAS = "Orders_CR_InsertRemoveSharedVAS";

        /// <summary>
        /// The orders cr change plan
        /// </summary>
        public const string Orders_CR_ChangePlan = "Orders_CR_ChangePlan";

        /// <summary>
        /// The orders cancel order
        /// </summary>
        public const string Orders_CancelOrder = "Orders_CancelOrder";

        /// <summary>
        /// The orders reschedule delivery
        /// </summary>
        public const string Orders_RescheduleDelivery = "Orders_RescheduleDelivery";
        /// <summary>
        /// The orders process reschedule delivery
        /// </summary>
        public const string Orders_ProcessRescheduleDelivery = "Orders_ProcessRescheduleDelivery";

        /// <summary>
        /// The cr get buddy details
        /// </summary>
        public const string CR_GetBuddyDetails = "CR_GetBuddyDetails";

        /// <summary>
        /// The orders get cr or order details for message queue
        /// </summary>
        public const string Orders_GetCROrOrderDetailsForMessageQueue = "Orders_GetCROrOrderDetailsForMessageQueue";

        /// <summary>
        /// The z SMS notifications log entry
        /// </summary>
        public const string z_SMSNotificationsLogEntry = "z_SMSNotificationsLogEntry";

        /// <summary>
        /// The z get SMS template by name
        /// </summary>
        public const string z_GetSMSTemplateByName = "z_GetSMSTemplateByName";

        /// <summary>
        /// The z email notifications log entry for dev purpose
        /// </summary>
        public const string z_EmailNotificationsLogEntryForDevPurpose = "z_EmailNotificationsLogEntryForDevPurpose";

        /// <summary>
        /// The orders cr get cr details
        /// </summary>
        public const string Orders_CR_GetCRDetails = "Orders_CR_GetCRDetails";
    }

}
