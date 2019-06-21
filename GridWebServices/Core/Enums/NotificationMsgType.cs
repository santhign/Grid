using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Core.Enums
{
    public enum NotificationMsgType
    {
        [EnumMember(Value = "Email")]
        [Description("Email")]
        Email = 1,

        [EnumMember(Value = "SMS")]
        [Description("SMS")]
        SMS = 2,

    }
    public enum NotificationEvent
    {
        [EnumMember(Value = "Registration")]
        [Description("Registration")]
        Registration = 1,

        [EnumMember(Value = "ForgotPassword")]
        [Description("ForgotPassword")]
        ForgetPassword = 2,

        [EnumMember(Value = "OrderSuccess")]
        [Description("OrderSuccess")]
        OrderSuccess = 3,

        [EnumMember(Value = "ICValidationChange")]
        [Description("ICValidationChange")]
        ICValidationChange = 4,

        [EnumMember(Value = "ICValidationReject")]
        [Description("ICValidationReject")]
        ICValidationReject = 5,

        [EnumMember(Value = "ReplaceSIM")]
        [Description("ReplaceSIM")]
        ReplaceSIM = 6,

        [EnumMember(Value = "RescheduleDelivery")]
        [Description("RescheduleDelivery")]
        RescheduleDelivery = 7,

    }

    public enum SNSNotification
    {
        [EnumMember(Value = "Invalid signature")]
        [Description("Invalid signature")]
        InvalidSignature = 1,

        [EnumMember(Value = "Subscription Confirmed")]
        [Description("Successfully confirmed endpoint subscription")]
        SubscriptionConfirmed = 2,

        [EnumMember(Value = "OrderSuccess")]
        [Description("OrderSuccess")]
        OrderSuccess = 3,

        [EnumMember(Value = "EmptySNSTypeHeader")]
        [Description("Empty SNS Header x-amz-sns-message-type")]
        EmptySNSTypeHeader = 3,

    }

}
