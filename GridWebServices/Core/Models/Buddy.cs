using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class BuddyCheckList
    {
        public int CustomerID { get; set; }
        public int OrderID { get; set; }
        public int OrderSubscriberID { get; set; }
        public string MobileNumber { get; set; }
        public int HasBuddyPromotion { get; set; }
        public bool IsProcessed { get; set; }

    }
    public class BuddyNumberUpdate
    {
        public int OrderSubscriberID { get; set; }
        public string UserId { get; set; }
        public string NewMobileNumber { get; set; }

    }
}
