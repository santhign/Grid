using System.ComponentModel;
using System.Runtime.Serialization;

namespace Core.Enums
{
    public enum AWSMessageType
    {
        [EnumMember(Value = "SubscriptionConfirmation")]
        [Description("SubscriptionConfirmation")]
        SubscriptionConfirmation = 1,

        [EnumMember(Value = "Notification")]
        [Description("Notification")]
        Notification = 2,

        [EnumMember(Value = "UnsubscribeConfirmation")]
        [Description("UnsubscribeConfirmation")]
        UnsubscribeConfirmation = 3,
    }
}
