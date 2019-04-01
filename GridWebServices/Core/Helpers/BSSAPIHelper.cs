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





namespace Core.Helpers
{
    public class BSSAPIHelper
    {
        List<RequestParam> paramList = new List<RequestParam>();
        public async Task<ResponseObject> GetAssetInventory( GridBSSConfi confi, int serviceCode, string requestId)
        {
            ApiClient client = new ApiClient(new Uri(confi.BSSAPIUrl));           

            BSSAssetRequest request = new BSSAssetRequest();

            SetParam param = new SetParam();

            RequestObject req = new RequestObject();

            var requestUrl = GetRequestUrl(confi.BSSAPIUrl, ref client);

            SetParams(confi,serviceCode);

            param.param = paramList;

            request.request_id = requestId;

            request.request_timestamp = DateTime.Now.ToString("ddmmyyyyhhmmss");

            request.action = BSSApis.GetAssets.ToString();

            request.userid = confi.GridId;

            request.username = confi.GridUserName;

            request.source_node = confi.GridSourceNode;

            request.dataset = param;            

            req.Request = request;

            return await client.PostAsync<ResponseObject, RequestObject>(requestUrl, req);
        }

        private void SetParams(GridBSSConfi confi, int serviceCode)
        {
            paramList = new List<RequestParam>();

            BSSParams bssParams = new BSSParams();

            AddParam(bssParams.AssetStatus, ((int)AssetStatus.New).ToString());

            AddParam(bssParams.CategoryId, serviceCode.ToString());

            AddParam(bssParams.ProductId, confi.GridProductId.ToString() ); 

            AddParam(bssParams.Offset,  confi.GridDefaultOffset.ToString());           

            AddParam(bssParams.Limit, confi.GridDefaultLimit.ToString());

            AddParam(bssParams.EntityId, confi.GridEntityId.ToString());

        }

        private void AddParam(string id, string value)
        {
            paramList.Add(new RequestParam {id=id,value=value});

        }
        private Uri GetRequestUrl(string url, ref ApiClient client)
        {
          return  client.CreateRequestUri(
              string.Format(System.Globalization.CultureInfo.InvariantCulture,url)
              );
        }

        public GridBSSConfi GetGridConfig(List<Dictionary<string, string>> configDict)
        {
            GridBSSConfi config = new GridBSSConfi();
            config.GridId = int.Parse(configDict.Single(x => x["key"] == "GridId")["value"]); 
            config.BSSAPIUrl = configDict.Single(x => x["key"] == "BSSAPIUrl")["value"];
            config.GridDefaultAssetLimit = int.Parse(configDict.Single(x => x["key"] == "GridDefaultAssetLimit")["value"]);
            config.GridDefaultLimit = int.Parse(configDict.Single(x => x["key"] == "GridDefaultLimit")["value"]);
            config.GridDefaultOffset = int.Parse(configDict.Single(x => x["key"] == "GridDefaultOffset")["value"]);
            config.GridEntityId = int.Parse(configDict.Single(x => x["key"] == "GridEntityId")["value"]);
            config.GridProductId = int.Parse(configDict.Single(x => x["key"] == "GridProductId")["value"]);
            config.GridSourceNode = configDict.Single(x => x["key"] == "GridSourceNode")["value"];
            config.GridUserName = configDict.Single(x => x["key"] == "GridUserName")["value"];

            return config;
        }

        public string GetAssetId(ResponseObject responseObject)
        {
          return  responseObject.Response.asset_details.assets.FirstOrDefault().asset_id;
        }

        public async Task<BSSUpdateResponseObject> UpdateAssetBlockNumber(GridBSSConfi confi, string requestId, string asset, bool unblock)
        {
            ApiClient client = new ApiClient(new Uri(confi.BSSAPIUrl));

            BSSUpdateRequest request = new BSSUpdateRequest();

            SetParam param = new SetParam();

            UpdateRequestObject req = new UpdateRequestObject();

            BSSOrderInformation orderInformation = new BSSOrderInformation();

            var requestUrl = GetRequestUrl(confi.BSSAPIUrl, ref client);

            // set param list
            SetParamsBlockNumber(confi, asset,unblock);

            param.param = paramList;

            request.request_id = requestId;

            request.request_timestamp = DateTime.Now.ToString("ddmmyyyyhhmmss");

            request.action = BSSApis.UpdateAssetStatus.ToString();

            request.userid = confi.GridId;

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

            return await client.PostAsync<BSSUpdateResponseObject, UpdateRequestObject>(requestUrl, req);
        }

        private void SetParamsBlockNumber(GridBSSConfi confi, string asset, bool unblock)
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
        public string GetResponseCode(BSSUpdateResponseObject responseObject)
        {
            return responseObject.Response.result_code;
        }

        public GridSystemConfig GetGridSystemConfig(List<Dictionary<string, string>> configDict)
        {
            GridSystemConfig config = new GridSystemConfig();
            config.DeliveryMarginInDays = int.Parse(configDict.Single(x => x["key"] == "DeliveryMarginInDays")["value"]);
            config.FreeNumberListCount = int.Parse(configDict.Single(x => x["key"] == "FreeNumberListCount")["value"]);
            config.PremiumNumberListCount = int.Parse(configDict.Single(x => x["key"] == "PremiumNumberListCount")["value"]); 
            return config;
        }

