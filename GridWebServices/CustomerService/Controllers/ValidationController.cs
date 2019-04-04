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
using Mandrill;

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


        /// <summary>
        /// This will send email
        /// </summary>
        /// <param name="emailSubscribers"></param>
        /// <returns>send status</returns>
        /// POST: api/SendEmail
        ///Body: 
        ///{
        ///  "Subject": "Email",
        ///  "Content": "email notify",
        ///  "BccAddress": "chinnu.rajan@gmail.com",
        ///  "EmailDetails":[ {
        ///        "Userid": 1,
        ///        "FName": "Chinnu",
        ///        "EMAIL": "chinnu.rajan@gmail.com",
        ///        "Param1": "",
        ///        "Param2": "",
        ///        "Param3": "",
        ///        "Param4": "",
        ///        "Param5": "",
        ///        "Param6": "",
        ///        "Param7": "",
        ///        "Param8": "",
        ///        "Param9": "",
        ///        "Param10": ""
        ///    }]
        ///    }
        [HttpPost]
        [Route("SendEmail")]
        public async Task<IActionResult> SendEmail([FromBody]NotificationEmail emailSubscribers)
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


                DatabaseResponse configResponse = ConfigHelper.GetValue("Email", _iconfiguration);

                List<Dictionary<string, string>> _result = ((List<Dictionary<string, string>>)configResponse.Results); 
                string _apiKeytest = "";
                string _fromEmail = "";
                string _fromEmailtest = "";
                string _fromName = "";
                string _apiKey = "";
                string _fromNametest = "";
                string _replyEmail = "";
                string _replyEmailtest = "";


                foreach (Dictionary<string, string> author in _result)
                {
                    foreach (KeyValuePair<string, string> response in author)
                    {
                        if ((response.Value == "MandrillKey") || (_apiKeytest == "MandrillKey"))
                        {
                            if (_apiKeytest.Trim().Length == 0)
                            {
                                _apiKeytest = "MandrillKey";
                            }
                            else
                            {
                                _apiKeytest = "";
                                _apiKey = response.Value;
                                break;
                            }
                        }
                        if ((response.Value == "FromEmail") || (_fromEmailtest == "FromEmail"))
                        {
                            if (_fromEmailtest.Trim().Length == 0)
                            {
                                _fromEmailtest = "FromEmail";
                            }
                            else
                            {
                                _fromEmailtest = "";
                                _fromEmail = response.Value;
                                break;
                            }
                        }
                        if ((response.Value == "FromName") || (_fromNametest == "FromName"))
                        {
                            if (_fromNametest.Trim().Length == 0)
                            {
                                _fromNametest = "FromName";
                            }
                            else
                            {
                                _fromNametest = "";
                                _fromName = response.Value;
                                break;
                            }
                        }
                        if ((response.Value == "ReplyEmail") || (_replyEmailtest == "ReplyEmail"))
                        {
                            if (_replyEmailtest.Trim().Length == 0)
                            {
                                _replyEmailtest = "ReplyEmail";
                            }
                            else
                            {
                                _replyEmailtest = "";
                                _replyEmail = response.Value;
                                break;
                            }
                        }
                    }
                }
                 

                MandrillApi api = new MandrillApi(_apiKey);

                Mandrill.Model.MandrillMessage eMsg = new Mandrill.Model.MandrillMessage();
                eMsg.Html = emailSubscribers.Content;
                eMsg.Subject = emailSubscribers.Subject;
                eMsg.FromEmail = _fromEmail;
                eMsg.FromName = _fromName;
                eMsg.BccAddress = emailSubscribers.BccAddress;
                Dictionary<string, object> mc = new Dictionary<string, object>();
                mc.Add("Reply-To", _replyEmail);
                eMsg.Headers = mc;
                eMsg.PreserveRecipients = false;
                eMsg.TrackClicks = true;
                eMsg.Merge = true;
                int recipientCount = emailSubscribers.EmailDetails.Count();
                List<Mandrill.Model.MandrillMailAddress> recipientList = new List<Mandrill.Model.MandrillMailAddress>();
                List<Mandrill.Model.MandrillRcptMergeVar> mergeVarList = new List<Mandrill.Model.MandrillRcptMergeVar>();
                //Mandrill.NameContentList<string> content = new Mandrill.NameContentList<string>();

                for (int counter = 0; counter < recipientCount; counter++)
                {
                    recipientList.Add(new Mandrill.Model.MandrillMailAddress(emailSubscribers.EmailDetails[counter].EMAIL, emailSubscribers.EmailDetails[counter].FName));

                    //mergeVarList.Add()
                    List<Mandrill.Model.MandrillMergeVar> mergeVars = new List<Mandrill.Model.MandrillMergeVar>();
                    Mandrill.Model.MandrillMergeVar mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "EMAIL";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].EMAIL.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "NAME";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].FName.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM1";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param1.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM2";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param2.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM3";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param3.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM4";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param4.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM5";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param5.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM6";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param6.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM7";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param7.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM8";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param8.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM9";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param9.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM10";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param10.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "UNIQUEID";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Userid.ToString();
                    mergeVars.Add(mergevar);
                    Mandrill.Model.MandrillRcptMergeVar rcptMergeVar = new Mandrill.Model.MandrillRcptMergeVar();
                    rcptMergeVar.Rcpt = emailSubscribers.EmailDetails[counter].EMAIL;
                    rcptMergeVar.Vars = mergeVars;
                    mergeVarList.Add(rcptMergeVar);
                }

                eMsg.To = recipientList;
                eMsg.MergeVars = mergeVarList;
                Task<IList<Mandrill.Model.MandrillSendMessageResponse>> result;
                result = api.Messages.SendAsync(eMsg);

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage
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


        /// <summary>
        /// This will validate postcode
        /// </summary>
        /// <param name="_postcodedata"></param>
        /// <returns>validation status</returns>
        /// POST: api/ValidatePostcode
        ///Body: 
        ///{
        ///  "APIKey":"xyz","APISecret":"abc","PostcodeNumber":"408600"
        /// }
        [HttpPost]
        [Route("ValidatePostcode")]
        public async Task<IActionResult> ValidatePostcode([FromBody]Postcode _postcodedata)
        {
            try
            {

                DatabaseResponse configResponse = ConfigHelper.GetValue("Postcode", _iconfiguration);

                List<Dictionary<string, string>> _result = ((List<Dictionary<string, string>>)configResponse.Results);
                 
                string _APIKey = "";
                string _APIKeytest = "";
                string _APISecret = "";
                string _APISecrettest = "";
                string _Postcodeurl = "";
                string _Postcodeurltest= ""; 

                foreach (Dictionary<string, string> author in _result)
                {
                    foreach (KeyValuePair<string, string> response in author)
                    {
                        if ((response.Value == "PostcodeApiKey") || (_APIKeytest == "PostcodeApiKey"))
                        {
                            if (_APIKeytest.Trim().Length == 0)
                            {
                                _APIKeytest = "PostcodeApiKey";
                            }
                            else
                            {
                                _APIKeytest = "";
                                _APIKey = response.Value;
                                break;
                            }
                        }
                        if ((response.Value == "PostcodeSecret") || (_APISecrettest == "PostcodeSecret"))
                        {
                            if (_APISecrettest.Trim().Length == 0)
                            {
                                _APISecrettest = "PostcodeSecret";
                            }
                            else
                            {
                                _APISecrettest = "";
                                _APISecret = response.Value;
                                break;
                            }
                        }
                        if ((response.Value == "Postcodeurl") || (_Postcodeurltest == "Postcodeurl"))
                        {
                            if (_Postcodeurltest.Trim().Length == 0)
                            {
                                _Postcodeurltest = "Postcodeurl";
                            }
                            else
                            {
                                _Postcodeurltest = "";
                                _Postcodeurl = response.Value;
                                break;
                            }
                        } 
                    }
                }

                _postcodedata.APIKey = _APIKey;
                _postcodedata.APISecret = _APISecret;
                string Postcodeurl = _Postcodeurl;

                _postcodedata.PostData = string.Format("APIKey={0}&APISecret={1}&Postcode={2}", _postcodedata.APIKey, _postcodedata.APISecret, _postcodedata.PostcodeNumber);

                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                ApiClient client = new ApiClient(new Uri(Postcodeurl));
                await client.PostAsync<ResponseObject, Postcode>(new Uri(Postcodeurl), _postcodedata);

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage

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


        /// <summary>
        /// This will send SMS
        /// </summary>
        /// <param name="_smsdata"></param>
        /// <returns>send status</returns>
        /// POST: api/SendSMS
        ///Body: 
        ///{
        ///  "PhoneNumber":"1234","SMSText":"Ok","ToPhoneNumber":"34567","PostData":"xyz"
        /// }
        [HttpPost]
        [Route("SendSMS")]
        public async Task<IActionResult> SendSMS([FromBody]Sms _smsdata)
        {
            string _warningmsg;
            try
            {
                if (_smsdata.PhoneNumber.Length < 10)
                {
                    _warningmsg = "Phone number should be atleast 10 numbres long.";
                    LogInfo.Warning(_warningmsg);
                    throw new Exception(_warningmsg);
                }
                if (_smsdata.PhoneNumber.Length > 13)
                {
                    _warningmsg = "Phone number should not be longer than 13 numbers.";
                    LogInfo.Warning(_warningmsg);
                    throw new Exception(_warningmsg);
                }

                long lPhoneNumber = 0;
                if (!long.TryParse(_smsdata.PhoneNumber, out lPhoneNumber))
                {
                    _warningmsg = "Phone number should not be a fully qualified number.";
                    LogInfo.Warning(_warningmsg);
                    throw new Exception(_warningmsg);
                }
                if (_smsdata.SMSText.Length > 4000)
                {
                    _warningmsg = "Maximum length of SMS text is 4000.";
                    LogInfo.Warning(_warningmsg);
                    throw new Exception(_warningmsg);
                }


                DatabaseResponse configResponse = ConfigHelper.GetValue("Nexmo", _iconfiguration);

                List<Dictionary<string, string>> _result = (List<Dictionary<string, string>>)configResponse.Results;

                string _nexmoUserName = "";
                string _nexmoUserNametest = "";
                string _nexmoPassword = "";
                string _nexmoPasswordtest = "";
                string _nexmoSmsSignature = "";
                string _nexmoSmsSignaturetest = "";
                string _nexmoWebRequestUrl = "";
                string _nexmoWebRequestUrltest = "";

                foreach (Dictionary<string, string> author in _result)
                {
                    foreach (KeyValuePair<string, string> response in author)
                    {
                        if ((response.Value == "NexmoPassword") || (_nexmoPasswordtest == "NexmoUserName"))
                        { 
                            if (_nexmoPasswordtest.Trim().Length == 0)
                            {
                                _nexmoPasswordtest = "NexmoUserName";
                            }
                            else
                            {
                                _nexmoPasswordtest = "";
                                _nexmoPassword = response.Value;
                                break;
                            }                           
                        }
                        if ((response.Value == "NexmoUserName") ||(_nexmoUserNametest == "NexmoPassword"))
                        {
                            if (_nexmoUserNametest.Trim().Length == 0)
                            {
                                _nexmoUserNametest = "NexmoPassword";
                            }
                            else
                            {
                                _nexmoUserNametest = "";
                                _nexmoUserName = response.Value;
                                break;
                            }     
                        }
                        if ((response.Value == "NexmoSmsSignature")|| (_nexmoSmsSignaturetest == "NexmoSmsSignature"))
                        { 
                            if (_nexmoSmsSignaturetest.Trim().Length == 0)
                            {
                                _nexmoSmsSignaturetest = "NexmoSmsSignature";
                            }
                            else
                            {
                                _nexmoSmsSignaturetest = "";
                                _nexmoSmsSignature = response.Value;
                                break;
                            } 
                        }
                        if ((response.Value == "NexmoWebRequestUrl")|| (_nexmoWebRequestUrltest == "NexmoWebRequestUrl"))
                        { 
                            if (_nexmoWebRequestUrltest.Trim().Length == 0)
                            {
                                _nexmoWebRequestUrltest = "NexmoWebRequestUrl";
                            }
                            else
                            {
                                _nexmoWebRequestUrltest = "";
                                _nexmoWebRequestUrl = response.Value;
                                break;
                            }  
                        }
                    }
                }
                 
                _smsdata.Username = _nexmoUserName;
                _smsdata.Password = _nexmoPassword;
                _smsdata.FromPhoneNumber = _nexmoSmsSignature;
                _smsdata.ToPhoneNumber = _smsdata.PhoneNumber;
                _smsdata.Type = "unicode";
                _smsdata.PostData = string.Empty;
                _smsdata.PostData = string.Format("username={0}&password={1}&from={2}&to={3}&text={4}&status-report-req=1", _smsdata.Username, _smsdata.Password, _smsdata.FromPhoneNumber, _smsdata.ToPhoneNumber, _smsdata.SMSText);
                byte[] buffer;

                if (ContainsUnicodeCharacter(_smsdata.SMSText))
                {
                    _smsdata.PostData = string.Format("username={0}&password={1}&from={2}&to={3}&type={4}&text={5}&status-report-req=1", _smsdata.Username, _smsdata.Password, _smsdata.FromPhoneNumber, _smsdata.ToPhoneNumber, _smsdata.Type, _smsdata.SMSText);
                    buffer = Encoding.UTF8.GetBytes(_smsdata.PostData);
                }
                else
                {
                    _smsdata.PostData = string.Format("username={0}&password={1}&from={2}&to={3}&text={4}&status-report-req=1", _smsdata.Username, _smsdata.Password, _smsdata.FromPhoneNumber, _smsdata.ToPhoneNumber, _smsdata.SMSText);
                    _smsdata.buffer = Encoding.ASCII.GetBytes(_smsdata.PostData);
                }


                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                ApiClient client = new ApiClient(new Uri(_nexmoWebRequestUrl));
                await client.PostAsync<ResponseObject, Sms>(new Uri(_nexmoWebRequestUrl), _smsdata);

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage

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

        private static bool ContainsUnicodeCharacter(string input)
        {
            const int MaxAnsiCode = 255;
            return input.Any(c => c > MaxAnsiCode);
        }


        /// <summary>
        /// This will check NRIC Validation
        /// </summary>
        /// <param name="_smsdata"></param>
        /// <returns>validtion result</returns>
        /// POST: api/NRICValidation/S1234567D 
        [HttpPost]
        [Route("NRICValidation/{NRIC}")]
        public async Task<IActionResult> NRICValidation([FromRoute] string NRIC)
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
                    Message = StatusMessages.SuccessMessage

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

        private static bool IsValidCheckSum(string NRIC)
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

        public static Dictionary<int, string> GetSeries(string SeriesLetter)
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
