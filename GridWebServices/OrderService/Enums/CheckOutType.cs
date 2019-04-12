using System.ComponentModel;
using System.Runtime.Serialization;

namespace OrderService.Enums
{
    public enum CheckOutType
    {
        [EnumMember(Value = "Orders")]
        [Description("Initial Order")]
        Orders = 1,

        [EnumMember(Value = "ChangeSim")]
        [Description("Sim Change Request")]
        ChangeSim = 2,

        [EnumMember(Value = "ChangeNumber")]
        [Description("Change Number Request")]
        ChangeNumber = 3,

        [EnumMember(Value = "ChangePlan")]
        [Description("ChangevPlan Request")]
        ChangePlan = 4,       
    }
}
