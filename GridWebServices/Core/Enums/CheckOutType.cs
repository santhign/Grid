using System.ComponentModel;
using System.Runtime.Serialization;

namespace Core.Enums
{
    public enum CheckOutType
    {
        [EnumMember(Value = "Orders")]
        [Description("Initial Order")]
        Orders = 1,

        [EnumMember(Value = "ChangeRequest")]
        [Description("Change Request")]
        ChangeRequest = 2,

        [EnumMember(Value = "AccountInvoices")]
        [Description("Account Invoices")]
        AccountInvoices = 3,

        [EnumMember(Value = "ChangeCard")]
        [Description("Change Card")]
        ChangeCard = 4,
    }
}
