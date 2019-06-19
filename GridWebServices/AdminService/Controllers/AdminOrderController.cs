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

namespace AdminService.Controllers
{
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
        [HttpGet("GetOrdersList")]
        public async Task<IActionResult> GetOrdersList([FromHeader(Name = "Grid-Authorization-Token")] string token, int ? deliveryStatus, DateTime ? fromDate, DateTime ? toDate )
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

                        

                        var orderList = await _adminOrderDataAccess.GetOrdersList(deliveryStatus,fromDate,toDate);

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

    }
}