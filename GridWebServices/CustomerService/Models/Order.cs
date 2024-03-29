﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerService.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public string ListingStatus { get; set; }
        public int AllowRescheduling { get; set; }
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
        public string InvoiceNumber { get; set; }
        public int? EventSalesRepresentativeID { get; set; }
        public string MaskedCardNumber { get; set; }
        public string CardBrand { get; set; }
        public int? ExpiryMonth { get; set; }
        public int? ExpiryYear { get; set; }
        public DateTime? PaymentOn { get; set; }
        public List<Subscribers> Subscribers { get; set; }
        public List<ServiceCharge> ServiceCharges { get; set; }
        public List<OrderStatuses> OrderStatuses { get; set; }

        public string RecieptNumber { get; set; }
    }
    public class OrderStatuses
    {
        public int? OrderID { get; set; }
        public string OrderStatus { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class Subscribers
    {
        public int? OrderID { get; set; }
        public int? OrderSubscriberID { get; set; }
        public int? SubscriberID { get; set; }
        public string MobileNumber { get; set; }
        public string DisplayName { get; set; }
        public int? IsPrimary { get; set; }
        public DateTime? ActivateOn { get; set; }
        public string PremiumName { get; set; }
        public int? PremiumType { get; set; }
        public int? IsBuddyLine { get; set; }
        public int? IsPorted { get; set; }
        public double? DepositFee { get; set; }
        public List<Bundle> Bundles { get; set; }
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
        public int? OrderSubscriberID { get; set; }
        public int? BundleID { get; set; }
        public string BundleName { get; set; }
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
    }
}
