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
        ForgotPassword = 2,

        [EnumMember(Value = "OrderSuccess")]
        [Description("OrderSuccess")]
        OrderSuccess = 3,

    }

}
