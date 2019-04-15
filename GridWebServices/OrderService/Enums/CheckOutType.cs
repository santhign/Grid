using System.ComponentModel;
using System.Runtime.Serialization;

namespace OrderService.Enums
{
    public enum CheckOutType
    {
        [EnumMember(Value = "Orders")]
        [Description("Initial Order")]
        Orders = 1,

        [EnumMember(Value = "ChangeRequest")]
        [Description("Change Request")]
        ChangeSim = 2,

        [EnumMember(Value = "AccountInvoices")]
        [Description("Account Invoices")]
        ChangeNumber = 3,       
    }
}
