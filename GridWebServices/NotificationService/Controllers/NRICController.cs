using Core.Enums;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NotificationService.DataAccess;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NRICController: ControllerBase
    {

        IConfiguration _iconfiguration;

        public NRICController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }

        [HttpPost]
        [Route("NRICValidation/{NRIC}")]
        public async Task<IActionResult> NRICValidation([FromRoute] string NRIC)
        {
            try
            {

                if (NRIC == null)
                {
                    Log.Error(StatusMessages.MissingRequiredFields);
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.MissingRequiredFields,
                        IsDomainValidationErrors = true
                    });
                }

                NRICDataAccess _nricdataaccess = new NRICDataAccess(_iconfiguration); 

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage,
                    Result = await _nricdataaccess.ValidateNRIC (NRIC)

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
