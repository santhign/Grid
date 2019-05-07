using System.ComponentModel;
using System.Runtime.Serialization;

namespace Core.Enums
{
    public enum DbReturnValue
    {
        [EnumMember(Value = "Create Success")]
        [Description("Record created successfully")]
        CreateSuccess = 100,

        [EnumMember(Value = "Update Success")]
        [Description("Record updated successfully")]
        UpdateSuccess = 101,

        [EnumMember(Value = "Not Exists")]
        [Description("Record does not exists")]
        NotExists = 102,

        [EnumMember(Value = "Delete Success")]
        [Description("Record deleted successfully")]
        DeleteSuccess = 103,

        [EnumMember(Value = "Active Try Delete")]
        [Description("Active record can not be deleted")]
        ActiveTryDelete = 104,

        [EnumMember(Value = "Record Exists")]
        [Description("Record exists in database")]
        RecordExists = 105,

        [EnumMember(Value = "Updation Failed")]
        [Description("Record updation failed")]
        UpdationFailed = 106,

        [EnumMember(Value = "Creation Failed")]
        [Description("Record creation failed")]
        CreationFailed = 107,

        [EnumMember(Value = "Email Exists")]
        [Description("Email Already Exists")]
        EmailExists = 108,

        [EnumMember(Value = "Email Not Exists")]
        [Description("Email Does not exists")]
        EmailNotExists = 109,

        [EnumMember(Value = "Password Incorrect")]
        [Description("Password Does not match")]
        PasswordIncorrect = 110,

        [EnumMember(Value = "Authentication Success")]
        [Description("Customer Authentication Success")]
        AuthSuccess = 111,

        [EnumMember(Value = "Reason Unknown")]
        [Description("Operation Failed Due to Unknown Reason")]
        ReasonUnknown = 112,

        [EnumMember(Value = "Token Auth Failed")]
        [Description("Token Auth Failed")]
        TokenAuthFailed = 113,

        [EnumMember(Value = "Token Expired")]
        [Description("Authorization Failed. Given Token is Expired")]
        TokenExpired = 114,

        [EnumMember(Value = "GetAssets Failed")]
        [Description("Failed to get Assets from BSS")]
        GetAssetsFailed = 115,

        [EnumMember(Value = "Blocking Failed")]
        [Description("BSSAPI Asset Update Number Blocking Failed")]
        BlockingFailed = 117,

        [EnumMember(Value = "Un Blocking Failed")]
        [Description("BSSAPI Asset Update Number Un Blocking Failed")]
        UnBlockingFailed = 118,

        [EnumMember(Value = "Record Doesn't Exists")]
        [Description("No Records found in database")]
        NoRecords = 119,

        [EnumMember(Value = "Transaction Success")]
        [Description("Transaction tables Updated Successfully")]
        TransactionSuccess = 120,

        [EnumMember(Value = "Transaction Failed")]
        [Description("Transaction tables updation Failed")]
        TransactionFailed = 121,

        [EnumMember(Value = "Primary Try Delete")]
        [Description("Primary Number can not be deleted")]
        PrimaryTryDelete = 122,

        [EnumMember(Value = "Order completed Try Delete")]
        [Description("Order completed, Number can not be deleted")]
        CompletedOrderDelete = 123,

        [EnumMember(Value = "Reset Password Token Valid")]
        [Description("Reset Password Token Valid")]
        ResetPasswordTokenValid = 124,

        [EnumMember(Value = "Reset Password Token Expired")]
        [Description("Reset Password Token Expired")]
        ResetPasswordTokenExpired = 125,

        [EnumMember(Value = "Already processed payment")]
        [Description("Already processed payment")]
        PaymentAlreadyProcessed = 126,

        [EnumMember(Value = "Update on active record is not allowed")]
        [Description("Update on active record is not allowed")]
        UpdateNotAllowed = 127,

        [EnumMember(Value = "Duplicate record Exist")]
        [Description("Same request is already raised")]
        DuplicateCRExists = 128,

        [EnumMember(Value = "Unsuspension Validation")]
        [Description("First raise suspension request")]
        UnSuspensionValidation = 129,

        [EnumMember(Value = "Delivery slot unavailablity")]
        [Description("Delivery slot unavailablity, Please select a new slot")]
        DeliverySlotUnavailability = 130,

        [EnumMember(Value = "Same Bundle is present")]
        [Description("Plan is not changed as you are raising request against same bundle")]
        SameBundleValidation = 131,

        [EnumMember(Value = "Order delivery information is missing")]
        [Description("Order delivery information is missing")]
        OrderDeliveryInformationMissing = 132,

        [EnumMember(Value = "Order Identity documents are missing")]
        [Description("Order Identity documents are missing")]
        OrderIDDocumentsMissing = 133,

        [EnumMember(Value = "Nationality is not defined for the profile")]
        [Description("Nationality is not defined for the profile")]
        OrderNationalityMissing = 134,

        [EnumMember(Value = "Reschedule delivery failed")]
        [Description("Reschedule delivery failed")]
        RescheduleOrderStatusNotCorrect = 135,

        [EnumMember(Value = "Buddy is already assigned for selected number")]
        [Description("Buddy is already assigned for selected number")]
        BuddyAlreadyExists = 136,

        [EnumMember(Value = "PaymentNotProcessed")]
        [Description("PaymentNotProcessed")]
        PaymentNotProcessed = 137,

        [EnumMember(Value = "Your account is locked")]
        [Description("Your account is locked")]
        AccountIsLocked = 138,

        [EnumMember(Value = "Action is Invalid")]
        [Description("Action is Invalid")]
        ActionIsInvalid = 139,
    }
}
