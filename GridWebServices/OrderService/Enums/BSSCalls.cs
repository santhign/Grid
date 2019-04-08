using System.ComponentModel;
using System.Runtime.Serialization;

namespace OrderService.Enums
{
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
