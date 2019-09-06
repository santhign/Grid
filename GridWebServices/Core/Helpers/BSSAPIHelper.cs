using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using Core.Enums;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Serilog;
using System.Net;
using System.IO;

namespace Core.Helpers
{
    public class BSSAPIHelper
    {
        List<RequestParam> paramList = new List<RequestParam>();
        public async Task<ResponseObject> GetAssetInventory(GridBSSConfi confi, int serviceCode, BSSAssetRequest assetRequest)
        {
            try
            {
                ApiClient client = new ApiClient(new Uri(confi.BSSAPIUrl));
                var requestUrl = GetRequestUrl(confi.BSSAPIUrl, ref client);

                RequestObject req = new RequestObject();
                SetParams(confi, serviceCode);
                BSSAssetRequest request = new BSSAssetRequest();
                SetParam param = new SetParam();
                param.param = paramList;

                request.request_id = assetRequest.request_id;
                request.request_timestamp = DateTime.Now.ToString("ddMMyyyyhhmmss");
                request.action = BSSApis.GetAssets.ToString();
                request.userid = assetRequest.userid;
                request.username = confi.GridUserName;
                request.source_node = confi.GridSourceNode;
                request.dataset = param;
                req.Request = request;
                string html = string.Empty;
                byte[] buffer;
                buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req));
                HttpWebRequest httprequest = (HttpWebRequest)WebRequest.Create(requestUrl);
                httprequest.AutomaticDecompression = DecompressionMethods.GZip;
                httprequest.Method = "POST";
                httprequest.ContentType = "application/x-www-form-urlencoded";
                httprequest.ContentLength = buffer.Length;
                Stream newStream = httprequest.GetRequestStream();
                await newStream.WriteAsync(buffer, 0, buffer.Length);
                newStream.Close();
                using (HttpWebResponse response = (HttpWebResponse)httprequest.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }
                ResponseObject _resp = JsonConvert.DeserializeObject<ResponseObject>(html);
                //return _resp.Result;



                

                

                Log.Information(JsonConvert.SerializeObject(req));
                Log.Information(JsonConvert.SerializeObject(html));

