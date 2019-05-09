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
        FailedToLocateCreatedSubscription =12,

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

        [EnumMember(Value = "Mandatory Record Empty")]
        [Description("Mandatory RecordEmpty")]
        MandatoryRecordEmpty = 26,

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
        [Description("Token header emapy")]
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

        [EnumMember(Value = "AccountNotExists")]
        [Description("Account Not Exists")]
        AccountNotExists = 55,

        [EnumMember(Value = "FailedToGetBillingAccount")]
        [Description("Failed To Get Billing Account")]
        FailedToGetBillingAccount = 56,

        [EnumMember(Value = "NoInvoiceFound")]
        [Description("No Invoice Found for Customer")]
        NoInvoiceFound = 57,
    }
}
