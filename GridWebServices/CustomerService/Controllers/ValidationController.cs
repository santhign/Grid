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
        ///<param name="emailid">abcd@gmail.com</param>
        /// <returns>validation result</returns> 
        [HttpGet]
        [Route("EmailValidation/{emailid}")]
        public async Task<IActionResult> EmailValidation([FromRoute] string emailid)
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


        ///// <summary>
        ///// This will validate postcode
        ///// </summary>
        ///// <param name="Token"></param>
        ///// <param name="postcode"></param>
        ///// <returns>validation status</returns>
        ///// POST: api/ValidateAuthenticatedPostcode
        /////Body: 
        /////{
        /////  "APIKey":"xyz","APISecret":"abc","PostcodeNumber":"408600"
        ///// }
        //[HttpGet]
        //[Route("ValidatePostcode/{postcode}")]
        //public async Task<IActionResult> PostcodeValidate([FromHeader] string Token, [FromRoute]string postcode)
        //{
        //    try
        //    {

        //        CustomerDataAccess _customerAccess = new CustomerDataAccess(_iconfiguration);
        //        DatabaseResponse tokenAuthResponse = await _customerAccess.AuthenticateCustomerToken(Token);

        //        if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
        //        {
        //            DatabaseResponse configResponse = ConfigHelper.GetValue("Postcode", _iconfiguration);

        //            List<Dictionary<string, string>> _result = ((List<Dictionary<string, string>>)configResponse.Results);

        //            string _APIKey = _result.Single(x => x["key"] == "PostcodeApiKey")["value"];
        //            string _APISecret = _result.Single(x => x["key"] == "PostcodeSecret")["value"];
        //            string _Postcodeurl = _result.Single(x => x["key"] == "Postcodeurl")["value"];
        //            PostCodeRequest _request = new PostCodeRequest();
        //            _request.APIKey = _APIKey;
        //            _request.APISecret = _APISecret;
        //            _request.Postcode = postcode;

        //            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        //            ApiClient client = new ApiClient(new Uri(_Postcodeurl));
        //            await client.PostAsync<ResponseObject>(new Uri(_Postcodeurl), _request);

        //            return Ok(new ServerResponse
        //            {
        //                HasSucceeded = true,
        //                Message = StatusMessages.SuccessMessage

        //            });
        //        }
        //        else
        //        {
        //            // token auth failure
        //            LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

        //            return Ok(new OperationResponse
        //            {
        //                HasSucceeded = false,
        //                Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
        //                IsDomainValidationErrors = false
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

        //        return Ok(new OperationResponse
        //        {
        //            HasSucceeded = false,
        //            Message = StatusMessages.ServerError,
        //            IsDomainValidationErrors = false
        //        });
        //    }

        //}
        
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

            string _warningmsg;
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
                    Message = StatusMessages.ServerError,
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

        public Dictionary<int, string> GetSeries(string SeriesLetter)
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
