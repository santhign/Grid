using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AdminService.DataAccess;
using Microsoft.Extensions.Configuration;
using Core.Models;
using Core.Helpers;
using Core.Enums;
using InfrastructureService;

namespace AdminService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        IConfiguration _iconfiguration;

        public ConfigController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }

        /// <summary>
        /// Get config details
        /// </summary>
        /// <param name="ConfigKey"></param>
        /// <returns>config value</returns> 
        /// 
        [HttpGet("{ConfigKey}")]
        public async Task<IActionResult> GetGenericConfigValue([FromRoute] string ConfigKey)
        {
            try
            {
                if (string.IsNullOrEmpty(ConfigKey))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.DomainValidationError,
                        IsDomainValidationErrors = true
                    });

                }

                ConfigDataAccess _configAccess = new ConfigDataAccess(_iconfiguration);

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage,
                    Result = await _configAccess.GetConfigValue(ConfigKey)

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

        /// <summary>
        /// Get config validation details
        /// </summary>
        /// <param name="ConfigKey"></param>
        /// <param name="Token"></param>
        /// <returns>config value</returns> 
        /// 
        [HttpGet("validate/{ConfigKey}")]
        public async Task<IActionResult> GetConfigValue([FromHeader] string Token, [FromRoute] string ConfigKey)
        {
            try
            {
                if (string.IsNullOrEmpty(ConfigKey))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.DomainValidationError,
                        IsDomainValidationErrors = true
                    });

                }

                ConfigDataAccess _configAccess = new ConfigDataAccess(_iconfiguration);

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage,
                    Result = await _configAccess.GetConfigValue(ConfigKey, Token)

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
