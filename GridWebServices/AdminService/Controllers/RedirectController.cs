using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Enums;
using Core.Models;
using Core.Extensions;
using Microsoft.Extensions.Configuration;

namespace AdminService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedirectController : ControllerBase
    {   

        private readonly IConfiguration _iconfiguration;

        public RedirectController(IConfiguration configuration)
        {
            _iconfiguration = configuration;           
        }

        [HttpGet("Forbidden")]      
        public async Task<IActionResult> Forbidden()
        {
            return await Task.Run(() =>
            {
                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = EnumExtensions.GetDescription(CommonErrors.Forbidden),
                    IsDomainValidationErrors = true,
                    StatusCode= ((int) CommonErrors.Forbidden).ToString()
                });

            });           
               
        }


        [HttpGet("TokenEmpty")]
        public async Task<IActionResult> TokenEmpty()
        {
            return await Task.Run(() =>
            {
                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty) + "," + EnumExtensions.GetDescription(CommonErrors.Unauthorized),
                    IsDomainValidationErrors = true,
                    StatusCode = ((int)CommonErrors.Unauthorized).ToString()
                });

            });

        }

        [HttpGet("TokenExpired")]
        public async Task<IActionResult> TokenExpired()
        {
            return await Task.Run(() =>
            {
                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = EnumExtensions.GetDescription(CommonErrors.ExpiredToken),
                    IsDomainValidationErrors = true,
                    StatusCode = ((int)CommonErrors.Unauthorized).ToString()
                });

            });

        }

        [HttpGet("InvalidToken")]
        public async Task<IActionResult> InvalidToken()
        {
            return await Task.Run(() =>
            {
                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                    IsDomainValidationErrors = false,
                    StatusCode= ((int)CommonErrors.Unauthorized).ToString()
                });

            });

        }
    }
}