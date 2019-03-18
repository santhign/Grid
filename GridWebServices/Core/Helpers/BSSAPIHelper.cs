using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Models;



namespace Core.Helpers
{
    public class BSSAPIHelper
    {
        List<RequestParam> paramList = new List<RequestParam>();
        public async Task<AssetsResponse> GetAssetInventory()
        {
            //ApiClient client = new ApiClient( new Uri("http://10.184.2.107:18080/APIGateway/APIRequest/Submit"));

          

            //var requestUrl =client. CreateRequestUri(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                
            //    "http://10.184.2.107:18080/APIGateway/APIRequest/Submit"));

            //BSSAssetRequest request = new BSSAssetRequest();

            //SetParam param = new SetParam();

            

            //AddParam("product_type", 94);

            //AddParam("asset_status", 1);

            //AddParam("asset_id",0);

            //AddParam("product_id", 41328);

            //AddParam("offset", 1);

            //AddParam("limit", 100);

            //AddParam("start_range", 0);

            //AddParam("end_range", 0);

            //AddParam("pos_id", 0);

            //AddParam("category_id", 0);

            //AddParam("entity_id", 41001343);

            //param.param = paramList;

            //request.request_id = "GR260110000000000005";

            //request.request_timestamp = DateTime.Now.ToString("ddmmyyyyhhmmss");

            //request.action = "GetAssets";

            //request.userid = 212;

            //request.username = "griduser";

            //request.source_node = "griduser";


            //request.dataset = param;

            //RequestObject req = new RequestObject();

            //req.Request = request;

            //return await client.PostAsync<AssetsResponse, RequestObject>(requestUrl, req);

        }

        private void AddParam(string id, int? value)
        {
            paramList.Add(new RequestParam {id=id,value=value.GetValueOrDefault()});

        }
    }
}
