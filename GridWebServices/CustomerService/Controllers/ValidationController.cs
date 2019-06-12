using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CustomerService.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using CustomerService.DataAccess;
using Core.Models;
using Core.Enums;
using Core.Extensions;
using Core.Helpers;
using InfrastructureService;
using Serilog;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace CustomerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ValidationController : ControllerBase
    {

        IConfiguration _iconfiguration;
        public ValidationController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }


        /// <summary>
        /// This will validate the email id
        /// </summary> 
        ///<param name="emailid">abcd@gmail.com</param>
        /// <returns>validation result</returns> 
        [HttpGet]
        [Route("EmailValidation/{emailid}")]
        public IActionResult EmailValidation([FromHeader(Name = "Grid-General-Token")] string Token, [FromRoute] string emailid)
        {
            try
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
                DatabaseResponse configResponseEmail = ConfigHelper.GetValue("EmailValidate", _iconfiguration);

                List<Dictionary<string, string>> _result = ((List<Dictionary<string, string>>)configResponseEmail.Results);
                string email1 = emailid;
                if (_result.Single(x => x["key"] == "AllowPlusSignEmailAddress")["value"] == "1")
                {
                    //Remove the Plus sign part from email before sending to neverbounce
                    Regex emailregx = new Regex("(\\+)(.*?)(\\@)"); //Replace the part after the plus "+) sign 
                    email1 = emailregx.Replace(emailid, "$3"); //$3 to putback the @symbol 
                }

                EmailValidationHelper emailhelper = new EmailValidationHelper();
                EmailConfig objEmailConfig = new EmailConfig();
                objEmailConfig.key = _result.Single(x => x["key"] == "NeverbouceKey")["value"];
                objEmailConfig.Email = email1;
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

                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.InvalidEmail));

                    return Ok(new ServerResponse
                    {
                        HasSucceeded = true,
                        Message = StatusMessages.InvalidMessage,
                        Result = _response
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

        /// <summary>
        /// This will validate postcode
        /// </summary>
        /// <param name="token"></param>
        /// <param name="postcode"></param>
        /// <returns>validation status</returns>
        /// POST: api/ValidateAuthenticatedPostcode
        ///Body: 
        ///{
        ///  "APIKey":"xyz","APISecret":"abc","PostcodeNumber":"408600"
        /// }
        [HttpPost]
        [Route("ValidatePostcode/{postcode}")]
        public async Task<IActionResult> ValidatePostcode([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute]string postcode)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });

                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        if (postcode.Length == 0)
                        {
                            LogInfo.Warning(StatusMessages.MissingRequiredFields);
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = StatusMessages.MissingRequiredFields,
                                IsDomainValidationErrors = true
                            });
                        }

                        ValidationDataAccess _validateDataAccess = new ValidationDataAccess(_iconfiguration);

                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = await _validateDataAccess.ValidatePostcode(postcode)

                        });
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

        /// <summary>
        /// This will check NRIC Validation.
        /// </summary>
        /// <param name="NRIC"></param>
        /// <returns>validtion result</returns>
        /// POST: api/NRICValidation/S1234567D 
        [HttpPost]
        [Route("NRICValidation/{NRIC}")]
        public IActionResult NRICValidation([FromHeader(Name = "Grid-General-Token")] string Token, [FromRoute] string NRIC)
        {

            string _warningmsg = "";
            try
            {
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

                EmailValidationHelper _helper = new EmailValidationHelper();
                if (_helper.NRICValidation(null, NRIC, out _warningmsg))
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = true,
                        Message = StatusMessages.ValidMessage
                    });
                }
                else
                {
                    LogInfo.Warning("NRIC Validation without type: " + _warningmsg);
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.InvalidMessage,
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
                    Message = (_warningmsg == "" ? StatusMessages.ServerError : StatusMessages.InvalidMessage),
                    IsDomainValidationErrors = false
                });
            }

        }

        /// <summary>
        /// This will check NRIC Validation: IDType - S=Singaporean;F=Forigner
        /// </summary>
        /// <param name="IDType"></param>
        /// <param name="NRIC"></param>
        /// <returns>validtion result</returns>
        /// POST: api/NRICValidation/S1234567D 
        [HttpPost]
        [Route("NRICTypeValidation/{IDType}/{NRIC}")]
        public IActionResult NRICTypeValidation([FromHeader(Name = "Grid-General-Token")] string Token, [FromRoute] string IDType, [FromRoute] string NRIC)
        {
            string _warningmsg = "";           

            try
            {
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

                EmailValidationHelper _helper = new EmailValidationHelper();
                if (_helper.NRICValidation(IDType, NRIC, out _warningmsg))
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = true,
                        Message = StatusMessages.ValidMessage
                    });
                }
                else
                {
                    LogInfo.Warning("NRIC Validation: " + IDType + "_" + _warningmsg);
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.InvalidMessage,
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
                    Message = (_warningmsg == "" ? StatusMessages.ServerError : StatusMessages.InvalidMessage),
                    IsDomainValidationErrors = false
                });
            }

        }       


    }
}
