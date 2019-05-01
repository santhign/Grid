using System.ComponentModel;
using System.Runtime.Serialization;

namespace OrderService.Enums
{
    public enum MPGSAPIOperation
    {
        [EnumMember(Value = "CREATE_CHECKOUT_SESSION")]
        [Description("Create MPGS checkout session action")]
        CREATE_CHECKOUT_SESSION = 1,

        [EnumMember(Value = "RETRIEVE_ORDER")]
        [Description("MPGS payment Order retrieve action")]
        RETRIEVE_ORDER = 2,

        [EnumMember(Value = "CAPTURE")]
        [Description("MPGS Caputre authorized amount")]
        CAPTURE = 3,

        [EnumMember(Value = "AUTHORIZE")]
        [Description("MPGS Authorize amount")]
        AUTHORIZE = 4,
    }

    public enum MPGSAPIResponse
    {
        [EnumMember(Value = "SUCCESS")]
        [Description("SUCCESS")]
        SUCCESS = 1,

        [EnumMember(Value = "Unsuccessful")]
        [Description("The payment was unsuccessful")]
        Unsuccessful = 2,

        [EnumMember(Value = "Problem Completing Transaction")]
        [Description("There was a problem completing your transaction")]
        ProblemCompletingTransaction = 3,

        [EnumMember(Value = "Checkout receipt error")]
        [Description("Hosted checkout receipt error")]
        HostedCheckoutReceiptError = 4,

        [EnumMember(Value = "Checkout retrieve receipt")]
        [Description("Hosted checkout retrieve order response")]
        HostedCheckoutRetrieveReceipt = 5,

        [EnumMember(Value = "Checkout response")]
        [Description("Hosted checkout response")]
        HostedCheckoutResponse = 6,


        [EnumMember(Value = "Webhook Notification Folder Error")]
        [Description("Error Creating Webhook notification folder")]
        WebhookNotificationFolderError = 7,

        [EnumMember(Value = "Webhook Notification Folder Delete Error")]
        [Description("Error Deleting Webhook notification folder")]
        WebhookNotificationFolderDeleteError = 8,

        [EnumMember(Value = "CAPTURED")]
        [Description("CAPTURED")]
        CAPTURED = 9,
    }
}
