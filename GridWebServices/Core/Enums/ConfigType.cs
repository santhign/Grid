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
        SNS_Topic_ChangeRequest = 1
        
    }

    public enum OrderStatus
    {
        [EnumMember(Value = "New Order")]
        [Description("New Order")]
        NewOrder = 1,

       
    }
}
