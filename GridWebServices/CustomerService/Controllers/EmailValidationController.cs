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

namespace CustomerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class EmailValidationController : ControllerBase
    {

        IConfiguration _iconfiguration;

        public EmailValidationController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }


        /// <summary>
        /// This will validate the email id
        /// </summary>
        /// <param name="apikey">abcdxyz</param>
        ///<param name="emailid">abcd@gmail.com</param>
        /// <returns>validation result</returns> 
        [HttpGet]
        [Route("GetEmailValidation/{apikey}/{emailid}")]
        public async Task<IActionResult> GetEmailValidation([FromRoute] string apikey,string emailid )
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    Log.Error(StatusMessages.DomainValidationError);
                    new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage))
                    };
                }
                 
                EmailValidationHelper helper = new EmailValidationHelper();
                EmailConfig objEmailConfig = new EmailConfig();
                objEmailConfig.key = apikey;
                objEmailConfig.Email = emailid;
                objEmailConfig.EmailAPIUrl = "https://api.neverbounce.com/v4/single/check";
                 

                ResponseObject configResponse = await helper.GetEmailValidation(objEmailConfig);

                return Ok(new OperationResponse
                {
                    HasSucceeded = true,
                    IsDomainValidationErrors = false,
                    ReturnedObject = configResponse
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
        /// This will send forget password mail
        /// </summary> 
        ///<param name="emailid">abcd@gmail.com</param>
        /// <returns>Customer Id and Token key</returns>
        [HttpGet]
        [Route("GetForgetPassword/{emailid}")]
        public async Task<IActionResult> GetForgetPassword([FromRoute] string emailid)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Log.Error(StatusMessages.DomainValidationError);
                    new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage))
                    };
                }


                EmailDataAccess _emailDataAccess = new EmailDataAccess(_iconfiguration);

                Emails _emails = new Emails();
                _emails.EmailId = emailid;
                ForgetPassword _forgetPassword = await _emailDataAccess.GetForgetPassword(_emails);

                if (_forgetPassword == null)
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.NotExists)

                    });
                }
                else
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = true,
                        Message = StatusMessages.SuccessMessage,
                        Result = _forgetPassword

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
