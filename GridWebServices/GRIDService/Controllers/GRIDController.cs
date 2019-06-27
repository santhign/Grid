using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Enums;
using Core.Extensions;
using Core.Helpers;
using Core.Models;
using GRIDService.DataAccess.Interfaces;
using GRIDService.Models;
using InfrastructureService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace GRIDService.Controllers
{
    /// <summary>
    /// GRID Controller
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route("api/[controller]")]
    [ApiController]
    public class GRIDController : Controller
    {

        /// <summary>
        /// The iconfiguration
        /// </summary>
        private readonly IConfiguration _iconfiguration;

        /// <summary>
        /// The change request data access
        /// </summary>
        private readonly IGRIDDataAccess _dataAccess;
        /// <summary>
        /// Initializes a new instance of the <see cref="GRIDController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="dataAccess">The data access.</param>
        public GRIDController(IConfiguration configuration, IGRIDDataAccess dataAccess)
        {
            _iconfiguration = configuration;
            _dataAccess = dataAccess;

        }

        /// <summary>
        /// Processes the change sim.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="changeRequestID">The change request identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="SIMID">The simid.</param>
        /// <param name="SusbcriberStateUpdate">The susbcriber state update.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("ProcessChangeSim/{changeRequestID}/{status}/{SIMID}/{SusbcriberStateUpdate}")]
        public async Task<IActionResult> ProcessChangeSim([FromHeader(Name = "Grid-Service-Header-Token")] string token,
            [FromRoute] int changeRequestID, [FromRoute] int status, [FromRoute] string SIMID, [FromRoute] int SusbcriberStateUpdate)
        {

            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage))
                    });
                }

                //TokenValidationHelper tokenValidationHelper = new TokenValidationHelper();
                if (!ValidateGridHeaderToken(token))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                var result = await _dataAccess.Grid_ProcessChangeSim(changeRequestID, status, SIMID, SusbcriberStateUpdate);
                if (result == Enum.ReturnSuccess)
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = true,
                        Message = Enum.ReturnSuccess.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }
                else
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = Enum.ReturnFailure.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }


            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });

            }
        }

        /// <summary>
        /// Processes the suspension.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="suspensionRequest">The suspension request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("ProcessSuspension")]
        public async Task<IActionResult> ProcessSuspension([FromHeader(Name = "Grid-Service-Header-Token")] string token,
            SuspensionRequest suspensionRequest)
        {

            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage))
                    });
                }

                if (!ValidateGridHeaderToken(token))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                var result = await _dataAccess.Grid_ProcessSuspension(suspensionRequest);
                if (result == Enum.ReturnSuccess)
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = true,
                        Message = Enum.ReturnSuccess.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }
                else
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = Enum.ReturnFailure.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }


            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });

            }
        }

        /// <summary>
        /// Processes the termination.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="terminationRequest">The termination request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("ProcessTermination")]
        public async Task<IActionResult> ProcessTermination([FromHeader(Name = "Grid-Service-Header-Token")] string token,
            [FromForm] TerminationOrUnsuspensionRequest terminationRequest)
        {

            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage))
                    });
                }

                if (!ValidateGridHeaderToken(token))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                var result = await _dataAccess.Grid_ProcessTermination(terminationRequest);
                if (result == Enum.ReturnSuccess)
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = true,
                        Message = Enum.ReturnSuccess.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }
                else
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = Enum.ReturnFailure.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }


            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });

            }
        }

        /// <summary>
        /// Processes the vas addition.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="ChangeRequestID">The change request identifier.</param>
        /// <param name="Status">The status.</param>
        /// <param name="ValidFrom">The valid from.</param>
        /// <param name="ValidTo">The valid to.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("ProcessVASAddition/{changeRequestID}/{status}/{ValidFrom}/{ValidTo}")]
        public async Task<IActionResult> ProcessVASAddition([FromHeader(Name = "Grid-Service-Header-Token")] string token,
            [FromRoute] int ChangeRequestID, [FromRoute] int Status, [FromRoute] DateTime ValidFrom, [FromRoute] DateTime ValidTo)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage))
                    });
                }

                if (!ValidateGridHeaderToken(token))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                var result = await _dataAccess.Grid_ProcessVASAddition(ChangeRequestID, Status, ValidFrom, ValidTo);
                if (result == Enum.ReturnSuccess)
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = true,
                        Message = Enum.ReturnSuccess.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }
                else
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = Enum.ReturnFailure.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }


            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });

            }
        }

        /// <summary>
        /// Grids the process vas removal.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="changeRequestID">The change request identifier.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("ProcessVASRemoval/{changeRequestID}/{status}")]
        public async Task<IActionResult> ProcessVASRemoval([FromHeader(Name = "Grid-Service-Header-Token")] string token,
            [FromRoute] int changeRequestID, [FromRoute] int status)
        {

            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage))
                    });
                }

                if (!ValidateGridHeaderToken(token))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                var result = await _dataAccess.Grid_ProcessVASRemoval(changeRequestID, status);
                if (result == Enum.ReturnSuccess)
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = true,
                        Message = Enum.ReturnSuccess.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }
                else
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = Enum.ReturnFailure.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }


            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });

            }
        }

        /// <summary>
        /// Updates the billing account.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="BillingAccountNumber">The billing account number.</param>
        /// <param name="BSSProfileid">The BSS profileid.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateBillingAccount/{AccountID}/{BillingAccountNumber}/{BSSProfileid}")]
        public async Task<IActionResult> UpdateBillingAccount([FromHeader(Name = "Grid-Service-Header-Token")] string token, [FromRoute] string AccountID, [FromRoute] string BillingAccountNumber, [FromRoute] string BSSProfileid)
        {

            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage))
                    });
                }

                if (!ValidateGridHeaderToken(token))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                var result = await _dataAccess.Grid_UpdateBillingAccount(AccountID, BillingAccountNumber, BSSProfileid);
                if (result == Enum.ReturnSuccess)
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = true,
                        Message = Enum.ReturnSuccess.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }
                else
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = Enum.ReturnFailure.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }


            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });

            }
        }

        /// <summary>
        /// Updates the delivery status.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="BillingAccountNumber">The billing account number.</param>
        /// <param name="BSSProfileid">The BSS profileid.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateDeliveryStatus/{AccountID}/{BillingAccountNumber}/{BSSProfileid}")]
        public async Task<IActionResult> UpdateDeliveryStatus([FromHeader(Name = "Grid-Service-Header-Token")] string token, [FromRoute]  string AccountID, [FromRoute] string BillingAccountNumber, [FromRoute] string BSSProfileid)
        {

            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage))
                    });
                }

                if (!ValidateGridHeaderToken(token))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                var result = await _dataAccess.Grid_UpdateDeliveryStatus(AccountID, BillingAccountNumber, BSSProfileid);
                if (result == Enum.ReturnSuccess)
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = true,
                        Message = Enum.ReturnSuccess.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }
                else
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = Enum.ReturnFailure.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }


            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });

            }
        }

        /// <summary>
        /// Updates the initial order subscriptions.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateInitialOrderSubscriptions")]
        public async Task<IActionResult> UpdateInitialOrderSubscriptions([FromHeader(Name = "Grid-Service-Header-Token")] string token, [FromForm]  UpdateInitialOrderSubscriptionsRequest request)
        {

            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage))
                    });
                }

                if (!ValidateGridHeaderToken(token))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                var result = await _dataAccess.Grid_UpdateInitialOrderSubscriptions(request);
                if (result == Enum.ReturnSuccess)
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = true,
                        Message = Enum.ReturnSuccess.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }
                else
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = Enum.ReturnFailure.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }


            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });

            }
        }

        /// <summary>
        /// Updates the order status.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateOrderStatus")]
        public async Task<IActionResult> UpdateOrderStatus([FromHeader(Name = "Grid-Service-Header-Token")] string token, [FromForm]  UpdateOrderStatus request)
        {

            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage))
                    });
                }

                if (!ValidateGridHeaderToken(token))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                var result = await _dataAccess.Grid_UpdateOrderStatus(request);
                if (result == Enum.ReturnSuccess)
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = true,
                        Message = Enum.ReturnSuccess.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }
                else
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = Enum.ReturnFailure.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }


            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });

            }
        }

        /// <summary>
        /// Updates the state of the subscriber.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="SubscriberID">The subscriber identifier.</param>
        /// <param name="state">The state.</param>
        /// <param name="error_reason">The error reason.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateSubscriberState")]
        public async Task<IActionResult> UpdateSubscriberState([FromHeader(Name = "Grid-Service-Header-Token")] string token, [FromRoute]int SubscriberID, [FromRoute] string state, [FromRoute] string error_reason)
        {

            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage))
                    });
                }

                if (!ValidateGridHeaderToken(token))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                var result = await _dataAccess.Grid_UpdateSubscriberState(SubscriberID, state, error_reason);
                if (result == Enum.ReturnSuccess)
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = true,
                        Message = Enum.ReturnSuccess.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }
                else
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = Enum.ReturnFailure.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }


            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });

            }
        }

        /// <summary>
        /// Updates the vendor.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="DeliveryinformationID">The deliveryinformation identifier.</param>
        /// <param name="shipnumber">The shipnumber.</param>
        /// <param name="vendor">The vendor.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateVendor")]
        public async Task<IActionResult> UpdateVendor([FromHeader(Name = "Grid-Service-Header-Token")] string token, [FromRoute] int DeliveryinformationID, [FromRoute] string shipnumber, [FromRoute] string vendor)
        {

            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage))
                    });
                }

                if (!ValidateGridHeaderToken(token))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                var result = await _dataAccess.Grid_UpdateVendor(DeliveryinformationID, shipnumber, vendor);
                if (result == Enum.ReturnSuccess)
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = true,
                        Message = Enum.ReturnSuccess.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }
                else
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = Enum.ReturnFailure.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }


            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });

            }
        }

        /// <summary>
        /// Updates the vendor tracking code.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="DeliveryinformationID">The deliveryinformation identifier.</param>
        /// <param name="TrackingCode">The tracking code.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateVendorTrackingCode")]
        public async Task<IActionResult> UpdateVendorTrackingCode([FromHeader(Name = "Grid-Service-Header-Token")] string token, [FromRoute] int DeliveryinformationID, [FromRoute] string TrackingCode)
        {

            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage))
                    });
                }

                if (!ValidateGridHeaderToken(token))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                var result = await _dataAccess.Grid_UpdateVendorTrackingCode(DeliveryinformationID, TrackingCode);
                if (result == Enum.ReturnSuccess)
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = true,
                        Message = Enum.ReturnSuccess.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }
                else
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = Enum.ReturnFailure.GetDescription(),
                        IsDomainValidationErrors = false
                    });
                }


            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });

            }
        }


        /// <summary>
        /// Validates the generic token.
        /// </summary>
        /// <param name="Token">The token.</param>
        /// <returns></returns>
        public bool ValidateGridHeaderToken(string Token)
        {
            var token = ConfigHelper.GetValueByKey(Enum.ConfigKeyForGridService, _iconfiguration).Results.ToString().Trim();
            return Token == token;
        }



    }
}