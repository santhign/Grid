﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Models
{ 
    public class Order
    {
        public int OrderID { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string BillingUnit { get; set; }
        public string BillingFloor { get; set; }
        public string BillingBuildingNumber { get; set; }
        public string BillingBuildingName { get; set; }
        public string BillingStreetName { get; set; }
        public string BillingPostCode { get; set; }
        public string BillingContactNumber { get; set; }
        public string ReferralCode { get; set; }
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
    }

    public class Bundle
    {
        public int? BundleID { get; set; }
        public string MobileNumber { get; set; }
        public string DisplayName { get; set; }
        public int IsPrimaryNumber { get; set; }
        public string PlanMarketingName { get; set; }
        public string PortalDescription { get; set; }
        public string TotalData { get; set; }
        public double? TotalSMS { get; set; }
        public double? TotalVoice { get; set; }
        public double? ApplicableSubscriptionFee { get; set; }
        public string ServiceName { get; set; }
        public double? ApplicableServiceFee { get; set; }
        public int PremiumType { get; set; }
        public int IsPorted { get; set; }
        public int IsOwnNumber { get; set; }
        public string DonorProvider { get; set; }
        public string PortedNumberTransferForm { get; set; }
        public string PortedNumberOwnedBy { get; set; }
        public string PortedNumberOwnerRegistrationID { get; set; }
    }

    public class OrderCustomer
    {
        public int CustomerId { get; set; }
    }

    public class DeliverySlot
    {
        public string PortalSlotID { get; set; }
        public DateTime SlotDate { get; set; }
        public TimeSpan SlotFromTime { get; set; }
        public TimeSpan SlotToTime { get; set; }
        public string Slot { get; set; }
        public double AdditionalCharge { get; set; }
    }

}
