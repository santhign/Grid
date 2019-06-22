using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
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
using System.IO;
using Core.DataAccess;
using System.Collections.Generic;


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
        /// <param name="Token"></param>
        /// <param name="loginRequest"></param>
        /// <returns>LoggedInPrinciple</returns>
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromHeader(Name = "Grid-General-Token")] string Token, [FromBody]LoginDto loginRequest)
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
        /// <param name="Token"></param>
        /// <param name="passwordResetRequest">
        /// body{
        /// "NewPassword" :"",
        /// "ResetToken" :"A4EDFE2A4EDFE23A4EDFE23A4EDFE23A4EDFE233",       
        /// }
        /// </param>
        /// <returns>OperationsResponse</returns>
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromHeader(Name = "Grid-General-Token")] string Token, [FromBody] ResetPassword passwordResetRequest)
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

        [Route("UpdateNRICIDDetails")]
        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> UpdateNRICIDDetails([FromHeader(Name = "Grid-General-Token")] string Token, [FromForm] UpdateOrderPersonalIDDetailsPublicRequest request)
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

                AccountDataAccess _AccountAccess = new AccountDataAccess(_iconfiguration);

                CommonDataAccess _commonDataAccess = new CommonDataAccess(_iconfiguration);

                DatabaseResponse customerResponse = await _commonDataAccess.GetCustomerIdFromOrderId(request.OrderID);

                if (customerResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                {
                    IFormFile frontImage = request.IDImageFront;

                    IFormFile backImage = request.IDImageBack;

                    BSSAPIHelper bsshelper = new BSSAPIHelper();

                    MiscHelper configHelper = new MiscHelper();

                    OrderDetails customerOrderDetails = await _commonDataAccess.GetOrderDetails(request.OrderID);

                    NRICDetailsRequest personalDetails = new NRICDetailsRequest
                    {
                        OrderID = request.OrderID,
                        IdentityCardNumber = customerOrderDetails.IdentityCardNumber,
                        IdentityCardType = customerOrderDetails.IdentityCardType,
                        Nationality = customerOrderDetails.Nationality,
                        NameInNRIC = customerOrderDetails.Name,
                        DOB = customerOrderDetails.DOB,
                        Expiry = customerOrderDetails.ExpiryDate,
                    };

                    //process file if uploaded - non null

                    if (frontImage != null && backImage != null)
                    {

                        DatabaseResponse awsConfigResponse = await _commonDataAccess.GetConfiguration(ConfiType.AWS.ToString());

                        if (awsConfigResponse != null && awsConfigResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                        {
                            GridAWSS3Config awsConfig = configHelper.GetGridAwsConfig((List<Dictionary<string, string>>)awsConfigResponse.Results);

                            AmazonS3 s3Helper = new AmazonS3(awsConfig);

                            string fileNameFront = customerOrderDetails.IdentityCardNumber.Substring(1, customerOrderDetails.IdentityCardNumber.Length - 2) + "_Front_" + DateTime.UtcNow.ToString("yyMMddhhmmss") + Path.GetExtension(frontImage.FileName); //Grid_IDNUMBER_yyyymmddhhmmss.extension

                            UploadResponse s3UploadResponse = await s3Helper.UploadFile(frontImage, fileNameFront);

                            if (s3UploadResponse.HasSucceed)
                            {
                                personalDetails.FrontImage = awsConfig.AWSEndPoint + s3UploadResponse.FileName;
                            }
                            else
                            {
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.S3UploadFailed));
                            }

                            string fileNameBack = customerOrderDetails.IdentityCardNumber.Substring(1, customerOrderDetails.IdentityCardNumber.Length - 2) + "_Back_" + DateTime.UtcNow.ToString("yyMMddhhmmss") + Path.GetExtension(frontImage.FileName); //Grid_IDNUMBER_yyyymmddhhmmss.extension

                            s3UploadResponse = await s3Helper.UploadFile(backImage, fileNameBack);

                            if (s3UploadResponse.HasSucceed)
                            {
                                personalDetails.BackImage = awsConfig.AWSEndPoint + s3UploadResponse.FileName;
                            }
                            else
                            {
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.S3UploadFailed));
                            }
                        }
                        else
                        {
                            // unable to get aws config
                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToGetConfiguration));

                        }
                    }    //file     

                    //update ID ReUpload details
                    DatabaseResponse updateNRICResponse = await _commonDataAccess.UpdateNRICDetails(null, 0, personalDetails);

                    if (updateNRICResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
                    {
                        return Ok(new OperationResponse
                        {
                            HasSucceeded = true,
                            Message = EnumExtensions.GetDescription(DbReturnValue.UpdateSuccess),
                            IsDomainValidationErrors = false
                        });
                    }
                    else if (updateNRICResponse.ResponseCode == (int)DbReturnValue.UpdateSuccessSendEmail)
                    {
                        LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.UpdateSuccessSendEmail) + "for " + request.OrderID + "Order");
                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = EnumExtensions.GetDescription(DbReturnValue.UpdateSuccessSendEmail),
                            IsDomainValidationErrors = false
                        });
                    }

                    else if (updateNRICResponse.ResponseCode == (int)DbReturnValue.NotExists)
                    {
                        LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NotExists) + " Order" + request.OrderID);
                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = EnumExtensions.GetDescription(DbReturnValue.UpdateSuccessSendEmail),
                            IsDomainValidationErrors = false
                        });
                    }
                    else
                    {
                        LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.UpdationFailed));
                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = EnumExtensions.GetDescription(DbReturnValue.UpdationFailed),
                            IsDomainValidationErrors = false
                        });
                    }
                }

                else
                {
                    // failed to locate customer
                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer));
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer),
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