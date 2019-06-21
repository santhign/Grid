using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Models
{
    [DataContract]
    public class OrderQM
    {
        [DataMember(Name = "accountID")]
        public int accountID { get; set; }

        [DataMember(Name = "customerID")]
        public int customerID { get; set; }

        [DataMember(Name = "orderID")]
        public int orderID { get; set; }

        [DataMember(Name = "orderNumber")]
        public string orderNumber { get; set; }

        [DataMember(Name = "orderDate")]
        public DateTime orderDate { get; set; }

        [DataMember(Name = "billingUnit")]
        public string billingUnit { get; set; }

        [DataMember(Name = "billingFloor")]
        public string billingFloor { get; set; }

        [DataMember(Name = "billingBuildingNumber")]
        public string billingBuildingNumber { get; set; }

        [DataMember(Name = "billingBuildingName")]
        public string billingBuildingName { get; set; }

        [DataMember(Name = "billingStreetName")]
        public string billingStreetName { get; set; }

        [DataMember(Name = "billingPostCode")]
        public string billingPostCode { get; set; }

        [DataMember(Name = "billingContactNumber")]
        public string billingContactNumber { get; set; }

        [DataMember(Name = "OrderReferralCode")]
        public string orderReferralCode { get; set; }

        [DataMember(Name = "title")]
        public string title { get; set; }

        [DataMember(Name = "name")]
        public string name { get; set; }

        [DataMember(Name = "email")]
        public string email { get; set; }

        [DataMember(Name = "nationality")]
        public string nationality { get; set; }

        [DataMember(Name = "idType")]
        public string idType { get; set; }

        [DataMember(Name = "idNumber")]
        public string idNumber { get; set; }

        [DataMember(Name = "isSameAsBilling")]
        public int? isSameAsBilling { get; set; }

        [DataMember(Name = "shippingUnit")]
        public string shippingUnit { get; set; }

        [DataMember(Name = "shippingFloor")]
        public string shippingFloor { get; set; }

        [DataMember(Name = "shippingBuildingNumber")]
        public string shippingBuildingNumber { get; set; }

        [DataMember(Name = "shippingBuildingName")]
        public string shippingBuildingName { get; set; }

        [DataMember(Name = "shippingStreetName")]
        public string shippingStreetName { get; set; }

        [DataMember(Name = "shippingPostCode")]
        public string shippingPostCode { get; set; }

        [DataMember(Name = "shippingContactNumber")]
        public string shippingContactNumber { get; set; }

        [DataMember(Name = "alternateRecipientContact")]
        public string alternateRecipientContact { get; set; }

        [DataMember(Name = "alternateRecipientName")]
        public string alternateRecipientName { get; set; }

        [DataMember(Name = "alternateRecipientEmail")]
        public string alternateRecipientEmail { get; set; }

        [DataMember(Name = "portalSlotID")]
        public string portalSlotID { get; set; }

        [DataMember(Name = "slotDate")]
        public DateTime? slotDate { get; set; }

        [DataMember(Name = "slotFromTime")]
        public TimeSpan? slotFromTime { get; set; }

        [DataMember(Name = "slotToTime")]
        public TimeSpan? slotToTime { get; set; }

        [DataMember(Name = "scheduledDate")]
        public DateTime? scheduledDate { get; set; }

        [DataMember(Name = "submissionDate")]
        public DateTime submissionDate { get; set; }

        [DataMember(Name = "serviceFee")]
        public double? serviceFee { get; set; }

        [DataMember(Name = "amountPaid")]
        public double? amountPaid { get; set; }

        [DataMember(Name = "paymentMode")]
        public string paymentMode { get; set; }

        [DataMember(Name = "MPGSOrderID")]
        public string MPGSOrderID { get; set; }

        [DataMember(Name = "MaskedCardNumber")]
        public string MaskedCardNumber { get; set; }

        [DataMember(Name = "Token")]
        public string Token { get; set; }

        [DataMember(Name = "CardType")]
        public string CardType { get; set; }

        [DataMember(Name = "CardHolderName")]
        public string CardHolderName { get; set; }

        [DataMember(Name = "ExpiryMonth")]
        public int? ExpiryMonth { get; set; }

        [DataMember(Name = "ExpiryYear")]
        public int? ExpiryYear { get; set; }

        [DataMember(Name = "CardFundMethod")]
        public string CardFundMethod { get; set; }

        [DataMember(Name = "CardBrand")]
        public string CardBrand { get; set; }

        [DataMember(Name = "CardIssuer")]
        public string CardIssuer { get; set; }

        [DataMember(Name = "DateofBirth")]
        public DateTime? DateofBirth { get; set; }

        [DataMember(Name = "ReferralCode")]
        public string ReferralCode { get; set; }

        [DataMember(Name = "ProcessedOn")]
        public DateTime? ProcessedOn { get; set; }

        [DataMember(Name = "InvoiceNumber")]
        public string InvoiceNumber { get; set; }

        [DataMember(Name = "InvoiceUrl")]
        public string InvoiceUrl { get; set; }

        [DataMember(Name = "CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [DataMember(Name = "Subscribers")]
        public List<OrderSubscriber> Subscribers { get; set; }        

        [DataMember(Name = "Charges")]
        public List<OrderServiceCharge> Charges { get; set; }        
    }

    public class OrderSubscriber
    {
        [DataMember(Name = "OrderID")]
        public int? OrderID { get; set; }

        [DataMember(Name = "subscriberID")]
        public int? subscriberID { get; set; }

        [DataMember(Name = "mobileNumber")]
        public string mobileNumber { get; set; }

        [DataMember(Name = "displayName")]
        public string displayName { get; set; }

        [DataMember(Name = "CreatedOn")]
        public int? isPrimaryNumber { get; set; }

        [DataMember(Name = "premiumType")]
        public int? premiumType { get; set; }

        [DataMember(Name = "isPorted")]
        public int? isPorted { get; set; }

        [DataMember(Name = "isOwnNumber")]
        public int? isOwnNumber { get; set; }

        [DataMember(Name = "donorProvider")]
        public string donorProvider { get; set; }

        [DataMember(Name = "DepositFee")]
        public double? DepositFee { get; set; }

        [DataMember(Name = "IsBuddyLine")]
        public int? IsBuddyLine { get; set; }

        [DataMember(Name = "LinkedSubscriberID")]
        public int? LinkedSubscriberID { get; set; }

        [DataMember(Name = "RefOrderSubscriberID")]
        public int? RefOrderSubscriberID { get; set; }

        [DataMember(Name = "portedNumberTransferForm")]
        public string portedNumberTransferForm { get; set; }

        [DataMember(Name = "portedNumberOwnedBy")]
        public string portedNumberOwnedBy { get; set; }

        [DataMember(Name = "portedNumberOwnerRegistrationID")]
        public string portedNumberOwnerRegistrationID { get; set; }

        [DataMember(Name = "Bundles")]
        public List<OrderSubscriptionQM> Bundles { get; set; } 
       
    }


    public class OrderSubscriptionQM
    {
        [DataMember(Name = "SubscriberID")]
        public int SubscriberID { get; set; }

        [DataMember(Name = "bundleID")]
        public int? bundleID { get; set; }

        [DataMember(Name = "bssPlanCode")]
        public string bssPlanCode { get; set; }

        [DataMember(Name = "bssPlanName")]
        public string bssPlanName { get; set; }

        [DataMember(Name = "planType")]
        public int? planType { get; set; }

        [DataMember(Name = "planMarketingName")]
        public string planMarketingName { get; set; }

        [DataMember(Name = "portalDescription")]
        public string portalDescription { get; set; }

        [DataMember(Name = "totalData")]
        public double? totalData { get; set; }

        [DataMember(Name = "totalSMS")]
        public double? totalSMS { get; set; }

        [DataMember(Name = "totalVoice")]
        public double? totalVoice { get; set; }

        [DataMember(Name = "applicableSubscriptionFee")]
        public double? applicableSubscriptionFee { get; set; }  
    }

    public class OrderServiceCharge
    {
        [DataMember(Name = "OrderID")]
        public int? OrderID { get; set; }

        [DataMember(Name = "SubscriberID")]
        public int? SubscriberID { get; set; }

        [DataMember(Name = "AdminServiceID")]
        public int? AdminServiceID { get; set; }

        [DataMember(Name = "portalServiceName")]
        public string portalServiceName { get; set; }

        [DataMember(Name = "serviceFee")]
        public double? serviceFee { get; set; }

        [DataMember(Name = "isRecurring")]
        public int? isRecurring { get; set; }

        [DataMember(Name = "isGSTIncluded")]
        public int? isGSTIncluded { get; set; }       
    }


    public class MessageQueueRequest
    {
        public string Source { get; set; }
        public string SNSTopic { get; set; }
        public string MessageAttribute { get; set; }
        public string MessageBody { get; set; }
        public int Status { get; set; }
        public DateTime? PublishedOn { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int NumberOfRetries { get; set; }
        public DateTime? LastTriedOn { get; set; }


    }

    public class MessageQueueRequestException : MessageQueueRequest
    {
        public string Remark { get; set; }
        public string Exception { get; set; }
    }
}
