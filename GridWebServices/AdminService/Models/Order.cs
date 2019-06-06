using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminService.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public string ListingStatus { get; set; }
        public string OrderNumber { get; set; }
        public string OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public string IdentityCardType { get; set; }
        public string IdentityCardNumber { get; set; }
        public string BillingUnit { get; set; }
        public string BillingFloor { get; set; }
        public string BillingBuildingNumber { get; set; }
        public string BillingBuildingName { get; set; }
        public string BillingStreetName { get; set; }
        public string BillingPostCode { get; set; }
        public string BillingContactNumber { get; set; }
        public string ReferralCode { get; set; }
        public string PromotionCode { get; set; }
        public bool HaveDocuments { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string IDType { get; set; }
        public string IDNumber { get; set; }
        public int? IsSameAsBilling { get; set; }
        public string ShippingUnit { get; set; }
        public string ShippingFloor { get; set; }
        public string ShippingBuildingNumber { get; set; }
        public string ShippingBuildingName { get; set; }
        public string ShippingStreetName { get; set; }
        public string ShippingPostCode { get; set; }
        public string ShippingContactNumber { get; set; }
        public string AlternateRecipientContact { get; set; }
        public string AlternateRecipientName { get; set; }
        public string AlternateRecipientEmail { get; set; }
        public string PortalSlotID { get; set; }
        public DateTime? SlotDate { get; set; }
        public TimeSpan? SlotFromTime { get; set; }
        public TimeSpan? SlotToTime { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public double? ServiceFee { get; set; }
        public List<Bundle> Bundles { get; set; }
        public List<ServiceCharge> ServiceCharges { get; set; }

        public int? AllowDeliveryOffset { get; set; }
    }

    public class ServiceCharge
    {
        public int? OrderID { get; set; }
        public string PortalServiceName { get; set; }
        public double? ServiceFee { get; set; }
        public int? IsRecurring { get; set; }
        public int? IsGSTIncluded { get; set; }
    }

    public class Bundle
    {
        public int? OrderID { get; set; }
        public int? BundleID { get; set; }
        public string MobileNumber { get; set; }
        public string DisplayName { get; set; }
        public int IsPrimaryNumber { get; set; }
        public string PlanMarketingName { get; set; }
        public string PortalDescription { get; set; }
        public string PortalSummaryDescription { get; set; }
        public double? TotalData { get; set; }
        public double? TotalSMS { get; set; }
        public double? TotalVoice { get; set; }
        public double? ActualSubscriptionFee { get; set; }
        public double? ApplicableSubscriptionFee { get; set; }
        public string ServiceName { get; set; }
        public double? ActualServiceFee { get; set; }
        public double? ApplicableServiceFee { get; set; }
        public int PremiumType { get; set; }
        public int IsPorted { get; set; }
        public int IsOwnNumber { get; set; }
        public string DonorProvider { get; set; }
        public string PortedNumberTransferForm { get; set; }
        public string PortedNumberOwnedBy { get; set; }
        public string PortedNumberOwnerRegistrationID { get; set; }
    }

    public class ChangeRequest
    {
        public int ChangeRequestID { get; set; }
        public int SubscriberID { get; set; }
        public string MobileNumber { get; set; }
        public string ListingStatus { get; set; }
        public string OrderNumber { get; set; }
        public string RequestType { get; set; }
        public string OrderStatus { get; set; }
        public DateTime RequestOn { get; set; }
        public string BillingUnit { get; set; }
        public string BillingFloor { get; set; }
        public string BillingBuildingNumber { get; set; }
        public string BillingBuildingName { get; set; }
        public string BillingStreetName { get; set; }
        public string BillingPostCode { get; set; }
        public string BillingContactNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string IDType { get; set; }
        public string IDNumber { get; set; }
        public int? IsSameAsBilling { get; set; }
        public string ShippingUnit { get; set; }
        public string ShippingFloor { get; set; }
        public string ShippingBuildingNumber { get; set; }
        public string ShippingBuildingName { get; set; }
        public string ShippingStreetName { get; set; }
        public string ShippingPostCode { get; set; }
        public string ShippingContactNumber { get; set; }
        public string AlternateRecipientContact { get; set; }
        public string AlternateRecipientName { get; set; }
        public string AlternateRecipientEmail { get; set; }
        public string PortalSlotID { get; set; }
        public DateTime? SlotDate { get; set; }
        public TimeSpan? SlotFromTime { get; set; }
        public TimeSpan? SlotToTime { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public double? ServiceFee { get; set; }       
    }
}
