﻿using System.ComponentModel;
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
        GetInvoiceDetails = 4,

        [EnumMember(Value = "QuerySubscriber")]
        [Description("QuerySubscriber")]
        QuerySubscriber = 5
    }

    public enum GridMicroservices
    {
        [EnumMember(Value = "Admn")]
        [Description("Grid Customer Micro Service")]
        Admin = 1,

        [EnumMember(Value = "Cust")]
        [Description("Grid Customer Micro Service")]
        Customer = 2,

        [EnumMember(Value = "Order")]
        [Description("Grid Customer Micro Service")]
        Order = 3,


        [EnumMember(Value = "Catalog")]
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
      

    public enum PremiumTypes
    {
        [EnumMember(Value = "Gold")]
        [Description("Premium Gold")]
        Gold = 1,

        [EnumMember(Value = "Platinum")]
        [Description("Premium Platinum")]
        Platinum = 2,

        [EnumMember(Value = "Silver")]
        [Description("Premium Silver")]
        Silver = 3,


    }

    public enum ConnectionTypes
    {
        [EnumMember(Value = "Prepaid")]
        [Description("Prepaid")]
        Prepaid = 1,

        [EnumMember(Value = "Postpaid")]
        [Description("Postpaid")]
        Postpaid = 2       
    }

    public enum Status
    {
        [EnumMember(Value = "Pending")]
        [Description("Pending")]
        Pending = 0,

        [EnumMember(Value = "Confirm")]
        [Description("Confirm")]
        Confirm = 3
    }

    public enum AssetStatus
    {
        [EnumMember(Value = "New")]
        [Description("New Number")]
        New = 1,

        [EnumMember(Value = "Out in Market")]
        [Description("Number Out in Market")]
        OutInMarket = 2,      

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
        DeActiv = 13,

        [EnumMember(Value = "Blocked")]
        [Description("Blocked Number")]
        Blocked = 14,
    }

    public enum Misc
    {
        [EnumMember(Value = "Account")]
        [Description("Pending")]
        Account = 0,

    }

    public enum BSSCalls
    {
        [EnumMember(Value = "NoSession")]
        [Description("No Session")]
        NoSession = 0,

        [EnumMember(Value = "BSSNewCallSession")]
        [Description("BSS New Call Session")]
        NewSession = 1,

        [EnumMember(Value = "BSSExistingSessionCall")]
        [Description("BSS Call with  Existing Session UserID")]
        ExistingSession = 2,


    }
}
