using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Core.Enums
{   
    public enum ConfiType
    {
        [EnumMember(Value = "System")]
        [Description("System Configuration")]
        System = 1,

        [EnumMember(Value = "BSS")]
        [Description("BSS API Configuration")]
        BSS = 2,

        [EnumMember(Value = "AWS")]
        [Description("AWS API Configuration")]
        AWS = 3,

        [EnumMember(Value = "MPGS")]
        [Description("MPGS Gateway Configuration")]
        MPGS = 4,

        [EnumMember(Value = "ForgotPasswordMsg")]
        [Description("ForgotPasswordMsg")]
        ForgotPasswordMsg = 5,

        [EnumMember(Value = "Notification")]
        [Description("Notification")]
        Notification = 6,

    }

    public enum ConfigKeys
    {
        [EnumMember(Value = "SNS_Topic_ChangeRequest")]
        [Description("SNS_Topic_ChangeRequest")]
        SNS_Topic_ChangeRequest = 1,

        [EnumMember(Value = "BuddyTrialIntervel")]
        [Description("BuddyTrialIntervel")]
        BuddyTrialIntervel = 2,

        [EnumMember(Value = "BSSInvoiceDefaultDateRangeInMonths")]
        [Description("BSSInvoiceDefaultDateRangeInMonths")]
        BSSInvoiceDefaultDateRangeInMonths = 3,

        [EnumMember(Value = "BSSInvoiceDownloadLink")]
        [Description("BSSInvoiceDownloadLink")]
        BSSInvoiceDownloadLink = 4,

        [EnumMember(Value = "CustomerTokenExpiryInDays")]
        [Description("CustomerTokenExpiryInDays")]
        CustomerTokenExpiryInDays = 5,

        [EnumMember(Value = "MQConsoleInterval")]
        [Description("MQConsoleInterval")]
        MQConsoleInterval = 6,

        [EnumMember(Value = "NRICReUploadLink")]
        [Description("NRICReUploadLink")]
        NRICReUploadLink = 7,

    }

    public enum OrderStatus
    {
        [EnumMember(Value = "New Order")]
        [Description("New Order")]
        NewOrder = 1,

        [EnumMember(Value = "OldUpdatedOrderr")]
        [Description("OldUpdatedOrder")]
        OldUpdatedOrder = 2,
        
    }

    /// <summary>
    /// Event Type String static class
    /// </summary>
    public static class EventTypeString
    {
        /// <summary>
        /// The event type constant string
        /// </summary>
        public const string EventType = "event_type";
    }

    /// <summary>
    /// Source class
    /// </summary>
    public static class Source
    {
        /// <summary>
        /// The change request constact value
        /// </summary>
        public const string ChangeRequest = "ChangeRequest";

        /// <summary>
        /// The order constant vvalue
        /// </summary>
        public const string Order = "Order";
    }
}
