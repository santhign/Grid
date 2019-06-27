using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Extensions;
using Core.Models;
using GRIDService.DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace GRIDService.Controllers
{
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
        public GRIDController(IConfiguration configuration, IGRIDDataAccess dataAccess)
        {
            _iconfiguration = configuration;
            _dataAccess = dataAccess;
            
        }

        [HttpPost]
        [Route("RemoveVasService/{mobileNumber}/{subscriptionId}")]
        public async Task<IActionResult> RemoveVasService([FromHeader(Name = "Grid-Service-Header-Token")] string token, [FromRoute] string mobileNumber, [FromRoute] int subscriptionId)
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
                var tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.DashBoard_RemoveVas);
                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;
                    var statusResponse =
                        await _changeRequestDataAccess.RemoveVasService(aTokenResp.CustomerID, mobileNumber, subscriptionId);
                    var removeVASResponse = (RemoveVASResponse)statusResponse.Results;
                    if (statusResponse.ResponseCode == (int)DbReturnValue.CreateSuccess)
                    {

                        MessageBodyForCR msgBody = new MessageBodyForCR();
                        Dictionary<string, string> attribute = new Dictionary<string, string>();
                        string topicName = string.Empty, subject = string.Empty;

                        try
                        {
                            msgBody = await _messageQueueDataAccess.GetMessageBodyByChangeRequest(removeVASResponse.ChangeRequestID);
                            if (msgBody == null || msgBody.ChangeRequestID == 0)
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
                        LogInfo.Warning(DbReturnValue.DuplicateCRExists.GetDescription());

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = DbReturnValue.DuplicateCRExists.GetDescription(),
                            IsDomainValidationErrors = false
                        });
                    }
                    else
                    {
                        LogInfo.Warning(DbReturnValue.NoRecords.GetDescription());

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
                    LogInfo.Warning(CommonErrors.ExpiredToken.GetDescription());
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