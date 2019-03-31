using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using Core.Enums;
using System.Linq;

namespace Core.Helpers
{
    public class EmailValidationHelper
    {
        List<RequestParam> paramList = new List<RequestParam>();
        public async Task<ResponseObject> EmailValidation(EmailConfig config)
        {
            ApiClient client = new ApiClient(new Uri(config.EmailAPIUrl));

            RequestParam _requestParam = new RequestParam();
            _requestParam.id = "key";
            _requestParam.value = config.key;
            paramList.Add(_requestParam);

            _requestParam = new RequestParam();
            _requestParam.id = "email";
            _requestParam.value = config.Email;
            paramList.Add(_requestParam);

            SetParam param = new SetParam();

            RequestObject req = new RequestObject();

            var requestUrl = GetRequestUrl(config.EmailAPIUrl, ref client);

            param.param = paramList;

            return await client.PostAsync<ResponseObject, RequestObject>(requestUrl, req);
        }

        private Uri GetRequestUrl(string url, ref ApiClient client)
        {
            return client.CreateRequestUri(
                string.Format(System.Globalization.CultureInfo.InvariantCulture, url)
                );
        }

    }
}
