using System.ComponentModel;
using System.Runtime.Serialization;

namespace Core.Enums
{
    public enum ErrorLevel
    {
        [EnumMember(Value = "Critical")]
        [Description("Critical Error")]
        Critical = 1,

        [EnumMember(Value = "Medium")]
        [Description("Medium Error")]
        Medium = 2,

        [EnumMember(Value = "Low")]
        [Description("Low")]
        Low = 3,
    }


    public enum CommonErrors
    {
        [EnumMember(Value = "GetAssetFailed")]
        [Description("GetAsset request Failed")]
        GetAssetFailed = 1,

        [EnumMember(Value = "Create order failed")]
        [Description("Create order failed, order already exists")]
        CreateOrderFailed = 2,

        [EnumMember(Value = "Expired Token")]
        [Description("Token authentication failed. Expired Token")]
        ExpiredToken = 3,

        [EnumMember(Value = "UpdateAssetBlockingFailed")]
        [Description("Update asset failed for blocking number")]
        UpdateAssetBlockingFailed = 4,

        [EnumMember(Value = "UpdateAssetUnBlockingFailed")]
        [Description("Update asset failed for unblocking number")]
        UpdateAssetUnBlockingFailed = 5,

        [EnumMember(Value = "UpdateSubscriptionFailed")]
        [Description("Update Subscription Failed")]
        UpdateSubscriptionFailed = 6,

        [EnumMember(Value = "Failed To Locate Updated Subscription")]
        [Description("Failed ToLocate Updated Subscription")]
        FailedToLocateUpdatedSubscription = 7,

        [EnumMember(Value = "Failed To Get Order Customer")]
        [Description("Failed To Get Order Customer")]
        FailedToGetCustomer = 8,

        [EnumMember(Value = "Failed To Get Configuration")]
        [Description("Failed To Get Configuration")]
        FailedToGetConfiguration = 9,

        [EnumMember(Value = "S3 Upload Failed")]
        [Description("AWS S3 Upload Failed")]
        S3UploadFailed = 10,

        [EnumMember(Value = "CreateSubscriptionFailed")]
        [Description("Create Subscription Failed")]
        CreateSubscriptionFailed = 11,

        [EnumMember(Value = "Failed To Locate Created Subscription")]
        [Description("Failed ToLocate Created Subscription")]
        FailedToLocateCreatedSubscription = 12,

        [EnumMember(Value = "No Delivery Slot Exists")]
        [Description("No Delivery Slot Exists")]
        DeliverySlotNotExists = 13,

        [EnumMember(Value = "Update Personal Details Failed")]
        [Description("Personal Details Updation Failed")]
        FailedUpdatePersonalDetails = 14,

        [EnumMember(Value = "Update Billing Details Failed")]
        [Description("Billing Details Updation Failed")]
        FailedUpdateBillingDetails = 15,

        [EnumMember(Value = "Update Shipping Details Failed")]
        [Description("Shipping Details Updation Failed")]
        FailedUpdateShippingDetails = 16,

        [EnumMember(Value = "Update LOA Details Failed")]
        [Description("LOA Details Updation Failed")]
        FailedUpdateLOADetails = 17,

        [EnumMember(Value = "Referral Code Exists")]
        [Description("Referral Code Exists")]
        ReferralCodeExists = 18,

        [EnumMember(Value = "Referral Code Not Exists")]
        [Description("Referral Code Not Exists")]
        ReferralCodeNotExists = 19,

        [EnumMember(Value = "Failed To Updated Order Subscription Details")]
        [Description("Failed To Update Subscription")]
        FailedToUpdatedSubscriptionDetails = 20,

        [EnumMember(Value = "Checkout Session Created")]
        [Description("Checkout Session Created Successfully")]
        CheckoutSessionCreated = 21,

        [EnumMember(Value = "Line Delete Failed")]
        [Description("Failed to delete additional line")]
        LineDeleteFailed = 22,

        [EnumMember(Value = "Line Deleted Successfully")]
        [Description("Successfully deleted additional line")]
        LineDeleteSuccess = 23,

        [EnumMember(Value = "Assign New Number Faild")]
        [Description("Failed to assign New Number")]
        AssignNewNumberFailed = 24,

        [EnumMember(Value = "New Number Assign Success")]
        [Description("Successfully Assigned New Number")]
        AssignNuewNumberSuccess = 25,

        [EnumMember(Value = "MandatoryFieldMissing")]
        [Description("Mandatory field missing")]
        MandatoryFieldMissing = 26,

        [EnumMember(Value = "Order Rolled Back")]
        [Description("Order Rolled Back")]
        OrderRolledBack = 27,

        [EnumMember(Value = "Order Rolled Back Failed")]
        [Description("Order Rolled Back Failed")]
        OrderRolledBackFailed = 28,

        [EnumMember(Value = "Invalid email address")]
        [Description("Invalid email address")]
        InvalidEmail = 29,

