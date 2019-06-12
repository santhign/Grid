using AdminService.DataAccess;
using AdminService.Models;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using InfrastructureService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
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


        /// <summary>
        /// This will get all FAQ based on pagename
        /// </summary>
        /// <param name="Pagename">Page1</param>
        /// <returns>LoggedInPrinciple</returns>  
        // GET: api/GetPageFAQ/page1
        [HttpGet("GetPageFAQ/{Pagename}")]
        public async Task<IActionResult> GetPageFAQ([FromHeader(Name = "Grid-General-Token")] string Token, [FromRoute] string Pagename)
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

                TokenValidationHelper tokenValidationHelper = new TokenValidationHelper();
                if (!tokenValidationHelper.ValidateGenericToken(Token, _iconfiguration))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = Core.Extensions.EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                FaqPageDataAccess _FaqPageDataAccess = new FaqPageDataAccess(_iconfiguration);

                List<FaqPages> faqresult = await _FaqPageDataAccess.GetPageFAQ(Pagename);
                string faqmessage;
                 
                if (faqresult!=null && faqresult.Count >0 )
                {
                    faqmessage = StatusMessages.SuccessMessage; 
                }
                else
                {
                    faqmessage = "Faq not found";
                }
               

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = faqmessage,
                    Result = faqresult 


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
    }
}
