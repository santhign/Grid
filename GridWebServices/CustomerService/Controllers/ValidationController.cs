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

    public class ValidationController : ControllerBase
    {

        IConfiguration _iconfiguration;
        private static string Weights = "2765432";
        public ValidationController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
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
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
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

                        EmailValidationHelper emailhelper = new EmailValidationHelper();
                        EmailConfig objEmailConfig = new EmailConfig();
                        objEmailConfig.key = _result.Single(x => x["key"] == "NeverbouceKey")["value"];
                        objEmailConfig.Email = emailid;
                        objEmailConfig.EmailAPIUrl = _result.Single(x => x["key"] == "Emailurl")["value"];


                        string configResponse = await emailhelper.EmailValidation(objEmailConfig);
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

                        LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                    LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
                            LogInfo.Error(StatusMessages.MissingRequiredFields);
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

                        LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                    LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// This will check NRIC Validation
        /// </summary>
        /// <param name="NRIC"></param>
        /// <returns>validtion result</returns>
        /// POST: api/NRICValidation/S1234567D 
        [HttpPost]
        [Route("NRICValidation/{NRIC}")]
        public IActionResult NRICValidation([FromRoute] string NRIC)
        {

            string _warningmsg = "";
            try
            {

                // Check any number is passed
                if (NRIC.Equals(string.Empty))
                {
                    _warningmsg = "Please give an NRIC number";
                    LogInfo.Warning(_warningmsg);
                    throw new Exception(_warningmsg);
                }

                // Check length
                if (NRIC.Length != 9)
                {
                    _warningmsg = "The length of NRIC should be 9";
                    LogInfo.Warning(_warningmsg);
                    throw new Exception(_warningmsg);
                }

                // Check the file letter
                if (!((NRIC[0].ToString().Equals("S"))
                    || (NRIC[0].ToString().Equals("T"))
                    || (NRIC[0].ToString().Equals("F"))
                    || (NRIC[0].ToString().Equals("G"))))
                {
                    _warningmsg = "First letter of NRIC should be S,T,F or G";
                    LogInfo.Warning(_warningmsg);
                    throw new Exception(_warningmsg);
                }

                // Check whether the NRIC is a number if first and last char are removed
                int NRIC_Internal_Number = 0;
                if (!int.TryParse(NRIC.Substring(1, 7), out NRIC_Internal_Number))
                {
                    _warningmsg = "NRIC should be a number excluding the first and last characters";
                    LogInfo.Warning(_warningmsg);
                    throw new Exception(_warningmsg);
                }

                // Check the CheckSumNumber
                if (!IsValidCheckSum(NRIC))
                {
                    _warningmsg = "Invalid NRIC checksum";
                    LogInfo.Warning(_warningmsg);
                    throw new Exception(_warningmsg);
                }

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.ValidMessage
                });

            }

            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = (_warningmsg == "" ? StatusMessages.ServerError : StatusMessages.InvalidMessage),
                    IsDomainValidationErrors = false
                });
            }

        }       

        private bool IsValidCheckSum(string NRIC)
        {
            string NRIC_Internal_Numbers = NRIC.Substring(1, 7);
            int CheckSum = 0;


            // Calcualte check sum
            for (int i = 0; i < 7; i++)
            {
                int Weight = Convert.ToInt32(Weights[i].ToString());
                int NRIC_Internal_Number = Convert.ToInt32(NRIC_Internal_Numbers[i].ToString());
                CheckSum += (Weight * NRIC_Internal_Number);
            }
            CheckSum = CheckSum % 11;

            // Get the series checksum letter
            Dictionary<int, string> Series = GetSeries(NRIC.Substring(0, 1));
            string ChecksumLetter = Series[CheckSum];

            // Check if the last char or NRIC and check sum is equal
            if (ChecksumLetter.Equals(NRIC[8].ToString()))
            {
                return true;
            }

            return false;
        }

        private Dictionary<int, string> GetSeries(string SeriesLetter)
        {
            Dictionary<int, string> Series = new Dictionary<int, string>();

            if (SeriesLetter.Equals("S"))
            {
                Series.Add(10, "A");
                Series.Add(9, "B");
                Series.Add(8, "C");
                Series.Add(7, "D");
                Series.Add(6, "E");
                Series.Add(5, "F");
                Series.Add(4, "G");
                Series.Add(3, "H");
                Series.Add(2, "I");
                Series.Add(1, "Z");
                Series.Add(0, "J");
            }
            else if (SeriesLetter.Equals("T"))
            {
                Series.Add(10, "H");
                Series.Add(9, "I");
                Series.Add(8, "Z");
                Series.Add(7, "J");
                Series.Add(6, "A");
                Series.Add(5, "B");
                Series.Add(4, "C");
                Series.Add(3, "D");
                Series.Add(2, "E");
                Series.Add(1, "F");
                Series.Add(0, "G");
            }
            else if (SeriesLetter.Equals("F"))
            {
                Series.Add(10, "K");
                Series.Add(9, "L");
                Series.Add(8, "M");
                Series.Add(7, "N");
                Series.Add(6, "P");
                Series.Add(5, "Q");
                Series.Add(4, "R");
                Series.Add(3, "T");
                Series.Add(2, "U");
                Series.Add(1, "W");
                Series.Add(0, "X");
            }
            else if (SeriesLetter.Equals("G"))
            {
                Series.Add(10, "T");
                Series.Add(9, "U");
                Series.Add(8, "W");
                Series.Add(7, "X");
                Series.Add(6, "K");
                Series.Add(5, "L");
                Series.Add(4, "M");
                Series.Add(3, "N");
                Series.Add(2, "P");
                Series.Add(1, "Q");
                Series.Add(0, "R");
            }
            else
            {
                return null;
            }

            return Series;
        }

    }
}
