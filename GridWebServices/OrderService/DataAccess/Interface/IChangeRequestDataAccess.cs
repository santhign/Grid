using Core.Models;
using OrderService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public interface IChangeRequestDataAccess
    {
        /// <summary>
        /// Removes the vas service
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <param name="subscriptionID">The subscription identifier.</param>
        /// <returns></returns>
        Task<DatabaseResponse> RemoveVasService(int customerId, string mobileNumber, int subscriptionID);

        /// <summary>
        /// Removes the shared vas service.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="accountSubscriptionId">The account subscription identifier.</param>
        /// <returns></returns>
        Task<DatabaseResponse> RemoveSharedVasService(int customerId, int accountSubscriptionId);
        /// <summary>
        /// Buys the vas service.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <param name="bundleId">The bundle identifier.</param>
        /// <param name="quantity">The quantity.</param>
        /// <returns></returns>
        Task<DatabaseResponse> BuyVasService(int customerId, string mobileNumber, int bundleId, int quantity);
        /// <summary>
        /// Sims the replacement request.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <returns></returns>
        Task<DatabaseResponse> SimReplacementRequest(int customerId, string mobileNumber);
        /// <summary>
        /// Terminations the or suspension request.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <param name="request">The request.</param>
        /// <param name="remark">The remark.</param>
        /// <returns></returns>
        Task<DatabaseResponse> TerminationOrSuspensionRequest(int customerId, string mobileNumber, string request,
            string remark);
        /// <summary>
        /// Changes the phone request.
        /// </summary>
        /// <param name="changePhone">The change phone.</param>
        /// <returns></returns>
        Task<DatabaseResponse> ChangePhoneRequest(ChangePhoneRequest changePhone, int customerID);
        /// <summary>
        /// Updates the cr shipping details.
        /// </summary>
        /// <param name="shippingDetails">The shipping details.</param>
        /// <returns></returns>
        Task<DatabaseResponse> UpdateCRShippingDetails(UpdateCRShippingDetailsRequest shippingDetails);

        /// <summary>
        /// Buys the shared service.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="bundleId">The bundle identifier.</param>
        /// <returns></returns>
        Task<DatabaseResponse> BuySharedService(int customerId, int bundleId);

        /// <summary>
        /// Changes the plan service.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <param name="bundleId">The bundle identifier.</param>
        /// <returns></returns>
        Task<DatabaseResponse> ChangePlanService(int customerId, string mobileNumber, int bundleId);

        /// <summary>
        /// Gets the buddy details.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <returns></returns>
        Task<BuddyResponse> GetBuddyDetails(int customerID, string mobileNumber);

        /// <summary>
        /// Gets the cr details with delivery information.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="changeRequestID">The change request identifier.</param>
        /// <returns></returns>
        Task<DatabaseResponse> GetCRDetailsWithDeliveryInfo(int customerID, int changeRequestID);

        /// <summary>
        /// Crs the remove loa details.
        /// </summary>
        /// <param name="ChangeRequestID">The change request identifier.</param>
        /// <returns></returns>
        Task<DatabaseResponse> CR_RemoveLOADetails(int ChangeRequestID);
        /// <summary>
        /// Updates the crloa details.
        /// </summary>
        /// <param name="loaDetails">The loa details.</param>
        /// <returns></returns>
        Task<DatabaseResponse> UpdateCRLOADetails(UpdateCRLOADetailsRequest loaDetails);
        Task<DatabaseResponse> CheckChangePhoneRequestStatus(ChangePhoneRequest changePhone, int customerID);
    }
}
