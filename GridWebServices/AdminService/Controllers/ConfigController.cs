using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AdminService.DataAccess;
using Microsoft.Extensions.Configuration;
using Core.Models;
using Core.Helpers;
using Core.Extensions;
using Core.Enums;
using InfrastructureService;
using System.Collections.Generic;

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
        /// Gets the generic configuration value.
        /// </summary>
        /// <param name="Token">The token.</param>
        /// <param name="ConfigKey">The configuration key.</param>
        /// <returns></returns>
        [HttpGet("{ConfigKey}")]
        public async Task<IActionResult> GetGenericConfigValue([FromHeader(Name = "Grid-General-Token")] string Token, [FromRoute] string ConfigKey)
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

        /// <summary>
        /// Get config validation details
        /// </summary>
        /// <param name="ConfigType"></param>
        /// <returns>config value list</returns> 
        [HttpGet("GetConfigByType/{ConfigType}")]
        public async Task<IActionResult> GetConfigByType([FromHeader(Name = "Grid-General-Token")] string Token, [FromRoute] string ConfigType)
        {
            try
            {
                if (string.IsNullOrEmpty(ConfigType))
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
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                DatabaseResponse configResponse =  await ConfigHelper.GetPubicValue(ConfigType, _iconfiguration);

                List<Dictionary<string, string>> configList = new List<Dictionary<string, string>>();

                if (configResponse!=null && configResponse.ResponseCode==(int) DbReturnValue.RecordExists)
                {
                    configList = (List<Dictionary<string, string>>)configResponse.Results;
                }

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = configResponse.ResponseCode == (int)DbReturnValue.NotExists ? EnumExtensions.GetDescription(DbReturnValue.NotExists) : EnumExtensions.GetDescription(DbReturnValue.RecordExists),
                    Result = configList

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
