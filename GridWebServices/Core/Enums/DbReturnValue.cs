﻿using System.ComponentModel;
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
        [Description("Record does not exist")]
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
        [Description("Email already exists")]
        EmailExists = 108,

        [EnumMember(Value = "Email Not Exists")]
        [Description("Login/password does not match")]
        EmailNotExists = 109,

        [EnumMember(Value = "Password Incorrect")]
        [Description("Login/password does not match")]
        PasswordIncorrect = 110,

        [EnumMember(Value = "Authentication Success")]
        [Description("Customer authentication success")]
        AuthSuccess = 111,

        [EnumMember(Value = "Reason Unknown")]
        [Description("Operation failed due to unknown reason")]
        ReasonUnknown = 112,

        [EnumMember(Value = "Token Auth Failed")]
        [Description("Authentication failed")]
        TokenAuthFailed = 113,

        [EnumMember(Value = "Token Expired")]
        [Description("Authorization Failed. Token expired")]
        TokenExpired = 114,

        [EnumMember(Value = "GetAssets Failed")]
        [Description("Failed to get assets from service provider")]
        GetAssetsFailed = 115,

        [EnumMember(Value = "Blocking Failed")]
        [Description("Phone number blocking failed at service provider")]
        BlockingFailed = 117,

        [EnumMember(Value = "Un Blocking Failed")]
        [Description("Unblocking assigned phone number failed at service provider")]
        UnBlockingFailed = 118,

        [EnumMember(Value = "Record Doesn't Exists")]
        [Description("No records found in database")]
        NoRecords = 119,

        [EnumMember(Value = "Transaction Success")]
        [Description("Transaction tables Updated Successfully")]
        TransactionSuccess = 120,

        [EnumMember(Value = "Transaction Failed")]
        [Description("Transaction tables updation failed")]
        TransactionFailed = 121,

        [EnumMember(Value = "Primary Try Delete")]
        [Description("Primary number can not be deleted")]
        PrimaryTryDelete = 122,

        [EnumMember(Value = "Order completed Try Delete")]
        [Description("Order completed, number can not be deleted")]
        CompletedOrderDelete = 123,

        [EnumMember(Value = "Reset Password Token Valid")]
        [Description("Reset password token is valid")]
        ResetPasswordTokenValid = 124,

        [EnumMember(Value = "Reset Password Token Expired")]
        [Description("Reset Password token expired")]
        ResetPasswordTokenExpired = 125,

        [EnumMember(Value = "Already processed payment")]
        [Description("Payment has been processed already")]
        PaymentAlreadyProcessed = 126,

        [EnumMember(Value = "Update on active record is not allowed")]
        [Description("Update on active record is not allowed")]
        UpdateNotAllowed = 127,

        [EnumMember(Value = "Duplicate record Exist")]
        [Description("Same request is already raised")]
        DuplicateCRExists = 128,

        [EnumMember(Value = "Unsuspension Validation")]
        [Description("Suspension request not supported")]
        UnSuspensionValidation = 129,

        [EnumMember(Value = "Delivery slot unavailablity")]
        [Description("Selected delivery slot is not available, please select a new slot")]
        DeliverySlotUnavailability = 130,

        [EnumMember(Value = "Same Bundle is present")]
        [Description("Plan is not changed as you are raising request against same bundle")]
        SameBundleValidation = 131,

        [EnumMember(Value = "Order delivery information is missing")]
        [Description("Order delivery information is missing")]
        OrderDeliveryInformationMissing = 132,

        [EnumMember(Value = "Order Identity documents are missing")]
        [Description("Order identity documents are missing")]
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

        [EnumMember(Value = "Subscriber state is not Active")]
        [Description("Subscriber state is not Active")]
        ActionIsInvalid = 139,

        [EnumMember(Value = "Allowed to add more subscribers")]
        [Description("Allowed to add more subscribers")]
        AllowSubscribers = 140,

        [EnumMember(Value = "Not allowed to add more subscribers")]
        [Description("You have reached the maximum limit of numbers. Not allowed to add more lines")]
        NotAllowSubscribers = 141,

        [EnumMember(Value = "Invalid ID detail")]
        [Description("Provided IC details are invalid")]
        InvalidNRIC = 142,

        [EnumMember(Value = "AdminTokenNotExists")]
        [Description("AdminTokenNotExists")]
        AdminTokenNotExists = 143,

        [EnumMember(Value = "AccountDeactivated")]
        [Description("Deactivated Account")]
        AccountDeactivated = 144,

        [EnumMember(Value = "ExistingCard")]
        [Description("Payed With Existing Card. Token updated")]
        ExistingCard = 145,
        
        [EnumMember(Value = "Mobile number mismatch")]
        [Description("Mobile number does not exist in specified order")]
        MobileNumberMismatch = 146,

        [EnumMember(Value = "Voucher is applicable only for delivery failed orders")]
        [Description("Voucher is applicable only for delivery failed orders")]
        VoucherNotApplicable = 147,

        [EnumMember(Value = "Voucher already exists for selected item")]
        [Description("Voucher already exists for selected item")]
        ExistingVoucher = 148,

        [EnumMember(Value = "Voucher is applicable only for active subscribers")]
        [Description("Voucher is applicable only for active subscribers")]
        VoucherNotApplicable_CR = 149,

        [EnumMember(Value = "Delete Failed")]
        [Description("Record deletion failed")]
        DeleteFailed = 150,

        [EnumMember(Value = "Termination Failed")]
        [Description("Invalid termination request raised")]
        TerminationFailed = 151,

        [EnumMember(Value = "Suspension Failed")]
        [Description("Invalid suspension request raised")]
        SuspensionFailed = 152,

        [EnumMember(Value = "Unsuspension Failed")]
        [Description("Invalid Unsuspension request raised")]
        UnsuspensionFailed = 153,

        [EnumMember(Value = "Duplicate NRIC Present")]
        [Description("Duplicate NRIC details are present for another customer")]
        DuplicateNRICNotAllowed = 154,

        [EnumMember(Value = "Linked buddy line is not active")]
        [Description("Linked buddy line is not active")]
        BuddyNotActive = 155,

        [EnumMember(Value = "PendingBuddyLocked")]
        [Description("PendingBuddyLocked")]
        PendingBuddyLocked = 156,

        [EnumMember(Value = "PendingBuddyUnLocked")]
        [Description("PendingBuddyUnLocked")]
        PendingBuddyUnLocked = 157,

        [EnumMember(Value = "Update Success Send Email")]
        [Description("Record updated successfully")]
        UpdateSuccessSendEmail = 158,

        [EnumMember(Value = "Request Token Expired")]
        [Description("Request token is already used. Token expired")]
        RequestTokenExpired = 159,

        [EnumMember(Value = "Password Policy Error")]
        [Description("Please input a stronger alpha-numberic password with minimum 8 characters")]
        PasswordPolicyError = 160,

        [EnumMember(Value = "Both Password should present")]
        [Description("Please enter old as well as new password")]
        BothPasswordsNotPresent = 161,

        [EnumMember(Value = "Old Password is not valid")]
        [Description("Current password is invalid.")]
        OldPasswordInvalid = 162,

        [EnumMember(Value = "RequestTypeNotFound")]
        [Description("RequestTypeNotFound")]
        RequestTypeNotFound = 163,

        [EnumMember(Value = "SubscriberNotExists")]
        [Description("SubscriberNotExists")]
        SubscriberNotExists = 164,

        [EnumMember(Value = "NumberChangeRequestAlreadyRaised")]
        [Description("Number ChangeRequest already raised")]
        NumberChangeRequestAlreadyRaised = 165,

        [EnumMember(Value = "NotBuddyBundle")]
        [Description("Selected Bundle is not a buddy bundle")]
        NotBuddyBundle = 170,

        [EnumMember(Value = "VasNotInAddedList")]
        [Description("Vas not in added list")]
        VasNotInAddedList = 171,

        [EnumMember(Value = "InvalidPromoCode")]
        [Description("Order creation failed due to invalid promotion code.")]
        InvalidPromoCode = 172,

        [EnumMember(Value = "PendingOrderSIM")]
        [Description("SIM allocation is pending for the roadshow order")]
        PendingOrderSIM = 173,
    }
}

