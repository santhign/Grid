using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminService.Models
{

    public class OrderList
    {
        public int OrderID { get; set; }
        public string OrderNumber { get; set; }
        public int ? AccountID { get; set; }
        public int ? OrderStatusNumber { get; set; }

        public string OrderStatus { get; set; }
        public string IDVerificationStatus { get; set; }
        public int ? IDVerificationStatusNumber { get; set; }
        public int ? RejectionCount { get; set; }
        public string Name { get; set; }
        public DateTime ? OrderDate { get; set; }

        public DateTime ? DeliveryDate { get; set; }
        public DateTime? DeliveryFromTime { get; set; }
        public DateTime? DeliveryToTime { get; set; }

    }

}