        public async Task<ResponseObject> GetAssetInventory(GridBSSConfi confi, int serviceCode, string requestId, int count)
        {
            ApiClient client = new ApiClient(new Uri(confi.BSSAPIUrl));

            BSSAssetRequest request = new BSSAssetRequest();

            SetParam param = new SetParam();

            RequestObject req = new RequestObject();

            var requestUrl = GetRequestUrl(confi.BSSAPIUrl, ref client);

            SetParams(confi, serviceCode, count);

            param.param = paramList;

            request.request_id = requestId;

            request.request_timestamp = DateTime.Now.ToString("ddmmyyyyhhmmss");

            request.action = BSSApis.GetAssets.ToString();

            request.userid = confi.GridId;

            request.username = confi.GridUserName;

            request.source_node = confi.GridSourceNode;

            request.dataset = param;

            req.Request = request;

            return await client.PostAsync<ResponseObject, RequestObject>(requestUrl, req);
        }

        private void SetParams(GridBSSConfi confi, int serviceCode, int limit)
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

        public List<FreeNumber> GetFreeNumbers(ResponseObject responseObject)
        {
            List<FreeNumber> numbers = new List<FreeNumber>();

            List<BSSAsset> assets = new List<BSSAsset>();

            assets = responseObject.Response.asset_details.assets;

            numbers = (from asset in assets
                       select new FreeNumber
                       {
                           MobileNumber = asset.asset_id
                       }).ToList();

            return numbers;
        }

        public List<PremiumNumbers> GetPremiumNumbers(ResponseObject responseObject, ServiceFees fee)
        {
            List<PremiumNumbers> numbers = new List<PremiumNumbers>();

            List<BSSAsset> assets = new List<BSSAsset>();

            assets = responseObject.Response.asset_details.assets;

            numbers = (from asset in assets
                       select new PremiumNumbers
                       {
                           MobileNumber = asset.asset_id,
                           PortalServiceName = fee.PortalServiceName,
                           ServiceCode =  fee.ServiceCode,
                           Price=fee.ServiceFee
                       }).ToList();

            return numbers;
        }

        public async Task<object> GetUsageHistory(GridBSSConfi confi, string mobileNumber, string requestId)
        {
            ApiClient client = new ApiClient(new Uri(confi.BSSAPIUrl));

            BSSQueryPlanRequest request = new BSSQueryPlanRequest();

            QueryPlanRequestObject req = new QueryPlanRequestObject();

            QueryPlanDataset dataset = new QueryPlanDataset();

            var requestUrl = GetRequestUrl(confi.BSSAPIUrl, ref client);

            SetParamsForUsageRequest(confi, mobileNumber);

            dataset.param = paramList;

            List<string> filters = new List<string>();

            filters.Add("base_plan / addon");

            dataset.filters = filters;

            request.request_id = requestId;

            request.request_timestamp = DateTime.Now.ToString("ddmmyyyyhhmmss");

            request.action = BSSApis.QueryPlan.ToString();

            request.userid = confi.GridId.ToString();

            request.username = confi.GridUserName;

            request.source_node = confi.GridSourceNode;

            request.dataset = dataset;

            req.Request = request;

            return await client.PostAsync<object, QueryPlanRequestObject>(requestUrl, req);
        }

        private void SetParamsForUsageRequest(GridBSSConfi confi, string mobileNumber)
        {
            paramList = new List<RequestParam>();

            BSSParams bssParams = new BSSParams();

            AddParam(bssParams.ServiceId, mobileNumber);           

            AddParam(bssParams.ConnectionType, ((int) ConnectionTypes.Prepaid).ToString());                

            AddParam(bssParams.EntityId, confi.GridEntityId.ToString());
        }

        public  BSSQueryPlanResponse GetQueryPlan(object response)
        {
            BSSQueryPlanResponse plan = new BSSQueryPlanResponse();

            var json = JsonConvert.SerializeObject(response, MicrosoftDateFormatSettings);  
            JObject jObject = JObject.Parse(json);
            var queryPlan = jObject["Response"];
            plan.request_id = queryPlan["request_id"].ToObject<String>();
            plan.request_timestamp =queryPlan["request_timestamp"].ToObject<String>();
            plan.response_timestamp = queryPlan["response_timestamp"].ToObject<String>();
            plan.result_code = queryPlan["result_code"].ToObject<String>();
            plan.source_node = queryPlan["source_node"].ToObject<String>();        
            plan.action = queryPlan["action"].ToObject<String>();
            plan.result_desc= queryPlan["result_desc"].ToObject<String>();
            plan.dataSet = queryPlan["dataSet"].ToObject<QueryPlanDataset>();

            if (plan.result_code == "SC0000")
            {                
                plan.bundles = queryPlan["bundles"].ToObject<List<BSSBundle>>();               
            }

            return plan;
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

    }
}
