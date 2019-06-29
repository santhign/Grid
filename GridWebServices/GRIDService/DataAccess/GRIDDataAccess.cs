using Core.Enums;
using Core.Extensions;
using Core.Helpers;
using Core.Models;
using GRIDService.DataAccess.Interfaces;
using GRIDService.Models;
using InfrastructureService;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace GRIDService.DataAccess
{
    /// <summary>
    /// GRID Data Access
    /// </summary>
    /// <seealso cref="GRIDService.DataAccess.Interfaces.IGRIDDataAccess" />
    public class GRIDDataAccess : IGRIDDataAccess
    {
        /// <summary>
        /// The data helper
        /// </summary>
        internal DataAccessHelper _DataHelper = null;

        /// <summary>
        /// The configuration
        /// </summary>
        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public GRIDDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Grids the process change sim.
        /// </summary>
        /// <param name="ChangeRequestID">The change request identifier.</param>
        /// <param name="Status">The status.</param>
        /// <param name="SIMID">The simid.</param>
        /// <param name="SusbcriberStateUpdate">The susbcriber state update.</param>
        /// <returns></returns>
        public async Task<int> Grid_ProcessChangeSim(int ChangeRequestID, int Status, string SIMID, int SusbcriberStateUpdate)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@ChangeRequestID",  SqlDbType.Int ),
                    new SqlParameter( "@Status",  SqlDbType.Int ),
                    new SqlParameter( "@SIMID",  SqlDbType.NVarChar),
                    new SqlParameter( "@SusbcriberStateUpdate",  SqlDbType.Int),
                    
                };

                parameters[0].Value = ChangeRequestID;
                parameters[1].Value = Status;
                parameters[2].Value = SIMID;
                parameters[3].Value = SusbcriberStateUpdate;
                


                _DataHelper = new DataAccessHelper("Grid_ProcessChangeSim", parameters, _configuration);
                DataTable dt = new DataTable();
                return await _DataHelper.RunAsync();
                
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Grids the process suspension.
        /// </summary>
        /// <param name="suspensionRequest">The suspension request.</param>
        /// <returns></returns>
        public async Task<int> Grid_ProcessSuspension(SuspensionRequest suspensionRequest)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@ChangeRequestID",  SqlDbType.Int ),
                    new SqlParameter( "@SuspensionType",  SqlDbType.Int ),
                    new SqlParameter( "@Remarks",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Status",  SqlDbType.NVarChar ),
                    new SqlParameter( "@RejectReason",  SqlDbType.NVarChar),
                    new SqlParameter( "@CurrentStatus",  SqlDbType.Int),

                };

                parameters[0].Value = suspensionRequest.ChangeRequestID;
                parameters[1].Value = suspensionRequest.SuspensionType;
                parameters[2].Value = suspensionRequest.Remarks;
                parameters[3].Value = suspensionRequest.Status;
                parameters[4].Value = suspensionRequest.RejectReason;
                parameters[5].Value = suspensionRequest.CurrentStatus;



                _DataHelper = new DataAccessHelper("Grid_ProcessSuspension_v1", parameters, _configuration);
                DataTable dt = new DataTable();
                return await _DataHelper.RunAsync();

            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Grids the process termination.
        /// </summary>
        /// <param name="terminationRequest">The termination request.</param>
        /// <returns></returns>
        public async Task<int> Grid_ProcessTermination(TerminationOrUnsuspensionRequest terminationRequest)
        {
            try
            {
                SqlParameter[] parameters =
                 {
                    new SqlParameter( "@ChangeRequestID",  SqlDbType.Int ),                    
                    new SqlParameter( "@Remarks",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Status",  SqlDbType.NVarChar ),
                    new SqlParameter( "@RejectReason",  SqlDbType.NVarChar),
                    new SqlParameter( "@CurrentStatus",  SqlDbType.Int),

                };

                parameters[0].Value = terminationRequest.ChangeRequestID;
                parameters[1].Value = terminationRequest.Remarks;
                parameters[2].Value = terminationRequest.Status;
                parameters[3].Value = terminationRequest.RejectReason;
                parameters[4].Value = terminationRequest.CurrentStatus;



                _DataHelper = new DataAccessHelper("Grid_ProcessTermination_v1", parameters, _configuration);
                DataTable dt = new DataTable();
                return await _DataHelper.RunAsync();

            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Grids the process un suspension.
        /// </summary>
        /// <param name="unsuspensionRequest">The unsuspension request.</param>
        /// <returns></returns>
        public async Task<int> Grid_ProcessUnSuspension(TerminationOrUnsuspensionRequest unsuspensionRequest)
        {
            try
            {
                SqlParameter[] parameters =
                 {
                    new SqlParameter( "@ChangeRequestID",  SqlDbType.Int ),                    
                    new SqlParameter( "@Remarks",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Status",  SqlDbType.NVarChar ),
                    new SqlParameter( "@RejectReason",  SqlDbType.NVarChar),
                    new SqlParameter( "@CurrentStatus",  SqlDbType.Int),

                };

                parameters[0].Value = unsuspensionRequest.ChangeRequestID;
                parameters[1].Value = unsuspensionRequest.Remarks;
                parameters[2].Value = unsuspensionRequest.Status;
                parameters[3].Value = unsuspensionRequest.RejectReason;
                parameters[4].Value = unsuspensionRequest.CurrentStatus;



                _DataHelper = new DataAccessHelper("Grid_ProcessUnSuspension_v1", parameters, _configuration);
                DataTable dt = new DataTable();
                return await _DataHelper.RunAsync();

            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Grids the process vas addition.
        /// </summary>
        /// <param name="ChangeRequestID">The change request identifier.</param>
        /// <param name="Status">The status.</param>
        /// <param name="ValidFrom">The valid from.</param>
        /// <param name="ValidTo">The valid to.</param>
        /// <returns></returns>
        public async Task<int> Grid_ProcessVASAddition(int ChangeRequestID, int Status, DateTime ValidFrom, DateTime ValidTo)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@ChangeRequestID",  SqlDbType.Int ),
                    new SqlParameter( "@Status",  SqlDbType.Int ),
                    new SqlParameter( "@ValidFrom",  SqlDbType.Date),
                    new SqlParameter( "@ValidTo",  SqlDbType.Date),

                };

                parameters[0].Value = ChangeRequestID;
                parameters[1].Value = Status;
                parameters[2].Value = ValidFrom.Date;
                parameters[3].Value = ValidTo.Date;



                _DataHelper = new DataAccessHelper("Grid_ProcessVASAddition", parameters, _configuration);
                DataTable dt = new DataTable();
                return await _DataHelper.RunAsync();

            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Grids the process vas removal.
        /// </summary>
        /// <param name="ChangeRequestID">The change request identifier.</param>
        /// <param name="Status">The status.</param>
        /// <returns></returns>
        public async Task<int> Grid_ProcessVASRemoval(int ChangeRequestID, int Status)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@ChangeRequestID",  SqlDbType.Int ),
                    new SqlParameter( "@Status",  SqlDbType.Int ),
                };

                parameters[0].Value = ChangeRequestID;
                parameters[1].Value = Status;

                _DataHelper = new DataAccessHelper("Grid_ProcessVASRemoval", parameters, _configuration);
                DataTable dt = new DataTable();
                return await _DataHelper.RunAsync();

            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Grids the update billing account.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="BillingAccountNumber">The billing account number.</param>
        /// <param name="BSSProfileid">The BSS profileid.</param>
        /// <returns></returns>
        public async Task<int> Grid_UpdateBillingAccount(string AccountID, string BillingAccountNumber, string BSSProfileid)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@AccountID",  SqlDbType.Int ),
                    new SqlParameter( "@BillingAccountNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@BSSProfileid",  SqlDbType.NVarChar ),
                };

                parameters[0].Value = AccountID;
                parameters[1].Value = BillingAccountNumber;
                parameters[2].Value = BSSProfileid;

                _DataHelper = new DataAccessHelper("Grid_UpdateBillingAccount", parameters, _configuration);
                DataTable dt = new DataTable();
                return await _DataHelper.RunAsync();

            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Grids the update delivery status.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="BillingAccountNumber">The billing account number.</param>
        /// <param name="BSSProfileid">The BSS profileid.</param>
        /// <returns></returns>
        public async Task<int> Grid_UpdateDeliveryStatus(string AccountID, string BillingAccountNumber, string BSSProfileid)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@AccountID",  SqlDbType.Int ),
                    new SqlParameter( "@BillingAccountNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@BSSProfileid",  SqlDbType.NVarChar ),
                };

                parameters[0].Value = AccountID;
                parameters[1].Value = BillingAccountNumber;
                parameters[2].Value = BSSProfileid;

                _DataHelper = new DataAccessHelper("Grid_UpdateDeliveryStatus", parameters, _configuration);
                DataTable dt = new DataTable();
                return await _DataHelper.RunAsync();

            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Grids the update initial order subscriptions.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task<int> Grid_UpdateInitialOrderSubscriptions(UpdateInitialOrderSubscriptionsRequest request)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@SubscriberID",  SqlDbType.Int ),
                    new SqlParameter( "@BundleID",  SqlDbType.Int ),
                    new SqlParameter( "@BSSPlanCode",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Status",  SqlDbType.Int ),
                    new SqlParameter( "@ValidFrom",  SqlDbType.Date ),
                    new SqlParameter( "@ValidTo",  SqlDbType.Date ),

                };

                parameters[0].Value = request.OrderID;
                parameters[1].Value = request.SubscriberID;
                parameters[2].Value = request.BundleID;
                parameters[3].Value = request.BSSPlanCode;
                parameters[4].Value = request.Status;
                parameters[5].Value = request.ValidFrom.Date;
                parameters[6].Value = request.ValidTo.Date;

                _DataHelper = new DataAccessHelper("Grid_UpdateInitialOrderSubscriptions", parameters, _configuration);
                DataTable dt = new DataTable();
                return await _DataHelper.RunAsync();

            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Grids the update order status.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task<int> Grid_UpdateOrderStatus(UpdateOrderStatus request)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@OrderID",  SqlDbType.Int ),
                    new SqlParameter( "@OrderNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Orderstatus",  SqlDbType.NVarChar ),
                    new SqlParameter( "@error_reason",  SqlDbType.NVarChar ),
                    
                };

                parameters[0].Value = request.OrderID;
                parameters[1].Value = request.OrderNumber;
                parameters[2].Value = request.Orderstatus;
                parameters[3].Value = request.error_reason;
                

                _DataHelper = new DataAccessHelper("Grid_UpdateOrderStatus", parameters, _configuration);
                DataTable dt = new DataTable();
                return await _DataHelper.RunAsync();

            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }
        /// <summary>
        /// Grids the state of the update subscriber.
        /// </summary>
        /// <param name="SubscriberID">The subscriber identifier.</param>
        /// <param name="state">The state.</param>
        /// <param name="error_reason">The error reason.</param>
        /// <returns></returns>
        public async Task<int> Grid_UpdateSubscriberState(int SubscriberID, string state, string error_reason)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@SubscriberID",  SqlDbType.Int ),
                    new SqlParameter( "@state",  SqlDbType.NVarChar ),                    
                    new SqlParameter( "@error_reason",  SqlDbType.NVarChar ),

                };

                parameters[0].Value = SubscriberID;
                parameters[1].Value = state;                
                parameters[2].Value = error_reason;


                _DataHelper = new DataAccessHelper("Grid_UpdateSubscriberState", parameters, _configuration);
                DataTable dt = new DataTable();
                return await _DataHelper.RunAsync();

            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }
        /// <summary>
        /// Grids the update vendor.
        /// </summary>
        /// <param name="DeliveryinformationID">The deliveryinformation identifier.</param>
        /// <param name="shipnumber">The shipnumber.</param>
        /// <param name="vendor">The vendor.</param>
        /// <returns></returns>
        public async Task<int> Grid_UpdateVendor(int DeliveryinformationID, string shipnumber, string vendor)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@DeliveryinformationID",  SqlDbType.Int ),
                    new SqlParameter( "@shipnumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@vendor",  SqlDbType.NVarChar ),

                };

                parameters[0].Value = DeliveryinformationID;
                parameters[1].Value = shipnumber;
                parameters[2].Value = vendor;


                _DataHelper = new DataAccessHelper("Grid_UpdateVendor", parameters, _configuration);
                DataTable dt = new DataTable();
                return await _DataHelper.RunAsync();

            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }
        /// <summary>
        /// Grids the update vendor tracking code.
        /// </summary>
        /// <param name="DeliveryinformationID">The deliveryinformation identifier.</param>
        /// <param name="TrackingCode">The tracking code.</param>
        /// <returns></returns>
        public async Task<int> Grid_UpdateVendorTrackingCode(int DeliveryinformationID, string TrackingCode)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@DeliveryinformationID",  SqlDbType.Int ),
                    new SqlParameter( "@TrackingCode",  SqlDbType.NVarChar )
                };

                parameters[0].Value = DeliveryinformationID;
                parameters[1].Value = TrackingCode;
                


                _DataHelper = new DataAccessHelper("Grid_UpdateVendorTrackingCode", parameters, _configuration);
                DataTable dt = new DataTable();
                return await _DataHelper.RunAsync();
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }
    }
}
