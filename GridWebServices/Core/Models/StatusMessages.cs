using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Models
{
    public static class StatusMessages
    {       
        public static string SuccessMessage = "Success";

        public static string FailureMessage = "Failed";

        public static string ServerError = "Server Error";

        public static string DomainValidationError = "Domain Validation Error";

        public static string MissingRequiredFields = "Required field missing";

        public static string NoRecordsFound = "No records found for request";

        public static string InvalidMessage = "Invalid";

        public static string ValidMessage = "Valid";

        public static string ResetPassowrdLinkSent = "An email has been sent to registered Email ID with Reset Password Link";
    }
}
