using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Enums
{
    public static class APISources
    {
        public const string Customer_UpdateProfile = "UpdateProfile";
        public const string Customer_ReferralCodeUpdate = "ReferralCodeUpdate";
        public const string Orders_UpdateSubscription = "Orders_UpdateSubscription";
        public const string Dashboard_CustomerOrders = "Dashboard_CustomerOrders";
        public const string DashBoard_BuyVas = "DashBoard_BuyVas";
        public const string DashBoard_RemoveVas = "DashBoard_RemoveVas";
        public const string Orders_CreateOrder = "Orders_CreateOrder";
        public const string Customer_VASPurchaseScreen = "Customer_VASPurchaseScreen";
        public const string Customer_SharedVASPurchaseScreen = "Customer_SharedVASPurchaseScreen";
        public const string Orders_UpdateSubscriberNumber = "Orders_UpdateSubscriberNumber";
        public const string Orders_number_portin = "Orders_number_portin";
        public const string Orders_order_get = "Orders_order_get";
        public const string Orders_personaldetails_update = "Orders_personaldetails_update";
        public const string Orders_billingaddress_update = "Orders_billingaddress_update";
        public const string Orders_shippingaddress_update = "Orders_shippingaddress_update";
        public const string Orders_deliveryslot_get = "Orders_deliveryslot_get";
        public const string Orders_deliveryslot_update = "Orders_deliveryslot_update";
        public const string Orders_loa_update = "Orders_loa_update";
        public const string Orders_referralcode_update = "Orders_referralcode_update";
        public const string Orders_validate_order_referral = "Orders_validate_order_referral";
        public const string Orders_numbers_get = "Orders_numbers_get";
        public const string Orders_get_usage_history = "Orders_get_usage_history";
        public const string Orders_subscriber_create = "Orders_subscriber_create";
        public const string Orders_subscriber_remove = "Orders_subscriber_remove";
        public const string Orders_subscriber_update = "Orders_subscriber_update";
        public const string Orders_subscriber_get = "Orders_subscriber_get";
        public const string Orders_subscribers_get = "Orders_subscribers_get";
        public const string Orders_document_upload = "Orders_document_upload";
        public const string Orders_postalcode_validate = "Orders_postalcode_validate";
        public const string Orders_order_pending_get = "Orders_order_pending_get";
        public const string Orders_payments_pending = "Orders_payments_pending";
        public const string Orders_cr_sharedvas_add = "Orders_cr_sharedvas_add";
        public const string Orders_cr_sharedvas_remove = "Orders_cr_sharedvas_remove";
        public const string Orders_cr_vas_add = "Orders_cr_vas_add";
        public const string Orders_cr_vas_remove = "Orders_cr_vas_remove";
        public const string Orders_cr_sim_replace_request = "Orders_cr_sim_replace_request";
        public const string Orders_order_reschedule_request = "Orders_order_reschedule_request";
        public const string Orders_cr_reschedule_request = "Orders_cr_reschedule_request";
        public const string Orders_terminateline_subscriber = "Orders_terminateline_subscriber";
        public const string Orders_suspendline_Subscriber = "Orders_suspendline_Subscriber";
        public const string Orders_placedorder_cancel = "Orders_placedorder_cancel";
        public const string Orders_get_checkout_details = "Orders_get_checkout_details";
        public const string Orders_update_checkout_response = "Orders_update_checkout_response";
        public const string Orders_Assign_newNumber = "Orders_Assign_newNumber";
        public const string Orders_process_webhook = "Orders_process_webhook";
        public const string Orders_list_webhook_notifications = "Orders_list_webhook_notifications";
        public const string Orders_process_webhook_mq = "Orders_process_webhook_mq";
        public const string orders_webhook_aws_mq = "orders_webhook_aws_mq";
        public const string Orders_placedorders_get = "Orders_placedorders_get";
        public const string Orders_CR_ChangePhoneNumber = "Orders_CR_ChangePhoneNumber";
        public const string Orders_unsuspendline_Subscriber = "Orders_unsuspendline_Subscriber";
        public const string Orders_CR_ChangeBasePlan = "Orders_CR_ChangeBasePlan";
    }
}
