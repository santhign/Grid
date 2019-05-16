using Core.Helpers;
using Core.Models;
using InfrastructureService;
using Microsoft.Extensions.Configuration;
using Nexmo.Api;
using NotificationService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.OutboundHelper
{
    public class OutboundSMS
    {      

        public async Task<string> SendSMSNotification(Sms _smsdata, IConfiguration _iconfiguration)
        {
            try
            {
                string _warningmsg = "";
                if (_smsdata.PhoneNumber.Length < 8)
                {
                    _warningmsg = "Phone number should be atleast 8 digit long.";
                    LogInfo.Warning(_warningmsg);
                    throw new Exception(_warningmsg);
                }
                else if (_smsdata.PhoneNumber.Length > 11)
                {
                    _warningmsg = "Phone number should not be longer than 11 digits.";
                    LogInfo.Warning(_warningmsg);
                    throw new Exception(_warningmsg);
                }

                long lPhoneNumber = 0;
                if (!long.TryParse(_smsdata.PhoneNumber, out lPhoneNumber))
                {
                    _warningmsg = "Alphabets are not allowed.";
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

                string _nexmoApiKey = _result.Single(x => x["key"] == "NexmoApiKey")["value"];
                string _nexmoSecret = _result.Single(x => x["key"] == "NexmoSecret")["value"];
                string _nexmoSmsSignature = _result.Single(x => x["key"] == "NexmoSmsSignature")["value"];
                string _nexmoWebRequestUrl = _result.Single(x => x["key"] == "NexmoWebRequestUrl")["value"];
                
                var client = new Client(creds: new Nexmo.Api.Request.Credentials
                {
                    ApiKey = _nexmoApiKey,
                    ApiSecret = _nexmoSecret
                });

                //Remove + sign if present
                var checkForPlus = _smsdata.PhoneNumber.Substring(0, 1);
                if(checkForPlus == "+")
                {
                    _smsdata.PhoneNumber = _smsdata.PhoneNumber.Remove(0,1);
                }

                //Append 65 country code if not present
                var checkForCountryCode = _smsdata.PhoneNumber.Substring(0, 2);              
                if (checkForCountryCode != "65")
                {
                    _smsdata.PhoneNumber = "65" + _smsdata.PhoneNumber;
                }

                var results = client.SMS.Send(request: new SMS.SMSRequest
                {
                    from = _nexmoSmsSignature,
                    to = _smsdata.PhoneNumber,
                    text = _smsdata.SMSText
                });              

                return "success";
            }
            catch (Exception ex)
            {
                LogInfo.Fatal(ex, "SMS outbound exception");
                return "failure";
            }
        }




        private bool ContainsUnicodeCharacter(string input)
        {
            const int MaxAnsiCode = 255;
            return input.Any(c => c > MaxAnsiCode);
        }
    }
}
