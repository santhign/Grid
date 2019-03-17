using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AdminService.DataAccess;
using Microsoft.Extensions.Configuration;
using Core.Models;
using Core.Helpers;
using Core.Enums;
using Serilog;

namespace AdminService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        IConfiguration _iconfiguration;

        public LookupController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }

        [HttpGet("{lookupType}")]
        public async Task<IActionResult> GetLookup([FromRoute] string lookupType)
        {
            try
            {
                if(string.IsNullOrEmpty(lookupType))
                {                   
                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = StatusMessages.DomainValidationError,
                            IsDomainValidationErrors = true
                        });
                    
                }

                LookupDataAccess _lookupAccess = new LookupDataAccess(_iconfiguration);

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage,
                    Result = await _lookupAccess.GetLookupList(lookupType)

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
