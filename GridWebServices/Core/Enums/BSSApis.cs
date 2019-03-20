using System.ComponentModel;
using System.Runtime.Serialization;

namespace Core.Enums
{
    public enum BSSApis
    {
        [EnumMember(Value = "BSS_GetAsset")]
        [Description("BSS API to Get Assets")]
        BSS_GetAsset = 1,

        [EnumMember(Value = "BSS_UpdateAsset")]
        [Description("BSS API to Update Asset")]
        BSS_UpdateAsset = 2,

        [EnumMember(Value = "BSS_QueryPlan")]
        [Description("BSS API to get Query Plan")]
        BSS_QueryPlan = 3,

        [EnumMember(Value = "BSS_Invoice")]
        [Description("BSS get Invoice")]
        BSS_Invoice = 4
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
    public enum ServiceCodes
    {
        [EnumMember(Value = "BSS Free")]
        [Description("BSS Free Number")]
        BSSFree = 0,
         
        [EnumMember(Value = "BSS Bronze")]
        [Description("BSS Bronze Number")]
        BSSBronze = 1,

        [EnumMember(Value = "BSS Silver")]
        [Description("BSS Silver Number")]
        BSSSilver = 2,

        [EnumMember(Value = "BSS Platinum")]
        [Description("BSS Platinum Number")]
        BSSPlatinum = 3,

        [EnumMember(Value = "BSS Gold")]
        [Description("BSS Gold Number")]
        BSSGold = 4
    }
}
