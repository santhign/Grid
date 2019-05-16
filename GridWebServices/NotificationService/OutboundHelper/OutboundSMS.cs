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

                string _nexmoApiKey = _result.Single(x => x["key"] == "NexmoApiKey")["value"];
                string _nexmoSecret = _result.Single(x => x["key"] == "NexmoSecret")["value"];
                string _nexmoSmsSignature = _result.Single(x => x["key"] == "NexmoSmsSignature")["value"];
                string _nexmoWebRequestUrl = _result.Single(x => x["key"] == "NexmoWebRequestUrl")["value"];

                //_smsdata.Username = _nexmoApiKey;
                //_smsdata.Password = _nexmoSecret;
                //_smsdata.FromPhoneNumber = _nexmoSmsSignature;
                //_smsdata.ToPhoneNumber = _smsdata.PhoneNumber;
                //_smsdata.Type = "unicode";
                
                var client = new Client(creds: new Nexmo.Api.Request.Credentials
                {
                    ApiKey = _nexmoApiKey,
                    ApiSecret = _nexmoSecret
                });


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
