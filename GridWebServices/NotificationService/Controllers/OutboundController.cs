﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Models;
using NotificationService.OutboundHelper;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Core.Models;
using Core.Enums;
using Core.Extensions;
using Core.Helpers;
using Core.DataAccess;
using InfrastructureService;
using Serilog;
using System.Collections.Generic;

namespace NotificationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutboundController : ControllerBase
    {
        IConfiguration _iconfiguration;

        public OutboundController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }

        /// <summary>
        /// This will send email
        /// </summary>
        /// <param name="emailSubscribers"></param>
        /// <returns>send status</returns>
        /// POST: api/SendEmail
        ///Body: 
        ///{
        ///  "Subject": "Email",
        ///  "Content": "email notify",
        ///  "BccAddress": "chinnu.rajan@gmail.com",
        ///  "EmailDetails":[ {
        ///        "Userid": 1,
        ///        "FName": "Chinnu",
        ///        "EMAIL": "chinnu.rajan@gmail.com",
        ///        "Param1": "",
        ///        "Param2": "",
        ///        "Param3": "",
        ///        "Param4": "",
        ///        "Param5": "",
        ///        "Param6": "",
        ///        "Param7": "",
        ///        "Param8": "",
        ///        "Param9": "",
        ///        "Param10": ""
        ///    }]
        ///    }
        [NonAction]
        [Route("SendEmail")]
        public async Task<IActionResult> PushEmail([FromBody]NotificationMessage emailSubscribers)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    LogInfo.Error(StatusMessages.DomainValidationError);
                    new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage))
                    };
                }

                OutboundEmail _email = new OutboundEmail();

                ConfigDataAccess _configAccess = new ConfigDataAccess(_iconfiguration);

                DatabaseResponse forgotPasswordMsgTemplate = await _configAccess.GetEmailNotificationTemplate(NotificationEvent.ForgetPassword.ToString());

                EmailTemplate template = (EmailTemplate)forgotPasswordMsgTemplate.Results;

                var responses = await _email.SendEmail(emailSubscribers, _iconfiguration,template);
             

                foreach (Mandrill.Model.MandrillSendMessageResponse response in responses)
                {
                    //DatabaseResponse notificationLogResponse = await _configAccess.CreateEMailNotificationLog(new NotificationLog { Status=response.Status.ToString(), Email=response.Email });
                }               

                // log notificaiton in db
             

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage
                });
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
        /// This will send SMS
        /// </summary>
        /// <param name="_smsdata"></param>
        /// <returns>send status</returns>
        /// POST: api/SendSMS
        ///Body: 
        ///{
        ///  "PhoneNumber":"1234","SMSText":"Ok","ToPhoneNumber":"34567","PostData":"xyz"
        /// }
        [NonAction]
        [Route("SendSMS")]
        public async Task<IActionResult> SendSMS([FromBody]TextMessage _smsdata)
        {
            try
            {
                OutboundSMS _SMS = new OutboundSMS();
                string response = await _SMS.SendSMSNotification(_smsdata, _iconfiguration);

                if (response != "failure")
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = true,
                        Message = StatusMessages.SuccessMessage

                    });
                }
                else
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.FailureMessage,
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
