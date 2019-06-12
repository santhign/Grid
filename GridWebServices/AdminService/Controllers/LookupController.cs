using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AdminService.DataAccess;
using Microsoft.Extensions.Configuration;
using Core.Models;
using Core.Helpers;
using Core.Enums;
using InfrastructureService;
using Core.Extensions;
using System.Linq;
using System.Collections.Generic;
using AdminService.Models;

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

        /// <summary>
        /// Get Lookup details
        /// </summary>
        /// <param name="lookupType"></param>
        /// <returns>Lookup details</returns> 
        /// 
        [HttpGet("{lookupType}")]
        public async Task<IActionResult> GetLookup([FromHeader(Name = "Grid-General-Token")] string Token, [FromRoute] string lookupType)
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
        /// This will validate the email id
        /// </summary> 
        /// <param name="token"></param>
        ///<param name="emailid">abcd@gmail.com</param>
        /// <returns>validation result</returns> 
        [HttpGet]
        [Route("EmailValidation/{emailid}")]
        public async Task<IActionResult> EmailValidation([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] string emailid)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AdminUsersDataAccess _adminUsersDataAccess = new AdminUsersDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _adminUsersDataAccess.AuthenticateAdminUserToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        if (!ModelState.IsValid)
                        {
                            LogInfo.Error(StatusMessages.DomainValidationError);
                            new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                    .SelectMany(x => x.Errors)
                                                    .Select(x => x.ErrorMessage))
                            };
                        }

                        DatabaseResponse configResponseEmail = ConfigHelper.GetValue("EmailValidate", _iconfiguration);

                        List<Dictionary<string, string>> _result = ((List<Dictionary<string, string>>)configResponseEmail.Results);

                        EmailValidationHelper emailhelper = new EmailValidationHelper();
                        EmailConfig objEmailConfig = new EmailConfig();
                        objEmailConfig.key = _result.Single(x => x["key"] == "NeverbouceKey")["value"];
                        objEmailConfig.Email = emailid;
                        objEmailConfig.EmailAPIUrl = _result.Single(x => x["key"] == "Emailurl")["value"];


                        string configResponse = emailhelper.EmailValidation(objEmailConfig);
                        EmailValidationResponse _response = new EmailValidationResponse();
                        _response.Status = configResponse;
                        if (configResponse.ToLower().Trim() != "invalid")
                        {
                            _response.IsValid = true;
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.ValidMessage,
                                Result = _response
                            });
                        }
                        else
                        {
                            //Invalid email
                            _response.IsValid = false;

                            LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.InvalidEmail));

                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.InvalidMessage,
                                Result = _response
                            });

                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = EnumExtensions.GetDescription(DbReturnValue.TokenExpired),
                            IsDomainValidationErrors = true
                        });

                    }
                }
                else
                {
                    // token auth failure
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = false
                    });
                }
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
