using Core.Enums;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NotificationService.DataAccess;
using NotificationService.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsController : ControllerBase
    {

        IConfiguration _iconfiguration;

        public SmsController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }

        [HttpPost]
        [Route("SendSMS")]
        public async Task<IActionResult> SendSMS([FromBody]Sms smsdetails)
        {
            try
            {

                if (smsdetails == null)
                {
                    Log.Error(StatusMessages.MissingRequiredFields);
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.MissingRequiredFields,
                        IsDomainValidationErrors = true
                    });
                }

                SmsDataAccess _smsDataAccess = new SmsDataAccess(_iconfiguration); 

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage,
                    Result = await _smsDataAccess.SendSMS(smsdetails)

                });
            }
            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

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
