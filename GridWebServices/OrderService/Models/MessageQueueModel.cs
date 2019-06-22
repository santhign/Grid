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
        public int? PremiumType { get; set; }
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
        public TimeSpan? SlotFromTime { get; set; }
        [DataMember]
        public TimeSpan? SlotToTime { get; set; }
        [DataMember]
        public DateTime? ScheduledDate { get; set; }
        [DataMember]
        public string OldMobileNumber { get; set; }
        [DataMember]
        public string NewMobileNumber { get; set; }
        [DataMember]
        public string OldSIM { get; set; }

        [DataMember]
        public double ? ServiceFee { get; set; }

        [DataMember]
        public double? AmountPaid { get; set; }
        [DataMember]
        public string PaymentMode { get; set; }
        [DataMember]
        public string MPGSOrderID { get; set; }
        [DataMember]
        public string MaskedCardNumber { get; set; }
        [DataMember]
        public string Token { get; set; }
        [DataMember]
        public string CardType { get; set; }
        [DataMember]
        public string CardHolderName { get; set; }
        [DataMember]
        public int? ExpiryMonth { get; set; }
        [DataMember]
        public int? ExpiryYear { get; set; }
        [DataMember]
        public string CardFundMethod { get; set; }
        [DataMember]
        public string CardBrand { get; set; }
        [DataMember]
        public string CardIssuer { get; set; }
        [DataMember]
        public DateTime? DateofBirth { get; set; }
        [DataMember]
        public string ReferralCode { get; set; }
        [DataMember]
        public DateTime? ProcessedOn { get; set; }
        [DataMember]
        public string InvoiceNumber { get; set; }
        [DataMember]
        public string InvoiceUrl { get; set; }

        [DataMember(Name = "Bundles")]
        public List<BundleDetails> Bundles { get; set; }

        [DataMember(Name = "CurrBundles")]
        public List<CurrBundleDetails> CurrBundles { get; set; }


        [DataMember(Name = "Charges")]
        public IList<ChargesDetails> Charges { get; set; }
    }
    public class CurrBundleDetails
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
        public DateTime? StartDate { get; set; }
        [DataMember]
        public DateTime? ExpiryDate { get; set; }


    }
    /// <summary>
    /// Subscriber details
    /// </summary>
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
        public string OldBSSPlanId { get; set; }
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

    public class ProfileMQ
    {
        public int accountID { get; set; }
        public int customerID { get; set; }
        public int? subscriberID { get; set; }
        public string mobilenumber { get; set; }
        public string MaskedCardNumber { get; set; }
        public string Token { get; set; }
        public string CardType { get; set; }
        public int? IsDefault { get; set; }
        public string CardHolderName { get; set; }
        public int? ExpiryMonth { get; set; }
        public int? ExpiryYear { get; set; }
        public string CardFundMethod { get; set; }
        public string CardBrand { get; set; }
        public string CardIssuer { get; set; }
        public string billingUnit { get; set; }
        public string billingFloor { get; set; }
        public string billingBuildingNumber { get; set; }
        public string billingBuildingName { get; set; }
        public string billingStreetName { get; set; }
        public string billingPostCode { get; set; }
        public string billingContactNumber { get; set; }
        public string email { get; set; }
        public string displayname { get; set; }
        public string paymentmode { get; set; }
        public double? amountpaid { get; set; }
        public string MPGSOrderID { get; set; }
        public string invoicelist { get; set; }
        public string invoiceamounts { get; set; }
    }

    public class RescheduleDeliveryMessage
    {
        public int ? accountID { get; set; }
        public int ? customerID { get; set; }
        public string SourceType { get; set; }
        public int ? orderID { get; set; }
        public string orderNumber { get; set; }
        public DateTime? orderDate { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string nationality { get; set; }
        public string shippingUnit { get; set; }
        public string shippingFloor { get; set; }
        public string shippingBuildingNumber { get; set; }
        public string shippingBuildingName { get; set; }
        public string shippingStreetName { get; set; }
        public string shippingPostCode { get; set; }
        public string shippingContactNumber { get; set; }
        public string alternateRecipientContact { get; set; }
        public string alternateRecipientName { get; set; }
        public string alternateRecipientEmail { get; set; }
        public string portalSlotID { get; set; }
        public DateTime? slotDate { get; set; }
        public TimeSpan? slotFromTime { get; set; }
        public TimeSpan? slotToTime { get; set; }
        public DateTime? scheduledDate { get; set; }
        public DateTime? submissionDate { get; set; }
        public int? serviceFee { get; set; }
        public double? amountPaid { get; set; }
        public string paymentMode { get; set; }
        public string MPGSOrderID { get; set; }
        public string MaskedCardNumber { get; set; }
        public string Token { get; set; }
        public string CardType { get; set; }
        public string CardHolderName { get; set; }
        public int? ExpiryMonth { get; set; }
        public int? ExpiryYear { get; set; }
        public string CardFundMethod { get; set; }
        public string CardBrand { get; set; }
        public string CardIssuer { get; set; }
        public DateTime? DateofBirth { get; set; }
        public string ReferralCode { get; set; }
        public DateTime? ProcessedOn { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceUrl { get; set; }
        public DateTime? CreatedOn { get; set; }
        public List<InvoiceCharges> invoiceCharges { get; set; }
    }

    public class InvoiceCharges
    {
        public int? AccountInvoiceID { get; set; }
        public int? AdminServiceID { get; set; }
        public string portalServiceName { get; set; }
        public double? serviceFee { get; set; }
        public int? isRecurring { get; set; }
        public int? isGSTIncluded { get; set; }
    }
}
