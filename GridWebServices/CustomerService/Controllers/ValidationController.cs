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
        ///<param name="emailid">abcd@gmail.com</param>
        /// <returns>validation result</returns> 
        [HttpGet]
        [Route("EmailValidation/{emailid}")]
        public async Task<IActionResult> EmailValidation([FromRoute] string emailid )
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

                DatabaseResponse configResponseEmail = ConfigHelper.GetValue("EmailValidate", _iconfiguration);

                List<Dictionary<string, string>> _result = ((List<Dictionary<string, string>>)configResponseEmail.Results);
 
                EmailValidationHelper helper = new EmailValidationHelper();
                EmailConfig objEmailConfig = new EmailConfig();
                objEmailConfig.key = _result.Single(x => x["key"] == "NeverbouceKey").Select(x => x.Value).ToString();  
                objEmailConfig.Email = emailid;
                objEmailConfig.EmailAPIUrl = _result.Single(x => x["key"] == "Emailurl").Select(x => x.Value).ToString();


                ResponseObject configResponse = await helper.EmailValidation(objEmailConfig);

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


      

    }
}
