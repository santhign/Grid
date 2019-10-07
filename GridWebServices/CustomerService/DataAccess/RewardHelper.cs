using Core.Enums;
using Core.Helpers;
using Core.Models;
using CustomerService.Models;
using InfrastructureService;
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
            Rewards _resp = null;
            try
            {
                ApiClient client = new ApiClient();
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
                _resp = JsonConvert.DeserializeObject<Rewards>(html);
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                throw;
            }
            return _resp;
        }
        public List<RewardDetails> GetRewardDetails(string RequestUrl, int AccountID, DateTime FromDate, DateTime ToDate)
        {
            List<RewardDetails> _resp = null;
            try
            {
                ApiClient client = new ApiClient();
                var requestUrl = GetRequestUrl(RequestUrl, ref client);

                RequestObject req = new RequestObject();
                string html = string.Empty;
                string url = requestUrl + $"{AccountID}?startDate={FromDate.ToString("yyyyMMdd")}&endDate={ToDate.ToString("yyyyMMdd")}";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }
                _resp = JsonConvert.DeserializeObject<List<RewardDetails>>(html);
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                throw;
            }
            return _resp;
        }

        private Uri GetRequestUrl(string url, ref ApiClient client)
        {
            return client.CreateRequestUri(new Uri(url),
                string.Format(System.Globalization.CultureInfo.InvariantCulture, url)
                );
        }
    }
}