                return _resp;
            }
            catch (Exception ex)
            {
                Log.Information("POST exception - " + ex.Message);
                throw ex;
            }
        }

        private void SetParams(GridBSSConfi confi, int serviceCode)
        {
            try
            {
                paramList = new List<RequestParam>();

                BSSParams bssParams = new BSSParams();

                AddParam(bssParams.AssetStatus, ((int)AssetStatus.New).ToString());

                AddParam(bssParams.CategoryId, serviceCode.ToString());

                AddParam(bssParams.ProductId, confi.GridProductId.ToString());

                AddParam(bssParams.Offset, confi.GridDefaultOffset.ToString());

                AddParam(bssParams.Limit, confi.GridDefaultLimit.ToString());

                AddParam(bssParams.EntityId, confi.GridEntityId.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void AddParam(string id, string value)
        {
            try
            {
                paramList.Add(new RequestParam { id = id, value = value });
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private Uri GetRequestUrl(string url, ref ApiClient client)
        {
            try
            {
                return client.CreateRequestUri(
                 string.Format(System.Globalization.CultureInfo.InvariantCulture, url)
                 );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public GridBSSConfi GetGridConfig(List<Dictionary<string, string>> configDict)
        {
            try
            {
                GridBSSConfi config = new GridBSSConfi();
                config.GridId = int.Parse(configDict.Single(x => x["key"] == "GridId")["value"]);

#if DEBUG
                config.BSSAPIUrl = configDict.Single(x => x["key"] == "BSSAPILocalUrl")["value"];
#else
                config.BSSAPIUrl = configDict.Single(x => x["key"] == "BSSAPIUrl")["value"];
#endif
                config.GridDefaultAssetLimit = int.Parse(configDict.Single(x => x["key"] == "GridDefaultAssetLimit")["value"]);
                config.GridDefaultLimit = int.Parse(configDict.Single(x => x["key"] == "GridDefaultLimit")["value"]);
                config.GridDefaultOffset = int.Parse(configDict.Single(x => x["key"] == "GridDefaultOffset")["value"]);
                config.GridEntityId = int.Parse(configDict.Single(x => x["key"] == "GridEntityId")["value"]);
                config.GridProductId = int.Parse(configDict.Single(x => x["key"] == "GridProductId")["value"]);
                config.GridSourceNode = configDict.Single(x => x["key"] == "GridSourceNode")["value"];
                config.GridUserName = configDict.Single(x => x["key"] == "GridUserName")["value"];
                config.GridInvoiceRecordLimit = int.Parse(configDict.Single(x => x["key"] == "GridInvoiceRecordLimit")["value"]);
                return config;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetAssetId(ResponseObject responseObject)
        {
            try
            {
                return responseObject.Response.asset_details.assets.FirstOrDefault().asset_id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<BSSUpdateResponseObject> UpdateAssetBlockNumber(GridBSSConfi confi, BSSAssetRequest assetReq, string asset, bool unblock)
        {
            try
            {
                ApiClient client = new ApiClient(new Uri(confi.BSSAPIUrl));

                BSSUpdateRequest request = new BSSUpdateRequest();

                SetParam param = new SetParam();

                UpdateRequestObject req = new UpdateRequestObject();

                BSSOrderInformation orderInformation = new BSSOrderInformation();

                var requestUrl = GetRequestUrl(confi.BSSAPIUrl, ref client);

                // set param list
                SetParamsBlockNumber(confi, asset, unblock);

                param.param = paramList;

                request.request_id = assetReq.request_id;

                request.request_timestamp = DateTime.Now.ToString("ddMMyyyyhhmmss");

                request.action = BSSApis.UpdateAssetStatus.ToString();

                request.userid = assetReq.userid;

                request.username = confi.GridUserName;

                request.source_node = confi.GridSourceNode;

                // set order information

                orderInformation.customer_name = "";

                orderInformation.order_type = BSSApis.UpdateAssetStatus.ToString();

                // sert parameter list in order information
                orderInformation.dataset = param;

                // set order information to request
                request.order_information = orderInformation;

                req.Request = request;

                Log.Information(JsonConvert.SerializeObject(req));

                return await client.PostAsync<BSSUpdateResponseObject, UpdateRequestObject>(requestUrl, req);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetParamsBlockNumber(GridBSSConfi confi, string asset, bool unblock)
        {
            try
            {
                paramList = new List<RequestParam>();

                BSSParams bssParams = new BSSParams();

                AddParam(bssParams.AssetId, asset);


                if (unblock)
                {
                    AddParam(bssParams.AssetStatus, ((int)AssetStatus.New).ToString());

                    AddParam(bssParams.UnBlockAsset, "true");
                }

                else
                {
                    AddParam(bssParams.AssetStatus, ((int)AssetStatus.Blocked).ToString());

                    AddParam(bssParams.UnBlockAsset, "false");
                }


                AddParam(bssParams.EntityId, confi.GridEntityId.ToString());

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string GetResponseCode(BSSUpdateResponseObject responseObject)
        {
            try
            {

                return responseObject.Response.result_code;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public GridSystemConfig GetGridSystemConfig(List<Dictionary<string, string>> configDict)
        {

            try
            {
                GridSystemConfig config = new GridSystemConfig();
                config.DeliveryMarginInDays = int.Parse(configDict.Single(x => x["key"] == "DeliveryMarginInDays")["value"]);
                config.FreeNumberListCount = int.Parse(configDict.Single(x => x["key"] == "FreeNumberListCount")["value"]);
                config.PremiumNumberListCount = int.Parse(configDict.Single(x => x["key"] == "PremiumNumberListCount")["value"]);
                return config;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseObject> GetAssetInventory(GridBSSConfi confi, int serviceCode, BSSAssetRequest assetRequest, int count)
        {
            try
            {
                ApiClient client = new ApiClient(new Uri(confi.BSSAPIUrl));

                BSSAssetRequest request = new BSSAssetRequest();

                SetParam param = new SetParam();

                RequestObject req = new RequestObject();

                var requestUrl = GetRequestUrl(confi.BSSAPIUrl, ref client);

                SetParams(confi, serviceCode, count);

                param.param = paramList;

                request.request_id = assetRequest.request_id;

                request.request_timestamp = DateTime.Now.ToString("ddMMyyyyhhmmss");

                request.action = BSSApis.GetAssets.ToString();

                request.userid = assetRequest.userid;

                request.username = confi.GridUserName;

                request.source_node = confi.GridSourceNode;

                request.dataset = param;

                req.Request = request;

                Log.Information(JsonConvert.SerializeObject(req));

                return await client.PostAsync<ResponseObject, RequestObject>(requestUrl, req);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetParams(GridBSSConfi confi, int serviceCode, int limit)
        {
            try
            {
                paramList = new List<RequestParam>();

                BSSParams bssParams = new BSSParams();

                AddParam(bssParams.AssetStatus, ((int)AssetStatus.New).ToString());

                AddParam(bssParams.CategoryId, serviceCode.ToString());

                AddParam(bssParams.ProductId, confi.GridProductId.ToString());

                AddParam(bssParams.Offset, confi.GridDefaultOffset.ToString());

                AddParam(bssParams.Limit, limit.ToString());

                AddParam(bssParams.EntityId, confi.GridEntityId.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public List<FreeNumber> GetFreeNumbers(ResponseObject responseObject)
        {
            try
            {
                List<FreeNumber> numbers = new List<FreeNumber>();

                List<BSSAsset> assets = new List<BSSAsset>();

                assets = responseObject.Response.asset_details.assets;

                numbers = (from asset in assets
                           select new FreeNumber
                           {
                               MobileNumber = asset.asset_id,
                               ServiceCode = asset.category_type
                           }).ToList();

                return numbers;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<PremiumNumbers> GetPremiumNumbers(ResponseObject responseObject, ServiceFees fee)
        {
            try
            {
                List<PremiumNumbers> numbers = new List<PremiumNumbers>();

                List<BSSAsset> assets = new List<BSSAsset>();

                assets = responseObject.Response.asset_details.assets;

                numbers = (from asset in assets
                           select new PremiumNumbers
                           {
                               MobileNumber = asset.asset_id,
                               PortalServiceName = fee.PortalServiceName,
                               ServiceCode = fee.ServiceCode,
                               Price = fee.ServiceFee
                           }).ToList();

                return numbers;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<BSSQueryPlanResponseObject> GetUsageHistory(GridBSSConfi confi, string mobileNumber, string requestId)
        {
            try
            {
                ApiClient client = new ApiClient(new Uri(confi.BSSAPIUrl));

                BSSQueryPlanRequest request = new BSSQueryPlanRequest();

                QueryPlanRequestObject req = new QueryPlanRequestObject();

                QueryPlanDataset dataset = new QueryPlanDataset();

                var requestUrl = GetRequestUrl(confi.BSSAPIUrl, ref client);

                SetParamsForUsageRequest(confi, mobileNumber);

                dataset.param = paramList;

                List<string> filters = new List<string>();

                filters.Add("base_plan/add_on");

                dataset.filters = filters;

                request.request_id = requestId;

                request.request_timestamp = DateTime.Now.ToString("ddMMyyyyhhmmss");

                request.action = BSSApis.QueryPlan.ToString();

                request.userid = confi.GridId.ToString();

                request.username = confi.GridUserName;

                request.source_node = confi.GridSourceNode;

                request.dataset = dataset;

                req.Request = request;

                Log.Information(JsonConvert.SerializeObject(req));

                return await client.PostAsync<BSSQueryPlanResponseObject, QueryPlanRequestObject>(requestUrl, req);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetParamsForUsageRequest(GridBSSConfi confi, string mobileNumber)
        {
            try
            {
                paramList = new List<RequestParam>();

                BSSParams bssParams = new BSSParams();

                AddParam(bssParams.ServiceId, mobileNumber);

                AddParam(bssParams.ConnectionType, ((int)ConnectionTypes.Prepaid).ToString());

                AddParam(bssParams.EntityId, confi.GridEntityId.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public BSSQueryPlanResponse GetQueryPlan(object response)
        {
            try
            {
                BSSQueryPlanResponse plan = new BSSQueryPlanResponse();

                var json = JsonConvert.SerializeObject(response, MicrosoftDateFormatSettings);
                JObject jObject = JObject.Parse(json);
                var queryPlan = jObject["Response"];
                plan.request_id = queryPlan["request_id"].ToObject<String>();
                plan.request_timestamp = queryPlan["request_timestamp"].ToObject<String>();
                plan.response_timestamp = queryPlan["response_timestamp"].ToObject<String>();
                plan.result_code = queryPlan["result_code"].ToObject<String>();
                plan.source_node = queryPlan["source_node"].ToObject<String>();
                plan.action = queryPlan["action"].ToObject<String>();
                plan.result_desc = queryPlan["result_desc"].ToObject<String>();
                plan.dataSet = queryPlan["dataSet"].ToObject<QueryPlanDataset>();

                if (plan.result_code == "SC0000")
                {
                    plan.bundles = queryPlan["bundles"].ToObject<List<BSSBundle>>();
                }

                return plan;

            }
            catch (Exception ex)
            {
                throw ex;
            }
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

        public async Task<BSSInvoiceResponseObject> GetBSSCustomerInvoice(GridBSSConfi confi, string requestId, string accountNumber, int dateRange)
        {
            try

            {
                ApiClient client = new ApiClient(new Uri(confi.BSSAPIUrl));

                BSSInvoiceRequest request = new BSSInvoiceRequest();

                BSSInvoiceRequestObject req = new BSSInvoiceRequestObject();

                SetParam dataset = new SetParam();

                var requestUrl = GetRequestUrl(confi.BSSAPIUrl, ref client);

                SetParamsForInvoiceRequest(confi, accountNumber, dateRange);

                dataset.param = paramList;

                request.request_id = requestId;

                request.request_timestamp = DateTime.Now.ToString("ddMMyyyyhhmmss");

                request.action = BSSApis.GetInvoiceDetails.ToString();

                request.userid = confi.GridId.ToString();

                request.username = confi.GridUserName;

                request.source_node = confi.GridSourceNode;

                request.dataset = dataset;

                req.Request = request;

                Log.Information(JsonConvert.SerializeObject(req));

                return await client.PostAsync<BSSInvoiceResponseObject, BSSInvoiceRequestObject>(requestUrl, req);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetParamsForInvoiceRequest(GridBSSConfi confi, string accountNumber, int rangeInMonths)
        {
            try
            {
                paramList = new List<RequestParam>();

                BSSParams bssParams = new BSSParams();               

                AddParam(bssParams.Limit, confi.GridInvoiceRecordLimit.ToString());

                AddParam(bssParams.Status, ((int)Status.Confirm).ToString());  // verify whether status need to input at any time pending/confirm

                if (rangeInMonths > 0)
                {
                    DateTime endDate = DateTime.Now;

                    DateTime startDate = endDate.AddMonths(-(rangeInMonths));

                   // AddParam(bssParams.FromDate, startDate.ToString("yyyy-MM-dd")); //"2016-11-15

                   // AddParam(bssParams.ToDate, endDate.ToString("yyyy-MM-dd")); //"2016-11-15
                }               

                AddParam(bssParams.AccountId, accountNumber);

                AddParam(bssParams.EntityId, confi.GridEntityId.ToString());

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetJsonString(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, MicrosoftDateFormatSettings);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<BSSAccountQuerySubscriberResponse> GetBSSOutstandingPayment(GridBSSConfi confi, string requestId, string accountNumber)
        {
            try

            {
                ApiClient client = new ApiClient(new Uri(confi.BSSAPIUrl));

                BSSQueryPlanRequest request = new BSSQueryPlanRequest();

                QueryPlanRequestObject req = new QueryPlanRequestObject();

                QueryPlanDataset dataset = new QueryPlanDataset();

                var requestUrl = GetRequestUrl(confi.BSSAPIUrl, ref client);

                SetParamsOutstandingPaymentRequest(confi, accountNumber);

                dataset.param = paramList;

                List<string> filters = new List<string>();

                filters.Add("account");

                dataset.filters = filters;

                request.request_id = requestId;

                request.request_timestamp = DateTime.Now.ToString("ddMMyyyyhhmmss");

                request.action = BSSApis.QuerySubscriber.ToString();

                request.userid = confi.GridId.ToString();

                request.username = confi.GridUserName;

                request.source_node = confi.GridSourceNode;

                request.dataset = dataset;

                req.Request = request;

                Log.Information(JsonConvert.SerializeObject(req));

                return await client.PostAsync<BSSAccountQuerySubscriberResponse, QueryPlanRequestObject>(requestUrl, req);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetParamsOutstandingPaymentRequest(GridBSSConfi confi, string accountNumber)
        {
            try
            {
                paramList = new List<RequestParam>();

                BSSParams bssParams = new BSSParams();             
              
                AddParam(bssParams.AccountId, accountNumber);              

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<byte[]> GetInvoiceStream(string url)
        {
            try

            {
                ApiClient client = new ApiClient(new Uri(url)); 

                return await client.DownloadAsync(url);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
