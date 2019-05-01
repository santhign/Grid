using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using Core.Enums;
using System.Linq;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace Core.Helpers
{
    public class EmailValidationHelper
    {
        List<RequestParam> paramList = new List<RequestParam>();

        public string EmailValidation(EmailConfig config)
        {
            //SingleRequestModel _model = new SingleRequestModel();
            //_model.email = config.Email;
            //NeverBounceSdk _neverbounce = new NeverBounceSdk(config.key);
            //SingleResponseModel _response = await _neverbounce.Single.Check(_model);
            //return _response.result;
            ApiClient client = new ApiClient(new Uri(config.EmailAPIUrl));

            var requestUrl = GetRequestUrl(config.EmailAPIUrl, ref client);

            RequestObject req = new RequestObject();
            string html = string.Empty;
            string url = requestUrl + $"?key={config.key}&email={config.Email}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }
            NBEmailResponse _resp = JsonConvert.DeserializeObject<NBEmailResponse>(html);
            return _resp.Result;
        }

        private Uri GetRequestUrl(string url, ref ApiClient client)
        {
            return client.CreateRequestUri(
                string.Format(System.Globalization.CultureInfo.InvariantCulture, url)
                );
        }

    }
}
