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
        public async Task<ResponseObject> GetEmailValidation(EmailConfig confi)
        {
            ApiClient client = new ApiClient(new Uri(confi.EmailAPIUrl));

            RequestParam req1 = new RequestParam();
            req1.id = "key";
            req1.value = confi.key;
            paramList.Add(req1);

            req1 = new RequestParam();
            req1.id = "email";
            req1.value = confi.Email;
            paramList.Add(req1);

            SetParam param = new SetParam();

            RequestObject req = new RequestObject();

            var requestUrl = GetRequestUrl(confi.EmailAPIUrl, ref client);

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
