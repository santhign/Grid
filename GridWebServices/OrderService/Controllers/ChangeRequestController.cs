using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.DataAccess;
using OrderService.Helpers;
using Core.Models;
using Core.Enums;
using Core.Extensions;
using InfrastructureService;
using Core.Helpers;
using System.IO;
using OrderService.Enums;
using System.Net;

namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChangeRequestController : ControllerBase
    {
        readonly IConfiguration _iconfiguration;

        public ChangeRequestController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }

        /// <summary>
        /// Removes the vas service.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("RemoveVasService")]
        public async Task<IActionResult> RemoveVasService(string token, string mobileNumber, int planId)
        {

            try
            {
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

                var orderAccess = new ChangeRequestDataAccess(_iconfiguration);
                var tokenAuthResponse = await orderAccess.AuthenticateCustomerToken(token);
                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;
                    var statusResponse =
                        await orderAccess.RemoveVasService(aTokenResp.CustomerID, mobileNumber, planId);

                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = statusResponse
                        });
                    }
                    else
                    {
                        LogInfo.Error(DbReturnValue.NoRecords.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.UpdationFailed.GetDescription(),
                            IsDomainValidationErrors = false
                        });
                    }
                }
                else
                {
                    //Token expired
                    LogInfo.Error(CommonErrors.ExpiredToken.GetDescription());
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = DbReturnValue.TokenExpired.GetDescription(),
                        IsDomainValidationErrors = true
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
        /// Buys the vas service.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <param name="bundleId">The bundle identifier.</param>
        /// <param name="quantity">The quantity.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("BuyVasService")]
        public async Task<IActionResult> BuyVasService(string token, string mobileNumber, int bundleId, int quantity)
        {

            try
            {
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

                var changeRequestDataAccess = new ChangeRequestDataAccess(_iconfiguration);
                var tokenAuthResponse = await changeRequestDataAccess.AuthenticateCustomerToken(token);
                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;
                    var statusResponse =
                        await changeRequestDataAccess.BuyVasService(aTokenResp.CustomerID, mobileNumber, bundleId, quantity);

                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = statusResponse
                        });
                    }
                    else
                    {
                        LogInfo.Error(DbReturnValue.NoRecords.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.UpdationFailed.GetDescription(),
                            IsDomainValidationErrors = false
                        });
                    }
                }
                else
                {
                    //Token expired
                    LogInfo.Error(CommonErrors.ExpiredToken.GetDescription());
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = DbReturnValue.TokenExpired.GetDescription(),
                        IsDomainValidationErrors = true
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
        /// Subscribers the termination request.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <param name="remark">The remark.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SubscriberTerminationRequest/{token}/{mobileNumber}")]
        public async Task<IActionResult> SubscriberTerminationRequest(string token, string mobileNumber, string remark)
        {

            try
            {
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

                var changeRequestDataAccess = new ChangeRequestDataAccess(_iconfiguration);

                var tokenAuthResponse = await changeRequestDataAccess.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    var statusResponse = await changeRequestDataAccess.TerminationOrSuspensionRequest(aTokenResp.CustomerID, mobileNumber, Core.Enums.RequestType.Termination.GetDescription(), remark);

                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = statusResponse
                        });
                    }
                    else
                    {
                        LogInfo.Error(DbReturnValue.NoRecords.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.UpdationFailed.GetDescription(),
                            IsDomainValidationErrors = false
                        });
                    }

                }
                else
                {
                    // token auth failure
                    LogInfo.Error(DbReturnValue.TokenAuthFailed.GetDescription());

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = DbReturnValue.TokenAuthFailed.GetDescription(),
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
        /// Subscribers the sim replacement request.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SubscriberSimReplacementRequest/{token}/{mobileNumber}")]
        public async Task<IActionResult> SubscriberSimReplacementRequest(string token, string mobileNumber)
        {

            try
            {
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

                var changeRequestDataAccess = new ChangeRequestDataAccess(_iconfiguration);

                var tokenAuthResponse = await changeRequestDataAccess.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    var statusResponse = await changeRequestDataAccess.SimReplacementRequest(aTokenResp.CustomerID, mobileNumber);

                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = statusResponse
                        });
                    }
                    else
                    {
                        LogInfo.Error(DbReturnValue.NoRecords.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.UpdationFailed.GetDescription(),
                            IsDomainValidationErrors = false
                        });
                    }

                }
                else
                {
                    // token auth failure
                    LogInfo.Error(DbReturnValue.TokenAuthFailed.GetDescription());

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = DbReturnValue.TokenAuthFailed.GetDescription(),
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
        /// Subscribers the suspension request.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <param name="remark">The remark.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SubscriberSuspensionRequest/{token}/{mobileNumber}")]
        public async Task<IActionResult> SubscriberSuspensionRequest(string token, string mobileNumber, string remark)
        {

            try
            {
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

                var changeRequestDataAccess = new ChangeRequestDataAccess(_iconfiguration);

                var tokenAuthResponse = await changeRequestDataAccess.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    var statusResponse = await changeRequestDataAccess.TerminationOrSuspensionRequest(aTokenResp.CustomerID, mobileNumber, Core.Enums.RequestType.Suspension.GetDescription(), remark);

                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = statusResponse
                        });
                    }
                    else
                    {
                        LogInfo.Error(DbReturnValue.NoRecords.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.UpdationFailed.GetDescription(),
                            IsDomainValidationErrors = false
                        });
                    }

                }
                else
                {
                    // token auth failure
                    LogInfo.Error(DbReturnValue.TokenAuthFailed.GetDescription());

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = DbReturnValue.TokenAuthFailed.GetDescription(),
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
        /// Changes the phone number request.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("ChangePhoneNumberRequest/{token}")]
        public async Task<IActionResult> ChangePhoneNumberRequest([FromRoute]string token, [FromBody]ChangePhoneRequest request)
        {
            try
            {
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

                var changeRequestDataAccess = new ChangeRequestDataAccess(_iconfiguration);

                var tokenAuthResponse = await changeRequestDataAccess.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;
                    request.CustomerId = aTokenResp.CustomerID;
                    var statusResponse =
                        await changeRequestDataAccess.ChangePhoneRequest(request);

                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = statusResponse
                        });
                    }
                    else
                    {
                        LogInfo.Error(DbReturnValue.NoRecords.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.UpdationFailed.GetDescription(),
                            IsDomainValidationErrors = false
                        });
                    }
                }
                else
                {
                    // token auth failure
                    LogInfo.Error(DbReturnValue.TokenAuthFailed.GetDescription());

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = DbReturnValue.TokenAuthFailed.GetDescription(),
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