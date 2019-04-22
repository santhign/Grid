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
        /// Removes the vas service.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        Task<DatabaseResponse> RemoveVasService(int customerId, string mobileNumber, int planId);
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
        Task<DatabaseResponse> ChangePhoneRequest(ChangePhoneRequest changePhone);
        /// <summary>
        /// Updates the cr shipping details.
        /// </summary>
        /// <param name="shippingDetails">The shipping details.</param>
        /// <returns></returns>
        Task<DatabaseResponse> UpdateCRShippingDetails(UpdateCRShippingDetailsRequest shippingDetails);

        Task<DatabaseResponse> BuySharedService(int customerId, int bundleId);

    }
}
