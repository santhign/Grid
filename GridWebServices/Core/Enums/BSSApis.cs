using System.ComponentModel;
using System.Runtime.Serialization;

namespace Core.Enums
{
    public enum BSSApis
    {
        [EnumMember(Value = "GetAssets")]
        [Description("BSS API to Get Assets")]
        GetAssets = 1,

        [EnumMember(Value = "UpdateAssetStatus")]
        [Description("BSS API to Update Asset")]
        UpdateAssetStatus = 2,

        [EnumMember(Value = "QueryPlan")]
        [Description("BSS API to get Query Plan")]
        QueryPlan = 3,

        [EnumMember(Value = "GetInvoiceDetails")]
        [Description("BSS get Invoice")]
        GetInvoiceDetails = 4
    }

    public enum GridMicroservices
    {
        [EnumMember(Value = "Admn")]
        [Description("Grid Customer Micro Service")]
        Admin = 1,

        [EnumMember(Value = "Cust")]
        [Description("Grid Customer Micro Service")]
        Customer = 2,

        [EnumMember(Value = "Cust")]
        [Description("Grid Customer Micro Service")]
        Order = 3,


        [EnumMember(Value = "Cust")]
        [Description("Grid Customer Micro Service")]
        Catalog = 4
       
    }
    public enum ServiceTypes
    {
        [EnumMember(Value = "Generic")]
        [Description("Grid Generic")]
        Generic = 1,
         
        [EnumMember(Value = "Premium")]
        [Description("BSS Premium")]
        Premium = 2,

        [EnumMember(Value = "Free")]
        [Description("BSS Free")]
        Free = 3,

        
    }

    public enum AssetStatus
    {
        [EnumMember(Value = "New")]
        [Description("New Number")]
        New = 1,

        [EnumMember(Value = "Out in Market")]
        [Description("Number Out in Market")]
        OutInMarket = 2,

        [EnumMember(Value = "Blocked")]
        [Description("Blocked Number")]
        Blocked = 3,

        [EnumMember(Value = "Sold")]
        [Description("Sold Number")]
        Sold = 4,

        [EnumMember(Value = "Archived")]
        [Description("Archived Number")]
        Archived = 5,

        [EnumMember(Value = "Return-Damaged")]
        [Description("Return-Damaged Number")]
        ReturnDamaged = 6,

        [EnumMember(Value = "Returned-Exchange")]
        [Description("Returned-Exchange Number")]
        ReturnedExchange = 7,

        [EnumMember(Value = "Reserved")]
        [Description("Reserved Number")]
        Reserved = 8,

        [EnumMember(Value = "In Transit")]
        [Description("In Transit Number")]
        InTransit = 9,

        [EnumMember(Value = "Lost")]
        [Description("Lost Number")]
        Lost = 10,

        [EnumMember(Value = "Repairing")]
        [Description("Repairing Number")]
        Repairing = 11,

        [EnumMember(Value = "Stolen")]
        [Description("Stolen Number")]
        Stolen = 12,

        [EnumMember(Value = "De-Activ")]
        [Description("De-Active Number")]
        DeActiv = 13
    }
}
