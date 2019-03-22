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
        UpdateAssetBlockingFailed = 1,

    }
}
