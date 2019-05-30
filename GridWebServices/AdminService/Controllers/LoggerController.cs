using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Models;
using Core.Helpers;
using Core.Enums;
using InfrastructureService;
using Newtonsoft.Json;


namespace AdminService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoggerController : ControllerBase
    {
        [HttpPost("Log")]
        public async Task<IActionResult> Log([FromBody] UILog logRequest)
        {
            try
            {
                if (logRequest==null || string.IsNullOrEmpty( logRequest.logMessage))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.DomainValidationError,
                        IsDomainValidationErrors = true
                    });

                }

                await Task.Run(() =>
                {
                    LogInfo.Error(JsonConvert.SerializeObject(logRequest));
                });


                return Ok();
               
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