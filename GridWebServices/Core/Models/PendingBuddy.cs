using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class PendingBuddy
    {
        public int PendingBuddyOrderListID { get; set; }
        public int PendingBuddyID { get; set; }
        public int OrderID { get; set; }
        public int OrderSubscriberID { get; set; }
        public string MobileNumber { get; set; }
        public bool IsProcessed { get; set; }
        public string DateCreated { get; set; }
    }

    public class PendingBuddyOrders
    {      
        public int OrderID { get; set; }
        
    }
}
