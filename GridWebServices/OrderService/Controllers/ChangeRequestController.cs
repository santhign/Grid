﻿using System;
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
        /// <param name="token"></param>
        /// <param name="mobileNumber"></param>
        /// <param name="subscriptionId"></param>
        /// <param name="planId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("RemoveVasService/{mobileNumber}/{subscriptionId}/{planId}")]
        public async Task<IActionResult> RemoveVasService([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] string mobileNumber, [FromRoute] int subscriptionId,  [FromRoute] int planId)
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
                        await _changeRequestDataAccess.RemoveVasService(aTokenResp.CustomerID, mobileNumber, subscriptionId, planId);
                    var removeVASResponse = (RemoveVASResponse)statusResponse.Results;
                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {

                        MessageBodyForCR msgBody = new MessageBodyForCR();
                        Dictionary<string, string> attribute = new Dictionary<string, string>();
                        string topicName = string.Empty, subject = string.Empty;
                        
                        try
                        {
                            msgBody = await _messageQueueDataAccess.GetMessageBodyByChangeRequest(removeVASResponse.ChangeRequestID);
                            if(msgBody == null)
                            {
                                throw new NullReferenceException("message body is null for ChangeRequest (" + removeVASResponse.ChangeRequestID + ") for RemoveVAS Service API");
                            }
                            topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration)
                            .Results.ToString().Trim();
                            if (string.IsNullOrWhiteSpace(topicName))
                            {
                                throw new NullReferenceException("topicName is null for ChangeRequest (" + removeVASResponse.ChangeRequestID + ") for RemoveVAS Request Service API");
                            }
                            attribute.Add(EventTypeString.EventType, Core.Enums.RequestType.RemoveVAS.GetDescription());
                            var pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, msgBody, attribute);
                            if (pushResult.Trim().ToUpper() == "OK")
                            {
                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = Source.ChangeRequest,
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.RemoveVAS.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 1
                                };

                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                            else
                            {
                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = Source.ChangeRequest,
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.RemoveVAS.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 0
                                };

                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                            MessageQueueRequestException queueRequest = new MessageQueueRequestException
                            {
                                Source = Source.ChangeRequest,
                                NumberOfRetries = 1,
                                SNSTopic = string.IsNullOrWhiteSpace(topicName) ? null : topicName,
                                CreatedOn = DateTime.Now,
                                LastTriedOn = DateTime.Now,
                                PublishedOn = DateTime.Now,
                                MessageAttribute = Core.Enums.RequestType.RemoveVAS.GetDescription().ToString(),
                                MessageBody = msgBody != null ? JsonConvert.SerializeObject(msgBody) : null,
                                Status = 0,
                                Remark = "Error Occured in RemoveVASService",
                                Exception = new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical)


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
                    else if (statusResponse.ResponseCode == (int)DbReturnValue.DuplicateCRExists)
                    {
                        LogInfo.Error(DbReturnValue.DuplicateCRExists.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.DuplicateCRExists.GetDescription(),
                            IsDomainValidationErrors = false
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
                    var buyVASResponse = (BuyVASResponse)statusResponse.Results;
                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        //Ninad K : Message Publish code
                        MessageBodyForCR msgBody = new MessageBodyForCR();
                        Dictionary<string, string> attribute = new Dictionary<string, string>();
                        string topicName = string.Empty, subject = string.Empty;
                        try
                        {
                            topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration)
                            .Results.ToString().Trim();

                            if (string.IsNullOrWhiteSpace(topicName))
                            {
                                throw new NullReferenceException("topicName is null for ChangeRequest (" +  buyVASResponse.ChangeRequestID + ") for BuyVAS Request Service API");
                            }
                            msgBody = await _messageQueueDataAccess.GetMessageBodyByChangeRequest(buyVASResponse.ChangeRequestID);

                            if (msgBody == null)
                            {
                                throw new NullReferenceException("message body is null for ChangeRequest (" + buyVASResponse.ChangeRequestID + ") for BuyVAS Service API");
                            }

                            attribute.Add(EventTypeString.EventType, Core.Enums.RequestType.AddVAS.GetDescription());
                            var pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, msgBody, attribute);
                            if (pushResult.Trim().ToUpper() == "OK")
                            {
                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = Source.ChangeRequest,
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.AddVAS.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 1
                                };

                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                            else
                            {
                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = Source.ChangeRequest,
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.AddVAS.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 0
                                };

                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                            MessageQueueRequestException queueRequest = new MessageQueueRequestException
                            {
                                Source = Source.ChangeRequest,
                                NumberOfRetries = 1,
                                SNSTopic = string.IsNullOrWhiteSpace(topicName) ? null : topicName,
                                CreatedOn = DateTime.Now,
                                LastTriedOn = DateTime.Now,
                                PublishedOn = DateTime.Now,
                                MessageAttribute = Core.Enums.RequestType.AddVAS.GetDescription().ToString(),
                                MessageBody = msgBody != null ? JsonConvert.SerializeObject(msgBody) : null,
                                Status = 0,
                                Remark = "Error Occured in BuyVASService",
                                Exception = new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical)


                            };

                            await _messageQueueDataAccess.InsertMessageInMessageQueueRequestException(queueRequest);
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

                    var statusResponse = await _changeRequestDataAccess.TerminationOrSuspensionRequest(aTokenResp.CustomerID, mobileNumber, Core.Enums.RequestType.Terminate.GetDescription(), remark);
                    var TorSresponse = (TerminationOrSuspensionResponse)statusResponse.Results;
                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        MessageBodyForCR msgBody = new MessageBodyForCR();
                        Dictionary<string, string> attribute = new Dictionary<string, string>();
                        string topicName = string.Empty, subject = string.Empty;
                        try
                        {
                            msgBody = await _messageQueueDataAccess.GetMessageBodyByChangeRequest(TorSresponse.ChangeRequestId);
                            if (msgBody == null)
                            {
                                throw new NullReferenceException("message body is null for ChangeRequest (" + TorSresponse.ChangeRequestId + ") for Termination Request Service API");
                            }
                            topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration)
                            .Results.ToString().Trim();
                            if (string.IsNullOrWhiteSpace(topicName))
                            {
                                throw new NullReferenceException("topicName is null for ChangeRequest (" + TorSresponse.ChangeRequestId + ") for Termination Request Service API");
                            }
                            attribute.Add(EventTypeString.EventType, Core.Enums.RequestType.Terminate.GetDescription());
                            var pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, msgBody, attribute);
                            if (pushResult.Trim().ToUpper() == "OK")
                            {


                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = Source.ChangeRequest,
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.Terminate.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 1
                                };
                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                            else
                            {
                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = Source.ChangeRequest,
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.Terminate.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 0
                                };
                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                            MessageQueueRequestException queueRequest = new MessageQueueRequestException
                            {
                                Source = Source.ChangeRequest,
                                NumberOfRetries = 1,
                                SNSTopic = string.IsNullOrWhiteSpace(topicName) ? null : topicName,
                                CreatedOn = DateTime.Now,
                                LastTriedOn = DateTime.Now,
                                PublishedOn = DateTime.Now,
                                MessageAttribute = Core.Enums.RequestType.Terminate.GetDescription().ToString(),
                                MessageBody = msgBody != null ? JsonConvert.SerializeObject(msgBody) : null,
                                Status = 0,
                                Remark = "Error Occured in TerminationRequestService",
                                Exception = new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical)


                            };

                            await _messageQueueDataAccess.InsertMessageInMessageQueueRequestException(queueRequest);
                        }

                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = statusResponse
                        });
                    }
                    else if (statusResponse.ResponseCode == (int)DbReturnValue.DuplicateCRExists)
                    {
                        LogInfo.Error(DbReturnValue.DuplicateCRExists.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.DuplicateCRExists.GetDescription(),
                            IsDomainValidationErrors = false
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
                    else if (statusResponse.ResponseCode == (int)DbReturnValue.DuplicateCRExists)
                    {
                        LogInfo.Error(DbReturnValue.DuplicateCRExists.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.DuplicateCRExists.GetDescription(),
                            IsDomainValidationErrors = false
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
                        Core.Enums.RequestType.Suspend.GetDescription(), remark);
                    var TorSresponse = (TerminationOrSuspensionResponse) statusResponse.Results;
                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        MessageBodyForCR msgBody = new MessageBodyForCR();
                        Dictionary<string, string> attribute = new Dictionary<string, string>();
                        string topicName = string.Empty, subject = string.Empty;
                        try
                        {
                            msgBody = await _messageQueueDataAccess.GetMessageBodyByChangeRequest(TorSresponse.ChangeRequestId);
                            if (msgBody == null)
                            {
                                throw new NullReferenceException("message body is null for ChangeRequest (" + TorSresponse.ChangeRequestId + ") for Suspension Request Service API");
                            }
                            topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration)
                            .Results.ToString().Trim();
                            if (string.IsNullOrWhiteSpace(topicName))
                            {
                                throw new NullReferenceException("topicName is null for ChangeRequest (" + TorSresponse.ChangeRequestId + ") for Suspension Request Service API");
                            }

                            attribute.Add(EventTypeString.EventType, Core.Enums.RequestType.Suspend.GetDescription());
                                var pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, msgBody, attribute);
                            if (pushResult.Trim().ToUpper() == "OK")
                            {


                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = Source.ChangeRequest,
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.Suspend.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 1
                                };
                                
                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                            else
                            {
                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = Source.ChangeRequest,
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.Suspend.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 0
                                };
                                
                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                            MessageQueueRequestException queueRequest = new MessageQueueRequestException
                            {
                                Source = Source.ChangeRequest,
                                NumberOfRetries = 1,
                                SNSTopic = string.IsNullOrWhiteSpace(topicName) ? null : topicName,
                                CreatedOn = DateTime.Now,
                                LastTriedOn = DateTime.Now,
                                PublishedOn = DateTime.Now,
                                MessageAttribute = Core.Enums.RequestType.Suspend.GetDescription().ToString(),
                                MessageBody = msgBody != null ? JsonConvert.SerializeObject(msgBody) : null,
                                Status = 0,
                                Remark = "Error Occured in SuspensionRequest Service",
                                Exception = new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical)


                            };

                            await _messageQueueDataAccess.InsertMessageInMessageQueueRequestException(queueRequest);
                        }

                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = statusResponse
                        });
                    }
                    else if (statusResponse.ResponseCode == (int)DbReturnValue.DuplicateCRExists)
                    {
                        LogInfo.Error(DbReturnValue.DuplicateCRExists.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.DuplicateCRExists.GetDescription(),
                            IsDomainValidationErrors = false
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

        [HttpPost]
        [Route("UnsuspensionRequest/{mobileNumber}/{remark}")]
        public async Task<IActionResult> UnsuspensionRequest([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] string mobileNumber, [FromRoute] string remark)
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
                        Core.Enums.RequestType.UnSuspend.GetDescription(), remark);
                    var TorSresponse = (TerminationOrSuspensionResponse)statusResponse.Results;
                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        MessageBodyForCR msgBody = new MessageBodyForCR();
                        Dictionary<string, string> attribute = new Dictionary<string, string>();
                        string topicName = string.Empty, subject = string.Empty;
                        try
                        {
                            msgBody = await _messageQueueDataAccess.GetMessageBodyByChangeRequest(TorSresponse.ChangeRequestId);
                            if (msgBody == null)
                            {
                                throw new NullReferenceException("message body is null for ChangeRequest (" + TorSresponse.ChangeRequestId + ") for UnSuspension Request Service API");
                            }
                            topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration)
                            .Results.ToString().Trim();

                            if (string.IsNullOrWhiteSpace(topicName))
                            {
                                throw new NullReferenceException("topicName is null for ChangeRequest (" + TorSresponse.ChangeRequestId + ") for UnSuspension Request Service API");
                            }

                            attribute.Add(EventTypeString.EventType, Core.Enums.RequestType.UnSuspend.GetDescription());
                            var pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, msgBody, attribute);
                            if (pushResult.Trim().ToUpper() == "OK")
                            {
                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = Source.ChangeRequest,
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.UnSuspend.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 1
                                };

                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                            else
                            {
                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = Source.ChangeRequest,
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.UnSuspend.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 0
                                };

                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                            MessageQueueRequestException queueRequest = new MessageQueueRequestException
                            {
                                Source = Source.ChangeRequest,
                                NumberOfRetries = 1,
                                SNSTopic = string.IsNullOrWhiteSpace(topicName) ? null : topicName,
                                CreatedOn = DateTime.Now,
                                LastTriedOn = DateTime.Now,
                                PublishedOn = DateTime.Now,
                                MessageAttribute = Core.Enums.RequestType.UnSuspend.GetDescription().ToString(),
                                MessageBody = msgBody != null ? JsonConvert.SerializeObject(msgBody) : null,
                                Status = 0,
                                Remark = "Error Occured in Unsuspension Service",
                                Exception = new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical)


                            };

                            await _messageQueueDataAccess.InsertMessageInMessageQueueRequestException(queueRequest);
                        }

                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = statusResponse
                        });
                    }
                    else if (statusResponse.ResponseCode == (int)DbReturnValue.DuplicateCRExists)
                    {
                        LogInfo.Error(DbReturnValue.DuplicateCRExists.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.DuplicateCRExists.GetDescription(),
                            IsDomainValidationErrors = false
                        });
                    }
                    else if (statusResponse.ResponseCode == (int)DbReturnValue.UnSuspensionValidation)
                    {
                        LogInfo.Error(DbReturnValue.UnSuspensionValidation.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.UnSuspensionValidation.GetDescription(),
                            IsDomainValidationErrors = false
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
        /// Buys the vas service.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="bundleId">The bundle identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("BuySharedVasService/{bundleId}")]
        public async Task<IActionResult> BuySharedVasService([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] int bundleId)
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
                        await _changeRequestDataAccess.BuySharedService(aTokenResp.CustomerID, bundleId);
                    var buySharedVASResponse = (BuyVASResponse)statusResponse.Results;
                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {
                        //Ninad K : Message Publish code
                        MessageBodyForCR msgBody = new MessageBodyForCR();
                        Dictionary<string, string> attribute = new Dictionary<string, string>();
                        string topicName = string.Empty, subject = string.Empty;
                        try
                        {
                            topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration)
                            .Results.ToString().Trim();
                            if (string.IsNullOrWhiteSpace(topicName))
                            {
                                throw new NullReferenceException("topicName is null for ChangeRequest (" + buySharedVASResponse.ChangeRequestID + ") for BuyShared VAS Request Service API");
                            }
                            msgBody = await _messageQueueDataAccess.GetMessageBodyByChangeRequest(buySharedVASResponse.ChangeRequestID);

                            if (msgBody == null)
                            {
                                throw new NullReferenceException("message body is null for ChangeRequest (" + buySharedVASResponse.ChangeRequestID + ") for BuyShared VAS Request Service API");
                            }


                            attribute.Add(EventTypeString.EventType, Core.Enums.RequestType.AddVAS.GetDescription());
                            var pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, msgBody, attribute);
                            if (pushResult.Trim().ToUpper() == "OK")
                            {
                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = Source.ChangeRequest,
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.AddVAS.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 1
                                };

                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                            else
                            {
                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = Source.ChangeRequest,
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.AddVAS.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 0
                                };

                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                            MessageQueueRequestException queueRequest = new MessageQueueRequestException
                            {
                                Source = Source.ChangeRequest,
                                NumberOfRetries = 1,
                                SNSTopic = string.IsNullOrWhiteSpace(topicName) ? null : topicName,
                                CreatedOn = DateTime.Now,
                                LastTriedOn = DateTime.Now,
                                PublishedOn = DateTime.Now,
                                MessageAttribute = Core.Enums.RequestType.AddVAS.GetDescription().ToString(),
                                MessageBody = msgBody != null ? JsonConvert.SerializeObject(msgBody) : null,
                                Status = 0,
                                Remark = "Error Occured in BuySharedVASService",
                                Exception = new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical)


                            };

                            await _messageQueueDataAccess.InsertMessageInMessageQueueRequestException(queueRequest);
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
        /// Remove Shared VAS Service
        /// </summary>
        /// <param name="token"></param>
        /// <param name="accountSubscriptionId"></param>
        /// <param name="planId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("RemoveSharedVasService/{accountSubscriptionId}/{planId}")]
        public async Task<IActionResult> RemoveSharedVasService([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] int accountSubscriptionId, [FromRoute] int planId)
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
                        await _changeRequestDataAccess.RemoveSharedVasService(aTokenResp.CustomerID, accountSubscriptionId, planId);
                    var removeVASResponse = (RemoveVASResponse)statusResponse.Results;
                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {

                        MessageBodyForCR msgBody = new MessageBodyForCR();
                        Dictionary<string, string> attribute = new Dictionary<string, string>();
                        string topicName = string.Empty, subject = string.Empty;
                        
                        try
                        {
                            msgBody = await _messageQueueDataAccess.GetMessageBodyByChangeRequest(removeVASResponse.ChangeRequestID);
                            if (msgBody == null)
                            {
                                throw new NullReferenceException("message body is null for ChangeRequest (" + removeVASResponse.ChangeRequestID + ") for Remove Shared VAS Request Service API");
                            }
                            topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration)
                            .Results.ToString().Trim();
                            if (string.IsNullOrWhiteSpace(topicName))
                            {
                                throw new NullReferenceException("topicName is null for ChangeRequest (" + removeVASResponse.ChangeRequestID + ") for Remove Shared VAS Request Service API");
                            }
                            attribute.Add(EventTypeString.EventType, Core.Enums.RequestType.RemoveVAS.GetDescription());
                            var pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, msgBody, attribute);
                            if (pushResult.Trim().ToUpper() == "OK")
                            {
                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = Source.ChangeRequest,
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.RemoveVAS.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 1
                                };

                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                            else
                            {
                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = Source.ChangeRequest,
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.RemoveVAS.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 0
                                };

                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                            MessageQueueRequestException queueRequest = new MessageQueueRequestException
                            {
                                Source = Source.ChangeRequest,
                                NumberOfRetries = 1,
                                SNSTopic = topicName,
                                CreatedOn = DateTime.Now,
                                LastTriedOn = DateTime.Now,
                                PublishedOn = DateTime.Now,
                                MessageAttribute = Core.Enums.RequestType.RemoveVAS.GetDescription().ToString(),
                                MessageBody = JsonConvert.SerializeObject(msgBody),
                                Status = 0,
                                Remark = "Error Occured in RemoveVASService",
                                Exception = ex.StackTrace.ToString()


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
                    else if (statusResponse.ResponseCode == (int)DbReturnValue.DuplicateCRExists)
                    {
                        LogInfo.Error(DbReturnValue.DuplicateCRExists.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.DuplicateCRExists.GetDescription(),
                            IsDomainValidationErrors = false
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

        [HttpPut]
        [Route("UpdatePlanService/{mobileNumber}/{bundleId}")]
        public async Task<IActionResult> UpdatePlanService([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] string mobileNumber, [FromRoute] int bundleId)
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
                        await _changeRequestDataAccess.ChangePlanService(aTokenResp.CustomerID, mobileNumber, bundleId);
                    var changePlanResponse = (RemoveVASResponse)statusResponse.Results;
                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {

                        MessageBodyForCR msgBody = new MessageBodyForCR();
                        Dictionary<string, string> attribute = new Dictionary<string, string>();
                        string topicName = string.Empty, subject = string.Empty;

                        try
                        {
                            msgBody = await _messageQueueDataAccess.GetMessageBodyByChangeRequest(changePlanResponse.ChangeRequestID);
                            if (msgBody == null)
                            {
                                throw new NullReferenceException("message body is null for ChangeRequest (" + changePlanResponse.ChangeRequestID + ") for Change Plan Service Request Service API");
                            }
                            topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration)
                            .Results.ToString().Trim();
                            if (string.IsNullOrWhiteSpace(topicName))
                            {
                                throw new NullReferenceException("topicName is null for ChangeRequest (" + changePlanResponse.ChangeRequestID + ") for Change Plan Service Request Service API");
                            }
                            attribute.Add(EventTypeString.EventType, Core.Enums.RequestType.ChangePlan.GetDescription());
                            var pushResult = await _messageQueueDataAccess.PublishMessageToMessageQueue(topicName, msgBody, attribute);
                            if (pushResult.Trim().ToUpper() == "OK")
                            {
                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = Source.ChangeRequest,
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.ChangePlan.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 1
                                };

                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                            else
                            {
                                MessageQueueRequest queueRequest = new MessageQueueRequest
                                {
                                    Source = Source.ChangeRequest,
                                    NumberOfRetries = 1,
                                    SNSTopic = topicName,
                                    CreatedOn = DateTime.Now,
                                    LastTriedOn = DateTime.Now,
                                    PublishedOn = DateTime.Now,
                                    MessageAttribute = Core.Enums.RequestType.ChangePlan.GetDescription().ToString(),
                                    MessageBody = JsonConvert.SerializeObject(msgBody),
                                    Status = 0
                                };

                                await _messageQueueDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                            MessageQueueRequestException queueRequest = new MessageQueueRequestException
                            {
                                Source = Source.ChangeRequest,
                                NumberOfRetries = 1,
                                SNSTopic = topicName,
                                CreatedOn = DateTime.Now,
                                LastTriedOn = DateTime.Now,
                                PublishedOn = DateTime.Now,
                                MessageAttribute = Core.Enums.RequestType.ChangePlan.GetDescription().ToString(),
                                MessageBody = JsonConvert.SerializeObject(msgBody),
                                Status = 0,
                                Remark = "Error Occured in Update Plan",
                                Exception = ex.StackTrace.ToString()


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
                    else if (statusResponse.ResponseCode == (int)DbReturnValue.DuplicateCRExists)
                    {
                        LogInfo.Error(DbReturnValue.DuplicateCRExists.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.DuplicateCRExists.GetDescription(),
                            IsDomainValidationErrors = false
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

    }
}