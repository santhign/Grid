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
    public class EmailController : ControllerBase
    {

        IConfiguration _iconfiguration;

        public EmailController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }


        [HttpPost]
        [Route("SendEmail")]
        public async Task<IActionResult> SendEmail([FromBody]NotificationEmail emaildetails)
        {
            try
            {

                if (emaildetails == null )
                {
                    Log.Error(StatusMessages.MissingRequiredFields);
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.MissingRequiredFields,
                        IsDomainValidationErrors = true
                    });
                }

                EmailNotificationDataAccess _emailNotificationDataAccess = new EmailNotificationDataAccess(_iconfiguration);

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage,
                    Result = await _emailNotificationDataAccess.SendEmail(emaildetails)

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
