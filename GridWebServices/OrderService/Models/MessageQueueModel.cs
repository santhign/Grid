using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace OrderService.Models
{

    /// <summary>
    /// MEssage Body for Change Request class model
    /// </summary>   
    [DataContract]
    public class MessageBodyForCR
    {
        [DataMember]
        public int ChangeRequestID { get; set; }
        [DataMember]
        public int AccountID { get; set; }
        [DataMember]
        public int CustomerID { get; set; }
        [DataMember]
        public int? SubscriberID { get; set; }
        [DataMember]
        public string OrderNumber { get; set; }
        [DataMember]
        public DateTime RequestOn { get; set; }
        [DataMember]
        public DateTime? EffectiveDate { get; set; }
        [DataMember]
        public string BillingUnit { get; set; }
        [DataMember]
        public string BillingFloor { get; set; }
        [DataMember]
        public string BillingBuildingNumber { get; set; }
        [DataMember]
        public string BillingBuildingName { get; set; }
        [DataMember]
        public string BillingStreetName { get; set; }
        [DataMember]
        public string BillingPostCode { get; set; }
        [DataMember]
        public string BillingContactNumber { get; set; }
        [DataMember]
        public string MobileNumber { get; set; }
        [DataMember]
        public int ? PremiumType { get; set; }
        [DataMember]
        public int? IsPorted { get; set; }
        [DataMember]
        public int? IsOwnNumber { get; set; }
        [DataMember]
        public string DonorProvider { get; set; }
        [DataMember]
        public string PortedNumberTransferForm { get; set; }
        [DataMember]
        public string PortedNumberOwnedBy { get; set; }
        [DataMember]
        public string PortedNumberOwnerRegistrationID { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Nationality { get; set; }
        [DataMember]
        public string IdType { get; set; }
        [DataMember]
        public string IdNumber { get; set; }
        [DataMember]
        public int? IsSameAsBilling { get; set; }
        [DataMember]
        public string ShippingUnit { get; set; }
        [DataMember]
        public string ShippingFloor { get; set; }
        [DataMember]
        public string ShippingBuildingNumber { get; set; }
        [DataMember]
        public string ShippingBuildingName { get; set; }
        [DataMember]
        public string ShippingStreetName { get; set; }
        [DataMember]
        public string ShippingPostCode { get; set; }
        [DataMember]
        public string ShippingContactNumber { get; set; }
        [DataMember]
        public string AlternateRecipientContact { get; set; }
        [DataMember]
        public string AlternateRecipientName { get; set; }
        [DataMember]
        public string AlternateRecipientEmail { get; set; }
        [DataMember]
        public string PortalSlotID { get; set; }
        [DataMember]
        public DateTime? SlotDate { get; set; }
        [DataMember]
        public DateTime ? SlotFromTime { get; set; }
        [DataMember]
        public DateTime? SlotToTime { get; set; }
        [DataMember]
        public DateTime? ScheduledDate { get; set; }
        [DataMember]
        public string OldMobileNumber { get; set; }
        [DataMember]
        public string NewMobileNumber { get; set; }
        [DataMember]
        public string OldSIM { get; set; }
        
        [DataMember(Name = "Bundles")]
        public List<BundleDetails> Bundles { get; set; }

       
        [DataMember(Name = "Charges")]
        public IList<ChargesDetails> Charges { get; set; }
    }

    /// <summary>
    /// Subscriber details
    /// </summary>
    public class BundleDetails
    {
        [DataMember]
        public int? BundleID { get; set; }
        [DataMember]
        public string BSSPlanCode { get; set; }
        [DataMember]
        public string BSSPlanName { get; set; }
        [DataMember]
        public int? PlanType { get; set; }
        [DataMember]
        public int? OldBundleID { get; set; }
        [DataMember]
        public string PlanMarketingName { get; set; }
        [DataMember]
        public string PortalDescription { get; set; }
        [DataMember]
        public double? TotalData { get; set; }
        [DataMember]
        public double? TotalSMS { get; set; }
        [DataMember]
        public double? TotalVoice { get; set; }
        [DataMember]
        public double? ApplicableSubscriptionFee { get; set; }
        [DataMember]
        public string ServiceName { get; set; }
        [DataMember]
        public double? ApplicableServiceFee { get; set; }
        
        [DataMember]
        public int? OldPlanID { get; set; }
        [DataMember]
        public int? OldBSSPlanId { get; set; }
        [DataMember]
        public string OldBSSPlanName { get; set; }
        

    }

    /// <summary>
    /// Bundle details
    /// </summary>
    public class ChargesDetails
    {
        [DataMember]
        public int ChangeRequestID { get; set; }
        [DataMember]
        public int? SubscriberID { get; set; }
        [DataMember]
        public string PortalServiceName { get; set; }
        [DataMember]
        public double? ServiceFee { get; set; }
        [DataMember]
        public int? IsRecurring { get; set; }
        [DataMember]
        public int? IsGSTIncluded { get; set; }
      

    }

    /// <summary>
    /// MEssage details for sending to message Queue
    /// </summary>
    public class MessageDetailsForCROrOrder
    {
        /// <summary>
        /// Gets or sets the request type identifier.
        /// </summary>
        /// <value>
        /// The request type identifier.
        /// </value>
        public int RequestTypeID { get; set; }
        /// <summary>
        /// Gets or sets the change request identifier.
        /// </summary>
        /// <value>
        /// The change request identifier.
        /// </value>
        public int ChangeRequestID { get; set; }
    }

    public class MessageQueueRequest
    {
        public string Source { get; set; }
        public string SNSTopic { get; set; }
        public string MessageAttribute { get; set; }
        public string MessageBody { get; set; }
        public int Status { get; set; }
        public DateTime ? PublishedOn { get; set; }
        public DateTime ? CreatedOn { get; set; }
        public int NumberOfRetries { get; set; }
        public DateTime ? LastTriedOn { get; set; }


    }

    public class MessageQueueRequestException : MessageQueueRequest
    {
        public string Remark { get; set; }
        public string Exception { get; set; }
    }
}
