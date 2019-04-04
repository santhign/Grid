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
    public class PostcodeController : ControllerBase
    {
        IConfiguration _iconfiguration;

        public PostcodeController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }

        [HttpPost]
        [Route("ValidatePostcode")]
        public async Task<IActionResult> ValidatePostcode([FromBody]Postcode postcodedetails)
        {
            try
            {

                if (postcodedetails.PostcodeNumber.ToString().Length == 0)
                {
                    Log.Error(StatusMessages.MissingRequiredFields);
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.MissingRequiredFields,
                        IsDomainValidationErrors = true
                    });
                }

                PostcodeDataAccess _PostcodeDataAccess = new PostcodeDataAccess(_iconfiguration); 

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage,
                    Result = await _PostcodeDataAccess.ValidatePostcode(postcodedetails)

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
