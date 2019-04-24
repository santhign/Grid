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

        public const string z_InsertIntoMessageQueueRequests = "z_InsertIntoMessageQueueRequests";

        public const string z_GetSingleMessageQueueRecord = "z_GetSingleMessageQueueRecord";

        public const string z_UpdateStatusInMessageQueueRequests = "z_UpdateStatusInMessageQueueRequests";

        public const string Orders_CR_BuySharedVAS = "Orders_CR_BuySharedVAS";
    }

}
