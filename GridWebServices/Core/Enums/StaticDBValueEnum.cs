﻿using System.ComponentModel;
using System.Runtime.Serialization;

namespace Core.Enums
{
    public enum RequestType
    {
        [EnumMember(Value = "NewCustomer")]
        [Description("NewCustomer")]
        NewCustomer = 1,
        [EnumMember(Value = "NewService")]
        [Description("NewService")]
        NewService = 2,
        [EnumMember(Value = "AddVAS")]
        [Description("AddVAS")]
        AddVAS = 3,
        [EnumMember(Value = "RemoveVAS")]
        [Description("RemoveVAS")]
        RemoveVAS = 4,
        [EnumMember(Value = "ChangePlan")]
        [Description("ChangePlan")]
        ChangePlan = 5,
        [EnumMember(Value = "Suspend")]
        [Description("Suspend")]
        Suspend = 6,
        [EnumMember(Value = "UnSuspend")]
        [Description("UnSuspend")]
        UnSuspend = 7,
        [EnumMember(Value = "Terminate")]
        [Description("Terminate")]
        Terminate = 8,
        [EnumMember(Value = "ReplaceSIM")]
        [Description("ReplaceSIM")]
        ReplaceSIM = 9,
        [EnumMember(Value = "EditBillAddress")]
        [Description("EditBillAddress")]
        EditBillAddress = 10,
        [EnumMember(Value = "EditContact")]
        [Description("EditContact")]
        EditContact = 11,
        [EnumMember(Value = "CancelOrder")]
        [Description("CancelOrder")]
        CancelOrder = 12,
        [EnumMember(Value = "PayBill")]
        [Description("PayBill")]
        PayBill = 13,
        [EnumMember(Value = "ChangeNumber")]
        [Description("ChangeNumber")]
        ChangeNumber = 14



    }

    /// <summary>Different PlanType </summary>
    public enum PlanType
    {
        [EnumMember(Value = "Base")]
        [Description("Base")]
        Base = 0,

        [EnumMember(Value = "VAS")]
        [Description("VAS Plan")]
        VAS = 1,

        [EnumMember(Value = "Shared_VAS")]
        [Description("Shared VAS")]
        Shared_VAS = 2,
    }

    public enum ConfigKey
    {
        [EnumMember(Value = "SNS_Subject_CreateCustomer")]
        [Description("SNS_Subject_CreateCustomer")]
        SNS_Subject_CreateCustomer,
        [EnumMember(Value = "SNS_Topic_ChangeRequest")]
        [Description("SNS_Topic_ChangeRequest")]
        SNS_Topic_ChangeRequest,
        [EnumMember(Value = "SNS_Topic_CreateCustomer")]
        [Description("SNS_Topic_CreateCustomer")]
        SNS_Topic_CreateCustomer
    }
}