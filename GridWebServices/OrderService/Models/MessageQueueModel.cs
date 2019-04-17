using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Models
{   

    /// <summary>
    /// MEssage Body for Change Request class model
    /// </summary>
    public class MessageBodyForCR
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBodyForCR"/> class.
        /// </summary>
        public MessageBodyForCR()
        {
            subscriberDetails = new SubscriberDetails();
        }
        /// <summary>
        /// Gets or sets the account identifier.
        /// </summary>
        /// <value>
        /// The account identifier.
        /// </value>
        public int AccountID { get; set; }
        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        public int CustomerID { get; set; }
        /// <summary>
        /// Gets or sets the change request identifier.
        /// </summary>
        /// <value>
        /// The change request identifier.
        /// </value>
        public int ChangeRequestID { get; set; }
        /// <summary>
        /// Gets or sets the order number.
        /// </summary>
        /// <value>
        /// The order number.
        /// </value>
        public string OrderNumber { get; set; }
        /// <summary>
        /// Gets or sets the request on.
        /// </summary>
        /// <value>
        /// The request on.
        /// </value>
        public DateTime RequestOn { get; set; }
        /// <summary>
        /// Gets or sets the billing unit.
        /// </summary>
        /// <value>
        /// The billing unit.
        /// </value>
        public string BillingUnit { get; set; }
        /// <summary>
        /// Gets or sets the billing floor.
        /// </summary>
        /// <value>
        /// The billing floor.
        /// </value>
        public string BillingFloor { get; set; }
        /// <summary>
        /// Gets or sets the billing building number.
        /// </summary>
        /// <value>
        /// The billing building number.
        /// </value>
        public string BillingBuildingNumber { get; set; }
        /// <summary>
        /// Gets or sets the name of the billing building.
        /// </summary>
        /// <value>
        /// The name of the billing building.
        /// </value>
        public string BillingBuildingName { get; set; }
        /// <summary>
        /// Gets or sets the name of the billing street.
        /// </summary>
        /// <value>
        /// The name of the billing street.
        /// </value>
        public string BillingStreetName { get; set; }
        /// <summary>
        /// Gets or sets the billing post code.
        /// </summary>
        /// <value>
        /// The billing post code.
        /// </value>
        public string BillingPostCode { get; set; }
        /// <summary>
        /// Gets or sets the billing contact number.
        /// </summary>
        /// <value>
        /// The billing contact number.
        /// </value>
        public string BillingContactNumber { get; set; }
        /// <summary>
        /// Gets or sets the referral code.
        /// </summary>
        /// <value>
        /// The referral code.
        /// </value>
        public string ReferralCode { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email { get; set; }
        /// <summary>
        /// Gets or sets the nationality.
        /// </summary>
        /// <value>
        /// The nationality.
        /// </value>
        public string Nationality { get; set; }
        /// <summary>
        /// Gets or sets the type of the identity card.
        /// </summary>
        /// <value>
        /// The type of the identity card.
        /// </value>
        public string IdentityCardType { get; set; }
        /// <summary>
        /// Gets or sets the identity card number.
        /// </summary>
        /// <value>
        /// The identity card number.
        /// </value>
        public string IdentityCardNumber { get; set; }
        /// <summary>
        /// Gets or sets the is same as billing.
        /// </summary>
        /// <value>
        /// The is same as billing.
        /// </value>
        public string IsSameAsBilling { get; set; }
        /// <summary>
        /// Gets or sets the shipping unit.
        /// </summary>
        /// <value>
        /// The shipping unit.
        /// </value>
        public string ShippingUnit { get; set; }
        /// <summary>
        /// Gets or sets the shipping floor.
        /// </summary>
        /// <value>
        /// The shipping floor.
        /// </value>
        public string ShippingFloor { get; set; }
        /// <summary>
        /// Gets or sets the shipping building number.
        /// </summary>
        /// <value>
        /// The shipping building number.
        /// </value>
        public string ShippingBuildingNumber { get; set; }
        /// <summary>
        /// Gets or sets the name of the shipping building.
        /// </summary>
        /// <value>
        /// The name of the shipping building.
        /// </value>
        public string ShippingBuildingName { get; set; }
        /// <summary>
        /// Gets or sets the name of the shipping street.
        /// </summary>
        /// <value>
        /// The name of the shipping street.
        /// </value>
        public string ShippingStreetName { get; set; }
        /// <summary>
        /// Gets or sets the shipping post code.
        /// </summary>
        /// <value>
        /// The shipping post code.
        /// </value>
        public string ShippingPostCode { get; set; }
        /// <summary>
        /// Gets or sets the shipping contact number.
        /// </summary>
        /// <value>
        /// The shipping contact number.
        /// </value>
        public string ShippingContactNumber { get; set; }
        /// <summary>
        /// Gets or sets the alternate recipient contact.
        /// </summary>
        /// <value>
        /// The alternate recipient contact.
        /// </value>
        public string AlternateRecipientContact { get; set; }
        /// <summary>
        /// Gets or sets the name of the alternate recipient.
        /// </summary>
        /// <value>
        /// The name of the alternate recipient.
        /// </value>
        public string AlternateRecipientName { get; set; }
        /// <summary>
        /// Gets or sets the alternate recipient email.
        /// </summary>
        /// <value>
        /// The alternate recipient email.
        /// </value>
        public string AlternateRecipientEmail { get; set; }
        /// <summary>
        /// Gets or sets the portal slot identifier.
        /// </summary>
        /// <value>
        /// The portal slot identifier.
        /// </value>
        public string PortalSlotID { get; set; }
        /// <summary>
        /// Gets or sets the slot date.
        /// </summary>
        /// <value>
        /// The slot date.
        /// </value>
        public DateTime SlotDate { get; set; }
        /// <summary>
        /// Gets or sets the slot from time.
        /// </summary>
        /// <value>
        /// The slot from time.
        /// </value>
        public DateTime SlotFromTime { get; set; }
        /// <summary>
        /// Gets or sets the slot to time.
        /// </summary>
        /// <value>
        /// The slot to time.
        /// </value>
        public DateTime SlotToTime { get; set; }
        /// <summary>
        /// Gets or sets the scheduled date.
        /// </summary>
        /// <value>
        /// The scheduled date.
        /// </value>
        public DateTime ScheduledDate { get; set; }
        /// <summary>
        /// Gets or sets the service fee.
        /// </summary>
        /// <value>
        /// The service fee.
        /// </value>
        public double ServiceFee { get; set; }


        /// <summary>
        /// The subscriber details
        /// </summary>
        public SubscriberDetails subscriberDetails;
    }

    /// <summary>
    /// Subscriber details
    /// </summary>
    public class SubscriberDetails
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriberDetails"/> class.
        /// </summary>
        public SubscriberDetails()
        {
            bundleDetails = new List<BundleDetails>();
        }
        /// <summary>
        /// Gets or sets the subscriber identifier.
        /// </summary>
        /// <value>
        /// The subscriber identifier.
        /// </value>
        public int SubscriberID { get; set; }
        /// <summary>
        /// Gets or sets the mobile number.
        /// </summary>
        /// <value>
        /// The mobile number.
        /// </value>
        public string MobileNumber { get; set; }
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }
        /// <summary>
        /// Gets or sets the is primary.
        /// </summary>
        /// <value>
        /// The is primary.
        /// </value>
        public int IsPrimary { get; set; }
        /// <summary>
        /// Gets or sets the type of the premium.
        /// </summary>
        /// <value>
        /// The type of the premium.
        /// </value>
        public int PremiumType { get; set; }
        /// <summary>
        /// Gets or sets the is ported.
        /// </summary>
        /// <value>
        /// The is ported.
        /// </value>
        public int IsPorted { get; set; }
        /// <summary>
        /// Gets or sets the name of the donor provider.
        /// </summary>
        /// <value>
        /// The name of the donor provider.
        /// </value>
        public string DonorProviderName { get; set; }

        /// <summary>
        /// The bundle details
        /// </summary>
        public IList<BundleDetails> bundleDetails;
    }

    /// <summary>
    /// Bundle details
    /// </summary>
    public class BundleDetails
    {
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
        /// Gets or sets the name of the BSS plan.
        /// </summary>
        /// <value>
        /// The name of the BSS plan.
        /// </value>
        public string BSSPlanName { get; set; }
        /// <summary>
        /// Gets or sets the type of the plan.
        /// </summary>
        /// <value>
        /// The type of the plan.
        /// </value>
        public int PlanType { get; set; }
        /// <summary>
        /// Gets or sets the name of the plan marketing.
        /// </summary>
        /// <value>
        /// The name of the plan marketing.
        /// </value>
        public string PlanMarketingName { get; set; }
        /// <summary>
        /// Gets or sets the portal description.
        /// </summary>
        /// <value>
        /// The portal description.
        /// </value>
        public string PortalDescription { get; set; }

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
        public object MessageBody { get; set; }
        public int Status { get; set; }
        public DateTime PublishedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public int NumberOfRetries { get; set; }
        public DateTime LastTriedOn { get; set; }


    }
}
