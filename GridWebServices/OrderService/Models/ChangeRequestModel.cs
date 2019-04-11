using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Models
{
    /// <summary>Change Phone Request</summary>
    public class ChangePhoneRequest
    {
        /// <summary>Gets or sets the customer identifier.</summary>
        /// <value>The customer identifier.</value>
        public int? CustomerId { get; set; }
        /// <summary>Gets or sets the mobile number.</summary>
        /// <value>The mobile number.</value>
        public string MobileNumber { get; set; }
        /// <summary>Creates new mobile number.</summary>
        /// <value>The new mobile number.</value>
        public string NewMobileNumber { get; set; }
        /// <summary>Gets or sets the type of the premium.</summary>
        /// <value>The type of the premium.</value>
        public int PremiumType { get; set; }
        /// <summary>Gets or sets the ported number transfer form.</summary>
        /// <value>The ported number transfer form.</value>
        public string PortedNumberTransferForm { get; set; }
        /// <summary>Gets or sets the ported number owned by.</summary>
        /// <value>The ported number owned by.</value>
        public string PortedNumberOwnedBy { get; set; }
        /// <summary>Gets or sets the ported number owner registration identifier.</summary>
        /// <value>The ported number owner registration identifier.</value>
        public string PortedNumberOwnerRegistrationId { get; set; }

    }

    /// <summary>Response class for Change Sim</summary>
    public class ChangeSimResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminationOrSuspensionResponse" /> class.
        /// </summary>
        public ChangeSimResponse()
        {
            ChangeRequestChargesList = new List<ChangeRequestCharges>();
        }
        /// <summary>
        /// Gets or sets the change request identifier.
        /// </summary>
        /// <value>
        /// The change request identifier.
        /// </value>
        public int ChangeRequestId { get; set; }
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
        /// Gets or sets the request type description.
        /// </summary>
        /// <value>
        /// The request type description.
        /// </value>
        public string RequestTypeDescription { get; set; }
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
        //public string IDType { get; set; }
        //public string IDNumber { get; set; }
        /// <summary>
        /// Gets or sets the is same as billing.
        /// </summary>
        /// <value>
        /// The is same as billing.
        /// </value>
        public int IsSameAsBilling { get; set; }
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
        /// Gets or sets the alternate recioient identifier number.
        /// </summary>
        /// <value>
        /// The alternate recioient identifier number.
        /// </value>
        public string AlternateRecioientIDNumber { get; set; }

        /// <summary>
        /// Gets or sets the type of the alternate recioient identifier.
        /// </summary>
        /// <value>
        /// The type of the alternate recioient identifier.
        /// </value>
        public string AlternateRecioientIDType { get; set; }
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
        public string DeliveryVendor { get; set; }
        /// <summary>
        /// Gets or sets the slot from time.
        /// </summary>
        /// <value>
        /// The slot from time.
        /// </value>
        public DateTime? DeliveryOn { get; set; }

        /// <summary>
        /// Gets or sets the vendor tracking code.
        /// </summary>
        /// <value>
        /// The vendor tracking code.
        /// </value>
        public string VendorTrackingCode { get; set; }

        /// <summary>
        /// Gets or sets the vendor tracking URL.
        /// </summary>
        /// <value>
        /// The vendor tracking URL.
        /// </value>
        public string VendorTrackingUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the slot to time.
        /// </summary>
        /// <value>
        /// The slot to time.
        /// </value>
        public DateTime? DeliveryTime { get; set; }
        /// <summary>
        /// Gets or sets the scheduled date.
        /// </summary>
        /// <value>
        /// The scheduled date.
        /// </value>
        public DateTime? ScheduledDate { get; set; }
        /// <summary>
        /// Gets or sets the service fee.
        /// </summary>
        /// <value>
        /// The service fee.
        /// </value>
        public double DeliveryFee { get; set; }

        /// <summary>
        /// The change request charges list
        /// </summary>
        public IList<ChangeRequestCharges> ChangeRequestChargesList;
    }


    /// <summary>
    /// Termination and Suspension Response
    /// </summary>
    public class TerminationOrSuspensionResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminationOrSuspensionResponse" /> class.
        /// </summary>
        public TerminationOrSuspensionResponse()
        {
            ChangeRequestChargesList = new List<ChangeRequestCharges>();
        }
        /// <summary>
        /// Gets or sets the change request identifier.
        /// </summary>
        /// <value>
        /// The change request identifier.
        /// </value>
        public int ChangeRequestId { get; set; }
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
        /// Gets or sets the request type description.
        /// </summary>
        /// <value>
        /// The request type description.
        /// </value>
        public string RequestTypeDescription { get; set; }

        /// <summary>
        /// The change request charges list
        /// </summary>
        public IList<ChangeRequestCharges> ChangeRequestChargesList;
    }

    /// <summary>
    /// 
    /// </summary>
    public class ChangeRequestCharges
    {
        /// <summary>
        /// Gets or sets the name of the portal service.
        /// </summary>
        /// <value>
        /// The name of the portal service.
        /// </value>
        public string PortalServiceName { get; set; }
        /// <summary>
        /// Gets or sets the service fee.
        /// </summary>
        /// <value>
        /// The service fee.
        /// </value>
        public double ServiceFee { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is recurring.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is recurring; otherwise, <c>false</c>.
        /// </value>
        public bool IsRecurring { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is GST included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is GST included; otherwise, <c>false</c>.
        /// </value>
        public bool IsGstIncluded { get; set; }

    }
}
