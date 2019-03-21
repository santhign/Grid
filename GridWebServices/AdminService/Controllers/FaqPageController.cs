using AdminService.DataAccess;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class FaqPageController : ControllerBase
    {
        IConfiguration _iconfiguration;
        public FaqPageController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }

        [HttpGet("GetPageFAQ/{Pagename}")]
        public async Task<IActionResult> GetPageFAQ([FromRoute] string Pagename)
        {
            try
            {
                if (string.IsNullOrEmpty(Pagename))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.DomainValidationError,
                        IsDomainValidationErrors = true
                    });

                }

                FaqPageDataAccess _FaqPageDataAccess = new FaqPageDataAccess(_iconfiguration); 

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage,
                    Result = await _FaqPageDataAccess.GetPageFAQ(Pagename)

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
