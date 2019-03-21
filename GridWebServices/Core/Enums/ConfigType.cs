﻿using System;
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
        BSS = 2
    }

    public enum OrderStatus
    {
        [EnumMember(Value = "New Order")]
        [Description("New Order")]
        New = 1,

       
    }
}
