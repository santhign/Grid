﻿using System;
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

namespace CustomerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        IConfiguration _iconfiguration;

        public AccountController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }
        /// <summary>
        /// Authenticate customer against Email and Password given.
        /// Returns logged in Principle with success status, auth token and logged customer details
        /// </summary>
        /// <param name="loginRequest"></param>
        /// <returns>LoggedInPrinciple</returns>
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody]LoginDto loginRequest)
        {
            try
            {
                
                if (!ModelState.IsValid)
                {
                    new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage))
                    };
                }
               

                AccountDataAccess _AccountAccess = new AccountDataAccess(_iconfiguration);

                DatabaseResponse response = await _AccountAccess.AuthenticateCustomer(loginRequest);

                if (response.ResponseCode == ((int)DbReturnValue.EmailNotExists))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.EmailNotExists),
                        IsDomainValidationErrors = true
                    });
                }
                else if (response.ResponseCode == ((int)DbReturnValue.AccountIsLocked))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.AccountIsLocked),
                        IsDomainValidationErrors = true
                    });
                }
                else if (response.ResponseCode == ((int)DbReturnValue.AccountDeactivated))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.AccountDeactivated),
                        IsDomainValidationErrors = true
                    });
                }
                else if (response.ResponseCode == ((int)DbReturnValue.PasswordIncorrect))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.PasswordIncorrect),
                        IsDomainValidationErrors = true
                    });
                }

                else if(response.ResponseCode == ((int)DbReturnValue.AuthSuccess))
                {
                    //Authentication success

                    var customer = new Customer();

                    customer = (Customer) response.Results;

                    var tokenHandler = new JwtSecurityTokenHandler();

                    var key = Encoding.ASCII.GetBytes("stratagile grid customer signin jwt hashing secret");

                    DatabaseResponse configResponse = ConfigHelper.GetValueByKey(ConfigKeys.CustomerTokenExpiryInDays.ToString(), _iconfiguration);

                    int expiry = 0;

                    if (configResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                    {
                        expiry = int.Parse(configResponse.Results.ToString());
                    }

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                             new Claim(ClaimTypes.Name, customer.CustomerID.ToString())
                        }),

                        Expires = DateTime.UtcNow.AddDays(expiry), 

                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };

                    var token = tokenHandler.CreateToken(tokenDescriptor);

                    var tokenString = tokenHandler.WriteToken(token);

                    DatabaseResponse tokenResponse = new DatabaseResponse();

                    tokenResponse=await _AccountAccess.LogCustomerToken(customer.CustomerID,tokenString);

                    // return basic user info (without password) and token to store client side
                    return Ok(new OperationResponse
                    {     HasSucceeded=true,
                          Message=EnumExtensions.GetDescription(DbReturnValue.AuthSuccess),
                           ReturnedObject = new LoggedInPrinciple
                           {
                               Customer = customer,
                               IsAuthenticated = true,
                               Token = tokenString
                           }
                    }                  
                    );
                }

                else
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.ReasonUnknown),
                        IsDomainValidationErrors = true
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
        /// Authenticate customer against token given.
        /// Returns logged in Principle with success status, auth token and logged customer details
        /// </summary>
        /// <param name="token"></param>
        /// <returns>LoggedInPrinciple</returns>
        [HttpGet("GetTokenDetails/{token}")]
        public async Task<IActionResult> GetTokenDetails([FromRoute]string token)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage))
                    };
                }


                AccountDataAccess _AccountAccess = new AccountDataAccess(_iconfiguration);

                DatabaseResponse response = await _AccountAccess.AuthenticateToken(token);

                if (response.ResponseCode == 105)
                {
                    //Authentication success

                    var _accesstoken = new AccessToken();

                    _accesstoken = (AccessToken)response.Results;

                    // return basic user info (without password) and token to store client side
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = true,
                        Message = EnumExtensions.GetDescription(DbReturnValue.AuthSuccess),
                        ReturnedObject = _accesstoken
                    });
                }

                else
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.ReasonUnknown),
                        IsDomainValidationErrors = true
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
        /// Validate reset password token
        /// </summary>
        /// <param name="passwordtoken"></param>
        /// <returns>
        /// OperationsResponse
        /// </returns>

        [HttpGet("ValidateResetPasswordToken/{passwordtoken}")]
        public async Task<IActionResult> ValidateResetPasswordToken([FromRoute] string passwordtoken)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage))
                    };
                }

                AccountDataAccess _AccountAccess = new AccountDataAccess(_iconfiguration);

                DatabaseResponse response = await _AccountAccess.ValidateResetPasswordToken(passwordtoken);

                if (response.ResponseCode == ((int)DbReturnValue.ResetPasswordTokenValid))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = true,
                        Message = EnumExtensions.GetDescription(DbReturnValue.ResetPasswordTokenValid),
                        IsDomainValidationErrors = true
                    });
                }

                else if (response.ResponseCode == ((int)DbReturnValue.ResetPasswordTokenExpired))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.ResetPasswordTokenExpired),
                        IsDomainValidationErrors = true
                    });
                }

                else
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.NotExists),
                        IsDomainValidationErrors = true
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
        /// Reset customer password
        /// </summary>
        /// <param name="passwordResetRequest">
        /// body{
        /// "NewPassword" :"",
        /// "ResetToken" :"A4EDFE2A4EDFE23A4EDFE23A4EDFE23A4EDFE233",       
        /// }
        /// </param>
        /// <returns>OperationsResponse</returns>
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword passwordResetRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                AccountDataAccess _accountDataAccess = new AccountDataAccess(_iconfiguration);

                DatabaseResponse response = await _accountDataAccess.ResetPassword(passwordResetRequest);

                if (response.ResponseCode == ((int)DbReturnValue.NotExists))
                { //102
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(CommonErrors.TokenNotExists),
                        IsDomainValidationErrors = true
                    });
                }

                if (response.ResponseCode == ((int)DbReturnValue.UpdateSuccess))
                { //101
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = true,
                        Message = EnumExtensions.GetDescription(CommonErrors.PasswordResetSuccess),
                        IsDomainValidationErrors = false
                    });
                }

                if (response.ResponseCode == ((int)DbReturnValue.UpdationFailed))
                {//106
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(CommonErrors.PasswordResetFailed),
                        IsDomainValidationErrors = true
                    });
                }
                else
                {    //125              

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.ResetPasswordTokenExpired),
                        IsDomainValidationErrors = false,

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