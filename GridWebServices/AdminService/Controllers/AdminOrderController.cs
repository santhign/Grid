using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Core.Extensions;
using InfrastructureService;
using AdminService.DataAccess;
using AdminService.Models;
using Microsoft.Extensions.Configuration;
using AdminService.DataAccess.Interfaces;
using InfrastructureService.MessageQueue;
using Newtonsoft.Json;

namespace AdminService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminOrderController : Controller
    {
        /// <summary>
        /// The iconfiguration
        /// </summary>
        private readonly IConfiguration _iconfiguration;

        /// <summary>
        /// The admin order data access
        /// </summary>
        private readonly IAdminOrderDataAccess _adminOrderDataAccess;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminOrderController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="adminOrderDataAccess">The admin order data access.</param>
        public AdminOrderController(IConfiguration configuration, IAdminOrderDataAccess adminOrderDataAccess)
        {
            _iconfiguration = configuration;
            _adminOrderDataAccess = adminOrderDataAccess;
        }

        /// <summary>
        /// Gets the orders list.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="deliveryStatus">The delivery status.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        [HttpGet("GetOrdersListForNRIC")]
        public async Task<IActionResult> GetOrdersList([FromHeader(Name = "Grid-Authorization-Token")] string token, string deliveryStatus, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AdminUsersDataAccess _adminUsersDataAccess = new AdminUsersDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _adminUsersDataAccess.AuthenticateAdminUserToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        if (!ModelState.IsValid)
                        {
                            return StatusCode((int)HttpStatusCode.OK,
                                new OperationResponse
                                {
                                    HasSucceeded = false,
                                    IsDomainValidationErrors = true,
                                    Message = string.Join("; ", ModelState.Values
                                                    .SelectMany(x => x.Errors)
                                                    .Select(x => x.ErrorMessage))
                                });
                        }
                        int? deliveryStatusNumber = null;
                        if(!string.IsNullOrWhiteSpace(deliveryStatus))
                        {
                            if(deliveryStatus.Trim().ToLower() == IDVerificationStatus.PendingVerification.GetDescription().Trim().ToLower())                            
                                deliveryStatusNumber = 0;
                            else if (deliveryStatus.Trim().ToLower() == IDVerificationStatus.AcceptedVerification.GetDescription().Trim().ToLower())
                                deliveryStatusNumber = 1;
                            else if (deliveryStatus.Trim().ToLower() == IDVerificationStatus.RejectedVerification.GetDescription().Trim().ToLower())
                                deliveryStatusNumber = 2;

                        }
                        
                        var orderList = await _adminOrderDataAccess.GetOrdersList(deliveryStatusNumber, fromDate, toDate);

                        if (orderList == null || orderList.Count == 0)
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NotExists)

                            });
                        }
                        else
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = orderList

                            });
                        }

                    }
                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = EnumExtensions.GetDescription(DbReturnValue.TokenExpired),
                            IsDomainValidationErrors = true
                        });

                    }

                }

                else
                {
                    // token auth failure
                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
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
        /// Gets the order details.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="orderID">The order identifier.</param>
        /// <returns></returns>
        [HttpGet("GetOrderDetailsForNRIC/{orderID}")]
        public async Task<IActionResult> GetOrderDetails([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] int orderID)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AdminUsersDataAccess _adminUsersDataAccess = new AdminUsersDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _adminUsersDataAccess.AuthenticateAdminUserToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        if (!ModelState.IsValid)
                        {
                            return StatusCode((int)HttpStatusCode.OK,
                                new OperationResponse
                                {
                                    HasSucceeded = false,
                                    IsDomainValidationErrors = true,
                                    Message = string.Join("; ", ModelState.Values
                                                    .SelectMany(x => x.Errors)
                                                    .Select(x => x.ErrorMessage))
                                });
                        }



                        var orderList = await _adminOrderDataAccess.GetOrderDetails(orderID);

                        if (orderList == null || orderList.OrderID == 0)
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NotExists)

                            });
                        }
                        else
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = orderList

                            });
                        }

                    }
                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = EnumExtensions.GetDescription(DbReturnValue.TokenExpired),
                            IsDomainValidationErrors = true
                        });

                    }

                }

                else
                {
                    // token auth failure
                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
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

        [HttpGet("UpdateNRICDetails")]
        public async Task<IActionResult> UpdateNRICDetails([FromHeader(Name = "Grid-Authorization-Token")] string token,  NRICDetailsRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AdminUsersDataAccess _adminUsersDataAccess = new AdminUsersDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _adminUsersDataAccess.AuthenticateAdminUserToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        if (!ModelState.IsValid)
                        {
                            return StatusCode((int)HttpStatusCode.OK,
                                new OperationResponse
                                {
                                    HasSucceeded = false,
                                    IsDomainValidationErrors = true,
                                    Message = string.Join("; ", ModelState.Values
                                                    .SelectMany(x => x.Errors)
                                                    .Select(x => x.ErrorMessage))
                                });
                        }

                        int deliveryStatusNumber = 0;
                        if (!string.IsNullOrWhiteSpace(request.IDVerificationStatus))
                        {
                            if (request.IDVerificationStatus.Trim().ToLower() == IDVerificationStatus.PendingVerification.GetDescription().Trim().ToLower())
                                deliveryStatusNumber = 0;
                            else if (request.IDVerificationStatus.Trim().ToLower() == IDVerificationStatus.AcceptedVerification.GetDescription().Trim().ToLower())
                                deliveryStatusNumber = 1;
                            else if (request.IDVerificationStatus.Trim().ToLower() == IDVerificationStatus.RejectedVerification.GetDescription().Trim().ToLower())
                                deliveryStatusNumber = 2;
                            else
                            {
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(DbReturnValue.TokenExpired),
                                    IsDomainValidationErrors = true
                                });
                            }

                        }
                        else
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.TokenExpired),
                                IsDomainValidationErrors = true
                            });
                        }

                        var authToken = (AuthTokenResponse)tokenAuthResponse.Results;
                        var returnResponse = await _adminOrderDataAccess.UpdateNRICDetails(authToken.CustomerID, deliveryStatusNumber,request);

                        if (returnResponse.ResponseCode == (int)DbReturnValue.UpdateSuccessSendEmail)
                        {
                            var emailDetails = (EmailResponse)returnResponse.Results;
                            //Sending message start
                            // Send email to customer email                            ConfigDataAccess _configAccess = new ConfigDataAccess(_iconfiguration);
                            DatabaseResponse registrationResponse = await _adminOrderDataAccess.GetEmailNotificationTemplate(emailDetails.VerificationStatus == 2 ? NotificationEvent.ICValidationReject.GetDescription() : NotificationEvent.ICValidationChange.GetDescription());

                            var notificationMessage = MessageHelper.GetMessage(emailDetails.Email, emailDetails.Name, emailDetails.VerificationStatus == 2 ? NotificationEvent.ICValidationReject.GetDescription() : NotificationEvent.ICValidationChange.GetDescription(),
                           ((EmailTemplate)registrationResponse.Results).TemplateName,
                       _iconfiguration);
                            var notificationResponse = await _adminOrderDataAccess.GetConfiguration(ConfiType.Notification.ToString());


                            MiscHelper parser = new MiscHelper();
                            var notificationConfig = parser.GetNotificationConfig((List<Dictionary<string, string>>)notificationResponse.Results);

                            Publisher customerNotificationPublisher = new Publisher(_iconfiguration, notificationConfig.SNSTopic);
                            await customerNotificationPublisher.PublishAsync(notificationMessage);
                            try
                            {
                                DatabaseResponse notificationLogResponse = await _adminOrderDataAccess.CreateEMailNotificationLogForDevPurpose(
                                    new NotificationLogForDevPurpose
                                    {
                                        EventType = NotificationEvent.OrderSuccess.ToString(),
                                        Message = JsonConvert.SerializeObject(notificationMessage)

                                    });

                            }
                            catch (Exception ex)
                            {
                                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                            }
                            //Sending message Stop
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = null

                            });
                        }
                        else if (returnResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = null

                            });
                        }
                        else
                        {
                            LogInfo.Error("UpdateNRICDetails failed for "+ request.OrderID + " Order Id "+ DbReturnValue.UpdationFailed);
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.UpdationFailed),
                                IsDomainValidationErrors = true
                            });
                        }

                    }
                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = EnumExtensions.GetDescription(DbReturnValue.TokenExpired),
                            IsDomainValidationErrors = true
                        });

                    }

                }

                else
                {
                    // token auth failure
                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
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

    }
}