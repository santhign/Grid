using GRIDService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GRIDService.DataAccess.Interfaces
{
    /// <summary>
    /// Interface GRIDDataAccess
    /// </summary>
    public interface IGRIDDataAccess
    {
        /// <summary>
        /// Grids the process change sim.
        /// </summary>
        /// <param name="ChangeRequestID">The change request identifier.</param>
        /// <param name="Status">The status.</param>
        /// <param name="SIMID">The simid.</param>
        /// <param name="SusbcriberStateUpdate">The susbcriber state update.</param>
        /// <returns></returns>
        Task<int> Grid_ProcessChangeSim(int ChangeRequestID, int Status, string SIMID, int SusbcriberStateUpdate);
        /// <summary>
        /// Grids the process suspension.
        /// </summary>
        /// <param name="suspensionRequest">The suspension request.</param>
        /// <returns></returns>
        Task<int> Grid_ProcessSuspension(SuspensionRequest suspensionRequest);
        /// <summary>
        /// Grids the process termination.
        /// </summary>
        /// <param name="terminationRequest">The termination request.</param>
        /// <returns></returns>
        Task<int> Grid_ProcessTermination(TerminationOrUnsuspensionRequest terminationRequest);
        /// <summary>
        /// Grids the process vas addition.
        /// </summary>
        /// <param name="ChangeRequestID">The change request identifier.</param>
        /// <param name="Status">The status.</param>
        /// <param name="ValidFrom">The valid from.</param>
        /// <param name="ValidTo">The valid to.</param>
        /// <returns></returns>
        Task<int> Grid_ProcessVASAddition(int ChangeRequestID, int Status, DateTime ValidFrom, DateTime ValidTo);
        /// <summary>
        /// Grids the process vas removal.
        /// </summary>
        /// <param name="ChangeRequestID">The change request identifier.</param>
        /// <param name="Status">The status.</param>
        /// <returns></returns>
        Task<int> Grid_ProcessVASRemoval(int ChangeRequestID, int Status);
        /// <summary>
        /// Grids the update billing account.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="BillingAccountNumber">The billing account number.</param>
        /// <param name="BSSProfileid">The BSS profileid.</param>
        /// <returns></returns>
        Task<int> Grid_UpdateBillingAccount(string AccountID, string BillingAccountNumber, string BSSProfileid);
        /// <summary>
        /// Grids the update initial order subscriptions.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        Task<int> Grid_UpdateInitialOrderSubscriptions(UpdateInitialOrderSubscriptionsRequest request);
        /// <summary>
        /// Grids the update order status.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        Task<int> Grid_UpdateOrderStatus(UpdateOrderStatus request);

        /// <summary>
        /// Grids the state of the update subscriber.
        /// </summary>
        /// <param name="SubscriberID">The subscriber identifier.</param>
        /// <param name="state">The state.</param>
        /// <param name="error_reason">The error reason.</param>
        /// <returns></returns>
        Task<int> Grid_UpdateSubscriberState(int SubscriberID, string state, string error_reason);

        /// <summary>
        /// Grids the update vendor.
        /// </summary>
        /// <param name="DeliveryinformationID">The deliveryinformation identifier.</param>
        /// <param name="shipnumber">The shipnumber.</param>
        /// <param name="vendor">The vendor.</param>
        /// <returns></returns>
        Task<int> Grid_UpdateVendor(int DeliveryinformationID, string shipnumber, string vendor);
        /// <summary>
        /// Grids the update vendor tracking code.
        /// </summary>
        /// <param name="DeliveryinformationID">The deliveryinformation identifier.</param>
        /// <param name="TrackingCode">The tracking code.</param>
        /// <returns></returns>
        Task<int> Grid_UpdateVendorTrackingCode(int DeliveryinformationID, string TrackingCode);
    }
}
