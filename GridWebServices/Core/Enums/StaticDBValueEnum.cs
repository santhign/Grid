using System.ComponentModel;
using System.Runtime.Serialization;

namespace Core.Enums
{
    public enum RequestType
    {
        [EnumMember(Value = "DefaultOrder")]
        [Description("DefaultOrder")]
        DefaultOrder = 1,
        [EnumMember(Value = "Addition")]
        [Description("Addition")]
        Addition = 2,
        [EnumMember(Value = "Removal")]
        [Description("Removal")]
        Removal = 3,
        [EnumMember(Value = "Termination")]
        [Description("Termination")]
        Termination = 4,
        [EnumMember(Value = "Suspension")]
        [Description("Suspension")]
        Suspension = 5,
        [EnumMember(Value = "ChangeSim")]
        [Description("ChangeSim")]
        ChangeSim = 6,
        [EnumMember(Value = "ChangeNumber")]
        [Description("ChangeNumber")]
        ChangeNumber = 7

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
}