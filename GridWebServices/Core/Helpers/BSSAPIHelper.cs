using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using Core.Enums;
using System.Linq;



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

        public async Task<BSSUpdateResponseObject> UpdateAssetBlockNumber(GridBSSConfi confi, string requestId, string asset)
        {
            ApiClient client = new ApiClient(new Uri(confi.BSSAPIUrl));

            BSSUpdateRequest request = new BSSUpdateRequest();

            SetParam param = new SetParam();

            UpdateRequestObject req = new UpdateRequestObject();

            BSSOrderInformation orderInformation = new BSSOrderInformation();

            var requestUrl = GetRequestUrl(confi.BSSAPIUrl, ref client);

            // set param list
            SetParamsBlockNumber(confi, asset);

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

        private void SetParamsBlockNumber(GridBSSConfi confi, string asset)
        {
            paramList = new List<RequestParam>();

            BSSParams bssParams = new BSSParams();

            AddParam(bssParams.AssetId, asset); // mobilenumber need to set here

            AddParam(bssParams.AssetStatus, ((int)AssetStatus.Blocked).ToString());           

            AddParam(bssParams.UnBlockAsset, "");          

            AddParam(bssParams.EntityId, confi.GridEntityId.ToString());
        }
        public string GetResponseCode(BSSUpdateResponseObject responseObject)
        {
            return responseObject.Response.result_code;
        }

    }
}
