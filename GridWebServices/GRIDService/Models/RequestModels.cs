using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GRIDService.Models
{
    /// <summary>
    /// Suspension Request Class
    /// </summary>
    public class SuspensionRequest
    {
        /// <summary>
        /// Gets or sets the change request identifier.
        /// </summary>
        /// <value>
        /// The change request identifier.
        /// </value>
        public int ChangeRequestID { get; set; }
        /// <summary>
        /// Gets or sets the type of the suspension. --0=Partial; 1=Full
        /// </summary>
        /// <value>
        /// The type of the suspension.
        /// </value>
        public int SuspensionType { get; set; }
        /// <summary>
        /// Gets or sets the remarks.
        /// </summary>
        /// <value>
        /// The remarks.
        /// </value>
        public string Remarks { get; set; }
        /// <summary>
        /// Gets or sets the status. --c=Completed;r=Rejected
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string Status { get; set; }
        /// <summary>
        /// Gets or sets the reject reason.
        /// </summary>
        /// <value>
        /// The reject reason.
        /// </value>
        public string RejectReason { get; set; }
        /// <summary>
        /// Gets or sets the current status.  --0=Partial; 1=Full; -1= No action
        /// </summary>
        /// <value>
        /// The current status.
        /// </value>
        public int CurrentStatus { get; set; }
    }

    /// <summary>
    /// Termination Or Unsuspension Request class
    /// </summary>
    public class TerminationOrUnsuspensionRequest
    {
        /// <summary>
        /// Gets or sets the change request identifier.
        /// </summary>
        /// <value>
        /// The change request identifier.
        /// </value>
        public int ChangeRequestID { get; set; }

        /// <summary>
        /// Gets or sets the remarks.
        /// </summary>
        /// <value>
        /// The remarks.
        /// </value>
        public string Remarks { get; set; }
        /// <summary>
        /// Gets or sets the status.--c=Completed;r=Rejected
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string Status { get; set; }
        /// <summary>
        /// Gets or sets the reject reason.
        /// </summary>
        /// <value>
        /// The reject reason.
        /// </value>
        public string RejectReason { get; set; }
        /// <summary>
        /// Gets or sets the current status.-- 1=Process; -1= No action
        /// </summary>
        /// <value>
        /// The current status.
        /// </value>
        public int CurrentStatus { get; set; }
    }

    /// <summary>
    /// Update Initial Order Subscriptions Request class
    /// </summary>
    public class UpdateInitialOrderSubscriptionsRequest
    {
        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        public int OrderID { get; set; }
        /// <summary>
        /// Gets or sets the subscriber identifier.
        /// </summary>
        /// <value>
        /// The subscriber identifier.
        /// </value>
        public int SubscriberID { get; set; }
        /// <summary>
        /// Gets or sets the bundle identifier.
        /// </summary>
        /// <value>
        /// The bundle identifier.
        /// </value>
        public int BundleID { get; set; }
        /// <summary>
        /// Gets or sets the BSS plan code.
        /// </summary>
        /// <value>
        /// The BSS plan code.
        /// </value>
        public string BSSPlanCode { get; set; }
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public int Status { get; set; }
        /// <summary>
        /// Gets or sets the valid from.--1=Activated; 2=OnHold; 3=Terminated
        /// </summary>
        /// <value>
        /// The valid from.
        /// </value>
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// Gets or sets the valid to.
        /// </summary>
        /// <value>
        /// The valid to.
        /// </value>
        public DateTime ValidTo { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class UpdateOrderStatus
    {
        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        public int OrderID { get; set; }
        /// <summary>
        /// Gets or sets the order number.
        /// </summary>
        /// <value>
        /// The order number.
        /// </value>
        public string OrderNumber { get; set; }
        /// <summary>
        /// Gets or sets the orderstatus. --C=Completed; F=Failed
        /// </summary>
        /// <value>
        /// The orderstatus.
        /// </value>
        public string Orderstatus { get; set; } 
        /// <summary>
        /// Gets or sets the error reason.
        /// </summary>
        /// <value>
        /// The error reason.
        /// </value>
        public string error_reason { get; set; }

    }
}
