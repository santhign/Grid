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
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace OrderService.Controllers
{
    /// <summary>
    /// Change Request Controller class
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ChangeRequestController : ControllerBase
    {
        /// <summary>
        /// The iconfiguration
        /// </summary>
        private readonly IConfiguration _iconfiguration;
        /// <summary>
        /// The change request data access
        /// </summary>
        private readonly IChangeRequestDataAccess _changeRequestDataAccess;
        /// <summary>
        /// The message queue data access
        /// </summary>
        private readonly IMessageQueueDataAccess _messageQueueDataAccess;
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeRequestController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="changeRequestDataAccess">The change request data access.</param>
        /// <param name="messageQueueDataAccess">The message queue data access.</param>
        public ChangeRequestController(IConfiguration configuration, IChangeRequestDataAccess changeRequestDataAccess, IMessageQueueDataAccess messageQueueDataAccess)
        {
            _iconfiguration = configuration;
            _changeRequestDataAccess = changeRequestDataAccess;
            _messageQueueDataAccess = messageQueueDataAccess;
        }

        /// <summary>
        /// Removes the vas service.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("RemoveVasService/{mobileNumber}/{planId}")]
        public async Task<IActionResult> RemoveVasService([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] string mobileNumber, [FromRoute] int planId)
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

                //var orderAccess = _changeRequestDataAccess;//new ChangeRequestDataAccess(_iconfiguration);
                var helper = new AuthHelper(_iconfiguration);
                var tokenAuthResponse = await helper.AuthenticateCustomerToken(token);
                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;
                    var statusResponse =
                        await _changeRequestDataAccess.RemoveVasService(aTokenResp.CustomerID, mobileNumber, planId);

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
        [Route("BuyVasService/{mobileNumber}/{bundleId}/{quantity}")]
        public async Task<IActionResult> BuyVasService([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] string mobileNumber, [FromRoute] int bundleId, [FromRoute] int quantity)
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


                var helper = new AuthHelper(_iconfiguration);
                var tokenAuthResponse = await helper.AuthenticateCustomerToken(token);
                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;
                    var statusResponse =
                        await _changeRequestDataAccess.BuyVasService(aTokenResp.CustomerID, mobileNumber, bundleId, quantity);

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
        [Route("TerminationRequest/{mobileNumber}/{remark}")]
        public async Task<IActionResult> TerminationRequest([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] string mobileNumber, [FromRoute] string remark)
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


                var helper = new AuthHelper(_iconfiguration);

                var tokenAuthResponse = await helper.AuthenticateCustomerToken(token);
                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    var statusResponse = await _changeRequestDataAccess.TerminationOrSuspensionRequest(aTokenResp.CustomerID, mobileNumber, Core.Enums.RequestType.Termination.GetDescription(), remark);
                    var TorSresponse = (TerminationOrSuspensionResponse)statusResponse.Results;
                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        MessageBodyForCR msgBody = new MessageBodyForCR();
                        Dictionary<string, string> attribute = new Dictionary<string, string>();
                        string topicName = string.Empty, subject = string.Empty;
                        try
                        {
                            msgBody = await _messageQueueDataAccess.GetMessageBodyByChangeRequest(TorSresponse.ChangeRequestId);

                            topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration)
                            .Results.ToString().Trim();
                            //Ninad K : Need to update Subject
                            //subject = ConfigHelper.GetValueByKey(ConfigKey.SNS_Subject_CreateCustomer.GetDescription(), _iconfiguration)
                            //    .Results.ToString().Trim();
                            attribute.Add("EventType", Core.Enums.RequestType.Termination.GetDescription());
                            var pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, msgBody, attribute, null);
                            if (pushResult.Trim().ToUpper() == "OK")
                            {


                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = "ChangeRequest",
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.Termination.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody)
                                };
                                queueRequest.CreatedOn = DateTime.Now;
                                queueRequest.Status = 1;
                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                            else
                            {
                                MessageQueueRequest queueRequest = new MessageQueueRequest();
                                queueRequest.Source = "ChangeRequest";
                                queueRequest.NumberOfRetries = 1;
                                queueRequest.SNSTopic = topicName;
                                queueRequest.CreatedOn = DateTime.Now;
                                queueRequest.LastTriedOn = DateTime.Now;
                                queueRequest.PublishedOn = DateTime.Now;
                                queueRequest.MessageAttribute = Core.Enums.RequestType.Termination.GetDescription().ToString();
                                queueRequest.MessageBody = JsonConvert.SerializeObject(msgBody);
                                queueRequest.CreatedOn = DateTime.Now;
                                queueRequest.Status = 0;
                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                            MessageQueueRequest queueRequest = new MessageQueueRequest
                            {
                                Source = "ChangeRequest",
                                NumberOfRetries = 1,
                                SNSTopic = topicName,
                                CreatedOn = DateTime.Now,
                                LastTriedOn = DateTime.Now,
                                PublishedOn = DateTime.Now,
                                MessageAttribute = JsonConvert.SerializeObject(attribute),
                                MessageBody = JsonConvert.SerializeObject(msgBody)
                            };
                            queueRequest.CreatedOn = DateTime.Now;
                            queueRequest.Status = 1;
                            await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                        }

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
        [Route("SimReplacementRequest/{mobileNumber}")]
        public async Task<IActionResult> SimReplacementRequest([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] string mobileNumber)
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

                var helper = new AuthHelper(_iconfiguration);

                var tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    var statusResponse = await _changeRequestDataAccess.SimReplacementRequest(aTokenResp.CustomerID, mobileNumber);

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
        [Route("SuspensionRequest/{mobileNumber}/{remark}")]
        public async Task<IActionResult> SuspensionRequest([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] string mobileNumber, [FromRoute] string remark)
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

                var helper = new AuthHelper(_iconfiguration);
                var tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    var statusResponse = await _changeRequestDataAccess.TerminationOrSuspensionRequest(aTokenResp.CustomerID, mobileNumber, 
                        Core.Enums.RequestType.Suspension.GetDescription(), remark);
                    var TorSresponse = (TerminationOrSuspensionResponse) statusResponse.Results;
                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        MessageBodyForCR msgBody = new MessageBodyForCR();
                        Dictionary<string, string> attribute = new Dictionary<string, string>();
                        string topicName = string.Empty, subject = string.Empty;
                        try
                        {
                            msgBody = await _messageQueueDataAccess.GetMessageBodyByChangeRequest(TorSresponse.ChangeRequestId);

                            topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration)
                            .Results.ToString().Trim();
                           
                                attribute.Add("EventType", Core.Enums.RequestType.Suspension.GetDescription());
                                var pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, msgBody, attribute);
                            if (pushResult.Trim().ToUpper() == "OK")
                            {


                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = "ChangeRequest",
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.Suspension.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 1
                                };
                                
                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                            else
                            {
                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = "ChangeRequest",
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.Suspension.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 0
                                };
                                
                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                            MessageQueueRequest queueRequest = new MessageQueueRequest
                            {
                                Source = "ChangeRequest",
                                NumberOfRetries = 1,
                                SNSTopic = topicName,
                                CreatedOn = DateTime.Now,
                                LastTriedOn = DateTime.Now,
                                PublishedOn = DateTime.Now,
                                MessageAttribute = Core.Enums.RequestType.Suspension.GetDescription().ToString(),
                                MessageBody = JsonConvert.SerializeObject(msgBody),
                                Status = 0
                            };
                           
                            await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                        }

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
        [Route("ChangePhoneNumberRequest")]
        public async Task<IActionResult> ChangePhoneNumberRequest([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody]ChangePhoneRequest request)
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

                var helper = new AuthHelper(_iconfiguration);
                var tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;
                    request.CustomerId = aTokenResp.CustomerID;
                    var statusResponse =
                        await _changeRequestDataAccess.ChangePhoneRequest(request);

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
        /// Updates the cr shipping details.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="shippingDetails">The shipping details.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateCRShippingDetails")]
        public async Task<IActionResult> UpdateCRShippingDetails([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody]UpdateCRShippingDetailsRequest shippingDetails)
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

                var helper = new AuthHelper(_iconfiguration);
                var tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;
                    shippingDetails.CustomerID = aTokenResp.CustomerID;
                    var statusResponse =
                        await _changeRequestDataAccess.UpdateCRShippingDetails(shippingDetails);

                    if (statusResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
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