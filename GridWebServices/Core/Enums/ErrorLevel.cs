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
}
