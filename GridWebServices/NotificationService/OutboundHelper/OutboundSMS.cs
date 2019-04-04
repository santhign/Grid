using Core.Helpers;
using Core.Models;
using InfrastructureService;
using Microsoft.Extensions.Configuration;
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
        public async Task<string> SendSMS(Sms _smsdata, IConfiguration _iconfiguration)
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

                string _nexmoUserName = _result.Single(x => x["key"] == "NexmoUserName")["value"];
                string _nexmoPassword = _result.Single(x => x["key"] == "NexmoPassword")["value"];
                string _nexmoSmsSignature = _result.Single(x => x["key"] == "NexmoSmsSignature")["value"];
                string _nexmoWebRequestUrl = _result.Single(x => x["key"] == "NexmoWebRequestUrl")["value"];

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
                ResponseObject _returnObject = await client.PostAsync<ResponseObject, Sms>(new Uri(_nexmoWebRequestUrl), _smsdata);
                return _returnObject.Response.resultCode;
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
