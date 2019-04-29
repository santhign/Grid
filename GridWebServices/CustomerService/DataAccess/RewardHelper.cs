using Core.Helpers;
using Core.Models;
using CustomerService.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CustomerService.DataAccess
{
    public class RewardHelper
    {
        public Rewards GetRewardSummary(string RequestUrl, int AccountID)
        {
            ApiClient client = new ApiClient(new Uri(RequestUrl));
            var requestUrl = GetRequestUrl(RequestUrl, ref client);

            RequestObject req = new RequestObject();
            string html = string.Empty;
            string url = requestUrl + $"{AccountID}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }
            Rewards _resp = JsonConvert.DeserializeObject<Rewards>(html);
            return _resp;
        }
        public RewardDetails GetRewardDetails(string RequestUrl, int AccountID, DateTime FromDate, DateTime ToDate)
        {
            ApiClient client = new ApiClient(new Uri(RequestUrl));
            var requestUrl = GetRequestUrl(RequestUrl, ref client);

            RequestObject req = new RequestObject();
            string html = string.Empty;
            string url = requestUrl + $"{AccountID}?sartDate={FromDate.ToString("yyyyMMdd")}&endDate={ToDate.ToString("yyyyMMdd")}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }
            RewardDetails _resp = JsonConvert.DeserializeObject<RewardDetails>(html);
            return _resp;
        }

        private Uri GetRequestUrl(string url, ref ApiClient client)
        {
            return client.CreateRequestUri(
                string.Format(System.Globalization.CultureInfo.InvariantCulture, url)
                );
        }
    }
}
