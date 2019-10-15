using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Core.Models
{
    /// <summary>
    /// Operation response convey status of request - success/failure/validation error etc. without a result data collection.
    /// Mainly used on failure of requested operation or for a request which only have success message to display and no related result data object to be diplayed
    /// </summary>
    public class OperationResponse:RequestStatus
    {
        public bool HasSucceeded { get; set; }
        public bool IsDomainValidationErrors { get; set; }
        public string Message { get; set; }       
        public string StatusCode { get; set; }
        /// <summary>
        /// Used to envelope created/updated object
        /// </summary>
        public object ReturnedObject { get; set; }

        public OperationResponse()
        {
            HasSucceeded = true;
            Message = "";
            IsDomainValidationErrors = false;
            StatusCode = Default;
        }
        public OperationResponse(bool hasSucceeded = true, string message = "")
        {
            HasSucceeded = hasSucceeded;
            Message = message;
        }
        public static RequestStatus OperationResponseFailed(string failReason, int customerId)
        {
            return new OperationResponse
            {
                HasSucceeded = false,
                Message = failReason,
                IsDomainValidationErrors = false,
                _loginCustomerId = customerId
            };
        }

        public ServerResponse GenerateResponse()
        {
            return new ServerResponse(this.ReturnedObject, HasSucceeded, Message);
        }
    }

    /// <summary>
    /// Server response convey status of request  along with a result data collection. 
    /// </summary>
    public class ServerResponse : RequestStatus
    {
        public object Result { get; set; }
        public bool HasSucceeded { get; set; }
        public string Message { get; set; }
        public string StatusCode { get; set; }


        public ServerResponse()
        {
            StatusCode = Default;
        }
        public ServerResponse(object result, bool hasSucceeded, string msg)
        {
            StatusCode = Default;
            Result = result;
            HasSucceeded = hasSucceeded;
            Message = msg;
        }

        public static RequestStatus ServerResponseSuccess(object results, int customerId)
        {
            return new ServerResponse
            {
                HasSucceeded = true,
                Message = StatusMessages.SuccessMessage,
                Result = results,
                _loginCustomerId = customerId
            };            
        }

        public static RequestStatus ServerResponseFailed(string failReason, int customerId)
        {
            return new OperationResponse
            {
                HasSucceeded = false,
                Message = failReason,
                IsDomainValidationErrors = false,
                _loginCustomerId = customerId
            };
        }

    }

    public class DatabaseResponse
    {
        public int ResponseCode { get; set; }

        public object  Results {get;set;}


    }


}