        [EnumMember(Value = "TokenGenerationFailed")]
        [Description("TokenGenerationFailed")]
        TokenGenerationFailed = 30,

        [EnumMember(Value = "UnableToRetrievePasswordToken")]
        [Description("Password Token Generated. But unable to retrieve it")]
        UnableToRetrievePasswordToken = 31,

        [EnumMember(Value = "TokenEmpty")]
        [Description("Token header empty")]
        TokenEmpty = 32,

        [EnumMember(Value = "Token Not Exists")]
        [Description("Token Not Exists")]
        TokenNotExists = 33,

        [EnumMember(Value = "Password Reset Success")]
        [Description("Password Reset Success")]
        PasswordResetSuccess = 34,

        [EnumMember(Value = "Password Reset Failed")]
        [Description("Password Reset Failed")]
        PasswordResetFailed = 35,

        [EnumMember(Value = "FailedToTokenizeCustomerAccount")]
        [Description("Failed ToTokenize Customer Account")]
        FailedToTokenizeCustomerAccount = 36,

        [EnumMember(Value = "PayWithTokenFailed")]
        [Description("Pay With Token Failed")]
        PayWithTokenFailed = 37,

        [EnumMember(Value = "FailedToGetUsageHistory")]
        [Description("Failed To Get Usage History")]
        FailedToGetUsageHistory = 38,

        [EnumMember(Value = "UsageHistoryNotAvailable")]
        [Description("Usage History Not Available")]
        UsageHistoryNotAvailable = 39,

        [EnumMember(Value = "UnableToGetTokenSession")]
        [Description("Unable To Get Token Session from db")]
        UnableToGetTokenSession = 40,

        [EnumMember(Value = "CheckOutDetailsUpdationFailed")]
        [Description("Check Out Details Updation Failed")]
        CheckOutDetailsUpdationFailed = 41,

        [EnumMember(Value = "FailedToCreatePaymentMethod")]
        [Description("Failed To Create Payment Method")]
        FailedToCreatePaymentMethod = 42,

        [EnumMember(Value = "FailedToRemovePaymentMethod")]
        [Description("Failed To Remove Payment Method from gateway")]
        FailedToRemovePaymentMethod = 43,

        [EnumMember(Value = "FailedToRemovePaymentMethod")]
        [Description("Failed To Remove Payment Method from database")]
        FailedToRemovePaymentMethodDb = 44,

        [EnumMember(Value = "PaymentMethodSuccessfullyRemoved")]
        [Description("Payment Method Successfully Removed from gateway")]
        PaymentMethodSuccessfullyRemoved = 45,

        [EnumMember(Value = "PaymentMethodSuccessfullyRemoved")]
        [Description("Payment Method Successfully Removed from database")]
        PaymentMethodSuccessfullyRemoveddb = 46,

        [EnumMember(Value = "CaptureSuccess")]
        [Description("Capture Success")]
        CaptureSuccess = 47,

        [EnumMember(Value = "CaptureFailed")]
        [Description("Capture Failed")]
        CaptureFailed = 48,

        [EnumMember(Value = "AuthorizeFailed")]
        [Description("Payment Authorize Failed")]
        AuthorizeFailed = 49,

        [EnumMember(Value = "AuthorizeSuccess")]
        [Description("Payment Authorize Success")]
        AuthorizeSuccess = 50,

        [EnumMember(Value = "PaymentMethodNotExists")]
        [Description("Payment Method Not Exists")]
        PaymentMethodNotExists = 51,

        [EnumMember(Value = "PaymentMethodSuccessfullyChanged")]
        [Description("Payment Method Successfully changed")]
        PaymentMethodSuccessfullyChanged = 52,

        [EnumMember(Value = "SourceTypeNotFound")]
        [Description("Source Type Not Found")]
        SourceTypeNotFound = 53,

        [EnumMember(Value = "ProcessingQue")]
        [Description("Processing Queue Message")]
        ProcessingQue = 54,

        [EnumMember(Value = "ProcessingQueFailed")]
        [Description("Processing Queue Message Failed")]
        ProcessingQueFailed = 55,

        [EnumMember(Value = "FailedToGetBillingAccount")]
        [Description("Failed To Get Billing Account")]
        FailedToGetBillingAccount = 56,

        [EnumMember(Value = "NoInvoiceFound")]
        [Description("No Invoice Found for Customer")]
        NoInvoiceFound = 57,

        [EnumMember(Value = "FailedToRemoveLoa")]
        [Description("Failed To Remove Loa")]
        FailedToRemoveLoa = 58,

        [EnumMember(Value = "LOARemoved")]
        [Description("LOA removed successfully")]
        LOARemoved = 59,

        [EnumMember(Value = "TokenNotMatching")]
        [Description("Invalid Account access. Token not matching")]
        TokenNotMatching = 60,

