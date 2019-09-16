using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Net;
using Serilog;
using System.Threading;

namespace Core.Helpers
{
    public  class ApiClient
    {

        private HttpClient _httpClient;
        private Uri BaseEndpoint { get; set; }

        public ApiClient(Uri baseEndpoint)
        {
            if (baseEndpoint == null)
            {
                throw new ArgumentNullException("baseEndpoint");
            }
            BaseEndpoint = baseEndpoint;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        /// <summary>  
        /// Common method for making GET calls  
        /// </summary>  
        public async Task<T> GetAsync<T>(Uri requestUrl)
        {
            addHeaders();
            var response = await _httpClient.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
            Log.Information(JsonConvert.SerializeObject(data));
            return JsonConvert.DeserializeObject<T>(data);
        }

        /// <summary>  
        /// Common method for making POST calls  
        /// </summary>  
        public async Task<T> PostAsync<T>(Uri requestUrl, T content)
        {
            addHeaders();
            var response = await _httpClient.PostAsync(requestUrl.ToString(), CreateHttpContent<T>(content));
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(data);
        }
        public async Task<T1> PostAsync<T1, T2>(Uri requestUrl, T2 content)
        {
            try
            {
                addHeaders();
                Thread.Sleep(300);
                var response = _httpClient.PostAsync(requestUrl.ToString(), CreateHttpContent<T2>(content));
                
                response.Wait();
                var data = await response.Result.Content.ReadAsStringAsync();
                Log.Information(JsonConvert.SerializeObject(data));
                return JsonConvert.DeserializeObject<T1>(data);
            }
            catch (Exception ex)
            {
                throw new Exception("BSS post failed on base class", ex);
            }
        } 
        public async Task<byte[]> DownloadAsync(string requestUri)
        {
            WebClient webClient = new WebClient();

            byte[] fileBytes = null;

            await Task.Run(() => {

            fileBytes=   webClient.DownloadData(requestUri);

            });
            return fileBytes;
        }        

        /// <summary>  
        /// Common method for making form POST calls  
        /// </summary>  
        public async Task<T> PostAsync<T>(Uri requestUrl, T content, string format)
        {
            addHeaders();
            var response = await _httpClient.PostAsync(requestUrl.ToString(), CreateHttpContent<T>(content));
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
            Log.Information(JsonConvert.SerializeObject(data));
            return JsonConvert.DeserializeObject<T>(data);
        }

        public Uri CreateRequestUri(string relativePath, string queryString = "")
        {
            var endpoint = new Uri(BaseEndpoint, relativePath);
            var uriBuilder = new UriBuilder(endpoint);
            uriBuilder.Query = queryString;
            return uriBuilder.Uri;
        }

        public HttpContent CreateHttpContent<T>(T content)
        {
            var json = JsonConvert.SerializeObject(content, MicrosoftDateFormatSettings);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        public static JsonSerializerSettings MicrosoftDateFormatSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
                };
            }
        }

        public void addHeaders()
        {
            // if athuentication required, add authorization header here
           
        }
    }
}
