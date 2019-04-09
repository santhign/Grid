using System.ComponentModel;
using System.Runtime.Serialization;

namespace Core.Enums
{
    public enum ErrorLevel
    {
        [EnumMember(Value = "Critical")]
        [Description("Critical Error")]
        Critical = 1,

        [EnumMember(Value = "Medium")]
        [Description("Medium Error")]
        Medium = 2,

        [EnumMember(Value = "Low")]
        [Description("Low")]
        Low = 3,       
    }


    public enum CommonErrors
    {
        [EnumMember(Value = "GetAssetFailed")]
        [Description("GetAsset request Failed")]
        GetAssetFailed = 1,

        [EnumMember(Value = "Create order failed")]
        [Description("Create order failed, order already exists")]
        CreateOrderFailed = 2,

        [EnumMember(Value = "Expired Token")]
        [Description("Token authentication failed. Expired Token")]
        ExpiredToken = 3,

        [EnumMember(Value = "UpdateAssetBlockingFailed")]
        [Description("Update asset failed for blocking number")]
        UpdateAssetBlockingFailed = 4,

        [EnumMember(Value = "UpdateAssetUnBlockingFailed")]
        [Description("Update asset failed for unblocking number")]
        UpdateAssetUnBlockingFailed = 5,

        [EnumMember(Value = "UpdateSubscriptionFailed")]
        [Description("Update Subscription Failed")]
        UpdateSubscriptionFailed = 6,

        [EnumMember(Value = "Failed To Locate Updated Subscription")]
        [Description("Failed ToLocate Updated Subscription")]
        FailedToLocateUpdatedSubscription = 7,

        [EnumMember(Value = "Failed To Get Order Customer")]
        [Description("Failed To Get Order Customer")]
        FailedToGetCustomer = 8,

        [EnumMember(Value = "Failed To Get Configuration")]
        [Description("Failed To Get Configuration")]
        FailedToGetConfiguration = 9,

        [EnumMember(Value = "S3 Upload Failed")]
        [Description("AWS S3 Upload Failed")]
        S3UploadFailed = 10,

        [EnumMember(Value = "CreateSubscriptionFailed")]
        [Description("Create Subscription Failed")]
        CreateSubscriptionFailed = 11,

        [EnumMember(Value = "Failed To Locate Created Subscription")]
        [Description("Failed ToLocate Created Subscription")]
        FailedToLocateCreatedSubscription =12,

        [EnumMember(Value = "No Delivery Slot Exists")]
        [Description("No Delivery Slot Exists")]
        DeliverySlotNotExists = 13,

        [EnumMember(Value = "Update Personal Details Failed")]
        [Description("Personal Details Updation Failed")]
        FailedUpdatePersonalDetails = 14,

        [EnumMember(Value = "Update Billing Details Failed")]
        [Description("Billing Details Updation Failed")]
        FailedUpdateBillingDetails = 15,

        [EnumMember(Value = "Update Shipping Details Failed")]
        [Description("Shipping Details Updation Failed")]
        FailedUpdateShippingDetails = 16,

        [EnumMember(Value = "Update LOA Details Failed")]
        [Description("LOA Details Updation Failed")]
        FailedUpdateLOADetails = 17,

        [EnumMember(Value = "Referral Code Exists")]
        [Description("Referral Code Exists")]
        ReferralCodeExists = 18,

        [EnumMember(Value = "Referral Code Not Exists")]
        [Description("Referral Code Not Exists")]
        ReferralCodeNotExists = 19,

        [EnumMember(Value = "Failed To Updated Order Subscription Details")]
        [Description("Failed To Update Subscription")]
        FailedToUpdatedSubscriptionDetails = 20,

        [EnumMember(Value = "Checkout Session Created")]
        [Description("Checkout Session Created Successfully")]
        CheckoutSessionCreated = 21,

        [EnumMember(Value = "Line Delete Failed")]
        [Description("Failed to delete additional line")]
        LineDeleteFailed = 22,

        [EnumMember(Value = "Line Deleted Successfully")]
        [Description("Successfully deleted additional line")]
        LineDeleteSuccess = 23,

        [EnumMember(Value = "Assign New Number Faild")]
        [Description("Failed to assign New Number")]
        AssignNewNumberFailed = 24,

        [EnumMember(Value = "New Number Assign Success")]
        [Description("Successfully Assigned New Number")]
        AssignNuewNumberSuccess = 25,

        [EnumMember(Value = "Mandatory Record Empty")]
        [Description("Mandatory RecordEmpty")]
        MandatoryRecordEmpty = 26,

        [EnumMember(Value = "Order Rolled Back")]
        [Description("Order Rolled Back")]
        OrderRolledBack = 27,

        [EnumMember(Value = "Order Rolled Back Failed")]
        [Description("Order Rolled Back Failed")]
        OrderRolledBackFailed = 28,

    }
}
