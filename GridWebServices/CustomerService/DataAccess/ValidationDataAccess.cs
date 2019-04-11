﻿using Core.Helpers;
using Core.Models;
using CustomerService.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CustomerService.DataAccess
{
    public class ValidationDataAccess
    {
        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public ValidationDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<DatabaseResponse> ValidatePostcode(string _postcode)
        {
            try
            {

                PostCodeRequest _postcodedata = new PostCodeRequest();
                DatabaseResponse configResponse = ConfigHelper.GetValue("Postcode", _configuration);

                List<Dictionary<string, string>> _result = ((List<Dictionary<string, string>>)configResponse.Results);

                _postcodedata.APIKey = _result.Single(x => x["key"] == "PostcodeApiKey")["value"];
                _postcodedata.APISecret = _result.Single(x => x["key"] == "PostcodeSecret")["value"];
                _postcodedata.Postcode = _postcode;
                string Postcodeurl = _result.Single(x => x["key"] == "Postcodeurl")["value"];

                byte[] buffer;
                string postData = string.Empty;

                postData = string.Format("APIKey={0}&APISecret={1}&Postcode={2}", _postcodedata.APIKey, _postcodedata.APISecret, _postcode);
                buffer = Encoding.UTF8.GetBytes(postData);

                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(Postcodeurl);
                myRequest.Method = "POST";
                myRequest.ContentType = "application/x-www-form-urlencoded";
                myRequest.ContentLength = buffer.Length;
                Stream newStream = myRequest.GetRequestStream();
                await newStream.WriteAsync(buffer, 0, buffer.Length);
                newStream.Close();
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myRequest.GetResponse();

                Stream streamResponse = myHttpWebResponse.GetResponseStream();
                StreamReader streamRead = new StreamReader(streamResponse);
                Char[] readBuffer = new Char[256];
                int count = streamRead.Read(readBuffer, 0, 256);
                StringBuilder Result = new StringBuilder();
                while (count > 0)
                {
                    String resultData = new String(readBuffer, 0, count);
                    Result.Append(resultData);
                    count = streamRead.Read(readBuffer, 0, 256);
                }
                streamRead.Close();
                streamResponse.Close();
                myHttpWebResponse.Close();

                dynamic data = JsonConvert.DeserializeObject(Result.ToString());

                return new DatabaseResponse { Results = data };
            }

            catch (Exception e)
            {
                throw e;
            }

        }
    }
}