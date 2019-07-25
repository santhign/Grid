using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Core.Enums
{
    public static class AdminServiceUserPermissions
    {
        // attribute params need to be constant, rather than 
        //defining an enum created this static class with const properties

        public const string CustomersList = "CustomersList";       
        public const string AdminUserList = "AdminUserList";
        public const string OrdersList = "OrdersList";
        public const string CustomersEdit = "CustomersEdit";
        public const string OrdersEdit = "OrdersEdit";
        public const string IDVerification = "IDVerification";

    }
    
}