        [EnumMember(Value = "PendingBuddyOrderProcessed")]
        [Description("Pending Buddy Order processed and order queue message published")]
        PendingBuddyOrderProcessed = 61,

        [EnumMember(Value = "PendingBuddyMQBodyFailed")]
        [Description("Pending buddy order processed. Failed to get message body")]
        PendingBuddyMQBodyFailed = 62,

        [EnumMember(Value = "AccountNotExists")]
        [Description("Account not exists")]
        AccountNotExists = 63,

        [EnumMember(Value = "CardAlreadyExists")]
        [Description("Card already exists")]
        CardAlreadyExists = 64,

        [EnumMember(Value = "DeliveryInfoNotExists")]
        [Description("Delivery info not exists")]
        DeliveryInfoNotExists = 65,

        [EnumMember(Value = "AlreadyProcessedOrder")]
        [Description("Already processed order")]
        AlreadyProcessedOrder = 66,

        [EnumMember(Value = "UnfishedOrderExists")]
        [Description("Order not created, proceed with unfinished existing order")]
        UnfishedOrderExists = 67,

        [EnumMember(Value = "BSSConnectionFailed")]
        [Description("BSSConnection failed")]
        BSSConnectionFailed = 68,

        [EnumMember(Value = "PaymentProcessed")]
        [Description("Payment successfully processed")]
        PaymentProcessed = 69,

        [EnumMember(Value = "FailedToUpdateAccessibility")]
        [Description("Failed to update accessibility")]
        FailedToUpdateAccessibility = 70,

        [EnumMember(Value = "AccountActivated")]
        [Description("Account activated")]
        AccountActivated = 71,

        [EnumMember(Value = "AccountDeactivated")]
        [Description("Account aeactivated")]
        AccountDeactivated = 72,

        [EnumMember(Value = "InvalidStatus")]
        [Description("Invalid account status")]
        InvalidStatus = 73,

        [EnumMember(Value = "FailedToRemoveRescheduleLoa")]
        [Description("Failed to remove reschedule LOA")]
        FailedToRemoveRescheduleLoa = 74,

        [EnumMember(Value = "OldNumberNotExists")]
        [Description("Old number does not exists")]
        OldNumberNotExists = 75,

        [EnumMember(Value = "FailedToGetInvoice")]
        [Description("Failed to get invoice from service provider")]
        FailedToGetInvoice = 76,

        [EnumMember(Value = "BillingAccountNumberEmpty")]
        [Description("BillingAccountNumber field empty")]
        BillingAccountNumberEmpty = 77,

        [EnumMember(Value = "RewardServerError")]
        [Description("Unable to get rewards details. Server error")]
        RewardServerError = 78,

        [EnumMember(Value = "UserNotExists")]
        [Description("User not exists")]
        UserNotExists = 79,

        [EnumMember(Value = "SuccessfullyLoggedOut")]
        [Description("Logged out successfully")]
        SuccessfullyLoggedOut = 80,

        [EnumMember(Value = "BuddyProcessed")]
        [Description("Buddy processed")]
        BuddyProcessed = 81,

        [EnumMember(Value = "BuddyProcessingFailed")]
        [Description("Buddy processing failed. Check on pending buddy tables")]
        BuddyProcessingFailed = 82,

        [EnumMember(Value = "MQSent")]
        [Description("MQ Sent")]
        MQSent = 83,

        [EnumMember(Value = "InvalidCheckoutType")]
        [Description("Invalid Checkout Type picked up after successful payment")]
        InvalidCheckoutType = 84,

        [EnumMember(Value = "SystemExceptionAfterPayment")]
        [Description("System exception eccured after successful payment")]
        SystemExceptionAfterPayment = 85,

        [EnumMember(Value = "UnableToProcess")]
        [Description("Unable to process the request at present")]
        UnableToProcess = 86,

        [EnumMember(Value = "UnableToProcess")]
        [Description("Image is already submitted")]
        ImageAlreadyUploaded = 87,


        [EnumMember(Value = "GridBillingAPIConnectionFailed")]
        [Description("GridBillingAPI Connection failed")]
        GridBillingAPIConnectionFailed = 88,

        [EnumMember(Value = "BillsFound")]
        [Description("Billing records exist")]
        BillsFound = 89,

        [EnumMember(Value = "NoBillsFound")]
        [Description("Currently no bills generated")]
        NoBillsFound = 90,

        [EnumMember(Value = "AmountNotMatching")]
        [Description("Bill amount not matching")]
        AmountNotMatching = 91,

        [EnumMember(Value = "FailedToMatchOutstanding")]
        [Description("Failed to compare outstanding amount, try again later")]
        FailedToMatchOutstanding = 92,

        [EnumMember(Value = "ZeroAmount")]
        [Description("Outstanding amount should be greater than 0")]
        ZeroAmount = 93,

    }
}
