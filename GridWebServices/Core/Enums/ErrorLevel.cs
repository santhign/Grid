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
        [Description("Update Subscription Failed for unblocking number")]
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
    }
}
