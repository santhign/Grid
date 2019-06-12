using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AdminService.Models;
using AdminService.DataAccess;
using Microsoft.Extensions.Configuration;
using Core.Models;
using Core.Enums;
using Core.Helpers;
using InfrastructureService;
using Core.Extensions;

namespace CatelogService.Controllers
{ 
   
    [Route("api/[controller]")]
    [ApiController]    
    public class BannersController : ControllerBase
    {
        IConfiguration _iconfiguration;
       
        public BannersController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }
 
        /// <summary>
        /// Get banner details
        /// </summary>
        /// <param name="request"></param>
        /// <returns>banner details</returns> 
        /// 
        [HttpPost]
        [Route("BannerDetails")]
        public async Task<IActionResult> BannerDetails([FromHeader(Name = "Grid-General-Token")] string Token, [FromBody] BannerDetailsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {                   
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.DomainValidationError ,
                        IsDomainValidationErrors = true
                    });
                }

                TokenValidationHelper tokenValidationHelper = new TokenValidationHelper();
                if (!tokenValidationHelper.ValidateGenericToken(Token, _iconfiguration))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                BannerDataAccess _bannerAccess = new BannerDataAccess(_iconfiguration);

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage,
                    Result = await _bannerAccess.GetBannerDetails(request)

                });
            }
            catch(Exception ex)
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